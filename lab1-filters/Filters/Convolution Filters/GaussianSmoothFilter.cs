using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters.Filters.Convolution_Filters {
    sealed class GaussianSmoothFilter : ConvolutionFilter
    {
        public override string FilterName {
            get { return "Gaussian Smoothing"; }
        }

        private double factor = 1.0 / 16.0;

        public override double Factor {
            get { return factor; }
            set { factor = value; }
        }

        private double bias = 0.0;

        public override double Offset {
            get { return bias; }
            set { bias = value; }
        }



        private double[,] filterMatrix =
            new double[,] { { 1, 2, 1, },  
                            { 2, 4, 2, },  
                            { 1, 2, 1, } };


        public override double[,] FilterMatrix {
            get { return filterMatrix; }
        }
    }
}
