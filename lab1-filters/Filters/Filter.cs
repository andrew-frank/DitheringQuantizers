using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_filters.Filters 
{
    public abstract class Filter 
    {
        public abstract string FilterName {
            get;
        }
    }
}