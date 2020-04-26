using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Extensions;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Models.Readers.Excel;
using MahApps.Metro.Controls.Dialogs;
using OfficeOpenXml;

namespace FracVisualisationSoftware.ViewModels.Readers.Excel
{
    public class ExcelDataReaderViewModel : ExcelReaderViewModelBase
    {
        #region fields

        private int _wellID;

        private string _dataName;

        private string _dataUnitOfMeasurement;

        private string _dataColumnHeading;
        private bool? _dataColumnFound;        
        
        private string _stageColumnHeading;
        private bool? _stageColumnFound;

        private string _timestampColumnHeading;
        private bool? _timestampColumnFound;

        #endregion fields

        #region properties

        public string DataName
        {
            get { return _dataName; }
            set { _dataName = value; RaisePropertyChanged();}
        }

        public string DataUnitOfMeasurement
        {
            get { return _dataUnitOfMeasurement; }
            set { _dataUnitOfMeasurement = value; RaisePropertyChanged(); }
        }

        public string DataColumnHeading
        {
            get { return _dataColumnHeading; }
            set
            {
                _dataColumnHeading = value.ToUpper();
                DataColumnFound = HeadingValidation(_dataColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? DataColumnFound
        {
            get { return _dataColumnFound; }
            set { _dataColumnFound = value; RaisePropertyChanged(); }
        }       
        
        public string StageColumnHeading
        {
            get { return _stageColumnHeading; }
            set
            {
                _stageColumnHeading = value.ToUpper();
                StageColumnFound = HeadingValidation(_dataColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? StageColumnFound
        {
            get { return _stageColumnFound; }
            set { _stageColumnFound = value; RaisePropertyChanged(); }
        }       
        
        public string TimestampColumnHeading
        {
            get { return _timestampColumnHeading; }
            set
            {
                _timestampColumnHeading = value.ToUpper();
                TimestampColumnFound = HeadingValidation(_timestampColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? TimestampColumnFound
        {
            get { return _timestampColumnFound; }
            set { _timestampColumnFound = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ExcelDataReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "ExcelDataDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.Excel, WellDataTypeEnum.Data), LoadExcelFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            DataName = null;
            DataUnitOfMeasurement = null;
            DataColumnHeading = null;
        }

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        protected override void LoadExcelFileCallback(FlyoutMessageModel flyoutMessage)
        {
            base.LoadExcelFileCallback(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        #region command methods

        protected override bool CanReadExcelFileAction()
        {
            if (DataColumnFound != null && StageColumnFound != null && TimestampColumnFound != null && !string.IsNullOrWhiteSpace(DataUnitOfMeasurement) && !string.IsNullOrWhiteSpace(DataName))
            {
                return DataColumnFound.Value && StageColumnFound.Value && TimestampColumnFound.Value;
            }

            return false;
        }

        protected override async void ReadExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                IEnumerable<ExcelRangeBase> dataColumn = _excelUsedRange.Single(cell => cell.Address == DataColumnHeading);

                IEnumerable<ExcelRangeBase> stageColumn = _excelUsedRange.Single(cell => cell.Address == StageColumnHeading);

                IEnumerable<ExcelRangeBase> timestampColumn = _excelUsedRange.Single(cell => cell.Address == TimestampColumnHeading);

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                List<DataValueModel> values = new List<DataValueModel>();

                int numberOfRows = _excelWorksheet.Dimension.Rows;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = dataColumn.First().Start.Row; currentRow < numberOfRows; currentRow++)
                {
                    object cellValue = _excelWorksheet.Cells[currentRow, dataColumn.First().Start.Column].Value;

                    bool shouldUse = true;

                    if (ExcelFilterModels.Any())
                    {
                        foreach (ExcelFilterModel excelFilterModel in ExcelFilterModels)
                        {
                            IEnumerable<ExcelRangeBase> filterColumn = _excelUsedRange.SingleOrDefault(cell => cell.Address == excelFilterModel.HeadingCellAddress);

                            shouldUse = (_excelWorksheet.Cells[currentRow, filterColumn.First().Start.Column].Value).ToString().Contains(excelFilterModel.FilterText);

                            if (shouldUse == false) break;
                        }
                    }

                    if (cellValue.IsNumeric() && shouldUse)
                    {
                        values.Add(new DataValueModel
                        {
                            Stage = int.Parse(((string)_excelWorksheet.Cells[currentRow, stageColumn.First().Start.Column].Value).Split('_').Last()),
                            Value = (double)_excelWorksheet.Cells[currentRow, dataColumn.First().Start.Column].Value,
                            Timestamp = (DateTime)_excelWorksheet.Cells[currentRow, timestampColumn.First().Start.Column].Value
                        });
                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow);
                }

                DataSetModel dataSetModel = new DataSetModel
                {
                    DataName = DataName,
                    DataUnitOfMeasurement = DataUnitOfMeasurement,
                    Values = values
                };

                MessengerInstance.Send((dataSetModel, _wellID), MessageTokenStrings.AddDataToManager);
            });

            await progressDialogController.CloseAsync();

            ResetProperties();
        }

        #endregion command methods

        #endregion methods

    }
}
