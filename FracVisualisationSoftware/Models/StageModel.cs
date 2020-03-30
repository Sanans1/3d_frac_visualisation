using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FracVisualisationSoftware.Models
{
    public class StageModel
    {
        public int StageNumber { get; set; }
        public Point3D Position { get; set; }
        public Dictionary<string, double> Values { get; set; }
    }
}
