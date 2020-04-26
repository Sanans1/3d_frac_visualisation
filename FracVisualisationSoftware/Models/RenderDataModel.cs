using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FracVisualisationSoftware.Models
{
    public class RenderDataModel
    {
        public string Name { get; set; }
        public string UnitOfMeasurement { get; set; }
        public double Value { get; set; }
        public string DisplayValue { get => $"{Value} {UnitOfMeasurement}"; }
        public Point3D Position { get; set; }
    }
}
