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

            DateTime dateTimeToWrite = value;

            // Los DateTime que vienen de la BD son Unspecified, pero YA están en hora de Argentina
            // (porque se guardaron con GetArgentinaTime())
            // Solo hacer conversión si es UTC explícito
            if (value.Kind == DateTimeKind.Utc)
            {
                var argentinaZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                dateTimeToWrite = TimeZoneInfo.ConvertTimeFromUtc(value, argentinaZone);
            }
            // Si es Unspecified o Local, devolver como está

            // Serializar en formato ISO 8601
            // Ej: "2026-04-27T22:18:00"
            string isoString = dateTimeToWrite.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            writer.WriteStringValue(isoString);
        }
    }
}
