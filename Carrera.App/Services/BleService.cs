using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Carrera.App.Models;

namespace Carrera.App.Services
{
    public class BleDeviceInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class BleService
    {
        public ObservableCollection<BleDeviceInfo> Devices { get; } = new();
        public ObservableCollection<BleCharacteristicInfo> Characteristics { get; } = new();
        public event Action<string>? OnLog;

        private BluetoothLEDevice? _device;

        public async Task ScanAsync()
        {
            Devices.Clear();
            var selector = BluetoothLEDevice.GetDeviceSelector();
            var list = await DeviceInformation.FindAllAsync(selector);
            foreach (var di in list)
            {
                var name = di.Name ?? string.Empty;
                if (name.Contains("Carrera", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("AppConnect", StringComparison.OrdinalIgnoreCase))
                {
                    Devices.Add(new BleDeviceInfo { Id = di.Id, Name = name });
                }
            }
            OnLog?.Invoke($"Scan fertig – {Devices.Count} Kandidaten gefunden.");
        }

        public async Task<bool> ConnectAsync(BleDeviceInfo? chosen)
        {
            if (chosen == null) return false;
            _device = await BluetoothLEDevice.FromIdAsync(chosen.Id);
            if (_device == null) { OnLog?.Invoke("Verbindung fehlgeschlagen."); return false; }
            OnLog?.Invoke($"Verbunden mit {_device.Name}");

            var svcResult = await _device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (svcResult.Status != GattCommunicationStatus.Success)
            {
                OnLog?.Invoke($"Services nicht lesbar: {svcResult.Status}");
                return false;
            }

            Characteristics.Clear();
            foreach (var svc in svcResult.Services)
            {
                var charsResult = await svc.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                if (charsResult.Status != GattCommunicationStatus.Success) continue;

                foreach (var ch in charsResult.Characteristics)
                {
                    var info = new BleCharacteristicInfo
                    {
                        ServiceUuid = svc.Uuid.ToString(),
                        CharacteristicUuid = ch.Uuid.ToString(),
                        Properties = ch.CharacteristicProperties.ToString(),
                    };
                    Characteristics.Add(info);

                    if (ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                    {
                        var status = await ch.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        OnLog?.Invoke($"Notify aktiviert: {ch.Uuid} – {status}");

                        ch.ValueChanged += (s, e) =>
                        {
                            var reader = DataReader.FromBuffer(e.CharacteristicValue);
                            byte[] data = new byte[e.CharacteristicValue.Length];
                            reader.ReadBytes(data);
                            var hex = BitConverter.ToString(data);

                            var row = Characteristics.FirstOrDefault(r => r.CharacteristicUuid == s.Uuid.ToString());
                            if (row != null) row.LastValueHex = hex;

                            OnLog?.Invoke($"Notify {s.Uuid}: {hex}");
                        };
                    }
                }
            }
            return true;
        }

        public async Task<bool> WriteCommandAsync(Guid characteristic, byte[] payload)
        {
            if (_device == null) return false;
            var svcResult = await _device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            foreach (var svc in svcResult.Services)
            {
                var charsResult = await svc.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                foreach (var ch in charsResult.Characteristics)
                {
                    if (ch.Uuid == characteristic && ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                    {
                        var writer = new DataWriter();
                        writer.WriteBytes(payload);
                        var status = await ch.WriteValueAsync(writer.DetachBuffer());
                        OnLog?.Invoke($"Write {ch.Uuid}: {status}");
                        return status == GattCommunicationStatus.Success;
                    }
                }
            }
            OnLog?.Invoke("Write-Char nicht gefunden/oder nicht beschreibbar.");
            return false;
        }
    }
}
