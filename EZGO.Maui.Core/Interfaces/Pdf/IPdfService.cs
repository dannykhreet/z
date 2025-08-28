using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Pdf
{
    public interface IPdfService : IDisposable
    {
        Task<Stream> GetPfdAsync(string uri);
    }
}
