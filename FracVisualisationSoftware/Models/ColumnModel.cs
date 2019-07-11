using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Annotations;
using SharpDX;

namespace FracVisualisationSoftware.Models
{
    public class ColumnModel : StageModelBase, INotifyPropertyChanged
    {
        private double _value;

        private double _highestValueOfAllStages;

        private Point3DCollection _columnPath;

        public ColumnModel()
        {
            ColumnPath = new Point3DCollection();
        }

        public override Point3D Position { get; set; }

        public override double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    ColumnPathCalculator();
                }
            }
        }

        public double HighestValueOfAllStages
        {
            get => _highestValueOfAllStages;
            set
            {
                if (_highestValueOfAllStages == value)
                {
                    _highestValueOfAllStages = value;
                    ColumnPathCalculator();
                }
            }
        }


        public Point3DCollection ColumnPath
        {
            get => _columnPath;
            set { _columnPath = value; OnPropertyChanged(); }
        }

        private void ColumnPathCalculator()
        {
            //TODO
        }

        private double InverseLerp(double x, double a, double b)
        {
            return (x - a) / (b - a);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
