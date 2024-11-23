using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;

namespace windows_full_widget
{
    public partial class MainWindow : Window
    {
        private PerformanceCounter? cpuCounter;
        private DispatcherTimer? timer;
        private float gpuUsage;
        private float gpuTemperature;
        private Computer computer;

        public MainWindow()
        {
            InitializeComponent();
            computer = new Computer();
            InitializeCpuCounter();
            StartCpuUsageUpdate();
            StartGpuUsageUpdate();
        }

        private void InitializeCpuCounter()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        private void StartCpuUsageUpdate()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            float cpuUsage = cpuCounter?.NextValue() ?? 0.0f;
            CpuUsageText.Text = $"CPU: {cpuUsage:F1}%";
            GpuUsageText.Text = $"GPU: {gpuUsage:F1}%";
            GpuTempText.Text = $"GPU Temp: {gpuTemperature:F1}°C";
        }

        private void StartGpuUsageUpdate()
        {
            computer = new Computer
            {
                IsGpuEnabled = true
            };
            computer.Open();

            Task.Run(() =>
            {
                while (true)
                {
                    foreach (var hardwareItem in computer.Hardware)
                    {
                        if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                        {
                            hardwareItem.Update();
                            foreach (var sensor in hardwareItem.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                                {
                                    gpuUsage = sensor.Value.GetValueOrDefault();
                                }
                                if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                                {
                                    gpuTemperature = sensor.Value.GetValueOrDefault();
                                }
                            }
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        GpuUsageText.Text = $"GPU: {gpuUsage:F1}%";
                        GpuTempText.Text = $"GPU Temp: {gpuTemperature:F1}°C";
                    });

                    Thread.Sleep(1000);
                }
            });
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}