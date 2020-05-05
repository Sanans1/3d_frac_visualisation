using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.EV;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;

namespace ReservoirVisualisationProject.ViewModels.Readers.EV
{
    public abstract class EVReaderViewModelBase : ViewModelBase
    {
        #region fields

        #region injected fields

        protected IDialogCoordinator _dialogCoordinator;

        #endregion injected fields
        
        protected List<string> _content;

        private readonly object _headingCollectionLock;
        private ObservableCollection<string> _headings;

        private ObservableCollection<EVFilterModel> _evFilterModels;
        private EVFilterModel _selectedEVFilterModel;

        #endregion fields

        #region properties

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

        public ObservableCollection<EVFilterModel> EVFilterModels
        {
            get { return _evFilterModels; }
            set { _evFilterModels = value; RaisePropertyChanged(); }
        }

        public EVFilterModel SelectedEVFilterModel
        {
            get { return _selectedEVFilterModel; }
            set { _selectedEVFilterModel = value; RaisePropertyChanged(); }
        }

        public string DialogHostIdentifier { get; protected set; }

        #endregion properties

        #region constructor

        public EVReaderViewModelBase(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            _headingCollectionLock = new object();
            Headings = new ObservableCollection<string>();

            _content = new List<string>();

            EVFilterModels = new ObservableCollection<EVFilterModel>();

            SetupButtonCommands();

            SetupMessengerInstance();
        }

        #endregion constructor

        #region commands

        public ICommand ReadEVFileCommand { get; set; }
        public ICommand AddFilterConditionCommand { get; set; }
        public ICommand RemoveFilterConditionCommand { get; set; }

        #endregion commands 

        #region methods

        protected virtual void SetupButtonCommands()
        {
            ReadEVFileCommand = new RelayCommand(ReadEVFileAction, CanReadEVFileAction);
            AddFilterConditionCommand = new RelayCommand(AddFilterConditionAction);
            RemoveFilterConditionCommand = new RelayCommand(RemoveFilterConditionAction);
        }

        protected abstract void SetupMessengerInstance();

        protected virtual void ResetProperties()
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                _content = new List<string>();
                Headings = new ObservableCollection<string>();
                EVFilterModels = new ObservableCollection<EVFilterModel>();
            });
        }

        protected virtual async void LoadEVFileCallback(FlyoutMessageModel flyoutMessage)
        {
            ResetProperties();

            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                progressDialogController.SetProgress(33);
                progressDialogController.SetMessage("Opening file...");

                progressDialogController.SetProgress(66);
                progressDialogController.SetMessage("Reading Fields...");

                using (StreamReader reader = new StreamReader(flyoutMessage.FileName))
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

        #region command methods

        protected abstract bool CanReadEVFileAction();

        protected abstract void ReadEVFileAction();

        private async void AddFilterConditionAction()
        {
            EVFilterModel evFilterModel = new EVFilterModel();

            object dialogResult = await DialogHost.Show(evFilterModel, DialogHostIdentifier);

            if (dialogResult is bool boolResult && boolResult)
            {
                EVFilterModels.Add(evFilterModel);
            }
        }

        private void RemoveFilterConditionAction()
        {
            EVFilterModels.Remove(SelectedEVFilterModel);
        }

        #endregion command methods

        #endregion methods
    }
}
