using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace ReservoirVisualisationProject.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        private static Timer _timer;

        private List<DateTime> _timestamps;
        private int _sliderPosition;
        private bool _isPlaying;

        public string LastTimestamp 
        {
            get
            {
                if (Timestamps.Any()) return Timestamps.Last().ToString();
                return "";
            }
        }

        public string CurrentTimestamp
        {
            get
            {
                if (Timestamps.Any()) return Timestamps[SliderPosition].ToString();
                return "";
            }
        }

        public bool HasTimestamps => Timestamps.Any();
        
        public double TimerInterval { get => _timer.Interval; set => _timer.Interval = value; }

        public List<DateTime> Timestamps
        {
            get => _timestamps;
            set
            {
                _timestamps = value; 
                RaisePropertyChanged();
                RaisePropertyChanged(() => LastTimestamp);
                RaisePropertyChanged(() => CurrentTimestamp);
                RaisePropertyChanged(() => SliderCap);
                RaisePropertyChanged(() => HasTimestamps);
            }
        }

        public int SliderCap => _timestamps.Count;

        public int SliderPosition
        {
            get => _sliderPosition;
            set
            {
                _sliderPosition = value; 
                MessengerInstance.Send(Timestamps[_sliderPosition], MessageTokenStrings.SelectDataByTimestamp);
                RaisePropertyChanged();
                RaisePropertyChanged(() => CurrentTimestamp);
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set 
            {
                _isPlaying = value;
                RaisePropertyChanged();
            }
        }

        public TimelineViewModel()
        {
            Timestamps = new List<DateTime>();
            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;

            PlayCommand = new RelayCommand(PlayAction, () => HasTimestamps);
            ForwardCommand = new RelayCommand(ForwardAction, () => HasTimestamps);
            BackwardCommand = new RelayCommand(BackwardAction, () => HasTimestamps);
            ScreenshotCommand = new RelayCommand<object>(ScreenshotAction);

            MessengerInstance.Register<List<DateTime>>(this, MessageTokenStrings.SetupTimeline, SetupTimeline);
            MessengerInstance.Register<HelixViewport3D>(this, "Send Viewport", CreateScreenshot);
        }

        public ICommand PlayCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand BackwardCommand { get; }
        public ICommand ScreenshotCommand { get; }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (SliderPosition < SliderCap) SliderPosition++;
            else
            {
                IsPlaying = false;
                _timer.Stop();
            }
        }

        private void PlayAction()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                _timer.Stop();
            }
            else if (!IsPlaying)
            {
                IsPlaying = true;
                _timer.Start();
            }
        }

        private void ForwardAction() => SliderPosition++;
        private void BackwardAction() => SliderPosition--;

        private void ScreenshotAction(object element)
        {
            MessengerInstance.Send(new NotificationMessage("Get Viewport"), "Get Viewport");
        }

        private void SetupTimeline(List<DateTime> timestamps)
        {
            Timestamps = timestamps;
        }

        private void CreateScreenshot(HelixViewport3D helixViewport3D)
        {
            SaveFileDialog openFileDialog = new SaveFileDialog {Filter = "PNG files|*.png*", Title = "Save as"};

            if (openFileDialog.ShowDialog() == true)
            {
                Size size = new Size(helixViewport3D.ActualWidth, helixViewport3D.ActualHeight);

                RenderTargetBitmap result =
                    new RenderTargetBitmap((int) size.Width, (int) size.Height, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual drawingvisual = new DrawingVisual();
                using (DrawingContext context = drawingvisual.RenderOpen())
                {
                    context.DrawRectangle(new VisualBrush(helixViewport3D), null, new Rect(new Point(), size));
                    context.Close();
                }

                result.Render(drawingvisual);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(result));

                using (FileStream file = File.OpenWrite($"{openFileDialog.FileName}.png"))
                {
                    encoder.Save(file);
                }
            }
        }
    }
}
