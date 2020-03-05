
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        #endregion

        #region properties

        public ObservableCollection<BoreholeModel> BoreholeModels
        {
            get { return _boreholeModels; }
            set { _boreholeModels = value; RaisePropertyChanged(() => BoreholeModels); }
        }

        #endregion

        #region constructor

        public BoreholeManagementViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            BoreholeModels = new ObservableCollection<BoreholeModel>();

            AddBoreholeCommand = new RelayCommand(AddBoreholeAction);
        }

        #endregion

        #region commands

        public ICommand AddBoreholeCommand { get; }

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
                            MessengerInstance.Send(openFileDialog.FileName, "Excel File Selected");
                            break;
                        case ".las":
                            MessengerInstance.Send(openFileDialog.FileName, "LAS File Selected");
                            break;
                        case ".dat":
                        case ".path":
                        case ".pdat":
                        case ".prod":
                        case ".tops":
                            MessengerInstance.Send(openFileDialog.FileName, "EarthVision File Selected");
                            break;
                    }
                }
            });

            await progressDialogController.CloseAsync();
        }

        #endregion
    }
}
