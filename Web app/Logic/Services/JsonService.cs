using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Language;

namespace WebApp.Logic.Services
{
    public class JsonService : IJsonService
    {
        private IWebHostEnvironment _hostingEnvironment;
        private const string filesFolder = "JsonFiles";

        public JsonService(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public async Task<T> ReadAsync<T>(string fileName)
        {
            try
            {
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, filesFolder);

                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                var filePath = Path.Combine(path, fileName);

                string strJson = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(strJson);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async Task<bool> WriteAsync<T>(string fileName, T model)
        {
            try
            {
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, filesFolder);

                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                var filePath = Path.Combine(path, fileName);

                string strJson = JsonSerializer.Serialize<T>(model);

                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync(strJson);

                }
                return true;
            }
            catch { return false; }
        }

        public async Task<List<T>> ReadAllAsync<T>()
        {
            try
            {
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, filesFolder);

                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

                List<T> result = new List<T>();
                foreach (var file in Directory.EnumerateFiles(path, "*.json"))
                {
                    try
                    {
                        string strJson = await File.ReadAllTextAsync(file);
                        result.Add(JsonSerializer.Deserialize<T>(strJson));
                    }
                    catch { }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

    }
}
