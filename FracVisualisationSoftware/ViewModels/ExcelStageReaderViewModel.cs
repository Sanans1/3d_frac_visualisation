using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Extensions;
using FracVisualisationSoftware.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;

namespace FracVisualisationSoftware.ViewModels
{
    public class ExcelStageReaderViewModel : ExcelPathReaderViewModel
    {
        #region fields

        private int _wellID;

        private string _stageColumnHeading;
        private bool? _stageColumnFound;

        #endregion fields

        #region properties

        public string StageColumnHeading
        {
            get { return _stageColumnHeading; }
            set
            {
                _stageColumnHeading = value;
                ZColumnFound = HeadingValidation(value);
                RaisePropertyChanged();
            }
        }

        public bool? StageColumnFound
        {
            get { return _stageColumnFound; }
            set { _stageColumnFound = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ExcelStageReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {

        }

        #endregion constructor

        #region methods

        protected override void ResetProperties()
        {
            base.ResetProperties();
            StageColumnHeading = null;
        }

        protected override void SelectExcelFileAction(FlyoutMessageModel flyoutMessage)
        {
            base.SelectExcelFileAction(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        #region command methods

        protected override async void ReadExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                IEnumerable<ExcelRangeBase> xColumn = _excelUsedRange.Single(cell => cell.Address == XColumnHeading);

                progressDialogController.SetProgress(33);

                IEnumerable<ExcelRangeBase> yColumn = _excelUsedRange.Single(cell => cell.Address == YColumnHeading);

                progressDialogController.SetProgress(66);

                IEnumerable<ExcelRangeBase> zColumn = _excelUsedRange.Single(cell => cell.Address == ZColumnHeading);

                IEnumerable<ExcelRangeBase> filterColumn = _excelUsedRange.SingleOrDefault(cell => cell.Address == FilterColumnHeading);

                IEnumerable<ExcelRangeBase> stageColumn = _excelUsedRange.Single(cell => cell.Address == StageColumnHeading);

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                List<StageModel> stageModels = new List<StageModel>();

                double initialX = 0;
                double initialY = 0;
                double initialZ = 0;

                bool initalValuesSet = false;

                int currentRow = xColumn.First().Start.Row;

                int numberOfRows = _excelWorksheet.Dimension.Rows;

                bool allValuesParsed = false;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                while (!allValuesParsed)
                {
                    object cellValue = _excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value;
                    bool shouldUse = string.IsNullOrWhiteSpace(FilterText) || ((string) _excelWorksheet.Cells[currentRow, filterColumn.First().Start.Column].Value).Contains(FilterText);

                    if (cellValue.IsNumeric() && shouldUse)
                    {
                        if (!initalValuesSet)
                        {
                            initialX = (double)_excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value * -1;
                            initialY = (double)_excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value * -1;
                            initialZ = (double)_excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value * -1;

                            initalValuesSet = true;
                        }
                        
                        double x = (double)_excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value * -1;
                        double y = (double)_excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value * -1;
                        double z = (double)_excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value * -1;
                        
                        x -= initialX;
                        y -= initialY;
                        z -= initialZ;

                        stageModels.Add(new StageModel
                        {
                            StageNumber = (int)_excelWorksheet.Cells[currentRow, stageColumn.First().Start.Column].Value,
                            Position = new Point3D(x, y, z)
                        });

                        currentRow++;
                    }
                    else
                    {
                        if (initalValuesSet)
                        {
                            allValuesParsed = true;
                        }
                        else
                        {
                            currentRow++;
                        }
                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow - 1);
                }

                MessengerInstance.Send((stageModels, _wellID), "Well Stages Added");
            });

            await progressDialogController.CloseAsync();

            ResetProperties();
        }

        #endregion command methods

        #endregion methods

    }
}
