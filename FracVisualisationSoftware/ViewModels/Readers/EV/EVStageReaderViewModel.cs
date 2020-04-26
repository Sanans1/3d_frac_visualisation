using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Models.Readers.EV;
using MahApps.Metro.Controls.Dialogs;

namespace FracVisualisationSoftware.ViewModels.Readers.EV
{
    public class EVStageReaderViewModel : EVReaderViewModelBase
    {
        #region fields

        private int _wellID;

        private int _selectedXHeadingIndex;

        private int _selectedYHeadingIndex;

        private int _selectedZHeadingIndex;

        private int _selectedStageHeadingIndex;

        #endregion fields

        #region properties

        public int SelectedXHeadingIndex
        {
            get { return _selectedXHeadingIndex; }
            set { _selectedXHeadingIndex = value; RaisePropertyChanged(); }
        }

        public int SelectedYHeadingIndex
        {
            get { return _selectedYHeadingIndex; }
            set { _selectedYHeadingIndex = value; RaisePropertyChanged(); }
        }

        public int SelectedZHeadingIndex
        {
            get { return _selectedZHeadingIndex; }
            set { _selectedZHeadingIndex = value; RaisePropertyChanged(); }
        }

        public int SelectedStageHeadingIndex
        {
            get { return _selectedStageHeadingIndex; }
            set { _selectedStageHeadingIndex = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public EVStageReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "EVStageDialogHost";
        }

        #endregion constructo

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.EV, WellDataTypeEnum.Stages), LoadEVFileCallback);
        }

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        protected override void LoadEVFileCallback(FlyoutMessageModel flyoutMessage)
        {
            base.LoadEVFileCallback(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        #region command methods

        protected override bool CanReadEVFileAction()
        {
            try
            {
                if (Headings.Any() && (Headings[SelectedXHeadingIndex] != null && Headings[SelectedXHeadingIndex] != null && Headings[SelectedXHeadingIndex] != null && Headings[SelectedStageHeadingIndex] != null && _content.Any()))
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        protected override async void ReadEVFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                int numberOfRows = _content.Count();

                List<StageModel> stageModels = new List<StageModel>();

                progressDialogController.SetMessage("Beginning to read data...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = 0; currentRow < numberOfRows; currentRow++)
                {
                    string[] splitLine = _content[currentRow].Split("\t".ToCharArray());

                    bool shouldUse = true;

                    if (EVFilterModels.Any())
                    {
                        foreach (EVFilterModel evFilterModel in EVFilterModels)
                        {
                            shouldUse = splitLine[evFilterModel.HeadingIndex].Contains(evFilterModel.FilterText);

                            if (shouldUse == false) break;
                        }
                    }

                    if (shouldUse)
                    {
                        double x = double.Parse(splitLine[SelectedXHeadingIndex]);
                        double y = double.Parse(splitLine[SelectedYHeadingIndex]);
                        double z = double.Parse(splitLine[SelectedZHeadingIndex]);

                        stageModels.Add(new StageModel
                        {
                            StageNumber = int.Parse(splitLine[SelectedStageHeadingIndex]),
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
