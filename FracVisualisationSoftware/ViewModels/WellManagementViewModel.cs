
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Services.Interfaces.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using static System.Windows.Application;

namespace FracVisualisationSoftware.ViewModels
{
    public class WellManagementViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;
        private IDataService _dataService;

        #endregion

        private ObservableCollection<WellModel> _wellModels;
        private WellModel _selectedWellModel;

        #endregion

        #region properties

        public ObservableCollection<WellModel> WellModels
        {
            get { return _wellModels; }
            set { _wellModels = value; RaisePropertyChanged(); }
        }

        public WellModel SelectedWellModel
        {
            get { return _selectedWellModel; }
            set { _selectedWellModel = value; RaisePropertyChanged(); }
        }

        #endregion

        #region constructor

        public WellManagementViewModel(IDialogCoordinator dialogCoordinator, IDataService dataService)
        {
            _dialogCoordinator = dialogCoordinator;
            _dataService = dataService;

            WellModels = new ObservableCollection<WellModel>();

            SaveWellCommand = new RelayCommand(SaveWellAction);
            LoadWellCommand = new RelayCommand(LoadWellAction);
            AddWellCommand = new RelayCommand(() => AddWellAction(WellDataTypeEnum.Path));
            RemoveWellCommand = new RelayCommand(RemoveWellAction); //TODO Add lambda thing to
            AddStageCommand = new RelayCommand<int>(i => AddWellAction(WellDataTypeEnum.Stages, i));
            RemoveStageCommand = new RelayCommand(RemoveWellAction); //TODO Add lambda thing to
            AddDataCommand = new RelayCommand<int>(i => AddWellAction(WellDataTypeEnum.Data, i));
            RemoveDataCommand = new RelayCommand(RemoveWellAction); //TODO Add lambda thing to
            DisplayDataCommand = new RelayCommand(CreateTimestampList);

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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";
            openFileDialog.Title = "Select a file to load well data from.";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (WellModel wellModel in _dataService.LoadWellData(openFileDialog.FileName))
                {
                    await AddWellToList(wellModel);
                } 

                if (WellModels.Any(x => x.DataSets.Any())) CreateTimestampList();
            }
        }

        private async void AddWellAction(WellDataTypeEnum dataTypeEnum, int wellID = -1)
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Awaiting user to select file...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Excel Files|*.xls;*.xlsx;*.xlsm|LAS File|*.las|DAT File|*.dat|PATH File|*.path|PDAT File|*.pdat|PROD File|*.prod|TOPS File|*.tops";
                openFileDialog.Title = "Select a file to import path data from.";

                if (openFileDialog.ShowDialog() == true)
                {
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
                    }
                }
            });

            await progressDialogController.CloseAsync();
        }

        private void RemoveWellAction()
        {
            MessengerInstance.Send(SelectedWellModel, MessageTokenStrings.RemoveWellFromViewport);
            WellModels.Remove(SelectedWellModel);
        }

        private async Task AddWellToList(WellModel wellModel)
        {
            wellModel.ID = WellModels.Count;

            await Current.Dispatcher?.InvokeAsync(() => { WellModels.Add(wellModel); });

            MessengerInstance.Send(wellModel, MessageTokenStrings.AddPathToViewport);
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
        }

        private void AddDataToWell(DataSetModel dataSetModel, int wellID)
        {
            Current.Dispatcher?.InvokeAsync(() => { WellModels[wellID].DataSets.Add(dataSetModel); });
        }

        private void CreateTimestampList()
        {
            List<DateTime> timestamps = new List<DateTime>();

            foreach (WellModel wellModel in WellModels)
            {
                List<DataValueModel> dataValueModels = wellModel.DataSets[wellModel.SelectedDataSetIndex].Values;

                timestamps.AddRange(dataValueModels.Select(x => x.Timestamp));
            }

            MessengerInstance.Send(timestamps.Distinct().ToList(), MessageTokenStrings.SetupTimeline);
        }

        private void GetDataValuesByTimestamp(DateTime timestamp)
        {
            List<RenderDataModel> renderDataModels = new List<RenderDataModel>();

            foreach (WellModel wellModel in WellModels)
            {
                List<DataValueModel> dataValueModels = wellModel.DataSets[wellModel.SelectedDataSetIndex].Values;

                foreach (StageModel stageModel in wellModel.Stages)
                {
                    DataValueModel dataValueModel = dataValueModels.Where(x => x.Stage == stageModel.StageNumber).Aggregate((x, y) => Math.Abs(x.Timestamp.Ticks - timestamp.Ticks) < Math.Abs(y.Timestamp.Ticks - timestamp.Ticks) ? x : y);

                    renderDataModels.Add(new RenderDataModel
                    {
                        Name = $"{wellModel.Name}-{dataValueModel.Stage}-",
                        UnitOfMeasurement = wellModel.DataSets[wellModel.SelectedDataSetIndex].DataUnitOfMeasurement,
                        Value = dataValueModel.Value,
                        Position = wellModel.Stages[dataValueModel.Stage - 1].Position
                    });
                }
            }

            MessengerInstance.Send(renderDataModels, MessageTokenStrings.AddDataToViewport);
        }

        #endregion
    }
}
