using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters.Filters.Convolution_Filters {

    /**
     * Diagonal Top Left To Bottom Right
     */ 
    sealed public class EdgeDetectionFilter : ConvolutionFilter {
        public override string FilterName {
            get { return "Edge Detection"; }
        }


        private double factor = 1.0;

        public override double Factor {
            get { return factor; }
            set { factor = value; }
        }

        private double bias = 127.0;

        public override double Offset {
            get { return bias; }
            set { bias = value; }
        }


        private double[,] filterMatrix =
            new double[,] { { 0 , -1 , 0 }, 

                                    { 0 , 1 , 0 }, 

                                    { 0 , 0 , 0 } };


        public override double[,] FilterMatrix {
            get { return filterMatrix; }
        }
    } 
}
