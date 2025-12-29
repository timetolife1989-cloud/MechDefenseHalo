using Godot;
using GdUnit4;
using MechDefenseHalo.SaveSystem;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for LocalSaveHandler
    /// Tests file operations for save files
    /// </summary>
    [TestSuite]
    public class LocalSaveHandlerTests
    {
        private LocalSaveHandler _handler;
        private const string TEST_FILE_NAME = "test_save.dat";
        private const string TEST_BACKUP_NAME = "test_backup.dat";

        [Before]
        public void Setup()
        {
            _handler = new LocalSaveHandler();
            
            // Clean up any existing test files
            _handler.DeleteSave(TEST_FILE_NAME);
            _handler.DeleteSave(TEST_BACKUP_NAME);
        }

        [After]
        public void Teardown()
        {
            // Clean up test files
            _handler.DeleteSave(TEST_FILE_NAME);
            _handler.DeleteSave(TEST_BACKUP_NAME);
        }

        [TestCase]
        public void WriteSave_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            string testData = "Test save data";

            // Act
            bool result = _handler.WriteSave(TEST_FILE_NAME, testData);

            // Assert
            AssertThat(result).IsTrue();
        }

        [TestCase]
        public void ReadSave_WithExistingFile_ShouldReturnContent()
        {
            // Arrange
            string testData = "Test save content";
            _handler.WriteSave(TEST_FILE_NAME, testData);

            // Act
            string content = _handler.ReadSave(TEST_FILE_NAME);

            // Assert
            AssertThat(content).IsEqual(testData);
        }

        [TestCase]
        public void ReadSave_WithNonExistentFile_ShouldReturnNull()
        {
            // Act
            string content = _handler.ReadSave("nonexistent_file.dat");

            // Assert
            AssertThat(content).IsNull();
        }

        [TestCase]
        public void SaveExists_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            _handler.WriteSave(TEST_FILE_NAME, "test");

            // Act
            bool exists = _handler.SaveExists(TEST_FILE_NAME);

            // Assert
            AssertThat(exists).IsTrue();
        }

        [TestCase]
        public void SaveExists_WithNonExistentFile_ShouldReturnFalse()
        {
            // Act
            bool exists = _handler.SaveExists("nonexistent_file.dat");

            // Assert
            AssertThat(exists).IsFalse();
        }

        [TestCase]
        public void DeleteSave_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            _handler.WriteSave(TEST_FILE_NAME, "test");

            // Act
            bool deleted = _handler.DeleteSave(TEST_FILE_NAME);

            // Assert
            AssertThat(deleted).IsTrue();
            AssertThat(_handler.SaveExists(TEST_FILE_NAME)).IsFalse();
        }

        [TestCase]
        public void DeleteSave_WithNonExistentFile_ShouldReturnFalse()
        {
            // Act
            bool deleted = _handler.DeleteSave("nonexistent_file.dat");

            // Assert
            AssertThat(deleted).IsFalse();
        }

        [TestCase]
        public void CopySave_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            string testData = "Original save data";
            _handler.WriteSave(TEST_FILE_NAME, testData);

            // Act
            bool copied = _handler.CopySave(TEST_FILE_NAME, TEST_BACKUP_NAME);

            // Assert
            AssertThat(copied).IsTrue();
            AssertThat(_handler.SaveExists(TEST_BACKUP_NAME)).IsTrue();
            
            string backupContent = _handler.ReadSave(TEST_BACKUP_NAME);
            AssertThat(backupContent).IsEqual(testData);
        }

        [TestCase]
        public void CopySave_WithNonExistentFile_ShouldReturnFalse()
        {
            // Act
            bool copied = _handler.CopySave("nonexistent_file.dat", TEST_BACKUP_NAME);

            // Assert
            AssertThat(copied).IsFalse();
        }

        [TestCase]
        public void WriteSave_OverwriteExisting_ShouldUpdateContent()
        {
            // Arrange
            _handler.WriteSave(TEST_FILE_NAME, "Original content");

            // Act
            _handler.WriteSave(TEST_FILE_NAME, "Updated content");
            string content = _handler.ReadSave(TEST_FILE_NAME);

            // Assert
            AssertThat(content).IsEqual("Updated content");
        }

        [TestCase]
        public void WriteSave_WithLargeData_ShouldHandleCorrectly()
        {
            // Arrange
            string largeData = new string('X', 100000); // 100KB

            // Act
            bool written = _handler.WriteSave(TEST_FILE_NAME, largeData);
            string content = _handler.ReadSave(TEST_FILE_NAME);

            // Assert
            AssertThat(written).IsTrue();
            AssertThat(content).IsEqual(largeData);
            AssertThat(content.Length).IsEqual(100000);
        }
    }
}
