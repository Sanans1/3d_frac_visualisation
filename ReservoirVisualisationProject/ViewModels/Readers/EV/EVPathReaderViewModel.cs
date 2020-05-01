using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ReservoirVisualisationProject.Enums;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.EV;
using MahApps.Metro.Controls.Dialogs;

namespace ReservoirVisualisationProject.ViewModels.Readers.EV
{
    public class EVPathReaderViewModel : EVReaderViewModelBase
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

        public EVPathReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "EVPathDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.EV, WellDataTypeEnum.Path), LoadEVFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            NameText = null;
        }

        #region command methods

        protected override bool CanReadEVFileAction()
        {
            try
            {
                if (Headings.Any() && Headings[SelectedXHeadingIndex] != null && Headings[SelectedYHeadingIndex] != null && Headings[SelectedZHeadingIndex] != null && _content.Any() && !string.IsNullOrWhiteSpace(NameText))
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
                List<Point3D> tubePath = new List<Point3D>();

                int numberOfRows = _content.Count();

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
        }

        #endregion command methods

        #endregion methods
    }
}
