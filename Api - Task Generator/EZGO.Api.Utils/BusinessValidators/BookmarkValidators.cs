using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// BookmarkValidators; contains all validation methods for validating bookmarks.
    /// </summary>
    public static class BookmarkValidators
    {
        public const string MESSAGE_OBJECT_ID_IS_NOT_VALID = "Object Id is not valid";
        public const string MESSAGE_OBJECT_TYPE_IS_NOT_VALID = "Object type is not valid";
        public const string MESSAGE_BOOKMARK_TYPE_IS_NOT_VALID = "Bookmark type is not valid";
        public static bool BookmarkObjectIdIsValid(int bookmarkObjectId)
        {
            if (bookmarkObjectId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool BookmarkObjectTypeIsValid(int bookmarkObjectType)
        {
            if (bookmarkObjectType > 0)
            {
                return true;
            }

            return false;
        }

        public static bool BookmarkTypeIsValid(int bookmarkType)
        {
            if (bookmarkType > 0)
            {
                return true;
            }

            return false;
        }
    }
}
