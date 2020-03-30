using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Annotations;

namespace FracVisualisationSoftware.Models
{
    public class WellModel : INotifyPropertyChanged
    {
        private ObservableCollection<StageModel> _stages;
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Point3D> Path { get; set; }

        public ObservableCollection<StageModel> Stages
        {
            get => _stages;
            set { _stages = value; RaisePropertyChanged();}
        }

        public List<string> ValueTypes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
