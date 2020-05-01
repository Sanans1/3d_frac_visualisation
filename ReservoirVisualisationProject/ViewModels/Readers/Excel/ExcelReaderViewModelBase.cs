using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.Excel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HelixToolkit.Wpf;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using OfficeOpenXml;

namespace ReservoirVisualisationProject.ViewModels.Readers.Excel
{
    public abstract class ExcelReaderViewModelBase : ViewModelBase
    {
        #region fields

        #region injected fields

        protected IDialogCoordinator _dialogCoordinator;

        #endregion

        private ExcelPackage _excelApplication;
        private ExcelWorkbook _excelWorkbook;
        protected ExcelWorksheet _excelWorksheet;
        protected ExcelRange _excelUsedRange;

        private object _excelWorksheetNamesCollectionLock;
        private ObservableCollection<string> _excelWorksheetNames;

        private int _selectedExcelWorksheetIndex;

        private bool? _filterColumnFound;
        private bool _canAddFilterCondition;

        private ObservableCollection<ExcelFilterModel> _excelFilterModels;
        private ExcelFilterModel _selectedExcelFilterModel;

        #endregion

        #region properties

        public ObservableCollection<string> ExcelWorksheetNames
        {
            get { return _excelWorksheetNames; }
            set
            {
                _excelWorksheetNames = value;
                BindingOperations.EnableCollectionSynchronization(_excelWorksheetNames, _excelWorksheetNamesCollectionLock);
                RaisePropertyChanged(() => ExcelWorksheetNames);
            }
        }

        public int SelectedExcelWorksheetIndex
        {
            get { return _selectedExcelWorksheetIndex; }
            set
            {
                if (_excelWorkbook == null) return;

                _selectedExcelWorksheetIndex = value + 1;

                try
                {
                    _excelWorksheet = _excelWorkbook.Worksheets[_selectedExcelWorksheetIndex];
                    _excelUsedRange = _excelWorksheet.Cells;
                }
                catch
                {
                    // ignored
                }

                RaisePropertyChanged();
            }
        }


        public bool? FilterColumnHeadingFound
        {
            get { return _filterColumnFound; }
            set { _filterColumnFound = value; RaisePropertyChanged(); }
        }

        public bool CanAddFilterCondition
        {
            get => _canAddFilterCondition;
            set { _canAddFilterCondition = value; RaisePropertyChanged();}
        }

        public ObservableCollection<ExcelFilterModel> ExcelFilterModels
        {
            get { return _excelFilterModels; }
            set { _excelFilterModels = value; RaisePropertyChanged(); }
        }

        public ExcelFilterModel SelectedExcelFilterModel
        {
            get { return _selectedExcelFilterModel; }
            set { _selectedExcelFilterModel = value; RaisePropertyChanged(); }
        }

        public string DialogHostIdentifier { get; protected set; }

        #endregion

        #region constructor

        public ExcelReaderViewModelBase(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            _excelWorksheetNamesCollectionLock = new object();
            ExcelWorksheetNames = new ObservableCollection<string>();

            ExcelFilterModels = new ObservableCollection<ExcelFilterModel>();

            SetupButtonCommands();

            SetupMessengerInstance();
        }

        #endregion

        #region commands

        public ICommand ReadExcelFileCommand { get; set; }
        public ICommand AddFilterConditionCommand { get; set; }
        public ICommand RemoveFilterConditionCommand { get; set; }

        #endregion commands 

        #region methods

        protected virtual void SetupButtonCommands()
        {
            ReadExcelFileCommand = new RelayCommand(ReadExcelFileAction, CanReadExcelFileAction);
            AddFilterConditionCommand = new RelayCommand(AddFilterConditionAction);
            RemoveFilterConditionCommand = new RelayCommand(RemoveFilterConditionAction);
        }

        protected abstract void SetupMessengerInstance();

        protected virtual void ResetProperties()
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                _excelApplication = null;
                _excelWorkbook = null;
                _excelWorksheet = null;
                _excelUsedRange = null;
                ExcelWorksheetNames = new ObservableCollection<string>();
                ExcelFilterModels = new ObservableCollection<ExcelFilterModel>();
            });
        }

        protected bool? HeadingValidation(string columnHeading)
        {
            if (_excelUsedRange == null) return null;

            if (string.IsNullOrWhiteSpace(columnHeading)) return null;

            ExcelRangeBase foundCell = _excelUsedRange.SingleOrDefault(cell => cell.Address == columnHeading && cell.Value != null);

            if (foundCell == null)
            {
                return false;
            }

            return true;
        }

        protected virtual async void LoadExcelFileCallback(FlyoutMessageModel flyoutMessage)
        {
            ResetProperties();

            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                progressDialogController.SetProgress(25);
                progressDialogController.SetMessage("Starting Excel process...");

                _excelApplication = new ExcelPackage(new FileInfo(flyoutMessage.FileName));

                progressDialogController.SetProgress(50);
                progressDialogController.SetMessage("Opening Excel Workbook...");

                _excelWorkbook = _excelApplication.Workbook;

                progressDialogController.SetProgress(75);
                progressDialogController.SetMessage("Reading Worksheet names...");
            });

            foreach (ExcelWorksheet excelWorksheet in _excelWorkbook.Worksheets)
            {
                ExcelWorksheetNames.Add(excelWorksheet.Name);
            }

            SelectedExcelWorksheetIndex = 0;

            await progressDialogController.CloseAsync();
        }

        #region command methods

        protected abstract bool CanReadExcelFileAction();

        protected abstract void ReadExcelFileAction();

        private async void AddFilterConditionAction()
        {
            ExcelFilterModel excelFilterModel = new ExcelFilterModel();

            excelFilterModel.PropertyChanged += OnExcelFilterModelOnPropertyChanged;

            object dialogResult = await DialogHost.Show(excelFilterModel, DialogHostIdentifier);

            if (dialogResult is bool boolResult && boolResult)
            {
                ExcelFilterModels.Add(excelFilterModel);
            }

            excelFilterModel.PropertyChanged -= OnExcelFilterModelOnPropertyChanged;
        }

        private void RemoveFilterConditionAction()
        {
            ExcelFilterModels.Remove(SelectedExcelFilterModel);
        }

        protected bool FilterRow(int currentRow)
        {
            bool shouldUse = true;

            if (ExcelFilterModels.Any())
            {
                foreach (ExcelFilterModel excelFilterModel in ExcelFilterModels)
                {
                    IEnumerable<ExcelRangeBase> filterColumn =
                        _excelUsedRange.SingleOrDefault(cell => cell.Address == excelFilterModel.HeadingCellAddress);

                    object value = _excelWorksheet.Cells[currentRow, filterColumn.First().Start.Column].Value;

                    if (value != null)
                    {
                        shouldUse = value.ToString().Contains(excelFilterModel.FilterText);
                    }
                    else if (!string.IsNullOrWhiteSpace(excelFilterModel.FilterText)) shouldUse = false;

                    if (shouldUse == false) break;
                }
            }

            return shouldUse;
        }

        #region event method

        private void OnExcelFilterModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is ExcelFilterModel excelFilterModel)
            {
                if (args.PropertyName == "HeadingCellAddress")
                {
                    FilterColumnHeadingFound = HeadingValidation(excelFilterModel.HeadingCellAddress);
                }

                CanAddFilterCondition = FilterColumnHeadingFound == true;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
