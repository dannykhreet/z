using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IJsonService
    {
        Task<T> ReadAsync<T>(string fileName);
        Task<List<T>> ReadAllAsync<T>();
        Task<bool> WriteAsync<T>(string fileName, T model);
    }
}
