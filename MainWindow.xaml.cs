using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace windows_full_widget
{
    public partial class MainWindow : Window
    {
        private PerformanceCounter cpuCounter;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCpuCounter();
            StartCpuUsageUpdate();
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            float cpuUsage = cpuCounter.NextValue();
            CpuUsageText.Text = $"CPU: {cpuUsage:F1}%";
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