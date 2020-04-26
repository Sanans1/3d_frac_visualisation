using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Models.Readers.LAS;
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;

namespace FracVisualisationSoftware.ViewModels.Readers.LAS
{
    public class LASDataReaderViewModel : LASReaderViewModelBase
    {
        #region fields

        private int _wellID;

        private string _dataName;

        private string _dataUnitOfMeasurement;

        private int _selectedDataHeadingIndex;

        private int _selectedStageHeadingIndex;

        private int _selectedTimestampHeadingIndex;

        #endregion fields

        #region properties

        public string DataName
        {
            get => _dataName;
            set { _dataName = value; RaisePropertyChanged(); }
        }        
        
        public string DataUnitOfMeasurement
        {
            get => _dataUnitOfMeasurement;
            set { _dataUnitOfMeasurement = value; RaisePropertyChanged(); }
        }

        public int SelectedDataHeadingIndex
        {
            get { return _selectedDataHeadingIndex; }
            set { _selectedDataHeadingIndex = value; RaisePropertyChanged(); }
        }

        public int SelectedStageHeadingIndex
        {
            get { return _selectedStageHeadingIndex; }
            set { _selectedStageHeadingIndex = value; RaisePropertyChanged(); }
        }        
        
        public int SelectedTimestampHeadingIndex
        {
            get { return _selectedTimestampHeadingIndex; }
            set { _selectedTimestampHeadingIndex = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LASDataReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "LASDataDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.LAS, WellDataTypeEnum.Data), LoadLASFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            DataName = null;
            DataUnitOfMeasurement = null;
        }

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        protected override void LoadLASFileCallback(FlyoutMessageModel flyoutMessage)
        {
            base.LoadLASFileCallback(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        #region command methods

        protected override bool CanReadLASFileAction()
        {
            try
            {
                if (Headings.Any() && Headings[SelectedStageHeadingIndex] != null && Headings[SelectedDataHeadingIndex] != null && Headings[SelectedTimestampHeadingIndex] != null && !string.IsNullOrWhiteSpace(DataName) && !string.IsNullOrWhiteSpace(DataUnitOfMeasurement))
                {
                    return true;
                }
            }
            catch
            {
                //ignored
            }

            return false;
        }

        protected override async void ReadLASFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                List<DataValueModel> values = new List<DataValueModel>();

                int numberOfRows = Sections[SelectedDataSectionIndex].Content.Count();

                progressDialogController.SetMessage("Beginning to read data...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = 0; currentRow < numberOfRows; currentRow++)
                {
                    string[] splitLine = Sections[SelectedDataSectionIndex].Content[currentRow].SplitOnWhitespace(); //TODO might need to change how this splits

                    bool shouldUse = true;

                    if (LASFilterModels.Any())
                    {
                        foreach (LASFilterModel lasFilterModel in LASFilterModels)
                        {
                            shouldUse = splitLine[lasFilterModel.HeadingIndex].Contains(lasFilterModel.FilterText);

                            if (shouldUse == false) break;
                        }
                    }

                    if (shouldUse)
                    {
                        values.Add(new DataValueModel
                        {
                            Stage = int.Parse(splitLine[SelectedStageHeadingIndex]),
                            Value = double.Parse(splitLine[SelectedDataHeadingIndex]),
                            Timestamp = DateTime.Parse(splitLine[SelectedTimestampHeadingIndex])
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
