using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using lab1_filters.Filters;

namespace lab1_filters.Quantizers
{
    public abstract class Quantizer : Filter
    {

        public Quantizer(bool singlePass)
        {
            _singlePass = singlePass;
            _pixelSize = Marshal.SizeOf(typeof(Color32));
        }

        public Bitmap Quantize(Image source)
        {
            // Get the size of the source image
            int height = source.Height;
            int width = source.Width;

            // And construct a rectangle from these dimensions
            Rectangle bounds = new Rectangle(0, 0, width, height);

            // First off take a 32bpp copy of the image
            Bitmap copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // And construct an 8bpp version
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Now lock the bitmap into memory
            using (Graphics g = Graphics.FromImage(copy))
            {
                g.PageUnit = GraphicsUnit.Pixel;

                // Draw the source image onto the copy bitmap,
                // which will effect a widening as appropriate.
                g.DrawImage(source, bounds);

            }

            // Define a pointer to the bitmap data
            BitmapData sourceData = null;

            try {
                // Get the source image bits and lock into memory
                sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                if (!_singlePass)
                    FirstPass(sourceData, width, height);

                // Then set the color palette on the output bitmap. I'm passing in the current palette 
                // as there's no way to construct a new, empty palette.
                output.Palette = GetPalette(output.Palette);


                // Then call the second pass which actually does the conversion
                SecondPass(sourceData, output, width, height, bounds);
            
            }  finally {
                // Ensure that the bits are unlocked
                copy.UnlockBits(sourceData);
            }

            // Last but not least, return the output bitmap
            return output;
        }

        protected virtual void FirstPass(BitmapData sourceData, int width, int height)
        {
            // Define the source data pointers. The source row is a byte to
            // keep addition of the stride value easier (as this is in bytes)              
            IntPtr pSourceRow = sourceData.Scan0;

            // Loop through each row
            for (int row = 0; row < height; row++)
            {
                // Set the source pixel to the first pixel in this row
                IntPtr pSourcePixel = pSourceRow;

                // And loop through each column
                for (int col = 0; col < width; col++)
                {
                    InitialQuantizePixel(new Color32(pSourcePixel));
                    pSourcePixel = (IntPtr)((Int32)pSourcePixel + _pixelSize);
                }	// Now I have the pixel, call the FirstPassQuantize function...

                // Add the stride to the source row
                pSourceRow = (IntPtr)((long)pSourceRow + sourceData.Stride);
            }
        }

        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData outputData = null;

            try {
                // Lock the output bitmap into memory
                outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

                // Define the source data pointers. The source row is a byte to
                // keep addition of the stride value easier (as this is in bytes)
                IntPtr pSourceRow = sourceData.Scan0;
                IntPtr pSourcePixel = pSourceRow;
                IntPtr pPreviousPixel = pSourcePixel;

                // Now define the destination data pointers
                IntPtr pDestinationRow = outputData.Scan0;
                IntPtr pDestinationPixel = pDestinationRow;

                // And convert the first pixel, so that I have values going into the loop

                byte pixelValue = QuantizePixel(new Color32(pSourcePixel));

                // Assign the value of the first pixel
                Marshal.WriteByte(pDestinationPixel, pixelValue);

                // Loop through each row
                for (int row = 0; row < height; row++)  {
                    // Set the source pixel to the first pixel in this row
                    pSourcePixel = pSourceRow;

                    // And set the destination pixel pointer to the first pixel in the row
                    pDestinationPixel = pDestinationRow;

                    // Loop through each pixel on this scan line
                    for (int col = 0; col < width; col++) {
                        // Check if this is the same as the last pixel. If so use that value
                        // rather than calculating it again. This is an inexpensive optimisation.
                        if (Marshal.ReadByte(pPreviousPixel) != Marshal.ReadByte(pSourcePixel))
                        {
                            // Quantize the pixel
                            pixelValue = QuantizePixel(new Color32(pSourcePixel));

                            // And setup the previous pointer
                            pPreviousPixel = pSourcePixel;
                        }

                        // And set the pixel in the output
                        Marshal.WriteByte(pDestinationPixel, pixelValue);

                        pSourcePixel = (IntPtr)((long)pSourcePixel + _pixelSize);
                        pDestinationPixel = (IntPtr)((long)pDestinationPixel + 1);

                    }

                    // Add the stride to the source row
                    pSourceRow = (IntPtr)((long)pSourceRow + sourceData.Stride);

                    // And to the destination row
                    pDestinationRow = (IntPtr)((long)pDestinationRow + outputData.Stride);
                }
            } finally {
                // Ensure that I unlock the output bits
                output.UnlockBits(outputData);
            }
        }

        protected virtual void InitialQuantizePixel(Color32 pixel)
        {
        }

        protected abstract byte QuantizePixel(Color32 pixel);

        protected abstract ColorPalette GetPalette(ColorPalette original);

        private bool _singlePass;
        private int _pixelSize;

        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {

            public Color32(IntPtr pSourcePixel)
            {
                this = (Color32)Marshal.PtrToStructure(pSourcePixel, typeof(Color32));

            }
        
            [FieldOffset(0)]
            public byte Blue;
            
            [FieldOffset(1)]
            public byte Green;
           
            [FieldOffset(2)]
            public byte Red;
            
            [FieldOffset(3)]
            public byte Alpha;

            [FieldOffset(0)]
            public int ARGB;

            public Color Color
            {
                get { return Color.FromArgb(Alpha, Red, Green, Blue); }
            }
        }
    }
}
