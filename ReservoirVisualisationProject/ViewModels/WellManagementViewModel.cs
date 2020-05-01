
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ReservoirVisualisationProject.Enums;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Services.Interfaces.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using static System.Windows.Application;

namespace ReservoirVisualisationProject.ViewModels
{
    public class WellManagementViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;
        private IDataService _dataService;

        #endregion

        private ObservableCollection<WellModel> _wellModels;
        private int _selectedWellIndex;

        #endregion

        #region properties

        public ObservableCollection<WellModel> WellModels
        {
            get { return _wellModels; }
            set { _wellModels = value; RaisePropertyChanged(); }
        }

        public int SelectedWellIndex
        {
            get => _selectedWellIndex;
            set
            {
                _selectedWellIndex = value; 
                RaisePropertyChanged();
                RaisePropertyChanged(() => WellIsSelected);
            }
        }

        public bool WellIsSelected => SelectedWellIndex > -1 && SelectedWellIndex < WellModels.Count;

        #endregion

        #region constructor

        public WellManagementViewModel(IDialogCoordinator dialogCoordinator, IDataService dataService)
        {
            _dialogCoordinator = dialogCoordinator;
            _dataService = dataService;

            WellModels = new ObservableCollection<WellModel>();

            SaveWellCommand = new RelayCommand(SaveWellAction);
            LoadWellCommand = new RelayCommand(LoadWellAction);
            AddWellCommand = new RelayCommand(() => AddAction(WellDataTypeEnum.Path));
            RemoveWellCommand = new RelayCommand(() => RemoveAction(WellDataTypeEnum.Path, SelectedWellIndex), () => WellIsSelected);
            AddStageCommand = new RelayCommand<int>(i => AddAction(WellDataTypeEnum.Stages, i));
            RemoveStageCommand = new RelayCommand<int>(i => RemoveAction(WellDataTypeEnum.Stages, i), i => i < WellModels.Count && WellModels[i].HasStages); 
            AddDataCommand = new RelayCommand<int>(i => AddAction(WellDataTypeEnum.Data, i));
            RemoveDataCommand = new RelayCommand<int>(i => RemoveAction(WellDataTypeEnum.Data, i), i => i < WellModels.Count && WellModels[i].DataSetIsSelected);
            DisplayDataCommand = new RelayCommand(async () => await CreateTimestampList());

            MessengerInstance.Register<WellModel>(this, MessageTokenStrings.AddPathToManager, async x => await AddWellToList(x));
            MessengerInstance.Register<(List<StageModel>, int)>(this, MessageTokenStrings.AddStagesToManager, x => AddStagesToWell(x.Item1, x.Item2));
            MessengerInstance.Register<(DataSetModel, int)>(this, MessageTokenStrings.AddDataToManager, x => AddDataToWell(x.Item1, x.Item2));
            MessengerInstance.Register<DateTime>(this, MessageTokenStrings.SelectDataByTimestamp, GetDataValuesByTimestamp);
        }

        #endregion

        #region commands

        public ICommand SaveWellCommand { get; }
        public ICommand LoadWellCommand { get; }
        public ICommand AddWellCommand { get; }
        public ICommand RemoveWellCommand { get; }
        public ICommand AddStageCommand { get; }
        public ICommand RemoveStageCommand { get; }
        public ICommand AddDataCommand { get; }
        public ICommand RemoveDataCommand { get; }
        public ICommand DisplayDataCommand { get; }

        #endregion

        #region methods

        private void SaveWellAction()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt";
            saveFileDialog.Title = "Select a file to save well data to.";

