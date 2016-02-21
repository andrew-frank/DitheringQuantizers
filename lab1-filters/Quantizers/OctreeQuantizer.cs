using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace lab1_filters.Quantizers
{
    public class OctreeQuantizer : Quantizer
    {
        private Octree _octree;
        private int _maxColors;

        public override string FilterName {
            get { return "OctreeQuantizer"; }
        }

        private class Octree
        {
            public Octree(int maxColorBits)
            {
                _maxColorBits = maxColorBits;
                _leafCount = 0;
                _reducibleNodes = new OctreeNode[9];
                _root = new OctreeNode(0, _maxColorBits, this);
                _previousColor = 0;
                _previousNode = null;
            }
            
            public void AddColor(Color32 pixel)
            {
                // Check if this request is for the same color as the last
                if (_previousColor == pixel.ARGB)
                {
                    // If so, check if I have a previous node setup. This will only ocurr if the first color in the image
                    // happens to be black, with an alpha component of zero.
                    if (null == _previousNode)
                    {
                        _previousColor = pixel.ARGB;
                        _root.AddColor(pixel, _maxColorBits, 0, this);
                    }
                    else
                        // Just update the previous node
                        _previousNode.Increment(pixel);
                }
                else
                {
                    _previousColor = pixel.ARGB;
                    _root.AddColor(pixel, _maxColorBits, 0, this);
                }
            }

            public void Reduce()
            {
                int index;

                // Find the deepest level containing at least one reducible node
                for (index = _maxColorBits - 1; (index > 0) && (null == _reducibleNodes[index]); index--) ;

                // Reduce the node most recently added to the list at level 'index'
                OctreeNode node = _reducibleNodes[index];
                _reducibleNodes[index] = node.NextReducible;

                // Decrement the leaf count after reducing the node
                _leafCount -= node.Reduce();


                // And just in case I've reduced the last color to be added, and the next color to
                // be added is the same, invalidate the previousNode...
                _previousNode = null;
            }

            public int Leaves
            {
                get { return _leafCount; }
                set { _leafCount = value; }
            }

            protected OctreeNode[] ReducibleNodes
            {
                get { return _reducibleNodes; }
            }

            protected void TrackPrevious(OctreeNode node)
            {
                _previousNode = node;
            }

            // Convert the nodes in the octree to a palette with a maximum of colorCount colors
            // colorCount - The maximum number of colors
            // returns An arraylist with the palettized colors
            public ArrayList Palletize(int colorCount)
            {
                while (Leaves > colorCount)
                    Reduce();

                // Now palettize the nodes
                ArrayList palette = new ArrayList(Leaves);
                int paletteIndex = 0;
                _root.ConstructPalette(palette, ref paletteIndex);

                // And return the palette
                return palette;
            }

            // Get the palette index for the passed color
            public int GetPaletteIndex(Color32 pixel)
            {
                return _root.GetPaletteIndex(pixel, 0);
            }

            // Mask used when getting the appropriate pixels for a given node
            private static int[] mask = new int[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            private OctreeNode _root;
            private int _leafCount;
            private OctreeNode[] _reducibleNodes;
            private int _maxColorBits;
            private OctreeNode _previousNode;
            private int _previousColor;
            protected class OctreeNode
            {
                private bool _leaf;
                private int _pixelCount;
                private int _red;
                private int _green;
                private int _blue;
                private OctreeNode[] _children;
                private OctreeNode _nextReducible;
                private int _paletteIndex;

                public OctreeNode(int level, int colorBits, Octree octree)
                {
                    // Construct the new node
                    _leaf = (level == colorBits);

                    _red = _green = _blue = 0;
                    _pixelCount = 0;

                    // If a leaf, increment the leaf count
                    if (_leaf)
                    {
                        octree.Leaves++;
                        _nextReducible = null;
                        _children = null;
                    }
                    else
                    {
                        // Otherwise add this to the reducible nodes
                        _nextReducible = octree.ReducibleNodes[level];
                        octree.ReducibleNodes[level] = this;
                        _children = new OctreeNode[8];
                    }
                }

                public void AddColor(Color32 pixel, int colorBits, int level, Octree octree)
                {
                    // Update the color information if this is a leaf
                    if (_leaf)
                    {
                        Increment(pixel);
                        // Setup the previous node
                        octree.TrackPrevious(this);
                    }
                    else
                    {
                        // Go to the next level down in the tree
                        int shift = 7 - level;
                        int index = ((pixel.Red & mask[level]) >> (shift - 2)) |
                            ((pixel.Green & mask[level]) >> (shift - 1)) |
                            ((pixel.Blue & mask[level]) >> (shift));

                        OctreeNode child = _children[index];

                        if (null == child)
                        {
                            // Create a new child node & store in the array
                            child = new OctreeNode(level + 1, colorBits, octree);
                            _children[index] = child;
                        }

                        // Add the color to the child node
                        child.AddColor(pixel, colorBits, level + 1, octree);
                    }

                }
    
                public OctreeNode NextReducible
                {
                    get { return _nextReducible; }
                    set { _nextReducible = value; }
                }

                public OctreeNode[] Children
                {
                    get { return _children; }
                }

                public int Reduce()
                {
                    _red = _green = _blue = 0;
                    int children = 0;

                    // Loop through all children and add their information to this node
                    for (int index = 0; index < 8; index++)
                    {
                        if (null != _children[index])
                        {
                            _red += _children[index]._red;
                            _green += _children[index]._green;
                            _blue += _children[index]._blue;
                            _pixelCount += _children[index]._pixelCount;
                            ++children;
                            _children[index] = null;
                        }
                    }

                    // Now change this to a leaf node
                    _leaf = true;

                    // Return the number of nodes to decrement the leaf count by
                    return (children - 1);
                }

                public void ConstructPalette(ArrayList palette, ref int paletteIndex)
                {
                    if (_leaf)
                    {
                        // Consume the next palette index
                        _paletteIndex = paletteIndex++;

                        // And set the color of the palette entry
                        palette.Add(Color.FromArgb(_red / _pixelCount, _green / _pixelCount, _blue / _pixelCount));
                    }
                    else
                    {
                        // Loop through children looking for leaves
                        for (int index = 0; index < 8; index++)
                        {
                            if (null != _children[index])
                                _children[index].ConstructPalette(palette, ref paletteIndex);
                        }
                    }
                }

                public int GetPaletteIndex(Color32 pixel, int level)
                {
                    int paletteIndex = _paletteIndex;

                    if (!_leaf)
                    {
                        int shift = 7 - level;
                        int index = ((pixel.Red & mask[level]) >> (shift - 2)) |
                            ((pixel.Green & mask[level]) >> (shift - 1)) |
                            ((pixel.Blue & mask[level]) >> (shift));

                        if (null != _children[index])
                            paletteIndex = _children[index].GetPaletteIndex(pixel, level + 1);
                        else
                            throw new Exception("Didn't expect this!");
                    }

                    return paletteIndex;
                }

                public void Increment(Color32 pixel)
                {
                    _pixelCount++;
                    _red += pixel.Red;
                    _green += pixel.Green;
                    _blue += pixel.Blue;
                }                

                

            }
        }

        // The Octree quantizer is a two pass algorithm. The initial pass sets up the octree,
        // the second pass quantizes a color based on the nodes in the tree
        public OctreeQuantizer(int maxColors, int maxColorBits) : base(false)
        {
            if (maxColors > 255)
                throw new ArgumentOutOfRangeException("maxColors", maxColors, "The number of colors should be less than 256");

            if ((maxColorBits < 1) | (maxColorBits > 8))
                throw new ArgumentOutOfRangeException("maxColorBits", maxColorBits, "This should be between 1 and 8");

            // Construct the octree
            _octree = new Octree(maxColorBits);
            _maxColors = maxColors;
        }

        protected override void InitialQuantizePixel(Color32 pixel)
        {
            // Add the color to the octree
            _octree.AddColor(pixel);
        }

        protected override byte QuantizePixel(Color32 pixel)
        {
            byte paletteIndex = (byte)_maxColors;	// The color at [_maxColors] is set to transparent

            // Get the palette index if this non-transparent
            if (pixel.Alpha > 0)
                paletteIndex = (byte)_octree.GetPaletteIndex(pixel);

            return paletteIndex;
        }
       
        // Retrieve the palette for the quantized image        
        protected override ColorPalette GetPalette(ColorPalette original)
        {
            // Converts the octree to _maxColors colors
            ArrayList palette = _octree.Palletize(_maxColors - 1);

            // Then convert the palette based on those colors
            for (int index = 0; index < palette.Count; index++)
                original.Entries[index] = (Color)palette[index];

            // Add the transparent color
            original.Entries[_maxColors] = Color.FromArgb(0, 0, 0, 0);

            return original;
        }        
        
    }
}
