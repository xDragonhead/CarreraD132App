namespace Carrera.App.Models
{
    public class DriverProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Car { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({Team}, {Car})";
        }
    }
}
