using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class EliminarSucursalNorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar la sucursal "Farmacia General Paz Norte"
            migrationBuilder.Sql(
                @"DELETE FROM Sucursales WHERE NombreSucursal = 'Farmacia General Paz Norte';"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar la sucursal en caso de rollback
            migrationBuilder.Sql(
                @"INSERT INTO Sucursales (NombreSucursal, Dirección, Teléfono) 
                  VALUES ('Farmacia General Paz Norte', 'Av. General Paz 500', '3514445577');"
            );
        }
    }
}
