using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;

namespace FracVisualisationSoftware.Models
{
    public abstract class StageModelBase
    {
        public abstract Point3D Position { get; set; }

        public abstract double Value { get; set; }
    }
}
