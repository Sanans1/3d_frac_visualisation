using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FracVisualisationSoftware.Models
{
    public class BoreholeModel
    {
        public string Name { get; set; }
        public Point3DCollection TubePath { get; set; }
    }
}
