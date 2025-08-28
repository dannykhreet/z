using System.IO;
using System.Threading.Tasks;
using static System.Environment;

namespace EZGO.Maui.Core.Interfaces.File
{
    public interface IFileService
    {
        void ClearInternalStorageFolder(string directoryName);

        Task<FileInfo[]> GetFilesFromAssetsAsync(string directoryName);

        Task<byte[]> ReadFromAssetsAsBytesAsync(string filename);

        FileInfo[] GetFilesFromInternalStorage(string directoryName);

        Task<string> ReadFromInternalStorageAsync(string filename, string directoryName);

        string ReadFromInternalStorage(string filename, string directoryName);

        string SaveFileToInternalStorage(byte[] file, string fileName, string directoryName);

        Task<string> SaveFileToInternalStorageAsync(string fileContents, string fileName, string directoryName);

        Task<Stream> ReadFromInternalStorageAsBytesAsync(string filename, string directoryName);

        Task<Stream> ReadFromInternalStorageAsStreamAsync(string filename, string directoryName);

        bool CheckIfFileExists(string filename, string directoryName);

        Task IsFileReady(string filename, string directoryName);

        Task<string> SaveFileToInternalStorageAsync(byte[] file, string fileName, string directoryName);

        void DeleteFile(string filename, string directoryName);
        void DeleteFile(string path);

        string GetAssetsPath();
    }
}
