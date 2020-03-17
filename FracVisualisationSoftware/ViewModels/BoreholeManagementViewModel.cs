
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

        #endregion

        private ObservableCollection<BoreholeModel> _boreholeModels;
        private BoreholeModel _selectedBoreholeModel;

        #endregion

        #region properties

        public ObservableCollection<BoreholeModel> BoreholeModels
        {
            get { return _boreholeModels; }
            set { _boreholeModels = value; RaisePropertyChanged(); }
        }

        public BoreholeModel SelectedBoreholeModel
        {
            get { return _selectedBoreholeModel; }
            set { _selectedBoreholeModel = value; RaisePropertyChanged(); }
        }

        #endregion

        #region constructor

        public BoreholeManagementViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            BoreholeModels = new ObservableCollection<BoreholeModel>();

            AddBoreholeCommand = new RelayCommand(AddBoreholeAction);
            RemoveBoreholeCommand = new RelayCommand(RemoveBoreholeAction);

            MessengerInstance.Register<BoreholeModel>(this, "Borehole Data Added", AddBoreholeToList);
        }

        #endregion

        #region commands

        public ICommand AddBoreholeCommand { get; }
        public ICommand RemoveBoreholeCommand { get; }

        #endregion

        #region methods

        private async void AddBoreholeAction()
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

                    switch (extension)
                    {
                        case ".xls":
                        case ".xlsx":
                        case ".xlsm":
                            MessengerInstance.Send(openFileDialog.FileName, FlyoutToggleEnum.ExcelBorehole);
                            MessengerInstance.Send(FlyoutToggleEnum.ExcelBorehole);
                            break;
                        case ".las":
                            MessengerInstance.Send(openFileDialog.FileName, FlyoutToggleEnum.LASBorehole);
                            MessengerInstance.Send(FlyoutToggleEnum.LASBorehole);
                            break;
                        case ".dat":
                        case ".path":
                        case ".pdat":
                        case ".prod":
                        case ".tops":
                            MessengerInstance.Send(openFileDialog.FileName, FlyoutToggleEnum.EVBorehole);
                            MessengerInstance.Send(FlyoutToggleEnum.EVBorehole);
                            break;
                    }
                }

                progressDialogController.CloseAsync();
            });
        }

        private async void RemoveBoreholeAction()
        {
            MessengerInstance.Send(SelectedBoreholeModel, "Delete BoreholeModel");
            BoreholeModels.Remove(SelectedBoreholeModel);
        }

        private void AddBoreholeToList(BoreholeModel boreholeModel)
        {
            Application.Current.Dispatcher?.InvokeAsync(() => { BoreholeModels.Add(boreholeModel); });
        }

        #endregion
    }
}
