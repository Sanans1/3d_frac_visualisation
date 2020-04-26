using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;
using LASFilterModel = FracVisualisationSoftware.Models.Readers.LAS.LASFilterModel;

namespace FracVisualisationSoftware.ViewModels.Readers.LAS
{
    public class LASPathReaderViewModel : LASReaderViewModelBase
    {
        #region fields

        private string _nameText;

        private int _selectedXHeadingIndex;
        private int _selectedYHeadingIndex;
        private int _selectedZHeadingIndex;

        #endregion fields

        #region properties

        public string NameText
        {
            get => _nameText;
            set { _nameText = value; RaisePropertyChanged(); }
        }

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

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LASPathReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "LASPathDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.LAS, WellDataTypeEnum.Path), LoadLASFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            NameText = null;
        }

        #region command methods

        protected override bool CanReadLASFileAction()
        {
            try
            {
                if (Headings.Any() && Headings[SelectedXHeadingIndex] != null && Headings[SelectedYHeadingIndex] != null && Headings[SelectedZHeadingIndex] != null && !string.IsNullOrWhiteSpace(NameText))
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
                List<Point3D> tubePath = new List<Point3D>();

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
                        double x = double.Parse(splitLine[SelectedXHeadingIndex]);
                        double y = double.Parse(splitLine[SelectedYHeadingIndex]);
                        double z = double.Parse(splitLine[SelectedZHeadingIndex]);

                        tubePath.Add(new Point3D(x, y, z));
                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow);
                }

                WellModel wellModel = new WellModel
                {
                    ID = 0,
                    Name = NameText,
                    Path = new ObservableCollection<Point3D>(tubePath)
                };

                MessengerInstance.Send(wellModel, MessageTokenStrings.AddPathToManager);
            });

            await progressDialogController.CloseAsync();

            ResetProperties();
        }

        #endregion command methods

        #endregion methods

    }
}
