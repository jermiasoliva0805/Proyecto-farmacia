namespace Back.Models
{
    public class Localidad
    {
        public int IDLocalidad { get; set; }
        public string Barrio { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;

        // Relación: Una localidad tiene muchos clientes
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}