using System.Text.Json;

namespace Foraria.DTOs
{
    public static class JsonElementExtensions
    {
        public static string? GetPropertyOrDefault(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.GetString();
            }
            return null;
        }

        public static string? GetNested(this JsonElement element, params string[] propertyNames)
        {
            JsonElement currentElement = element;

            foreach (var propertyName in propertyNames)
            {
                if (currentElement.TryGetProperty(propertyName, out JsonElement property))
                {
                    currentElement = property;
                }
                else
                {
                    return null;
                }
            }

            return currentElement.GetString();
        }

    }


}

