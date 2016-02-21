using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace lab1_filters.Filters {
    public static class ImageProcessor 
    {

       
        /**
         *  Extension methods - Writable bitmap image to byte[] and vice versa
         */

#region Writable bitmap image to byte[] and vice versa
        public static byte[] WriteableBitMapImageToArray(this WriteableBitmap bitmapSource) 
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var bitmapData = new byte[height * stride];
            bitmapSource.CopyPixels(bitmapData, stride, 0);
            return bitmapData;
        }

        public static WriteableBitmap ByteArrayToWritableBitmap(this BitmapImage sourceBitmap, byte[] byteArray) 
        {
            WriteableBitmap filteredWritableBitmap = new WriteableBitmap(sourceBitmap);
            /* 
             Summary:
                 Updates the pixels in the specified region of the bitmap.
            
             Parameters:
               sourceRect:
                 The rectangle of the System.Windows.Media.Imaging.WriteableBitmap to update.
            
               pixels:
                 The pixel array used to update the bitmap.
            
               stride:
                 The stride of the update region in pixels.
            
               offset:
                 The input buffer offset.
             */
            Int32Rect rect = new Int32Rect(0, 0, filteredWritableBitmap.PixelWidth, filteredWritableBitmap.PixelHeight);
            int stride = filteredWritableBitmap.PixelWidth * filteredWritableBitmap.Format.BitsPerPixel / 8;
            int offset = 0;
            filteredWritableBitmap.WritePixels(rect, byteArray, stride, offset);

            return filteredWritableBitmap;
        }

#endregion

        /**
         *  Extension method - converts BitmapSource to Bitmap
         */
        public static Bitmap ConvertToBitmap(this BitmapSource bitmapSource) 
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }

        /**
        *  Extension method - converts Bitmap to BitmapImage
        */
        public static BitmapImage BitmapToImageSource(this Bitmap bitmap) {
            using (System.IO.MemoryStream memory = new System.IO.MemoryStream()) {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        //Convolution filter:
        //Bitmap extension method (thanks to this keyword in argument and static method)
        public static Bitmap ConvolutionFilter<T>(this Bitmap sourceBitmap, T filter) where T : ConvolutionFilter 
        {

            //Locking a Bitmap into memory prevents the Garbage Collector
            //from moving a Bitmap object to a new location in memory.
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,  sourceBitmap.Width, sourceBitmap.Height),
                                                          ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            /*
             * The BitmapData.Stride property represents the number of bytes 
             * in a single Bitmap pixel row. 
             * 
             * In this scenario the BitmapData.Stride property should be equal
             * to the Bitmap’s width in pixels multiplied by four seeing as 
             * every pixel consists of four bytes: Alpha, Red, Green and Blue.
             */
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];


            /*
             * The BitmapData property BitmapData.Scan0 of type IntPtr represents the
             * memory address of the first byte value of a Bitmap’s underlying byte buffer.
             */
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            double red, green, blue = 0.0;

            int filterWidth = filter.FilterMatrix.GetLength(1);
            int filterHeight = filter.FilterMatrix.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset, byteOffset = 0;

            //iterate row by row
            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++) {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++) {
                    
                    blue = 0;
                    green = 0;
                    red = 0;

                   /* current pixel index =  (current row index (offsetY) * the number of ARGB byte values
                    * per row of pixels (sourceData.Stride)) + current column/pixel index (offsetX) * four.
                    */

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset ; filterY <= filterOffset; filterY++) {
                        for (int filterX = -filterOffset ; filterX <= filterOffset; filterX++) {

                            //calculatee the index of the neighbouring pixel in relation to the current pixel
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            //apply matrix to components
                            blue += (double)(pixelBuffer[calcOffset]) * 
                                filter.FilterMatrix[filterY + filterOffset, filterX + filterOffset];

                            green += (double)(pixelBuffer[calcOffset + 1]) * 
                                filter.FilterMatrix[filterY + filterOffset, filterX + filterOffset];

                            red += (double)(pixelBuffer[calcOffset + 2]) * 
                                filter.FilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    blue = filter.Factor * blue + filter.Offset;
                    green = filter.Factor * green + filter.Offset;
                    red = filter.Factor * red + filter.Offset;

                    //trim
                    if(blue > 255)  blue = 255; 
                    else if(blue < 0) blue = 0;
                    if(green > 255)  green = 255; 
                    else if(green < 0)  green = 0;
                    if(red > 255)  red = 255; 
                    else if(red < 0) red = 0;

                    //save
                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                    resultBitmap.Width, resultBitmap.Height),
                                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

    }
}





        ///**
        // * Extension method - convert BitmapImage to byte array
        // */ 
        //public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
        //{
        //    byte[] data;
        //    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
        //    using (MemoryStream ms = new MemoryStream()) {
        //        encoder.Save(ms);
        //        data = ms.ToArray();
        //    }
        //    return data;
        //}


