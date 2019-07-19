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
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
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

        private Point3DCollection _tubePath;

        private double _tubeLength;
        private double _tubeDiameter;

        #endregion fields

        #region properties

        public Point3DCollection TubePath
        {
            get { return _tubePath; }
            set { _tubePath = value; RaisePropertyChanged(() => TubePath); }
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

            ViewportObjects = new ObservableCollection<Visual3D>();
            TubePath = new Point3DCollection();

            GenerateModelsCommand = new RelayCommand(GenerateModelsAction, CanGenerateModelsAction);
            //HelixViewport3DLoaded = new R(); //TODO Get reference to the ViewPort so we can manipulate the camera

            MessengerInstance.Register<GenericMessage<List<Point3D>>>(this, TubePathMessageCallback);
        }

        #endregion constructor

        #region commands

        public ICommand GenerateModelsCommand { get; }
        public ICommand HelixViewport3DLoaded { get; }

        #endregion commands 

        #region methods

        #region command methods

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

        #region event methods

        private void TubePathMessageCallback(GenericMessage<List<Point3D>> message)
        {
            if (message.Content == null)
                return;

            TubePath.Dispatcher.Invoke(() => 
            { 
                TubePath = new Point3DCollection(message.Content);
            });
        }

        #endregion event methods

        #endregion methods

    }
}
