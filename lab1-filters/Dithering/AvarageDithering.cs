using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using lab1_filters.Filters;

namespace lab1_filters.Dithering {

    public class AvarageDithering : Filter, IImageProcessor
    {
        public override string FilterName {
            get { return "Average Dithering"; }
        }

        private int _levels = 4;

        public int Levels {
            get {  return _levels; }
            set { _levels = value; }
        }

        private double _threshold = 0.5;
        public double Thresholds {
            get { return _threshold; }
            set { _threshold = value; }
        }

        public WriteableBitmap ProcessImage(BitmapImage originalBitmapImage) {
            WriteableBitmap writableImage = new WriteableBitmap(originalBitmapImage);

            byte[] byteArr = writableImage.WriteableBitMapImageToArray();

            for (int i = 0; i < byteArr.Length; i += 4) {
                byte a = byteArr[i + 3];

                if (a > 0) {
                    double ad = (double)a / 255.0;
                    double rd = (double)byteArr[i + 2] / ad;
                    double gd = (double)byteArr[i + 1] / ad;
                    double bd = (double)byteArr[i + 0] / ad;

                    double luminance = 0.2126 * rd + 0.7152 * gd + 0.0722 * bd;
                    double newR = luminance * ad;

                    int interval = 256/this.Levels;
                    int low=0, high=interval;
                    for (int j = 0; j < this.Levels; j++ ) {

                        if (newR > low && newR < high) {
                            if (newR - low > high - newR)
                                newR = high;
                            else
                                newR = low;
                        }

                        low += interval;
                        high += interval;
                    }

                    if (newR > 255) newR = 255;
                    byteArr[i + 0] = (byte)newR;
                    byteArr[i + 1] = (byte)newR;
                    byteArr[i + 2] = (byte)newR;
                }
            }

            WriteableBitmap result = originalBitmapImage.ByteArrayToWritableBitmap(byteArr);
            return result;
        }
    }
}
