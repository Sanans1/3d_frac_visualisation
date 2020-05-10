using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using ReservoirVisualisationProject.Enums;
using ReservoirVisualisationProject.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using MahApps.Metro.Controls.Dialogs;
using OfficeOpenXml.Packaging.Ionic.Zip;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using DiffuseMaterial = System.Windows.Media.Media3D.DiffuseMaterial;

namespace ReservoirVisualisationProject.ViewModels
{
    public class ViewportViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields

        private double _tubeDiameter;
        private ObservableCollection<Visual3D> _viewportObjects;

        private Point3D _cameraPosition;

        private HelixViewport3D _viewport;

        #endregion fields

        #region properties

        public double TubeDiameter
        {
            get => _tubeDiameter;
            set { _tubeDiameter = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<Visual3D> ViewportObjects
        {
            get => _viewportObjects;
            set { _viewportObjects = value; RaisePropertyChanged(); }
        }

        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set { _cameraPosition = value; RaisePropertyChanged(); }
        }

        public HelixViewport3D Viewport
        {
            get => _viewport;
            set { _viewport = value; RaisePropertyChanged(); }
        }

        #endregion properties

        #region constructor

        public ViewportViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            ViewportObjects = new ObservableCollection<Visual3D>();

            TubeDiameter = 50;

            HelixViewport3DLoadedCommand = new RelayCommand<HelixViewport3D>(HelixViewport3DLoadedAction);

            MessengerInstance.Register<WellModel>(this, MessageTokenStrings.AddPathToViewport, AddPathMessageCallback);
            MessengerInstance.Register< (WellDataTypeEnum, int)>(this, MessageTokenStrings.RemoveWellFromViewport, tuple => DeleteMessageCallback(tuple.Item1, tuple.Item2));
            MessengerInstance.Register<List<RenderDataModel>>(this, MessageTokenStrings.AddDataToViewport, AddDataMessageCallback);
            MessengerInstance.Register<NotificationMessage>(this, "Get Viewport", GetViewportCallback);
        }

        #endregion constructor

        #region commands

        public ICommand HelixViewport3DLoadedCommand { get; }

        #endregion commands 

        #region methods

        private void HelixViewport3DLoadedAction(HelixViewport3D viewPort)
        {
            Viewport = viewPort;
        }

        private Point3D InvertPoint3D(Point3D point3D)
        {
            return point3D.Multiply(-1);
        }

        #region event methods

        private void AddPathMessageCallback(WellModel wellModel)
        {
            if (wellModel == null || !wellModel.Path.Any())
                return;

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                //Reset the lighting
                ViewportObjects.Remove(ViewportObjects.SingleOrDefault(sunLight => sunLight.GetName() == "Light"));

                SunLight light = new SunLight();

                light.SetName("Light");

                ViewportObjects.Add(light);

                //Add the new path
                Point3DCollection invertedPath = new Point3DCollection();

                foreach (Point3D point3D in wellModel.Path)
                {
                    invertedPath.Add(InvertPoint3D(point3D));
                }

                TubeVisual3D tube = new TubeVisual3D {AddCaps = true, Path = invertedPath, Diameter = TubeDiameter, Fill = Brushes.Blue };

                tube.SetName($"{wellModel.ID}-well");

                ViewportObjects.Add(tube);
            });

            CameraPosition = wellModel.Path.First();
        }

        private void AddDataMessageCallback(List<RenderDataModel> renderDataModels)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                //Add each data point
                foreach (RenderDataModel renderDataModel in renderDataModels)
                {
                    Point3DCollection point3DCollection = new Point3DCollection();

                    //Invert the point so it is placed correctly in the viewport.
                    Point3D invertedPoint3D = InvertPoint3D(renderDataModel.Position);

                    point3DCollection.Add(invertedPoint3D);

                    Point3D valuePoint3D = invertedPoint3D;

                    valuePoint3D.Z += (renderDataModel.Value * 100);

                    point3DCollection.Add(valuePoint3D);

                    TubeVisual3D tube = new TubeVisual3D { AddCaps = true, Path = point3DCollection, Diameter = TubeDiameter, Fill = Brushes.Blue };

                    tube.SetName($"{renderDataModel.Name}-model");

                    ViewportObjects.Remove(ViewportObjects.SingleOrDefault(visual3D => visual3D.GetName().Contains($"{renderDataModel.Name}-model")));

                    ViewportObjects.Add(tube);

                    //Create a point to place text for each stage to identify it
                    Point3D stageTextPoint3D = invertedPoint3D;

                    stageTextPoint3D.Z -= 100;

                    BillboardTextVisual3D stageText = new BillboardTextVisual3D { Position = stageTextPoint3D, Text = $"{renderDataModel.Stage}", Foreground = Brushes.White, Padding = new Thickness(5), MaterialType = MaterialType.Emissive, Background = Brushes.Black };

                    stageText.SetName($"{renderDataModel.Name}-stage");

                    ViewportObjects.Remove(ViewportObjects.SingleOrDefault(visual3D => visual3D.GetName().Contains($"{renderDataModel.Name}-stage")));

                    ViewportObjects.Add(stageText);

                    //Create a point to place the value of each stage
                    Point3D valueTextPoint3D = valuePoint3D;

                    valueTextPoint3D.Z += 100;

                    BillboardTextVisual3D valueText = new BillboardTextVisual3D { Position = valueTextPoint3D, Text = $"{renderDataModel.DisplayValue}", Foreground = Brushes.White, Padding = new Thickness(5), MaterialType = MaterialType.Emissive, Background = Brushes.Black };

                    valueText.SetName($"{renderDataModel.Name}-value");

                    ViewportObjects.Remove(ViewportObjects.SingleOrDefault(visual3D => visual3D.GetName().Contains($"{renderDataModel.Name}-value")));

                    ViewportObjects.Add(valueText);
                }
            });
        }

        private void DeleteMessageCallback(WellDataTypeEnum wellDataType, int wellID)
        {
            switch (wellDataType)
            {
                case WellDataTypeEnum.Path:
                    foreach (Visual3D visual3D in ViewportObjects.Where(visual3D => visual3D.GetName().Contains($"{wellID}-")))
                    {
                        ViewportObjects.Remove(visual3D);
                    }
                    break;                
                case WellDataTypeEnum.Stages:
                case WellDataTypeEnum.Data:
                    foreach (Visual3D visual3D in ViewportObjects.Where(visual3D => visual3D.GetName().Contains($"{wellID}-") && !visual3D.GetName().Contains("well")))
                    {
                        ViewportObjects.Remove(visual3D);
                    }
                    break;
            }
        }

        private void GetViewportCallback(NotificationMessage _)
        {
            MessengerInstance.Send(Viewport, "Send Viewport");
        }

        #endregion event methods

        #endregion methods

    }
}
