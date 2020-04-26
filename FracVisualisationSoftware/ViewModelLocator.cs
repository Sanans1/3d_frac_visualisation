using System.Windows.Media.Animation;
using System.Windows.Threading;
using CommonServiceLocator;
using FracVisualisationSoftware.Services.Implementations.Data;
using FracVisualisationSoftware.Services.Interfaces.Data;
using FracVisualisationSoftware.ViewModels;
using FracVisualisationSoftware.ViewModels.Readers.EV;
using FracVisualisationSoftware.ViewModels.Readers.Excel;
using FracVisualisationSoftware.ViewModels.Readers.LAS;
using FracVisualisationSoftware.Views;
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
            SimpleIoc.Default.Register<ExcelDataReaderViewModel>();
            SimpleIoc.Default.Register<LASPathReaderViewModel>();
            SimpleIoc.Default.Register<LASStageReaderViewModel>();
            SimpleIoc.Default.Register<LASDataReaderViewModel>();
            SimpleIoc.Default.Register<EVPathReaderViewModel>();
            SimpleIoc.Default.Register<EVStageReaderViewModel>();
            SimpleIoc.Default.Register<EVDataReaderViewModel>();
            SimpleIoc.Default.Register<WellManagementViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<TimelineViewModel>();
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
        
        public ExcelDataReaderViewModel ExcelDataReader
        {
            get { return ServiceLocator.Current.GetInstance<ExcelDataReaderViewModel>(); }
        }

        public LASPathReaderViewModel LasPathReader
        {
            get { return ServiceLocator.Current.GetInstance<LASPathReaderViewModel>(); }
        }

        public LASStageReaderViewModel LasStageReader
        {
            get { return ServiceLocator.Current.GetInstance<LASStageReaderViewModel>(); }
        }        
        
        public LASDataReaderViewModel LasDataReader
        {
            get { return ServiceLocator.Current.GetInstance<LASDataReaderViewModel>(); }
        }

        public EVPathReaderViewModel EvPathReader
        {
            get { return ServiceLocator.Current.GetInstance<EVPathReaderViewModel>(); }
        }

        public EVStageReaderViewModel EvStageReader
        {
            get { return ServiceLocator.Current.GetInstance<EVStageReaderViewModel>(); }
        }        
        
        public EVDataReaderViewModel EvDataReader
        {
            get { return ServiceLocator.Current.GetInstance<EVDataReaderViewModel>(); }
        }

        public WellManagementViewModel WellManagement
        {
            get { return ServiceLocator.Current.GetInstance<WellManagementViewModel>(); }
        }

        public TimelineViewModel Timeline
        {
            get { return ServiceLocator.Current.GetInstance<TimelineViewModel>(); }
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }
    }
}