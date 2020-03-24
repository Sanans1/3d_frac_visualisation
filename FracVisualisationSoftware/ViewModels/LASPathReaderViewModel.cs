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
    public class LASPathReaderViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields
        
        private string _nameText;
        
        private readonly object _sectionsCollectionLock;
        private ObservableCollection<LASSectionModel> _sections;
        private int _selectedCurveSectionIndex;
        private int _selectedDataSectionIndex;

        private double _nullValue;

        private readonly object _headingCollectionLock;
        private ObservableCollection<LASInformationModel> _headings;
        private int _selectedFilterHeadingIndex;
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

        public ObservableCollection<LASSectionModel> Sections
        {
            get { return _sections; }
            set
            {
                _sections = value;
                BindingOperations.EnableCollectionSynchronization(_sections, _sectionsCollectionLock);
                RaisePropertyChanged();
            }
        }

        public int SelectedCurveSectionIndex
        {
            get { return _selectedCurveSectionIndex; }
            set 
            { 
                _selectedCurveSectionIndex = value;
                SetHeadings();
                RaisePropertyChanged();
            }
        }

        public int SelectedDataSectionIndex
        {
            get { return _selectedDataSectionIndex; }
            set
            {
                _selectedDataSectionIndex = value; 
                RaisePropertyChanged();
            }
        }

        public double NullValue
        {
            get { return _nullValue; }
            set { _nullValue = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<LASInformationModel> Headings
        {
            get { return _headings; }
            set
            {
                _headings = value;
                BindingOperations.EnableCollectionSynchronization(_headings, _headingCollectionLock);
                RaisePropertyChanged();
            }
        }

        public int SelectedFilterHeadingIndex
        {
            get { return _selectedFilterHeadingIndex; }
            set { _selectedFilterHeadingIndex = value; RaisePropertyChanged(); }
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
        public LASPathReaderViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            _sectionsCollectionLock = new object();
            Sections = new ObservableCollection<LASSectionModel>();

            _headingCollectionLock = new object();
            Headings = new ObservableCollection<LASInformationModel>();

            ReadLASFileCommand = new RelayCommand(ReadLASFileAction, CanReadLASFileAction);

            MessengerInstance.Register<string>(this, FlyoutToggleEnum.LASBorehole, SelectLASFileAction);
        }

        #endregion constructor

        #region commands

        public ICommand ReadLASFileCommand { get; }

        #endregion commands 

        #region methods

        private void ResetProperties()
        {
            NameText = null;
            Sections = new ObservableCollection<LASSectionModel>();
            Headings = new ObservableCollection<LASInformationModel>();
        }

        private void SetHeadings()
        {
            try
            {
                Headings.Clear();

                LASSectionModel lasSectionModel = Sections[SelectedCurveSectionIndex];

                lasSectionModel.Content.ForEach(content =>
                {
                    LASInformationModel lasInformationModel = new LASInformationModel();

                    string[] splitContent = content.SplitOnWhitespace();

                    lasInformationModel.Name = splitContent[0];
                    lasInformationModel.MeasurementUnit = splitContent[1];

                    Headings.Add(lasInformationModel);
                });
            }
            catch
            {
                // ignored
            }
        }

        #region command methods

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        private async void SelectLASFileAction(string filePath)
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() => 
            {
                progressDialogController.SetProgress(33);
                progressDialogController.SetMessage("Opening file...");

                progressDialogController.SetProgress(66);
                progressDialogController.SetMessage("Reading Sections...");

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        if (line != null && line.StartsWith("~"))
                        {
                            LASSectionModel newSection = new LASSectionModel
                            {
                                Name = line
                            };

                            newSection.Content = new List<string>();
                            line = reader.ReadLine();

                            while (line != null && !line.StartsWith("~"))
                            {
                                newSection.Content.Add(line);
                                line = reader.ReadLine();
                            }

                            Sections.Add(newSection);
                        }
                    }
                }
            });

            await progressDialogController.CloseAsync();
        }

        private bool CanReadLASFileAction()
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

        private async void ReadLASFileAction()
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
