using System;
using System.Text;

namespace MechDefenseHalo.SaveSystem
{
    /// <summary>
    /// Provides encryption/decryption for save files
    /// Uses simple XOR encryption (can be upgraded to AES for production)
    /// 
    /// NOTE: The encryption key is currently hard-coded for simplicity.
    /// For production use, consider:
    /// 1. Generating the key dynamically based on device/user ID
    /// 2. Storing the key in a secure keychain/keystore
    /// 3. Using environment-specific keys
    /// 4. Upgrading to AES encryption with proper key management
    /// </summary>
    public static class SaveEncryption
    {
        // TODO: Move to secure storage or generate dynamically
        private const string ENCRYPTION_KEY = "MechDefenseHalo2025SecretKey!@#";
        
        /// <summary>
        /// Encrypt plain text using XOR encryption
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>Base64 encoded encrypted text</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[i % key.Length];
            }
            
            return Convert.ToBase64String(bytes);
        }
        
        /// <summary>
        /// Decrypt encrypted text using XOR encryption
        /// </summary>
        /// <param name="encrypted">Base64 encoded encrypted text</param>
        /// <returns>Decrypted plain text</returns>
        public static string Decrypt(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
                return encrypted;

            byte[] bytes = Convert.FromBase64String(encrypted);
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[i % key.Length];
            }
            
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
