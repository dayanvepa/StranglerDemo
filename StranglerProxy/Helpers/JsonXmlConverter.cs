using System;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Globalization;

namespace StranglerProxy.Helpers
{
    /// <summary>
    /// Proporciona métodos utilitarios para convertir cadenas JSON en representaciones XML.
    /// </summary>
    /// <remarks>
    /// El convertidor maneja una variedad de tipos de valores JSON (objetos, arreglos, cadenas, números,
    /// booleanos, nulos) y produce un documento XML correctamente formateado con declaraciones de espacio
    /// de nombres para <c>xsi</c> y <c>xsd</c>. También intenta "singularizar" los nombres de elementos cuando
    /// se encuentran arreglos y limpia los nombres de entrada para que sean válidos como nombres de elementos XML.
    /// 
    /// <para>Ejemplo de uso:</para>
    /// <code>
    /// var json = "{ 'data': [ { 'id': 1, 'name': 'Widget' } ] }";
    /// string xml = JsonXmlConverter.ConvertJsonToXml(json, "Products");
    /// </code>
    /// </remarks>
    public static class JsonXmlConverter
    {
        /// <summary>
        /// Convierte un documento JSON en una cadena XML.
        /// </summary>
        /// <param name="json">El texto JSON a convertir. Debe ser un objeto o arreglo JSON válido.</param>
        /// <param name="rootElementName">
        /// El nombre que se usará para el elemento raíz XML. Por defecto es "Items". El método intentará
        /// singularizar este nombre al generar elementos hijos a partir de arreglos.
        /// </param>
        /// <returns>Una cadena XML que representa el JSON de entrada. El documento incluye la declaración XML.</returns>
        public static string ConvertJsonToXml(string json, string rootElementName = "Items")
        {
            using var doc = JsonDocument.Parse(json);
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                // UTF-16 (Unicode) para que la declaración salga como encoding="utf-16"
                Encoding = Encoding.Unicode,
                OmitXmlDeclaration = false
            };

            using var writer = XmlWriter.Create(sb, settings);
            writer.WriteStartDocument();

            // Escribir la raíz 
            writer.WriteStartElement(rootElementName);
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

            // Singular para cada item 
            string singularName = ToPascalCase(GetSingular(rootElementName));

            // Si el JSON contiene la propiedad "data" y es un array, procesamos solo ese array
            if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    WriteElement(writer, item, singularName);
                }
            }
            else
            {
                // Fallback: procesar todo el objeto
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    WriteElement(writer, prop.Value, ToPascalCase(SanitizeXmlName(prop.Name)));
                }
            }

            writer.WriteEndElement(); 
            writer.WriteEndDocument();
            writer.Flush();

            return sb.ToString();
        }

        /// <summary>
        /// Escribe recursivamente un <see cref="JsonElement"/> en un <see cref="XmlWriter"/> usando el
        /// nombre de elemento especificado.
        /// </summary>
        /// <param name="writer">El escritor XML al que se escribe la salida.</param>
        /// <param name="element">El elemento JSON que se está convirtiendo.</param>
        /// <param name="name">El nombre del elemento XML que se usará para este elemento JSON.</param>
        private static void WriteElement(XmlWriter writer, JsonElement element, string name)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartElement(name);
                    foreach (var prop in element.EnumerateObject())
                    {
                        var childName = ToPascalCase(SanitizeXmlName(prop.Name));
                        WriteElement(writer, prop.Value, childName);
                    }
                    writer.WriteEndElement();
                    break;

                case JsonValueKind.Array:
                    // Si se encuentra un array dentro de un objeto, usamos el nombre plural para envolver
                    writer.WriteStartElement(name); // por ej. Order -> Orders (si tiene arrays anidados)
                    string singular = ToPascalCase(GetSingular(name));
                    foreach (var item in element.EnumerateArray())
                    {
                        WriteElement(writer, item, singular);
                    }
                    writer.WriteEndElement();
                    break;

                case JsonValueKind.String:
                    writer.WriteElementString(name, element.GetString());
                    break;

                case JsonValueKind.Number:
                    // Mantener formato númerico 
                    writer.WriteElementString(name, element.GetRawText());
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    writer.WriteElementString(name, element.GetBoolean().ToString());
                    break;

                case JsonValueKind.Null:
                    writer.WriteStartElement(name);
                    writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                    writer.WriteEndElement();
                    break;

                default:
                    writer.WriteElementString(name, element.GetRawText());
                    break;
            }
        }
        

        /// <summary>
        /// Limpia una cadena de entrada para que sea un nombre válido de elemento XML. Los caracteres inválidos
        /// se reemplazan con guiones bajos y, si comienza con un dígito, se antepone un guión bajo.
        /// </summary>
        /// <param name="name">El nombre candidato proveniente de una propiedad JSON o clave de arreglo.</param>
        /// <returns>Una cadena que es segura para usar como nombre de elemento XML.</returns>
        private static string SanitizeXmlName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Property";
            var sb = new StringBuilder();
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.')
                    sb.Append(ch);
                else
                    sb.Append('_');
            }
            if (sb.Length == 0) return "Property";
            if (char.IsDigit(sb[0])) sb.Insert(0, '_');
            return sb.ToString();
        }

        /// <summary>
        /// Convierte una cadena a PascalCase, manejando entradas en camelCase, snake_case y kebab-case.
        /// </summary>
        /// <param name="input">La cadena a transformar.</param>
        /// <returns>Una versión en PascalCase de la entrada, o la cadena original si es nula o está vacía.</returns>
        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Normalizar separadores a espacio para TitleCase
            var separators = new[] { '_', '-', ' ' };
            foreach (var sep in separators)
            {
                input = input.Replace(sep, ' ');
            }

            // Si está en camelCase (ej. itemsCount) simplemente capitalizamos primera letra
            if (!input.Contains(' '))
            {
                return char.ToUpperInvariant(input[0]) + (input.Length > 1 ? input.Substring(1) : string.Empty);
            }

            // Para words separadas, usar TextInfo para TitleCase y luego quitar espacios
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var titled = textInfo.ToTitleCase(input.ToLowerInvariant()).Replace(" ", string.Empty);
            return titled;
        }

        /// <summary>
        /// Intenta generar la forma singular de un sustantivo plural utilizando reglas simples.
        /// </summary>
        /// <param name="plural">Un término en plural como "Items" o "Categories".</param>
        /// <returns>Una versión singular, regresando a "Item" si la entrada está vacía.</returns>
        private static string GetSingular(string plural)
        {
            if (string.IsNullOrEmpty(plural)) return "Item";

            // Reglas básicas y comunes
            if (plural.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                return plural.Substring(0, plural.Length - 3) + "y"; // categories -> category

            if (plural.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                // cases like "Orders" -> "Order", "Statuses" -> "Status"
                return plural.Substring(0, plural.Length - 2);
            }

            if (plural.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return plural.Substring(0, plural.Length - 1);

            return plural;
        }
    }
}