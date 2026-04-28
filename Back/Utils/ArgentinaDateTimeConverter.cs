using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Back.Utils
{
    /// <summary>
    /// Converter personalizado para serializar DateTime en zona horaria de Argentina (UTC-3)
    /// </summary>
    public class ArgentinaDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Leer el string del JSON
            string? dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            // Intentar parsear como ISO 8601
            if (DateTime.TryParse(dateString, out var dateTime))
            {
                return dateTime;
            }

            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Si es DateTime.MinValue o similar, devolver null
            if (value == DateTime.MinValue)
            {
                writer.WriteNullValue();
                return;
            }

            // Convertir a zona horaria de Argentina (UTC-3)
            var argentinaZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            
            // Si el DateTime no tiene Kind especificado, asumir que es Local
            DateTime dateTimeToConvert = value;
            if (value.Kind == DateTimeKind.Unspecified)
            {
                // Tratarlo como Local y convertir a Argentina
                dateTimeToConvert = TimeZoneInfo.ConvertTime(value, argentinaZone);
            }
            else if (value.Kind == DateTimeKind.Utc)
            {
                // Si es UTC, convertir a Argentina
                dateTimeToConvert = TimeZoneInfo.ConvertTimeFromUtc(value, argentinaZone);
            }
            else
            {
                // Si es Local, convertir a Argentina
                dateTimeToConvert = TimeZoneInfo.ConvertTime(value, argentinaZone);
            }

            // Serializar en formato ISO 8601 sin información de zona (ya es hora de Argentina)
            // Ej: "2026-04-27T22:09:00"
            string isoString = dateTimeToConvert.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            writer.WriteStringValue(isoString);
        }
    }
}
