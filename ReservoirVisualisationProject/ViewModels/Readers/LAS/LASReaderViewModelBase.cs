using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.LAS;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HelixToolkit;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;

namespace ReservoirVisualisationProject.ViewModels.Readers.LAS
{
    public abstract class LASReaderViewModelBase : ViewModelBase
    {
        #region fields

        #region injected fields

        protected IDialogCoordinator _dialogCoordinator;

        #endregion injected fields
        
        private readonly object _sectionsCollectionLock;
        private ObservableCollection<LASSectionModel> _sections;
        private int _selectedCurveSectionIndex;
        private int _selectedDataSectionIndex;

        private double _nullValue;

        private readonly object _headingCollectionLock;
        private ObservableCollection<LASInformationModel> _headings;

        private ObservableCollection<LASFilterModel> _lasFilterModels;
        private LASFilterModel _selectedLASFilterModel;

        #endregion fields

        #region properties

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

        public ObservableCollection<LASFilterModel> LASFilterModels
        {
            get { return _lasFilterModels; }
            set { _lasFilterModels = value; RaisePropertyChanged(); }
        }

        public LASFilterModel SelectedLASFilterModel
        {
            get { return _selectedLASFilterModel; }
            set { _selectedLASFilterModel = value; RaisePropertyChanged(); }
        }

        public string DialogHostIdentifier { get; protected set; }

        #endregion properties

        #region constructor

        public LASReaderViewModelBase(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            _sectionsCollectionLock = new object();
            Sections = new ObservableCollection<LASSectionModel>();

            _headingCollectionLock = new object();
            Headings = new ObservableCollection<LASInformationModel>();

            LASFilterModels = new ObservableCollection<LASFilterModel>();

            SetupButtonCommands();

            SetupMessengerInstance();
        }

        #endregion constructor

        #region commands

        public ICommand ReadLASFileCommand { get; protected set; }
        public ICommand AddFilterConditionCommand { get; set; }
        public ICommand RemoveFilterConditionCommand { get; set; }

        #endregion commands 

        #region methods

        protected virtual void SetupButtonCommands()
        {
            ReadLASFileCommand = new RelayCommand(ReadLASFileAction, CanReadLASFileAction);
            AddFilterConditionCommand = new RelayCommand(AddFilterConditionAction);
            RemoveFilterConditionCommand = new RelayCommand(RemoveFilterConditionAction);
        }

        protected abstract void SetupMessengerInstance();

        protected virtual void ResetProperties()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Sections = new ObservableCollection<LASSectionModel>();
                Headings = new ObservableCollection<LASInformationModel>();
                LASFilterModels = new ObservableCollection<LASFilterModel>();
            });
        }

        protected virtual async void LoadLASFileCallback(FlyoutMessageModel flyoutMessage)
        {
            ResetProperties();

            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                progressDialogController.SetProgress(33);
                progressDialogController.SetMessage("Opening file...");

                progressDialogController.SetProgress(66);
                progressDialogController.SetMessage("Reading Sections...");

                using (StreamReader reader = new StreamReader(flyoutMessage.FileName))
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

        protected abstract bool CanReadLASFileAction();

        protected abstract void ReadLASFileAction();

        private async void AddFilterConditionAction()
        {
            LASFilterModel lasFilterModel = new LASFilterModel();

            object dialogResult = await DialogHost.Show(lasFilterModel, DialogHostIdentifier);

            if (dialogResult is bool boolResult && boolResult)
            {
                LASFilterModels.Add(lasFilterModel);
            }
        }

        private void RemoveFilterConditionAction()
        {
            LASFilterModels.Remove(SelectedLASFilterModel);
        }

        #endregion command methods

        #endregion methods

    }
}
