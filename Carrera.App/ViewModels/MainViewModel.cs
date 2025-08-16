using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Carrera.App.Helpers;
using Carrera.App.Models;
using Carrera.App.Services;

namespace Carrera.App.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<string> Log { get; } = new();
        public ObservableCollection<BleCharacteristicInfo> Characteristics => _ble.Characteristics;
        public ObservableCollection<BleDeviceInfo> Devices => _ble.Devices;

        private readonly BleService _ble = new();
        private BleDeviceInfo? _selectedDevice;
        public BleDeviceInfo? SelectedDevice
        {
            get => _selectedDevice;
            set { _selectedDevice = value; OnPropertyChanged(); }
        }

        private string _statusText = "Bereit";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private int _countdown = 0;
        public string CountdownDisplay => _countdown > 0 ? new string('●', _countdown) : "–";

        private bool _safetyCar;
        public bool SafetyCar
        {
            get => _safetyCar;
            set { _safetyCar = value; OnPropertyChanged(); }
        }

        private bool _ghostCar;
        public bool GhostCar
        {
            get => _ghostCar;
            set { _ghostCar = value; OnPropertyChanged(); }
        }

        // Commands
        public RelayCommand ScanCommand { get; }
        public RelayCommand ConnectCommand { get; }
        public RelayCommand StartCountdownCommand { get; }
        public RelayCommand RaceStartCommand { get; }
        public RelayCommand RaceStopCommand { get; }
        public RelayCommand ShowTelemetryCommand { get; }

        public MainViewModel()
        {
            _ble.OnLog += s => Application.Current.Dispatcher.Invoke(() => Log.Add(s));
            ScanCommand = new RelayCommand(async () => await ScanAsync());
            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            StartCountdownCommand = new RelayCommand(async () => await CountdownAsync());
            RaceStartCommand = new RelayCommand(async () => await RaceStartAsync());
            RaceStopCommand = new RelayCommand(async () => await RaceStopAsync());
            ShowTelemetryCommand = new RelayCommand(() => ShowTelemetry());
        }

        private async Task ScanAsync()
        {
            StatusText = "Scanne nach AppConnect…";
            await _ble.ScanAsync();
            StatusText = $"Scan fertig ({Devices.Count})";
        }

        private async Task ConnectAsync()
        {
            StatusText = "Verbinde…";
            var ok = await _ble.ConnectAsync(SelectedDevice);
            StatusText = ok ? "Verbunden" : "Nicht verbunden";
        }

        private async Task CountdownAsync()
        {
            for (int i = 1; i <= 5; i++)
            {
                _countdown = i; OnPropertyChanged(nameof(CountdownDisplay));
                Log.Add($"Countdown: {i}");
                await Task.Delay(800);
            }
            _countdown = 0; OnPropertyChanged(nameof(CountdownDisplay));
            Log.Add("Start!");
        }

        private async Task RaceStartAsync()
        {
            Log.Add("[CMD] Rennen START – (Befehl an CU folgt, sobald UUIDs feststehen)");
            await Task.CompletedTask;
        }

        private async Task RaceStopAsync()
        {
            Log.Add("[CMD] Rennen STOP – (Befehl an CU folgt, sobald UUIDs feststehen)");
            await Task.CompletedTask;
        }

        private void ShowTelemetry()
        {
            Log.Add("[UI] Telemetry-Ansicht öffnen – (hier später Erweiterung für Beamer/2. Monitor)");
        }
    }
}
