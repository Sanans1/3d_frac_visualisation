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

        private bool _isExcelPathReaderFlyoutOpen;
        private bool _isExcelStageReaderFlyoutOpen;
        private bool _isLASPathReaderFlyoutOpen;
        private bool _isLASStageReaderFlyoutOpen;
        private bool _isEVPathReaderFlyoutOpen;
        private bool _isEVStageReaderFlyoutOpen;

        #endregion

        #region properties

        public bool IsExcelPathReaderFlyoutOpen
        {
            get => _isExcelPathReaderFlyoutOpen;
            set { _isExcelPathReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }        
        
        public bool IsExcelStageReaderFlyoutOpen
        {
            get => _isExcelStageReaderFlyoutOpen;
            set { _isExcelStageReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }

        public bool IsLASPathReaderFlyoutOpen
        {
            get => _isLASPathReaderFlyoutOpen;
            set { _isLASPathReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }        
        
        public bool IsLASStageReaderFlyoutOpen
        {
            get => _isLASStageReaderFlyoutOpen;
            set { _isLASStageReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }

        public bool IsEVPathReaderFlyoutOpen
        {
            get => _isEVPathReaderFlyoutOpen;
            set { _isEVPathReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }        
        
        public bool IsEVStageReaderFlyoutOpen
        {
            get => _isEVStageReaderFlyoutOpen;
            set { _isEVStageReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }

        #endregion

        public MainViewModel()
        {
            MessengerInstance.Register<(FileTypeEnum, WellDataTypeEnum)>(this, FlyoutToggle);
        }

        private void FlyoutToggle(ValueTuple<FileTypeEnum, WellDataTypeEnum> fileTuple)
        {
            IsExcelPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.Excel, WellDataTypeEnum.Path);
            IsExcelStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.Excel, WellDataTypeEnum.Stages);
            IsLASPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.LAS, WellDataTypeEnum.Path);
            IsLASStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.LAS, WellDataTypeEnum.Stages);
            IsEVPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.EV, WellDataTypeEnum.Path);
            IsEVStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.EV, WellDataTypeEnum.Stages);
        }
    }
}
