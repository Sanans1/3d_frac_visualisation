using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using Timer = System.Timers.Timer;

namespace FracVisualisationSoftware.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        private static Timer _timer;

        private List<DateTime> _timestamps;
        private int _sliderPosition;
        private bool _isPlaying;

        public double TimerTicks { get => _timer.Interval; set => _timer.Interval = value; }

        public List<DateTime> Timestamps
        {
            get => _timestamps;
            set
            {
                _timestamps = value; 
                RaisePropertyChanged();
                RaisePropertyChanged(() => SliderCap);
            }
        }

        public int SliderCap => _timestamps.Count - 1;

        public int SliderPosition
        {
            get => _sliderPosition;
            set
            {
                _sliderPosition = value; 
                MessengerInstance.Send(Timestamps[_sliderPosition], MessageTokenStrings.SelectDataByTimestamp);
                RaisePropertyChanged();
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
            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;

            PlayCommand = new RelayCommand(PlayAction);
            ScreenshotCommand = new RelayCommand<object>(ScreenshotAction);

            MessengerInstance.Register<List<DateTime>>(this, MessageTokenStrings.SetupTimeline, SetupTimeline);
            MessengerInstance.Register<HelixViewport3D>(this, "Send Viewport", CreateScreenshot);
        }

        public ICommand PlayCommand { get; }
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

        private void SetupTimeline(List<DateTime> timestamps)
        {
            Timestamps = timestamps;
        }

        private void ScreenshotAction(object element)
        {
            MessengerInstance.Send(new NotificationMessage("Get Viewport"), "Get Viewport");
        }

        private void CreateScreenshot(HelixViewport3D helixViewport3D)
        {
            Size size = new Size(helixViewport3D.ActualWidth, helixViewport3D.ActualHeight);

            //if (size.IsEmpty)
            //    return null;

            RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(helixViewport3D), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(result));

            using (FileStream file = File.OpenWrite(@"C:\Test\Test.png"))
            {
                encoder.Save(file);
            }
        }
    }
}
