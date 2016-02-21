using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace lab1_filters.Filters.Function_Filters 
{
    class GammaFilter : FunctionFilterOffset 
    {
        public override string FilterName {
            get { return "Gamma"; }
        }

        //private double offset = 0;

        //public double Offset {
        //    get { return offset; }
        //    set { offset = value; }
        //}

        public override byte[] Function {
            get {
                byte[] function = new byte[256];

                int num = 0;
                for (int i = 0; i < 256; i++) {

                    double d = (double)(Math.Pow(((double)i) / 255.0, this.Offset));
                    num = (int)(d * 255.0);

                    num = (num < 0 ? 0 : num);
                    num = (num > 255 ? 255 : num);

                    function[i] = (byte)num;
                }

                _function = function;
                return _function;


            }
            set {
                base.Function = value;
            }
        }

        public override WriteableBitmap ApplyFunctionFilter(System.Windows.Media.Imaging.BitmapImage originalBitmapImage) 
        {
            WriteableBitmap writableImage = new WriteableBitmap(originalBitmapImage);
            byte[] byteArr = writableImage.WriteableBitMapImageToArray();

            int num = 0;
            for (int i = 0; i < byteArr.Length; i++) {

                double d = (double)(Math.Pow((double)byteArr[i]/255.0, this.Offset));
                num = (int) (d*255.0);

                num = (num < 0 ? 0 : num);
                num = (num > 255 ? 255 : num);

                byteArr[i] = (byte)num;
            }

            WriteableBitmap result = originalBitmapImage.ByteArrayToWritableBitmap(byteArr);
            return result;
        }

    }
}
