using System;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Platforms.iOS.Services;
using static System.Environment;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class FileService : IFileService
    {
        public void ClearInternalStorageFolder(string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        public Task<FileInfo[]> GetFilesFromAssetsAsync(string directoryName)
        {
            return Task.Run(() =>
            {
                FileInfo[] fileInfos = null;

                if (Directory.Exists(directoryName))
                {
                    string[] files = Directory.GetFiles(directoryName);

                    if (files != null && files.Any())
                        fileInfos = files.Select(item => new FileInfo(item)).OrderBy(item => item.Name).ToArray();
                }

                return fileInfos;
            });
        }

        public Task<byte[]> ReadFromAssetsAsBytesAsync(string filename)
        {
            return Task.Run(() =>
            {
                byte[] file = File.ReadAllBytes(filename);

                return file;
            });
        }

        public FileInfo[] GetFilesFromInternalStorage(string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            return GetFilesFromAssetsAsync(folder).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> ReadFromInternalStorageAsync(string filename, string directoryName)
        {
            string fileContents = null;
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            string filePath = Path.Combine(folder, filename);

            if (File.Exists(filePath))
                fileContents = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

            return fileContents;
        }

        public string ReadFromInternalStorage(string filename, string directoryName)
        {
            string fileContents = null;
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            string filePath = Path.Combine(folder, filename);

            if (File.Exists(filePath))
                fileContents = File.ReadAllText(filePath);

            return fileContents;
        }

        public string SaveFileToInternalStorage(byte[] file, string fileName, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, fileName);
            File.WriteAllBytes(filePath, file);

            return filePath;
        }

        public async Task<string> SaveFileToInternalStorageAsync(byte[] file, string fileName, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(filePath, file);

            return filePath;
        }

        public async Task<string> SaveFileToInternalStorageAsync(string fileContents, string fileName, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(SpecialFolder.Personal), directoryName);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, fileName);
            await File.WriteAllTextAsync(filePath, fileContents).ConfigureAwait(false);

            return filePath;
        }

        public async Task<Stream> ReadFromInternalStorageAsBytesAsync(string filename, string directoryName)
        {
            Stream fileContents = null;
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            string filePath = Path.Combine(folder, filename);

            if (File.Exists(filePath))
                fileContents = File.OpenRead(filePath);

            return fileContents;
        }

        public async Task<Stream> ReadFromInternalStorageAsStreamAsync(string filename, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            string filePath = Path.Combine(folder, filename);

            if (File.Exists(filePath))
            {
                Stream fileContents = new FileStream(filePath, FileMode.Open);

                if (fileContents != null)
                {
                    var memoryStream = new MemoryStream();
                    await fileContents.CopyToAsync(memoryStream);

                    fileContents.Close();
                    fileContents.Dispose();

                    return memoryStream;
                }
            }

            return null;
        }

        public bool CheckIfFileExists(string filename, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);

            string filePath = Path.Combine(folder, filename);

            return File.Exists(filePath);
        }

        public void DeleteFile(string filename, string directoryName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
            string filePath = Path.Combine(folder, filename);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public async Task IsFileReady(string filename, string directoryName)
        {
            await Task.Run(() =>
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
                string filePath = Path.Combine(folder, filename);

                if (!File.Exists(filePath))
                {
                    throw new IOException("File does not exist!");
                }

                var isReady = false;

                while (!isReady)
                {
                    // If the file can be opened for exclusive access it means that the file
                    // is no longer locked by another process.
                    try
                    {
                        using (FileStream inputStream =
                            File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                            isReady = inputStream.Length > 0;
                    }
                    catch (Exception e)
                    {
                        // Check if the exception is related to an IO error.
                        if (e.GetType() == typeof(IOException))
                        {
                            isReady = false;
                        }
                        else
                        {
                            // Rethrow the exception as it's not an exclusively-opened-exception.
                            throw;
                        }
                    }
                }
            });
        }

        // Not needed in ios case
        public string GetAssetsPath()
        {
            return "";
        }
    }
}

