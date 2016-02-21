using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters.Filters 
{
    public abstract class ConvolutionFilter : Filter
    {
        public abstract double Factor {
            get;
            set;
        }

        public abstract double Offset {
            get;
            set;
        }

        public abstract double[,] FilterMatrix {
            get;
        } 
    }
}
