using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracVisualisationSoftware.Models
{
    public class DataSetModel : ModelBase
    {
        private List<DataValueModel> _values;

        public string DataName { get; set; }
        public string DataUnitOfMeasurement { get; set; }

        public List<DataValueModel> Values
        {
            get => _values;
            set { _values = value; RaisePropertyChanged();}
        }
    }
}
