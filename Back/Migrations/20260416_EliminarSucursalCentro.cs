using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class EliminarSucursalCentro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar la sucursal "Central"
            migrationBuilder.Sql(
                @"DELETE FROM Sucursales WHERE NombreSucursal = 'Central';"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar la sucursal en caso de rollback
            migrationBuilder.Sql(
                @"INSERT INTO Sucursales (NombreSucursal, Dirección, Teléfono) 
                  VALUES ('Central', 'centro 123', '0000000000');"
            );
        }
    }
}
