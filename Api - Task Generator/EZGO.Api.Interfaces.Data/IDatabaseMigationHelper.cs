using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    public interface IDatabaseMigationHelper
    {
        Task<List<string>> RetrieveMergedScript(string resourcePart);
        Task<bool> RunMigrations(string migrationKey);
        Task<bool> RunMigration(string migrationKey, string specificMigration);
        Task<List<string>> RetrieveMigrations(string migrationKey);
        Task<List<string>> RetrieveMigration(string migrationKey, string specificMigration);
    }
}
