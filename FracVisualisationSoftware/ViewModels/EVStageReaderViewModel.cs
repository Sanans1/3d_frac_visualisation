using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;

namespace FracVisualisationSoftware.ViewModels
{
    public class EVStageReaderViewModel : EVPathReaderViewModel
    {
        #region fields

        private int _wellID;

        private int _selectedStageHeadingIndex;

        #endregion fields

        #region properties

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

        }

        #endregion constructo

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.EV, WellDataTypeEnum.Stages), SelectEVFileAction);
        }

        #region command methods

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        protected async void SelectEVFileAction(FlyoutMessageModel flyoutMessage)
        {
            base.SelectEVFileAction(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        protected override async void ReadEVFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                List<StageModel> stageModels = new List<StageModel>();

                double initialX = 0;
                double initialY = 0;
                double initialZ = 0;

                bool initalValuesSet = false;

                int currentRow = 0;

                int numberOfRows = _content.Count();

                bool allValuesParsed = false;

                progressDialogController.SetMessage("Beginning to read data...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                foreach (string line in _content)
                {
                    string[] splitLine = line.Split("\t".ToCharArray());

                    if (splitLine[SelectedFilterColumnIndex].Contains(FilterText))
                    {
                        if (!initalValuesSet)
                        {
                            initialX = double.Parse(splitLine[SelectedXHeadingIndex]) * -1;
                            initialY = double.Parse(splitLine[SelectedYHeadingIndex]) * -1;
                            initialZ = double.Parse(splitLine[SelectedZHeadingIndex]) * -1;

                            initalValuesSet = true;
                        }

                        double x = double.Parse(splitLine[SelectedXHeadingIndex]) * -1;
                        double y = double.Parse(splitLine[SelectedYHeadingIndex]) * -1;
                        double z = double.Parse(splitLine[SelectedZHeadingIndex]) * -1;

                        x -= initialX;
                        y -= initialY;
                        z -= initialZ;

                        stageModels.Add(new StageModel
                        {
                            StageNumber = int.Parse(splitLine[SelectedStageHeadingIndex]),
                            Position = new Point3D(x, y, z)
                        });
                    }

                    currentRow++;

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
