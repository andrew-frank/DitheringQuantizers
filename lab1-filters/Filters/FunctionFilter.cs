using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
namespace lab1_filters.Filters 
{
    //interface IFunctionFilterOffset 
    //{
    //    double Offset {
    //        get;
    //        set;
    //    }
    //}

    public abstract class FunctionFilterOffset : FunctionFilter 
    {
        private double offset = 0;

        public double Offset {
            get { return offset; }
            set { offset = value; }
        }

        public override WriteableBitmap ApplyFunctionFilter(System.Windows.Media.Imaging.BitmapImage originalBitmapImage) {
            WriteableBitmap writableImage = new System.Windows.Media.Imaging.WriteableBitmap(originalBitmapImage);
            byte[] byteArr = writableImage.WriteableBitMapImageToArray();

            int num = 0;
            byte[] function = this.Function;
            for (int i = 0; i < byteArr.Length; i++) {
                num = (int)(byteArr[i]);
                int val = (int)function[num] + (int)this.Offset;
                val = (val < 0 ? 0 : val);
                val = (val > 255 ? 255 : val);
                byteArr[i] = (byte)(val);
            }

            WriteableBitmap result = originalBitmapImage.ByteArrayToWritableBitmap(byteArr);
            return result;
        }
    }

    public abstract class FunctionFilter : Filter
    {
        public abstract WriteableBitmap ApplyFunctionFilter(BitmapImage originalBitmapImage);

        protected byte[] _function;
        public virtual byte[] Function {
            get { return _function; }
            set {
                if (value.Length == 256)
                    _function = value;
                else
                    System.Diagnostics.Debug.WriteLine("wtf");
            }
        }
    }
}