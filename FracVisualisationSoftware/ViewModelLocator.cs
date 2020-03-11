using System.Windows.Threading;
using CommonServiceLocator;
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

            //ViewModels
            SimpleIoc.Default.Register<ViewportViewModel>();
            SimpleIoc.Default.Register<ExcelReaderViewModel>();
            SimpleIoc.Default.Register<BoreholeManagementViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public ViewportViewModel Viewport
        {
            get { return ServiceLocator.Current.GetInstance<ViewportViewModel>(); }
        }

        public ExcelReaderViewModel ExcelEditor
        {
            get { return ServiceLocator.Current.GetInstance<ExcelReaderViewModel>(); }
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