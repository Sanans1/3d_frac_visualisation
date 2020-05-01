using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ReservoirVisualisationProject.Models
{
    public class WellModel : ModelBase
    {
        private ObservableCollection<Point3D> _path;
        private ObservableCollection<StageModel> _stages;
        private ObservableCollection<DataSetModel> _dataSets = new ObservableCollection<DataSetModel>();
        private int _selectedDataSetIndex;

        public int ID { get; set; }
        public string Name { get; set; }


        public ObservableCollection<Point3D> Path
        {
            get => _path;
            set { _path = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<StageModel> Stages
        {
            get => _stages;
            set
            {
                _stages = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasStages));
            }
        }

        public bool HasStages => Stages.Any();

        public ObservableCollection<DataSetModel> DataSets
        {
            get => _dataSets;
            set { _dataSets = value; RaisePropertyChanged(); }
        }

        public int SelectedDataSetIndex
        {
            get => _selectedDataSetIndex;
            set
            {
                _selectedDataSetIndex = value; 
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DataSetIsSelected));
            }
        }

        public bool DataSetIsSelected => SelectedDataSetIndex > -1 && SelectedDataSetIndex < DataSets.Count;
    }
}