            if (saveFileDialog.ShowDialog() == true)
            {
                _dataService.SaveWellData(WellModels.ToList(), saveFileDialog.FileName);
            }
        }        
        
        private async void LoadWellAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");

            await Task.Run(async () =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text Files|*.txt";
                openFileDialog.Title = "Select a file to load well data from.";

                if (openFileDialog.ShowDialog() == true)
                {
                    progressDialogController.SetMessage($"Reading saved data...");

                    List<WellModel> data = _dataService.LoadWellData(openFileDialog.FileName);

                    progressDialogController.Maximum = data.Count;

                    for (int index = 0; index < data.Count; index++)
                    {
                        WellModel wellModel = data[index];

                        progressDialogController.SetMessage($"Loading {wellModel.Name}...");
                        progressDialogController.SetProgress(index + 1);

                        await AddWellToList(wellModel);
                    }

                    if (WellModels.Any(x => x.DataSets.Any())) await CreateTimestampList();
                }

                await progressDialogController.CloseAsync();
            });
        }

        private async void AddAction(WellDataTypeEnum dataTypeEnum, int wellID = -1)
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");

            await Task.Run(async () =>
            { 
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All files|*.*|All Excel Files|*.xls;*.xlsx;*.xlsm|LAS Files|*.las|DAT Files|*.dat|PATH Files|*.path|PDAT Files|*.pdat|PROD Files|*.prod|TOPS Files|*.tops";
                openFileDialog.Title = "Select a file to import path data from.";

                if (openFileDialog.ShowDialog() == true)
                {
                    await progressDialogController.CloseAsync();

                    string extension = Path.GetExtension(openFileDialog.FileName);

                    FlyoutMessageModel flyoutMessage = new FlyoutMessageModel
                        {FileName = openFileDialog.FileName, WellID = wellID};

                    switch (extension)
                    {
                        case ".xls":
                        case ".xlsx":
                        case ".xlsm":
                            MessengerInstance.Send(flyoutMessage, (FileTypeEnum.Excel, dataTypeEnum));
                            MessengerInstance.Send((FileTypeEnum.Excel, dataTypeEnum));
                            break;
                        case ".las":
                            MessengerInstance.Send(flyoutMessage, (FileTypeEnum.LAS, dataTypeEnum));
                            MessengerInstance.Send((FileTypeEnum.LAS, dataTypeEnum));
                            break;
                        case ".dat":
                        case ".path":
                        case ".pdat":
                        case ".prod":
                        case ".tops":
                            MessengerInstance.Send(flyoutMessage, (FileTypeEnum.EV, dataTypeEnum));
                            MessengerInstance.Send((FileTypeEnum.EV, dataTypeEnum));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            });
        }

        private async void RemoveAction(WellDataTypeEnum dataTypeEnum, int wellID)
        {
            if (Current.Dispatcher != null)
            {
                await Current.Dispatcher?.InvokeAsync(async () =>
                {
                    switch (dataTypeEnum)
                    {
                        case WellDataTypeEnum.Path:
                            MessengerInstance.Send(WellModels[wellID], MessageTokenStrings.RemoveWellFromViewport);
                            WellModels.Remove(WellModels[wellID]);
                            break;
                        case WellDataTypeEnum.Stages:
                            MessengerInstance.Send(WellModels[wellID], MessageTokenStrings.RemoveWellFromViewport);
                            WellModels[wellID].Stages.Clear();
                            WellModels[wellID].DataSets.Clear();
                            break;
                        case WellDataTypeEnum.Data:
                            MessengerInstance.Send(WellModels[wellID], MessageTokenStrings.RemoveWellFromViewport);
                            WellModels[wellID].DataSets.RemoveAt(WellModels[wellID].SelectedDataSetIndex);
                            break;
                    }

                    if (WellModels.Any(x => x.DataSets.Any())) await CreateTimestampList();
                });
            }
        }

        private async Task AddWellToList(WellModel wellModel)
        {
            wellModel.ID = WellModels.Count;

            await Current.Dispatcher?.InvokeAsync(() => { WellModels.Add(wellModel); });

            MessengerInstance.Send(wellModel, MessageTokenStrings.AddPathToViewport);
            MessengerInstance.Send((FileTypeEnum.None, WellDataTypeEnum.None));
        }

        private void AddStagesToWell(List<StageModel> stageModels, int wellID)
        {
            Current.Dispatcher?.InvokeAsync(() => 
            { 
                WellModel selectedWellModel = WellModels[wellID];

                selectedWellModel.Stages = new ObservableCollection<StageModel>(stageModels);

                foreach (StageModel stageModel in stageModels)
                {
                    Point3D point3D = selectedWellModel.Path.FirstOrDefault(x => x.Y <= stageModel.Position.Y);

                    Point3D editedPosition = stageModel.Position;

                    editedPosition.X = selectedWellModel.Path.Last().X;

                    stageModels.Single(x => x.StageNumber == stageModel.StageNumber).Position = editedPosition;

                    if (point3D == default)
                    {
                        selectedWellModel.Path.Add(editedPosition);
                    }
                }
            });

            MessengerInstance.Send((FileTypeEnum.None, WellDataTypeEnum.None));
        }

        private void AddDataToWell(DataSetModel dataSetModel, int wellID)
        {
            Current.Dispatcher?.InvokeAsync(() => { WellModels[wellID].DataSets.Add(dataSetModel); });

            MessengerInstance.Send((FileTypeEnum.None, WellDataTypeEnum.None));
        }

        private async Task CreateTimestampList()
        {
            await Task.Run(() =>
            {
                List<DateTime> timestamps = new List<DateTime>();

                foreach (WellModel wellModel in WellModels)
                {
                    List<DataValueModel> dataValueModels = wellModel.DataSets[wellModel.SelectedDataSetIndex].Values;

                    timestamps.AddRange(dataValueModels.Select(x => x.Timestamp));
                }

                MessengerInstance.Send(timestamps.Distinct().OrderBy(x => x.Date).ToList(),
                    MessageTokenStrings.SetupTimeline);
            });
        }

        private void GetDataValuesByTimestamp(DateTime timestamp)
        {
            List<RenderDataModel> renderDataModels = new List<RenderDataModel>();

            foreach (WellModel wellModel in WellModels)
            {
                if (wellModel.DataSets.Any())
                {
                    List<DataValueModel> dataValueModels = wellModel.DataSets[wellModel.SelectedDataSetIndex].Values;

                    foreach (StageModel stageModel in wellModel.Stages)
                    {
                        DataValueModel dataValueModel = dataValueModels.Where(x => x.Stage == stageModel.StageNumber)
                            .Aggregate((x, y) =>
                                Math.Abs(x.Timestamp.Ticks - timestamp.Ticks) <
                                Math.Abs(y.Timestamp.Ticks - timestamp.Ticks)
                                    ? x
                                    : y);

                        renderDataModels.Add(new RenderDataModel
                        {
                            WellID = wellModel.ID,
                            Stage = dataValueModel.Stage,
                            UnitOfMeasurement = wellModel.DataSets[wellModel.SelectedDataSetIndex].DataUnitOfMeasurement,
                            Value = dataValueModel.Value,
                            Position = wellModel.Stages[dataValueModel.Stage - 1].Position
                        });
                    }
                }
            }
        
            MessengerInstance.Send(renderDataModels, MessageTokenStrings.AddDataToViewport);
        }

        #endregion
    }
}
