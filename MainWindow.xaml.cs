using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace windows_full_widget
{
    public partial class MainWindow : Window
    {
        private PerformanceCounter? cpuCounter;
        private DispatcherTimer? timer;
        private float gpuUsage;

        public MainWindow()
        {
            InitializeComponent();
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
        }

        private void StartGpuUsageUpdate()
{
    Task.Run(() =>
    {
        while (true)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine"))
                {
                    var gpuLoad = 0.0f;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        gpuLoad += Convert.ToSingle(obj["UtilizationPercentage"]);
                    }

                    gpuUsage = gpuLoad;
                    
                    Dispatcher.Invoke(() =>
                    {
                        GpuUsageText.Text = $"GPU: {gpuUsage:F1}%";
                    });
                }
            }
            catch (Exception)
            {
                gpuUsage = 0.0f;
            }

            Thread.Sleep(1000); // Update every second
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