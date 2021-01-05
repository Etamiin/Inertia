using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    /// <summary>
    /// Default storage class interface
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Save the current storage to the specified folder path using the specified file name
        /// </summary>
        /// <param name="folderPath">Folder path where to save the file</param>
        /// <param name="fileName">Target file name</param>
        void Save(string folderPath, string fileName);
        /// <summary>
        /// Save asynchronously the current storage to the specified folder path using the specified file name
        /// </summary>
        /// <param name="folderPath">Folder path where to save the file</param>
        /// <param name="fileName">Target file name</param>
        void SaveAsync(string folderPath, string fileName);
        /// <summary>
        /// Load the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        void Load(string filePath);
        /// <summary>
        /// Load asynchronously the target file in the current storage
        /// </summary>
        /// <param name="filePath">File path to load</param>
        void LoadAsync(string filePath);
        /// <summary>
        /// Load the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        void Load(byte[] data);
        /// <summary>
        /// Load asynchronously the target data in the current storage
        /// </summary>
        /// <param name="data">Data to load</param>
        void LoadAsync(byte[] data);
    }
}
