using Godot;
using System;

namespace MechDefenseHalo.SaveSystem
{
    /// <summary>
    /// Handles local file operations for save files
    /// </summary>
    public class LocalSaveHandler
    {
        private string _saveDirectory;
        
        public LocalSaveHandler()
        {
            string userDataDir = OS.GetUserDataDir();
            _saveDirectory = System.IO.Path.Combine(userDataDir, "saves");
            
            if (!DirAccess.DirExistsAbsolute(_saveDirectory))
            {
                DirAccess.MakeDirRecursiveAbsolute(_saveDirectory);
                GD.Print($"Created save directory: {_saveDirectory}");
            }
        }
        
        /// <summary>
        /// Write save data to file
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <param name="content">Content to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteSave(string fileName, string content)
        {
            try
            {
                string filePath = System.IO.Path.Combine(_saveDirectory, fileName);
                
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open file for writing: {FileAccess.GetOpenError()}");
                    return false;
                }
                
                file.StoreString(content);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error writing save file: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Read save data from file
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>File content or null if failed</returns>
        public string ReadSave(string fileName)
        {
            try
            {
                string filePath = System.IO.Path.Combine(_saveDirectory, fileName);
                
                if (!FileAccess.FileExists(filePath))
                {
                    return null;
                }
                
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open file for reading: {FileAccess.GetOpenError()}");
                    return null;
                }
                
                return file.GetAsText();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error reading save file: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if save file exists
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if file exists</returns>
        public bool SaveExists(string fileName)
        {
            return FileAccess.FileExists(System.IO.Path.Combine(_saveDirectory, fileName));
        }
        
        /// <summary>
        /// Delete a save file
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if deletion was successful</returns>
        public bool DeleteSave(string fileName)
        {
            try
            {
                string filePath = System.IO.Path.Combine(_saveDirectory, fileName);
                
                if (FileAccess.FileExists(filePath))
                {
                    DirAccess.RemoveAbsolute(filePath);
                    GD.Print($"Deleted save file: {fileName}");
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error deleting save file: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Copy a save file (useful for backups)
        /// </summary>
        /// <param name="sourceFileName">Source file name</param>
        /// <param name="destFileName">Destination file name</param>
        /// <returns>True if copy was successful</returns>
        public bool CopySave(string sourceFileName, string destFileName)
        {
            string content = ReadSave(sourceFileName);
            if (content == null)
            {
                return false;
            }
            
            return WriteSave(destFileName, content);
        }
    }
}
