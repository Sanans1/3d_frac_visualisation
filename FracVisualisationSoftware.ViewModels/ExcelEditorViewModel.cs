using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FracVisualisationSoftware.Misc.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using Action = System.Action;

namespace FracVisualisationSoftware.ViewModel
{
    public class ExcelEditorViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields
        
        private ExcelPackage _excelApplication;
        private ExcelWorkbook _excelWorkbook;
        private ExcelWorksheet _excelWorksheet;
        private ExcelRange _excelUsedRange;

        private string _excelFileName;

        private ObservableCollection<string> _excelWorksheetNames;
        private int _selectedExcelWorksheetIndex;

        private string _xColumnHeading; //Easting
        private bool? _xColumnFound;

        private string _yColumnHeading; //TVD
        private bool? _yColumnFound;

        private string _zColumnHeading; //Northing
        private bool? _zColumnFound;

        #endregion fields

        #region properties

        public string ExcelFileName
        {
            get { return _excelFileName; }
            set { _excelFileName = value; RaisePropertyChanged(() => ExcelFileName); }
        }

        public ObservableCollection<string> ExcelWorksheetNames
        {
            get { return _excelWorksheetNames; }
            set { _excelWorksheetNames = value; RaisePropertyChanged(() => ExcelWorksheetNames); }
        }

        public int SelectedExcelWorksheetIndex
        {
            get { return _selectedExcelWorksheetIndex; }
            set
            {
                _selectedExcelWorksheetIndex = value + 1;

                _excelWorksheet = _excelWorkbook.Worksheets[_selectedExcelWorksheetIndex];
                _excelUsedRange = _excelWorksheet.Cells;

                RaisePropertyChanged(() => SelectedExcelWorksheetIndex);
            }
        }

        public string XColumnHeading
        {
            get { return _xColumnHeading; }
            set
            {
                _xColumnHeading = value;
                XColumnFound = HeadingValidation(value);
                RaisePropertyChanged(() => XColumnHeading);
            }
        }

        public bool? XColumnFound
        {
            get { return _xColumnFound; }
            set { _xColumnFound = value; RaisePropertyChanged(() => XColumnFound); }
        }

        public string YColumnHeading
        {
            get { return _yColumnHeading; }
            set
            {
                _yColumnHeading = value;
                YColumnFound = HeadingValidation(value);
                RaisePropertyChanged(() => YColumnHeading);
            }
        }

        public bool? YColumnFound
        {
            get { return _yColumnFound; }
            set { _yColumnFound = value; RaisePropertyChanged(() => YColumnFound); }
        }

        public string ZColumnHeading
        {
            get { return _zColumnHeading; }
            set
            {
                _zColumnHeading = value;
                ZColumnFound = HeadingValidation(value);
                RaisePropertyChanged(() => ZColumnHeading);
            }
        }

        public bool? ZColumnFound
        {
            get { return _zColumnFound; }
            set { _zColumnFound = value; RaisePropertyChanged(() => ZColumnFound); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ExcelEditorViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            ExcelWorksheetNames = new ObservableCollection<string>();

            SelectExcelFileCommand = new RelayCommand(SelectExcelFileAction);
            ReadExcelFileCommand = new RelayCommand(ReadExcelFileAction, CanReadExcelFileAction);
        }

        #endregion constructor

        #region commands

        public ICommand SelectExcelFileCommand { get; }
        public ICommand ReadExcelFileCommand { get; }

        #endregion commands 

        #region methods

        private bool? HeadingValidation(string columnHeading)
        {
            IEnumerable<ExcelRangeBase> foundCell = from cell in _excelUsedRange where cell.Value?.ToString() == columnHeading select cell;

            if (string.IsNullOrWhiteSpace(columnHeading))
            {
                return null;
            }

            if (!foundCell.Any())
            {
                return false;
            }

            return true;
        }

        #region command methods

        /// <summary>
        /// Opens a OpenFileDialog to allow the user to select an Excel file.
        /// This also populates the Worksheet dropdown with names of Worksheets.
        /// </summary>
        private async void SelectExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() => 
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog.Title = "Select an Excel file with Frac Lateral Data.";

                if (openFileDialog.ShowDialog() == true)
                {
                    progressDialogController.SetProgress(25);
                    progressDialogController.SetMessage("Starting Excel process...");

                    string excelFilePath = openFileDialog.FileName;
                    ExcelFileName = excelFilePath.Substring(excelFilePath.LastIndexOf('\\') + 1);

                    _excelApplication = new ExcelPackage(new FileInfo(excelFilePath));

                    progressDialogController.SetProgress(50);
                    progressDialogController.SetMessage("Opening Excel Workbook...");

                    _excelWorkbook = _excelApplication.Workbook;

                    progressDialogController.SetProgress(75);
                    progressDialogController.SetMessage("Reading Worksheet names...");
                }
            });

            foreach (ExcelWorksheet excelWorksheet in _excelWorkbook.Worksheets)
            {
                ExcelWorksheetNames.Add(excelWorksheet.Name);
            }

            SelectedExcelWorksheetIndex = 0;

            await progressDialogController.CloseAsync();
        }

        private bool CanReadExcelFileAction()
        {
            if (XColumnFound != null && YColumnFound != null && ZColumnFound != null)
            {
                return (!string.IsNullOrWhiteSpace(ExcelFileName) && XColumnFound.Value && YColumnFound.Value &&
                        ZColumnFound.Value);
            }

            return false;
        }

        private async void ReadExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                IEnumerable<ExcelRangeBase> xColumn = from cell in _excelUsedRange where cell.Value?.ToString() == _xColumnHeading select cell;

                progressDialogController.SetProgress(33);

                IEnumerable<ExcelRangeBase> yColumn = from cell in _excelUsedRange where cell.Value?.ToString() == _yColumnHeading select cell;

                progressDialogController.SetProgress(66);

                IEnumerable<ExcelRangeBase> zColumn = from cell in _excelUsedRange where cell.Value?.ToString() == _zColumnHeading select cell;

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                List<Point3D> tubePath = new List<Point3D>();

                double initialX = 0;
                double initialY = 0;
                double initialZ = 0;

                bool initalValuesSet = false;

                int currentRow = xColumn.First().Start.Row;

                int numberOfRows = _excelWorksheet.Dimension.Rows;

                bool allValuesParsed = false;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                while (!allValuesParsed)
                {
                    object cellValue = _excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value;
                    if (cellValue.IsNumeric())
                    {
                        if (!initalValuesSet)
                        {
                            initialX = (double)_excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value * -1;
                            initialY = (double)_excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value * -1;
                            initialZ = (double)_excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value * -1;

                            initalValuesSet = true;
                        }
                        
                        double x = (double)_excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value * -1;
                        double y = (double)_excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value * -1;
                        double z = (double)_excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value * -1;
                        
                        x -= initialX;
                        y -= initialY;
                        z -= initialZ;

                        tubePath.Add(new Point3D(x, y, z));

                        currentRow++;
                    }
                    else
                    {
                        if (initalValuesSet)
                        {
                            allValuesParsed = true;
                        }
                        else
                        {
                            currentRow++;
                        }
                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow - 1);
                }

                MessengerInstance.Send(new GenericMessage<List<Point3D>>(tubePath));
            });

            await progressDialogController.CloseAsync();
        }

        #endregion command methods

        #endregion methods

    }
}
