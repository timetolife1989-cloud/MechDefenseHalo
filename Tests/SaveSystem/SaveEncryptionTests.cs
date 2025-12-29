using Godot;
using GdUnit4;
using MechDefenseHalo.SaveSystem;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for SaveEncryption
    /// Tests encryption and decryption functionality
    /// </summary>
    [TestSuite]
    public class SaveEncryptionTests
    {
        [TestCase]
        public void Encrypt_WithPlainText_ShouldReturnEncryptedString()
        {
            // Arrange
            string plainText = "Hello, World!";

            // Act
            string encrypted = SaveEncryption.Encrypt(plainText);

            // Assert
            AssertThat(encrypted).IsNotNull();
            AssertThat(encrypted).IsNotEmpty();
            AssertThat(encrypted).IsNotEqual(plainText);
        }

        [TestCase]
        public void Decrypt_WithEncryptedText_ShouldReturnOriginalString()
        {
            // Arrange
            string original = "Test save data 12345";
            string encrypted = SaveEncryption.Encrypt(original);

            // Act
            string decrypted = SaveEncryption.Decrypt(encrypted);

            // Assert
            AssertThat(decrypted).IsEqual(original);
        }

        [TestCase]
        public void EncryptDecrypt_WithComplexJson_ShouldPreserveData()
        {
            // Arrange
            string jsonData = @"{""Level"":10,""Health"":100.5,""Name"":""Player""}";

            // Act
            string encrypted = SaveEncryption.Encrypt(jsonData);
            string decrypted = SaveEncryption.Decrypt(encrypted);

            // Assert
            AssertThat(decrypted).IsEqual(jsonData);
        }

        [TestCase]
        public void Encrypt_WithEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            string empty = "";

            // Act
            string encrypted = SaveEncryption.Encrypt(empty);

            // Assert
            AssertThat(encrypted).IsEqual(empty);
        }

        [TestCase]
        public void Encrypt_WithNull_ShouldReturnNull()
        {
            // Act
            string encrypted = SaveEncryption.Encrypt(null);

            // Assert
            AssertThat(encrypted).IsNull();
        }

        [TestCase]
        public void Decrypt_WithEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            string empty = "";

            // Act
            string decrypted = SaveEncryption.Decrypt(empty);

            // Assert
            AssertThat(decrypted).IsEqual(empty);
        }

        [TestCase]
        public void Decrypt_WithNull_ShouldReturnNull()
        {
            // Act
            string decrypted = SaveEncryption.Decrypt(null);

            // Assert
            AssertThat(decrypted).IsNull();
        }

        [TestCase]
        public void EncryptDecrypt_WithUnicodeCharacters_ShouldPreserveData()
        {
            // Arrange
            string unicode = "Hello 世界! 🎮 Привет";

            // Act
            string encrypted = SaveEncryption.Encrypt(unicode);
            string decrypted = SaveEncryption.Decrypt(encrypted);

            // Assert
            AssertThat(decrypted).IsEqual(unicode);
        }

        [TestCase]
        public void Encrypt_SameInputMultipleTimes_ShouldProduceSameOutput()
        {
            // Arrange
            string plainText = "Consistent encryption test";

            // Act
            string encrypted1 = SaveEncryption.Encrypt(plainText);
            string encrypted2 = SaveEncryption.Encrypt(plainText);

            // Assert
            AssertThat(encrypted1).IsEqual(encrypted2);
        }

        [TestCase]
        public void Encrypt_WithLargeText_ShouldHandleCorrectly()
        {
            // Arrange
            string largeText = new string('A', 10000); // 10KB of text

            // Act
            string encrypted = SaveEncryption.Encrypt(largeText);
            string decrypted = SaveEncryption.Decrypt(encrypted);

            // Assert
            AssertThat(decrypted).IsEqual(largeText);
            AssertThat(decrypted.Length).IsEqual(10000);
        }
    }
}
