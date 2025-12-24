using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IBookmarkManager
    {
        Task<Bookmark> GetBookmarkAsync(int companyId, Guid guid, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<Bookmark> CreateOrRetrieveBookmarkAsync(int companyId, int userId, ObjectTypeEnum objectType, int objectId, BookmarkTypeEnum bookmarkType);
        Task<bool> SetBookmarkActiveAsync(int companyId, int userId, Guid bookmarkGuid, bool isActive);
        Task<bool> CheckBookmarkAllowedAreas(Bookmark bookmark, int companyId, int userId);
        List<Exception> GetPossibleExceptions();
    }
}
