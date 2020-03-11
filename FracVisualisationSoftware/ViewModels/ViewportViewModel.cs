using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FracVisualisationSoftware.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;

namespace FracVisualisationSoftware.ViewModels
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
        private HelixViewport3D _camera;
        private ObservableCollection<Visual3D> _viewportObjects;

        #endregion fields

        #region properties

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

        public ObservableCollection<Visual3D> ViewportObjects
        {
            get => _viewportObjects;
            set => _viewportObjects = value;
        }

        public HelixViewport3D Camera
        {
            get => _camera;
            set { _camera = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ViewportViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            ViewportObjects = new ObservableCollection<Visual3D>();

            _tubePath = new Point3DCollection();

            TubeDiameter = 5;

            HelixViewport3DLoadedCommand = new RelayCommand<HelixViewport3D>(HelixViewport3DLoadedAction); //TODO Get reference to the ViewPort so we can manipulate the camera

            MessengerInstance.Register<BoreholeModel>(this, "Borehole Data Added",AddBoreholeMessageCallback);
        }

        #endregion constructor

        #region commands

        public ICommand HelixViewport3DLoadedCommand { get; }

        #endregion commands 

        #region methods

        #region command methods

        private void HelixViewport3DLoadedAction(HelixViewport3D viewPort)
        {
            //Camera.
        }

        #endregion command methods

        #region event methods

        private async void AddBoreholeMessageCallback(BoreholeModel boreholeModel)
        {
            if (boreholeModel == null || !boreholeModel.TubePath.Any())
                return;

            _tubePath.Dispatcher?.Invoke(() =>
            {
                _tubePath = new Point3DCollection(boreholeModel.TubePath);
            });

            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                ViewportObjects.Clear();

                ViewportObjects.Add(new SunLight());

                ViewportObjects.Add(new TubeVisual3D { AddCaps = true, Path = _tubePath, Diameter = TubeDiameter });
            });
        }

        #endregion event methods

        #endregion methods

    }
}
