using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace lab1_filters 
{

    public sealed class CanvasRenderer 
    {
        private static readonly Lazy<CanvasRenderer> lazy =
            new Lazy<CanvasRenderer>(() => new CanvasRenderer());

        public static CanvasRenderer Instance { get { return lazy.Value; } }

        private CanvasRenderer() {}
    }
}
