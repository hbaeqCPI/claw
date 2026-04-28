using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LawPortal.Core.Helpers
{
    /// <summary>
    /// Helper functions for column encryption
    /// </summary>
    public static class EncryptionHelper
    {
        public static string? Key { get; private set; }

        public static void SetColumnEncryption(this IConfiguration configuration)
        {
            Key = configuration["EncryptionKey"];

            if (string.IsNullOrEmpty(Key))
                Key = GetKey();
        }

        public static string? Encrypt(string? dataToEncrypt)
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(dataToEncrypt)) 
                return dataToEncrypt;

            try
            {
                return dataToEncrypt.Encrypt(Key);
            }
            catch (Exception)
            {
                return dataToEncrypt;
            }
        }

        public static string? Decrypt(string? dataToDecrypt)
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(dataToDecrypt)) 
                return dataToDecrypt;

            try
            {
                return dataToDecrypt.Decrypt(Key);
            }
            catch (Exception)
            {
                return dataToDecrypt;
            }
        }

        public static void UseEncryption(this ModelBuilder modelBuilder)
        {
            //only use encryption if encryption key is not empty
            if (string.IsNullOrEmpty(Key))
                return;

            var converter = new EncryptionConverter();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) && !IsDiscriminator(property))
                    {
                        var attributes = property.PropertyInfo?.GetCustomAttributes(typeof(EncryptedAttribute), false);
                        if (attributes != null && attributes.Any())
                        {
                            property.SetValueConverter(converter);
                        }
                    }
                }
            }
        }

        private static bool IsDiscriminator(IMutableProperty property)
        {
            return property.Name == "Discriminator" || property.PropertyInfo == null;
        }

        private static string? GetKey()
        {
            try
            {
                var key = "";
                var jsonPath = "Resources/encryption.json";
                var jsonData = "";

                if (File.Exists(jsonPath))
                    jsonData = File.ReadAllText(jsonPath);

                // Get key from file
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonData);
                    if (jsonObject != null)
                        key = jsonObject["EncryptionKey"]?.ToString();
                }

                // Create key and save to file
                if (string.IsNullOrEmpty(key))
                {
                    key = GenerateKey();
                    var jsonString = $"{{ \"EncryptionKey\": \"{key}\" }}";
                    File.WriteAllText(jsonPath, jsonString);
                }

                return key;
            }
            catch 
            {
                return string.Empty;
            }
        }

        private static string GenerateKey()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var keyLength = 32;
            var keyChars = new char[keyLength];

            for (int i = 0; i < keyLength; i++)
            {
                keyChars[i] = characters[random.Next(characters.Length)];
            }

            return new string(keyChars);
        }
    }
}
