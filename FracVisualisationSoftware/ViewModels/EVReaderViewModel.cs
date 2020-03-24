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
    public class EVReaderViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields

        private string _nameText;

        private string _evFileName;

        private List<string> _content;

        private readonly object _headingCollectionLock;
        private ObservableCollection<string> _headings;
        private int _selectedFilterColumnIndex;
        private int _selectedXHeadingIndex;
        private int _selectedYHeadingIndex;
        private int _selectedZHeadingIndex;

        private string _filterText;

        #endregion fields

        #region properties

        public string NameText
        {
            get => _nameText;
            set { _nameText = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<string> Headings
        {
            get => _headings;
            set
            {
                _headings = value;
                BindingOperations.EnableCollectionSynchronization(_headings, _headingCollectionLock);
                RaisePropertyChanged();
            }
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

        public int SelectedFilterColumnIndex
        {
            get => _selectedFilterColumnIndex;
            set { _selectedFilterColumnIndex = value; RaisePropertyChanged(); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public EVReaderViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            _headingCollectionLock = new object();
            Headings = new ObservableCollection<string>();

            _content = new List<string>();

            ReadEVFileCommand = new RelayCommand(ReadEVFileAction, CanReadEVFileAction);

            MessengerInstance.Register<string>(this, FlyoutToggleEnum.EVBorehole, SelectEVFileAction);
        }

        #endregion constructor

        #region commands

        public ICommand ReadEVFileCommand { get; }

        #endregion commands 

        #region methods

        private void ResetProperties()
        {
            NameText = null;
            Headings = new ObservableCollection<string>();
        }

        #region command methods

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        private async void SelectEVFileAction(string filePath)
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                progressDialogController.SetProgress(33);
                progressDialogController.SetMessage("Opening file...");

                progressDialogController.SetProgress(66);
                progressDialogController.SetMessage("Reading Fields...");

                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line != null)
                        {
                            if (line.StartsWith("# Field:"))
                            {
                                Headings.Add(line.Replace("# Field:", ""));
                            }
                            else if (!line.StartsWith("#"))
                            {
                                _content.Add(line);
                            }
                        }
                    }
                }
            });

            await progressDialogController.CloseAsync();
        }

        private bool CanReadEVFileAction()
        {
            try
            {
                if (Headings[SelectedXHeadingIndex] != null && Headings[SelectedXHeadingIndex] != null && Headings[SelectedXHeadingIndex] != null)
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

        private async void ReadEVFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                List<Point3D> tubePath = new List<Point3D>();

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

                    if (splitLine[_selectedFilterColumnIndex].Contains(_filterText))
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

                        tubePath.Add(new Point3D(x, y, z));
                    }

                    currentRow++;

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow - 1);
                }

                BoreholeModel boreholeModel = new BoreholeModel
                {
                    ID = 0,
                    Name = NameText,
                    TubePath = tubePath
                };

                MessengerInstance.Send(boreholeModel, "Borehole Data Added");
            });

            await progressDialogController.CloseAsync();

            ResetProperties();
        }

        #endregion command methods

        #endregion methods
    }
}
