
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FracVisualisationSoftware.Enums;
using FracVisualisationSoftware.Models;
using FracVisualisationSoftware.Services.Interfaces.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;

namespace FracVisualisationSoftware.ViewModels
{
    public class BoreholeManagementViewModel : ViewModelBase
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

        public BoreholeManagementViewModel(IDialogCoordinator dialogCoordinator, IDataService dataService)
        {
            _dialogCoordinator = dialogCoordinator;
            _dataService = dataService;

            WellModels = new ObservableCollection<WellModel>();

            SaveWellCommand = new RelayCommand(SaveWellAction);
            LoadWellCommand = new RelayCommand(LoadWellAction);
            AddWellCommand = new RelayCommand(() => AddAction(WellDataTypeEnum.Path));
            RemoveWellCommand = new RelayCommand(RemoveAction); //TODO Add lambda thing to
            AddStageCommand = new RelayCommand<int>(i => AddAction(WellDataTypeEnum.Stages, i));
            RemoveStageCommand = new RelayCommand(RemoveAction); //TODO Add lambda thing to

            MessengerInstance.Register<WellModel>(this, "Borehole Data Added", AddWellToList);
            MessengerInstance.Register<(List<StageModel>, int)>(this, "Well Stages Added", x => AddStagesToWell(x.Item1, x.Item2));
        }

        #endregion

        #region commands

        public ICommand SaveWellCommand { get; }
        public ICommand LoadWellCommand { get; }
        public ICommand AddWellCommand { get; }
        public ICommand RemoveWellCommand { get; }
        public ICommand AddStageCommand { get; }
        public ICommand RemoveStageCommand { get; }

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
        
        private void LoadWellAction()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";
            openFileDialog.Title = "Select a file to load well data from.";

            if (openFileDialog.ShowDialog() == true)
            {
                WellModels = new ObservableCollection<WellModel>(_dataService.LoadWellData(openFileDialog.FileName));_dataService.LoadWellData(openFileDialog.FileName);
            }
        }

        private async void AddAction(WellDataTypeEnum dataTypeEnum, int wellID = -1)
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

                progressDialogController.CloseAsync();
            });
        }

        private async void RemoveAction()
        {
            MessengerInstance.Send(SelectedWellModel, "Delete BoreholeModel");
            WellModels.Remove(SelectedWellModel);
        }

        private void AddWellToList(WellModel wellModel)
        {
            wellModel.ID = WellModels.Count;

            Application.Current.Dispatcher?.InvokeAsync(() => { WellModels.Add(wellModel); });
        }

        private void AddStagesToWell(List<StageModel> stageModels, int wellID)
        {
            WellModels[wellID].Stages = new ObservableCollection<StageModel>(stageModels);
        }

        #endregion
    }
}
