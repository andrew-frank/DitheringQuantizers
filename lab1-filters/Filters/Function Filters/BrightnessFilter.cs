using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace lab1_filters.Filters.Function_Filters {
    sealed class BrightnessFilter : FunctionFilterOffset
    {
        public override string FilterName {
            get { return "Brightness"; }
        }

        public override byte[] Function {
            get {
                byte[] function = new byte[256];

                int num = 0;
                for (int i = 0; i < 256; i++) {

                    num = (int)(i + this.Offset);

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


        //public override WriteableBitmap ApplyFunctionFilter(System.Windows.Media.Imaging.BitmapImage originalBitmapImage) 
        //{
        //    WriteableBitmap writableImage = new System.Windows.Media.Imaging.WriteableBitmap(originalBitmapImage);
        //    byte[] byteArr = writableImage.WriteableBitMapImageToArray();

        //    int num = 0;
        //    for (int i = 0; i < byteArr.Length; i++) {
        //        num = (int)(byteArr[i]);
        //        int val = (int)_function[num] + (int)this.Offset;
        //        val = (val < 0 ? 0 : val);
        //        val = (val > 255 ? 255 : val);
        //        byteArr[i] = (byte)(val);
        //    }

        //    WriteableBitmap result = originalBitmapImage.ByteArrayToWritableBitmap(byteArr);
        //    return result;

        //    //WriteableBitmap writableImage = new WriteableBitmap(originalBitmapImage);
        //    //byte[] byteArr = writableImage.WriteableBitMapImageToArray();

        //    //int num = 0;
        //    //for (int i = 0; i < byteArr.Length; i++) {
        //    //    num = (int)(byteArr[i] + this.Offset);

        //    //    num = (num < 0 ? 0 : num);
        //    //    num = (num > 255 ? 255 : num);

        //    //    byteArr[i] = (byte)num;
        //    //}

        //    //WriteableBitmap result = originalBitmapImage.ByteArrayToWritableBitmap(byteArr);
        //    //return result;
        //}
    }
}
