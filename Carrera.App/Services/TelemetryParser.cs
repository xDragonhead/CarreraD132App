using System;
using System.Text;

namespace Carrera.App.Services
{
    public class TelemetryData
    {
        public int CarId { get; set; }
        public int Fuel { get; set; }
        public int Laps { get; set; }
        public int Position { get; set; }
    }

    public class TelemetryParser
    {
        public event Action<TelemetryData>? OnTelemetry;

        // Wird von BleService aufgerufen, wenn Notify-Daten kommen
        public void OnBleNotify(Guid uuid, byte[] data)
        {
            // Hier nur ein sehr einfacher Platzhalter:
            // Später muss das echte Carrera-Format dekodiert werden!
            if (data.Length >= 4)
            {
                var td = new TelemetryData
                {
                    CarId = data[0],
                    Fuel = data[1],
                    Laps = data[2],
                    Position = data[3]
                };
                OnTelemetry?.Invoke(td);
            }
        }
    }
}

