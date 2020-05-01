using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ReservoirVisualisationProject.Enums;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.LAS;
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;

namespace ReservoirVisualisationProject.ViewModels.Readers.LAS
{
    public class LASStageReaderViewModel : LASReaderViewModelBase
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

        public LASStageReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "LASStageDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.LAS, WellDataTypeEnum.Stages), LoadLASFileCallback);
        }

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
                if (Headings.Any() && Headings[SelectedXHeadingIndex] != null && Headings[SelectedYHeadingIndex] != null && Headings[SelectedZHeadingIndex] != null && Headings[SelectedStageHeadingIndex] != null)
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

        protected override async void ReadLASFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                List<StageModel> stageModels = new List<StageModel>();

                int numberOfRows = Sections[SelectedDataSectionIndex].Content.Count();

                progressDialogController.SetMessage("Beginning to read data...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = 0; currentRow < numberOfRows; currentRow++)
                {
                    string[] splitLine = Sections[SelectedDataSectionIndex].Content[currentRow].SplitOnWhitespace();

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
        }

        #endregion command methods

        #endregion methods

    }
}
