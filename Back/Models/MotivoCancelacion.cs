namespace Back.Models
{
    public class MotivoCancelacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // Ejemplo: "Falta de stock", "Cliente no responde"
        public bool Activo { get; set; } = true;
    }
}