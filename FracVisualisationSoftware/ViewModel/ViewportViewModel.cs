using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HelixToolkit.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Action = System.Action;

namespace FracVisualisationSoftware.ViewModel
{
    public class ViewportViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields

        private Application _excelApplication;
        private Workbook _excelWorkbook;
        private Worksheet _excelWorksheet;
        private Range _excelUsedRange;

        private Point3DCollection _tubePath;

        private string _excelFileName;

        private ObservableCollection<string> _excelWorksheetNames;
        private int _selectedExcelWorksheetIndex;

        private string _xColumnHeading; //Easting
        private bool? _xColumnFound;

        private string _yColumnHeading; //TVD
        private bool? _yColumnFound;

        private string _zColumnHeading; //Northing
        private bool? _zColumnFound;

        private double _tubeLength;
        private double _tubeDiameter;

        #endregion fields

        #region properties

        public Point3DCollection TubePath
        {
            get { return _tubePath; }
            set { _tubePath = value; RaisePropertyChanged(() => TubePath); }
        }

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

                try
                {
                    _excelWorksheet = _excelWorkbook.Sheets[_selectedExcelWorksheetIndex];
                    _excelUsedRange = _excelWorksheet.UsedRange;
                }
                catch (Exception e)
                {

                }

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

        public double TubeLength
        {
            get => _tubeLength;
            set { _tubeLength = value; RaisePropertyChanged(); }
        }

        public double TubeDiameter
        {
            get => _tubeDiameter;
            set { _tubeDiameter = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<Visual3D> ViewportObjects { get; set; }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ViewportViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            SelectExcelFileCommand = new RelayCommand(SelectExcelFileAction);
            ReadExcelFileCommand = new RelayCommand(ReadExcelFileAction, CanReadExcelFileAction);
            GenerateModelsCommand = new RelayCommand(GenerateModelsAction, CanGenerateModelsAction);

            ExcelWorksheetNames = new ObservableCollection<string>();
            ViewportObjects = new ObservableCollection<Visual3D>();
            _tubePath = new Point3DCollection();
        }

        #endregion constructor

        #region commands

        public ICommand SelectExcelFileCommand { get; }
        public ICommand ReadExcelFileCommand { get; }
        public ICommand GenerateModelsCommand { get; }

        #endregion commands 

        #region methods

        private bool? HeadingValidation(string columnHeading)
        {
            Range columnRange = _excelUsedRange.Find(columnHeading, LookAt: XlLookAt.xlWhole);
            if (string.IsNullOrWhiteSpace(columnHeading))
            {
                return null;
            }
            else if (columnRange == null || string.IsNullOrWhiteSpace(columnRange.Value2 as string))
            {
                return false;
            }
            else
            {
                return true;
            }
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
                    ExcelFileName = excelFilePath.Split('\\').Last();

                    _excelApplication = new Application();

                    progressDialogController.SetProgress(50);
                    progressDialogController.SetMessage("Opening Excel Workbook...");

                    _excelWorkbook = _excelApplication.Workbooks.Open(excelFilePath);

                    progressDialogController.SetProgress(75);
                    progressDialogController.SetMessage("Reading Worksheet names...");

                    foreach (Worksheet excelWorksheet in _excelWorkbook.Worksheets)
                    {
                        App.Current.Dispatcher.BeginInvoke((Action)delegate
                        {
                            ExcelWorksheetNames.Add(excelWorksheet.Name);
                        });
                    }
                }
            });

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
                Range xColumn = _excelUsedRange.Find(_xColumnHeading);

                progressDialogController.SetProgress(33);

                Range yColumn = _excelUsedRange.Find(_yColumnHeading);

                progressDialogController.SetProgress(66);

                Range zColumn = _excelUsedRange.Find(_zColumnHeading);

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                App.Current.Dispatcher.BeginInvoke((Action)delegate
               {
                   _tubePath.Clear();
               });

                double initialX = 0;
                double initialY = 0;
                double initialZ = 0;

                bool initalValuesSet = false;

                int currentRow = xColumn.Row;

                int numberOfRows = _excelUsedRange.Rows.Count;

                bool allValuesParsed = false;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                while (!allValuesParsed)
                {
                    if (IsNumeric(_excelUsedRange.Cells[currentRow, xColumn.Column].Value2))
                    {
                        if (!initalValuesSet)
                        {
                            initialX = (_excelUsedRange.Cells[currentRow, xColumn.Column].Value2 / TubeLength) * -1;
                            initialY = (_excelUsedRange.Cells[currentRow, yColumn.Column].Value2 / TubeLength) * -1;
                            initialZ = (_excelUsedRange.Cells[currentRow, zColumn.Column].Value2 / TubeLength) * -1;

                            initalValuesSet = true;
                        }

                        double x = (_excelUsedRange.Cells[currentRow, xColumn.Column].Value2 / TubeLength) * -1;
                        double y = (_excelUsedRange.Cells[currentRow, yColumn.Column].Value2 / TubeLength) * -1;
                        double z = (_excelUsedRange.Cells[currentRow, zColumn.Column].Value2 / TubeLength) * -1;
                        
                        x -= initialX;
                        y -= initialY;
                        z -= initialZ;

                        App.Current.Dispatcher.BeginInvoke((Action)delegate
                       {
                           TubePath.Add(new Point3D(x, y, z));
                       });

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
            });

            await progressDialogController.CloseAsync();
        }

        private bool CanGenerateModelsAction()
        {
            return TubePath.Any();
        }

        private void GenerateModelsAction()
        {
            ViewportObjects.Clear();

            ViewportObjects.Add(new SunLight());

            ViewportObjects.Add(new TubeVisual3D { AddCaps = true, Path = TubePath, Diameter = TubeDiameter });
        }

        #endregion command methods

        #region generic Methods

        private bool IsNumeric(object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        #endregion generic Methods

        #endregion methods

    }
}
