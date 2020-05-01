namespace ReservoirVisualisationProject.Models.Readers.Excel
{
    public class ExcelFilterModel : ModelBase
    {
        private string _headingCellAddress;

        public string HeadingCellAddress
        {
            get => _headingCellAddress;
            set
            {
                _headingCellAddress = value.ToUpper(); 
                RaisePropertyChanged();
            }
        }

        public string FilterText { get; set; }
    }
}
