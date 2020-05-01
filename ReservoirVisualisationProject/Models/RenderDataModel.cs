using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ReservoirVisualisationProject.Models
{
    public class RenderDataModel
    {
        public int WellID{ get; set; }
        public int Stage { get; set; }
        public string Name => $"{WellID}-{Stage}-";

        public string UnitOfMeasurement { get; set; }
        public double Value { get; set; }
        public string DisplayValue { get => $"{Value:N2} {UnitOfMeasurement}"; }
        public Point3D Position { get; set; }
    }
}
