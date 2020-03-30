using System.Windows.Threading;
using CommonServiceLocator;
using FracVisualisationSoftware.Services.Implementations.Data;
using FracVisualisationSoftware.Services.Interfaces.Data;
using FracVisualisationSoftware.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;

namespace FracVisualisationSoftware
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //Services
            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();
            SimpleIoc.Default.Register<IDataService, DataService>();

            //ViewModels
            SimpleIoc.Default.Register<ViewportViewModel>();
            SimpleIoc.Default.Register<ExcelPathReaderViewModel>();
            SimpleIoc.Default.Register<ExcelStageReaderViewModel>();
            SimpleIoc.Default.Register<LASPathReaderViewModel>();
            SimpleIoc.Default.Register<LASStageReaderViewModel>();
            SimpleIoc.Default.Register<EVPathReaderViewModel>();
            SimpleIoc.Default.Register<EVStageReaderViewModel>();
            SimpleIoc.Default.Register<BoreholeManagementViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public ViewportViewModel Viewport
        {
            get { return ServiceLocator.Current.GetInstance<ViewportViewModel>(); }
        }

        public ExcelPathReaderViewModel ExcelPathReader
        {
            get { return ServiceLocator.Current.GetInstance<ExcelPathReaderViewModel>(); }
        }

        public ExcelStageReaderViewModel ExcelStageReader
        {
            get { return ServiceLocator.Current.GetInstance<ExcelStageReaderViewModel>(); }
        }

        public LASPathReaderViewModel LasPathReader
        {
            get { return ServiceLocator.Current.GetInstance<LASPathReaderViewModel>(); }
        }

        public LASStageReaderViewModel LasStageReader
        {
            get { return ServiceLocator.Current.GetInstance<LASStageReaderViewModel>(); }
        }

        public EVPathReaderViewModel EvPathReader
        {
            get { return ServiceLocator.Current.GetInstance<EVPathReaderViewModel>(); }
        }

        public EVStageReaderViewModel EvStageReader
        {
            get { return ServiceLocator.Current.GetInstance<EVStageReaderViewModel>(); }
        }

        public BoreholeManagementViewModel BoreholeManagement
        {
            get { return ServiceLocator.Current.GetInstance<BoreholeManagementViewModel>(); }
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }
    }
}