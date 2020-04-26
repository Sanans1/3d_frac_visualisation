using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Extensions;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Models.Readers.Excel;
using MahApps.Metro.Controls.Dialogs;
using OfficeOpenXml;

namespace FracVisualisationSoftware.ViewModels.Readers.Excel
{
    public class ExcelStageReaderViewModel : ExcelReaderViewModelBase
    {
        #region fields

        private int _wellID;

        private string _stageColumnHeading;
        private bool? _stageColumnFound;

        private string _xColumnHeading; //Easting
        private bool? _xColumnFound;

        private string _yColumnHeading; //TVD
        private bool? _yColumnFound;

        private string _zColumnHeading; //Northing
        private bool? _zColumnFound;

        #endregion fields

        #region properties

        public string StageColumnHeading
        {
            get { return _stageColumnHeading; }
            set
            {
                _stageColumnHeading = value;
                StageColumnFound = HeadingValidation(value);
                RaisePropertyChanged();
            }
        }

        public bool? StageColumnFound
        {
            get { return _stageColumnFound; }
            set { _stageColumnFound = value; RaisePropertyChanged(); }
        }

        public string XColumnHeading
        {
            get { return _xColumnHeading; }
            set
            {
                _xColumnHeading = value.ToUpper();
                XColumnFound = HeadingValidation(_xColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? XColumnFound
        {
            get { return _xColumnFound; }
            set { _xColumnFound = value; RaisePropertyChanged(() => XColumnFound); }
        }

        public string YColumnHeading
        {
            get { return _yColumnHeading; }
            set
            {
                _yColumnHeading = value.ToUpper();
                YColumnFound = HeadingValidation(_yColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? YColumnFound
        {
            get { return _yColumnFound; }
            set { _yColumnFound = value; RaisePropertyChanged(() => YColumnFound); }
        }

        public string ZColumnHeading
        {
            get { return _zColumnHeading; }
            set
            {
                _zColumnHeading = value.ToUpper();
                ZColumnFound = HeadingValidation(_zColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? ZColumnFound
        {
            get { return _zColumnFound; }
            set { _zColumnFound = value; RaisePropertyChanged(() => ZColumnFound); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ExcelStageReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "ExcelStageDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.Excel, WellDataTypeEnum.Stages), LoadExcelFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            StageColumnHeading = null;
            XColumnHeading = null;
            YColumnHeading = null;
            ZColumnHeading = null;
        }

        protected override void LoadExcelFileCallback(FlyoutMessageModel flyoutMessage)
        {
            base.LoadExcelFileCallback(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        #region command methods

        protected override bool CanReadExcelFileAction()
        {
            if (XColumnFound != null && YColumnFound != null && ZColumnFound != null && StageColumnFound != null)
            {
                return (XColumnFound.Value && YColumnFound.Value && ZColumnFound.Value && StageColumnFound.Value);
            }

            return false;
        }

        protected override async void ReadExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                IEnumerable<ExcelRangeBase> xColumn = _excelUsedRange.Single(cell => cell.Address == XColumnHeading);

                progressDialogController.SetProgress(25);

                IEnumerable<ExcelRangeBase> yColumn = _excelUsedRange.Single(cell => cell.Address == YColumnHeading);

                progressDialogController.SetProgress(50);

                IEnumerable<ExcelRangeBase> zColumn = _excelUsedRange.Single(cell => cell.Address == ZColumnHeading);

                progressDialogController.SetProgress(75);

                IEnumerable<ExcelRangeBase> stageColumn = _excelUsedRange.Single(cell => cell.Address == StageColumnHeading);

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                List<StageModel> stageModels = new List<StageModel>();

                int numberOfRows = _excelWorksheet.Dimension.Rows;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = xColumn.First().Start.Row; currentRow < numberOfRows; currentRow++)
                {
                    object cellValue = _excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value;

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
                        double x = (double)_excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value;
                        double y = (double)_excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value;
                        double z = (double)_excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value;

                        stageModels.Add(new StageModel
                        {
                            StageNumber = (int)_excelWorksheet.Cells[currentRow, stageColumn.First().Start.Column].Value,
                            Position = new Point3D(x, y, z)
                        });

                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow);
                }

                MessengerInstance.Send((stageModels, _wellID), MessageTokenStrings.AddStagesToManager);
            });

            await progressDialogController.CloseAsync();

            ResetProperties();
        }

        #endregion command methods

        #endregion methods

    }
}
