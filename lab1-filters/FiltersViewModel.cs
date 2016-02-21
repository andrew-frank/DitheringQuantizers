using lab1_filters.Filters;
using lab1_filters.Filters.Convolution_Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lab1_filters.Filters.Function_Filters;
using lab1_filters.Dithering;
using lab1_filters.Quantizers;

namespace lab1_filters {
    class FiltersViewModel 
    {
        public FiltersViewModel() {
            IList = new List<Filter>();
            IList.Add(new AvarageDithering());
            IList.Add(new OctreeQuantizer(8,8));
            //IList.Add(new IdentityFilter());
            //IList.Add(new BlurFilter());
            //IList.Add(new EdgeDetectionFilter());
            //IList.Add(new EmbossFilter());
            //IList.Add(new GaussianSmoothFilter());
            //IList.Add(new SharpenFilter());
            //IList.Add(new BrightnessFilter());
            //IList.Add(new ContrastFilter());
            //IList.Add(new NegationFilter());
            //IList.Add(new GammaFilter());
            //IList.Add(new ThresholdFilter());
        }

        public List<Filter> IList { get; set; }
    }
}
