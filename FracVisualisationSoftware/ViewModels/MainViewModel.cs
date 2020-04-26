using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FracVisualisationSoftware.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace FracVisualisationSoftware.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region fields

        private bool _isExcelPathReaderFlyoutOpen;
        private bool _isExcelStageReaderFlyoutOpen;
        private bool _isExcelDataReaderFlyoutOpen;
        private bool _isLASPathReaderFlyoutOpen;
        private bool _isLASStageReaderFlyoutOpen;
        private bool _isLASDataReaderFlyoutOpen;
        private bool _isEVPathReaderFlyoutOpen;
        private bool _isEVStageReaderFlyoutOpen;
        private bool _isEVDataReaderFlyoutOpen;

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
        
        public bool IsExcelDataReaderFlyoutOpen
        {
            get => _isExcelDataReaderFlyoutOpen;
            set { _isExcelDataReaderFlyoutOpen = value; RaisePropertyChanged(); }
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
        
        public bool IsLASDataReaderFlyoutOpen
        {
            get => _isLASDataReaderFlyoutOpen;
            set { _isLASDataReaderFlyoutOpen = value; RaisePropertyChanged(); }
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
        
        public bool IsEVDataReaderFlyoutOpen
        {
            get => _isEVDataReaderFlyoutOpen;
            set { _isEVDataReaderFlyoutOpen = value; RaisePropertyChanged(); }
        }

        #endregion

        public MainViewModel()
        {
            MessengerInstance.Register<(FileTypeEnum, WellDataTypeEnum)>(this, ToggleReaderFlyouts);
        }

        private void ToggleReaderFlyouts(ValueTuple<FileTypeEnum, WellDataTypeEnum> fileTuple)
        {
            IsExcelPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.Excel, WellDataTypeEnum.Path);
            IsExcelStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.Excel, WellDataTypeEnum.Stages);
            IsExcelDataReaderFlyoutOpen = fileTuple == (FileTypeEnum.Excel, WellDataTypeEnum.Data);
            IsLASPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.LAS, WellDataTypeEnum.Path);
            IsLASStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.LAS, WellDataTypeEnum.Stages);
            IsLASDataReaderFlyoutOpen = fileTuple == (FileTypeEnum.LAS, WellDataTypeEnum.Data);
            IsEVPathReaderFlyoutOpen = fileTuple == (FileTypeEnum.EV, WellDataTypeEnum.Path);
            IsEVStageReaderFlyoutOpen = fileTuple == (FileTypeEnum.EV, WellDataTypeEnum.Stages);
            IsEVDataReaderFlyoutOpen = fileTuple == (FileTypeEnum.EV, WellDataTypeEnum.Data);
        }
    }
}
