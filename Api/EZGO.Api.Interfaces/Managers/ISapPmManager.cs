using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ISapPmManager
    {
        Task<List<SapPmLocation>> SearchLocationsAsync(int companyId, string searchText, int? functionalLocationId);
        Task<List<SapPmLocation>> GetLocationChildren(int companyId, int? functionalLocationId);
        Task<List<SapPmLocationImportData>> GetLocationImportDataForCsv(Stream reader);
        List<Exception> GetPossibleExceptions();
        Task<SapPmNotificationOptions> GetSapPmNotificationOptionsAsync(int companyId, int userId, int? areaId = null);
        Task<SapPmLocation> GetSapPmFunctionalLocationAsync(int companyId, int functionalLocationId);
        Task<DateTime> GetLastChangeDateForFunctionalLocationsByCompanyId(int companyId);
        Task<int> SendFunctionalLocationsToDatabase(int companyId, string sapFunctionalLocations);
        Task<int> ImportFunctionalLocationsInDatabase(string sapFunctionalLocations, string companyIds, bool recalculateTreeStructure = true);
        Task<int> RegenerateFunctionalLocationsTreeStructure(int companyId);
        Task<bool> SetSapPmCredentialsAsync(int userId, int companyId, int? holdingId, Login sapPmCredentials);
        Task<List<SapPmNotificationMessage>> GetSapPmNotificationMessagesAsync(int? companyId);
        Task<List<AreaFunctionalLocationRelation>> GetAreaFunctionalLocationRelationsAsync(int companyId);
        Task<List<AreaFunctionalLocationRelation>> GetAreaFunctionalLocationRelationsAsync(int companyId, int areaId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> AddAreaSapPmLocationRelationAsync(int companyId, int areaId, int locationId, int userId);
        Task<int> RemoveAreaSapPmLocationRelationAsync(int id, int areaId, int locationId, int companyId, int userId);
        Task<List<SapPmLocation>> GetFunctionalLocationsByLocationIdsAsync(int companyId, List<int> locationIds);
        Task<int> ClearFunctionalLocationsInDatabase(string companyIds);
        Task<List<SapPmNotificationFailure>> GetSapPmNotificationMessageFailures();
    }

}