//public static Image byteArrayToImage(byte[] byteArrayIn) {
//    MemoryStream ms = new MemoryStream(byteArrayIn);
//    Image returnImage = Image.FromStream(ms);
//    return returnImage;
//}



//public static BitmapImage DecodePhoto(byte[] byteVal, int width, int height) 
//{
//    if (byteVal == null) return null;

//    //try {
//        MemoryStream strmImg = new MemoryStream(byteVal);
//        BitmapImage myBitmapImage = new BitmapImage();
//        myBitmapImage.StreamSource = strmImg;
//        myBitmapImage.DecodePixelWidth = width;
//        myBitmapImage.DecodePixelHeight = height;
//        myBitmapImage.BeginInit();
//        myBitmapImage.Freeze();
//        return myBitmapImage;
//    //} catch (Exception ex) {
//    //    string message = ex.Message;
//    //    return null;
//    //}
//}


//public static Bitmap ByteArrayToBitmap(this BitmapImage sourceBitmap, byte[] byteArray)
//{
//    //int w = sourceBitmap.PixelWidth;
//    //int h = sourceBitmap.PixelHeight;
//    //int ch = 3; //number of channels (ie. assuming 24 bit RGB in this case)

//    //byte[] imageData = new byte[w * h * ch]; //you image data here
//    //Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
//    //BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
//    //IntPtr pNative = bmData.Scan0;
//    //Marshal.Copy(imageData, 0, pNative, w * h * ch);
//    //bitmap.UnlockBits(bmData);
//    //return bitmap;

//    WriteableBitmap filteredWritableBitmap = new WriteableBitmap(sourceBitmap);
//    filteredWritableBitmap.WritePixels(new Int32Rect(0, 0, filteredWritableBitmap.PixelWidth, filteredWritableBitmap.PixelHeight),
//        byteArray, filteredWritableBitmap.PixelWidth * filteredWritableBitmap.Format.BitsPerPixel / 8, 0);
//    return filteredWritableBitmap.BitmapFromWriteableBitmap();
//}




//public static byte[] ConvertToBytes(this Bitmap bitmap) 
//{
//    BitmapData bmpdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
//    int numbytes = bmpdata.Stride * bitmap.Height;
//    byte[] bytedata = new byte[numbytes];
//    IntPtr ptr = bmpdata.Scan0;

//    Marshal.Copy(ptr, bytedata, 0, numbytes);

//    bitmap.UnlockBits(bmpdata);

//    return bytedata;
//}


///**
// *  Extension method - converts Writable to Bitmap
// */
//public static System.Drawing.Bitmap BitmapFromWriteableBitmap(this WriteableBitmap writeBmp) 
//{
//    System.Drawing.Bitmap bmp;
//    using (MemoryStream outStream = new MemoryStream()) {
//        BitmapEncoder enc = new BmpBitmapEncoder();
//        enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
//        enc.Save(outStream);
//        bmp = new System.Drawing.Bitmap(outStream);
//    }
//    return bmp;
//}














///wtf
//
        //public static Bitmap CopyToSquareCanvas(this Bitmap sourceBitmap, int canvasWidthLenght) 
        //{
        //    float ratio = 1.0f;
        //    int maxSide = sourceBitmap.Width > sourceBitmap.Height ?
        //                  sourceBitmap.Width : sourceBitmap.Height;

        //    ratio = (float)maxSide / (float)canvasWidthLenght;

        //    Bitmap bitmapResult;
        //    if (sourceBitmap.Width > sourceBitmap.Height)
        //        bitmapResult = new Bitmap(canvasWidthLenght, (int)(sourceBitmap.Height / ratio));
        //    else
        //        bitmapResult = new Bitmap((int)(sourceBitmap.Width / ratio), canvasWidthLenght);

        //    using (Graphics graphicsResult = Graphics.FromImage(bitmapResult)) {
        //        graphicsResult.CompositingQuality = CompositingQuality.HighQuality;
        //        graphicsResult.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphicsResult.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        graphicsResult.DrawImage(sourceBitmap,
        //                                new Rectangle(0, 0,
        //                                    bitmapResult.Width, bitmapResult.Height),
        //                                new Rectangle(0, 0,
        //                                    sourceBitmap.Width, sourceBitmap.Height),
        //                                    GraphicsUnit.Pixel);
        //        graphicsResult.Flush();
        //    }

        //    return bitmapResult;
        //}