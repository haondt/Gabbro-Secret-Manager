using GabbroSecretManager.Domain.Api.Models;
using GabbroSecretManager.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GabbroSecretManager.UI.Shared.Extensions
{
    public static class FormFileExtensions
    {
        public static T DeserializeFromJson<T>(this IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonString = reader.ReadToEnd();
                var deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, ApiConstants.SerializerSettings)
                    ?? throw new UserException($"The JSON file could not be deserialized into type {typeof(T).Name}.");
                return deserializedObject;
            }
            catch (Exception ex) when (ex is not UserException)
            {
                throw new UserException($"Error reading or deserializing the JSON file: {ex.Message}", ex);
            }
        }
    }
}
