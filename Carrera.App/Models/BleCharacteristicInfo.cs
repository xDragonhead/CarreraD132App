namespace Carrera.App.Models
{
    public class BleCharacteristicInfo
    {
        public string ServiceUuid { get; set; } = string.Empty;
        public string CharacteristicUuid { get; set; } = string.Empty;
        public string Properties { get; set; } = string.Empty;
        public string LastValueHex { get; set; } = string.Empty;
    }
}
