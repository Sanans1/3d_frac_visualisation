using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FracVisualisationSoftware.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FracVisualisationSoftware.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region fields

        private bool _isExcelFlyoutOpen;
        private bool _isLasFlyoutOpen;
        private bool _isEvFlyoutOpen;

        #endregion

        #region properties

        public bool IsExcelFlyoutOpen
        {
            get => _isExcelFlyoutOpen;
            set { _isExcelFlyoutOpen = value; RaisePropertyChanged(); }
        }

        public bool IsLASFlyoutOpen
        {
            get => _isLasFlyoutOpen;
            set { _isLasFlyoutOpen = value; RaisePropertyChanged(); }
        }

        public bool IsEVFlyoutOpen
        {
            get => _isEvFlyoutOpen;
            set { _isEvFlyoutOpen = value; RaisePropertyChanged(); }
        }

        #endregion

        public MainViewModel()
        {
            MessengerInstance.Register<FlyoutToggleEnum>(this, FlyoutToggle);
        }

        private void FlyoutToggle(FlyoutToggleEnum flyoutToggleEnum)
        {
            IsExcelFlyoutOpen = flyoutToggleEnum == FlyoutToggleEnum.ExcelBorehole && !IsExcelFlyoutOpen;
            IsLASFlyoutOpen = flyoutToggleEnum == FlyoutToggleEnum.LASBorehole && !IsLASFlyoutOpen;
            IsEVFlyoutOpen = flyoutToggleEnum == FlyoutToggleEnum.EVBorehole && !IsEVFlyoutOpen;
        }
    }
}
