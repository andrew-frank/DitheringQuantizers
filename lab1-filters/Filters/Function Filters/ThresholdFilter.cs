using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace lab1_filters.Filters.Function_Filters {
    class ThresholdFilter : FunctionFilterOffset
    {
        public override string FilterName {
            get { return "Threshold"; }
        }

        /*
         public override byte[] Function {
            get {
                byte[] function = new byte[256];

                int num = 0;

                int subvalues = 4;
                int length = 256 / subvalues;
                for (int i = 0; i * subvalues < length; i += subvalues) {

                    int grayScalePx = (int)((i * .299) + ((i + 1) * .587) + ((i + 2) * .114));
         
                    if (grayScalePx < this.Offset)
                        num = 0;
                    else
                        num = 255;

                    for (int j = i; j < (i + subvalues); j++) {
                        function[j] = (byte)num;
                    }
                }

                _function = function;
                return _function;
            }
            set {
                base.Function = value;
            }
        }
         */

        public override WriteableBitmap ApplyFunctionFilter(System.Windows.Media.Imaging.BitmapImage originalBitmapImage) {
            WriteableBitmap writableImage = new System.Windows.Media.Imaging.WriteableBitmap(originalBitmapImage);

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

                    if (newR < this.Offset) newR = 0;
                    else newR = 255;

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
