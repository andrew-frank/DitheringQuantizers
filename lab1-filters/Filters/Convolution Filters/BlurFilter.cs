using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters.Filters.Convolution_Filters {
    sealed class BlurFilter : ConvolutionFilter
    {
        public override string FilterName {
            get { return "Blur"; }
        }

        private double factor = 1.0;
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
            new double[,] { { 0.111 , 0.111 , 0.111 }, 
                            { 0.111 , 0.111 , 0.111 }, 
                            { 0.111 , 0.111 , 0.111 }  };

        public override double[,] FilterMatrix {
            get { return filterMatrix; }
        }
    }
}
