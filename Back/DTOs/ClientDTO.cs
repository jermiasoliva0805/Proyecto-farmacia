namespace Back.DTOs
{
    public class ClientDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public int IDLocalidad { get; set; }
        public string NombreLocalidad { get; set; } // Para mostrar en listas
    }
}