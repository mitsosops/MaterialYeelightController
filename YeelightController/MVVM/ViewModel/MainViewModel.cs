using ImageMagick;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using ScreenCapturerNS;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YeelightController.Core;
using YeelightController.DependencyContainer;
using YeelightController.MVVM.View;
using YeelightController.ThemeManager;

namespace YeelightController.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private DevicesView _devicesView;

        private bool isCapturing = false;
        public BaseDeviceControllerView BaseDeviceControllerView { get; private set; }

        public DevicesView DevicesView
        {
            get { return _devicesView; }
            private set { _devicesView = value; }
        }

        public IBaseViewModel BaseViewModel { get; private set; }
        public IThemeController ThemeController { get; private set; }

        private RelayCommand _exitAppCommand;

        public SettingsView SettingsView { get; private set; }
        public ThemeManagerView ThemeManagerView { get; private set; }

        public RelayCommand ExitAppCommand
        {
            get { return _exitAppCommand; }
            set { _exitAppCommand = value; }
        }

        public RelayCommand ShowDialogCommand { get; private set; }
        public RelayCommand ToggleCaptureCommand { get; private set; }

        private string averageColorHex;

        public string AverageColorHex
        {
            get { return averageColorHex; }
            set
            {
                if (averageColorHex != value)
                {
                    averageColorHex = value;
                    OnPropertyChanged(nameof(AverageColorHex));
                }
            }
        }

        private float luminance;

        public float Luminance
        {
            get { return luminance; }
            set { luminance = value; OnPropertyChanged(nameof(Luminance)); }
        }

        private float saturation;

        public float Saturation
        {
            get { return saturation; }
            set { saturation = value; OnPropertyChanged(nameof(Saturation)); }
        }



        public MainViewModel()
        {
            InitMVVMContext();
            InitCommands();
        }

        private void InitCommands()
        {
            ExitAppCommand = new RelayCommand(async (o) =>
            {
                try
                {
                    if (Properties.Settings.Default.TurnOffDevicesOnExit)
                    {
                        await BaseViewModel.TurnAllDevicesState("off");
                    }
                    Properties.Settings.Default.Save();
                }
                catch (Exception) { }
                finally
                {
                    Environment.Exit(0);
                }
            });
            ShowDialogCommand = new RelayCommand(async (view) =>
            {
                if (view.ToString() == "settings")
                    await DialogHost.Show(SettingsView);
                else if (view.ToString() == "theme")
                    await DialogHost.Show(ThemeManagerView);

            });
            ToggleCaptureCommand = new RelayCommand(async _ =>
            {
                if (isCapturing)
                {
                    await StopCaptureAsync();
                }
                else
                {
                    await StartCaptureAsync();
                }
            });
        }
        private void InitMVVMContext()
        {
            BaseViewModel = ContainerConfig.ServiceProvider.GetService<IBaseViewModel>();
            ThemeController = ContainerConfig.ServiceProvider.GetService<IThemeController>();
            DevicesView = new DevicesView();
            DevicesView.Loaded += DevicesView_Loaded;

            var deviceControllerView = new DeviceControllerView();
            var dcVM = new DeviceControllerViewModel(BaseViewModel, ThemeController);
            deviceControllerView.DataContext = dcVM;

            var colorFlowView = new ColorFlowView();
            var cfVM = new ColorFlowViewModel(BaseViewModel);
            colorFlowView.DataContext = cfVM;

            SettingsView = new SettingsView();
            var sVM = new SettingsViewModel(ThemeController);
            SettingsView.DataContext = sVM;

            ThemeManagerView = new ThemeManagerView(ThemeController);

            BaseDeviceControllerView = new BaseDeviceControllerView();
            var baseDeviceControllerViewModel = new BaseDeviceControllerViewModel(BaseViewModel, deviceControllerView, colorFlowView);
            BaseDeviceControllerView.DataContext = baseDeviceControllerViewModel;

        }


        private async void DevicesView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var dVM = new DevicesViewModel(BaseViewModel, ThemeController);
            try
            {
                await dVM.DiscoverDevicesAsync();

                if (Properties.Settings.Default.TurnOnDevicesOnStartup)
                {
                    BaseViewModel.TurnAllDevicesState("on");
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                DevicesView.DataContext = dVM;
            }

        }

        private async Task StartCaptureAsync()
        {
            ScreenCapturer.OnScreenUpdated += ScreenCapturer_OnScreenUpdated;
            ScreenCapturer.StartCapture();
            await Task.CompletedTask;
        }

        long frameCount = 0;
        DateTime lastUpdate;
        private void ScreenCapturer_OnScreenUpdated(object? sender, OnScreenUpdatedEventArgs e)
        {

            frameCount++;
            if (DateTime.Now - lastUpdate >= TimeSpan.FromSeconds(1))
            {
                System.Diagnostics.Debug.WriteLine($"FPS: {frameCount}");
                frameCount = 0;
                lastUpdate = DateTime.Now;
            }
            //avg - 

            IMagickColor<ushort> color;
            using (var image = e.Bitmap.ToMagickImage())
            {
                // Get average color
                //image.ColorSpace = ColorSpace.HSL;
                //image.Modulate(new Percentage(200), new Percentage(200), new Percentage(0));
                image.Resize(1, 1);
                //color = image.GetPixels().First().ToColor();

                var pixel = image.GetPixels().First();
                //pixel.SetChannel(2, (ushort)(Quantum.Max / 2));
                //pixel.SetChannel(3, (ushort)(Quantum.Max / 2));
                color = pixel.ToColor();

                var r = lerp(color.R, 0, Quantum.Max, 0, 255);
                var g = lerp(color.G, 0, Quantum.Max, 0, 255);
                var b = lerp(color.B, 0, Quantum.Max, 0, 255);
                //var colorBytes = color.ToByteArray();
                System.Diagnostics.Debug.WriteLine($"Avg color: {color.ToHexString()}");
                var hslColor = FromRGB(r, g, b);


                //hslColor.L = lerp(hslColor.L, 0, 255, 100, 180);
                //hslColor.S = lerp(hslColor.S, 0, 255, 100, 180);

                hslColor.S = Saturation;
                hslColor.L = Luminance;


                var rgbColor = ToRGB(hslColor.H, hslColor.S, hslColor.L);

                AverageColorHex = MagickColor.FromRgb(rgbColor.R, rgbColor.G, rgbColor.B).ToHexString();
                //AverageColorHex = color.ToHexString();
            }

        }

        public Color ToRGB(float h, float s, float l)
        {
            byte r, g, b;
            if (s == 0)
            {
                r = (byte)Math.Round(l * 255d);
                g = (byte)Math.Round(l * 255d);
                b = (byte)Math.Round(l * 255d);
            }
            else
            {
                double t1, t2;
                double th = h / 6.0d;

                if (l < 0.5d)
                {
                    t2 = l * (1d + s);
                }
                else
                {
                    t2 = (l + s) - (l * s);
                }
                t1 = 2d * l - t2;

                double tr, tg, tb;
                tr = th + (1.0d / 3.0d);
                tg = th;
                tb = th - (1.0d / 3.0d);

                tr = ColorCalc(tr, t1, t2);
                tg = ColorCalc(tg, t1, t2);
                tb = ColorCalc(tb, t1, t2);
                r = (byte)Math.Round(tr * 255d);
                g = (byte)Math.Round(tg * 255d);
                b = (byte)Math.Round(tb * 255d);
            }
            return Color.FromArgb(r, g, b);
        }
        private static double ColorCalc(double c, double t1, double t2)
        {

            if (c < 0) c += 1d;
            if (c > 1) c -= 1d;
            if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
            if (2.0d * c < 1.0d) return t2;
            if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
            return t1;
        }

        public float lerp(float x, float x0, float x1, float y0, float y1)
        {
            if ((x1 - x0) == 0)
            {
                return (y0 + y1) / 2;
            }
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

        public struct HSLColor
        {
            public HSLColor(float h, float s, float l)
            {
                H = h;
                S = s;
                L = l;
            }

            public float H { get; set; }
            public float S { get; set; }
            public float L { get; set; }
        }

        public HSLColor FromRGB(float R, float G, float B)
        {
            float _R = (R / 255f);
            float _G = (G / 255f);
            float _B = (B / 255f);

            float _Min = Math.Min(Math.Min(_R, _G), _B);
            float _Max = Math.Max(Math.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (float)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4f + (_R - _G) / _Delta;
                }
            }

            return new HSLColor(H, S, L);
        }

        private async Task StopCaptureAsync()
        {
            ScreenCapturer.StopCapture();
            ScreenCapturer.OnScreenUpdated -= ScreenCapturer_OnScreenUpdated;
            await Task.CompletedTask;
        }

    }
}
