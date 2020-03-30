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
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using Geometry3D = HelixToolkit.Wpf.SharpDX.Geometry3D;

namespace FracVisualisationSoftware.ViewModels
{
    public class LASStageReaderViewModel : LASPathReaderViewModel
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
        public LASStageReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {

        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.EV, WellDataTypeEnum.Stages), SelectLASFileAction);
        }

        #region command methods

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        protected override async void SelectLASFileAction(FlyoutMessageModel flyoutMessage)
        {
            base.SelectLASFileAction(flyoutMessage);

            if (flyoutMessage.WellID != -1) _wellID = flyoutMessage.WellID;
            else throw new InvalidOperationException();
        }

        protected override async void ReadLASFileAction()
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

                int numberOfRows = Sections[SelectedDataSectionIndex].Content.Count();

                progressDialogController.SetMessage("Beginning to read data...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                foreach (string line in Sections[SelectedDataSectionIndex].Content)
                {
                    string[] splitLine = line.SplitOnWhitespace();

                    if (string.IsNullOrWhiteSpace(_filterText) || splitLine[SelectedFilterHeadingIndex].Contains(_filterText))
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

                        if (x == NullValue) x = 0;
                        else x -= initialX;

                        if (y == NullValue) y = 0;
                        else y -= initialY;

                        if (z == NullValue) z = 0;
                        else z -= initialZ;

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
