using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters {
    interface IImageProcessor 
    {
        System.Windows.Media.Imaging.WriteableBitmap ProcessImage(System.Windows.Media.Imaging.BitmapImage originalBitmapImage);
    }
}
