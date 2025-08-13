using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Classes
{
    public class Pagination
    {
        private readonly int _pageSize;
        private int _currentPage = 1;

        public Pagination(int pageSize)
        {
            _pageSize = pageSize;
        }

        public void IncrementPage()
        {
            _currentPage++;
        }

        private int GetItemsToSkip => (_currentPage - 1) * _pageSize;

        public List<T> GetPagedList<T>(List<T> list)
        {
            var result = list.Skip(GetItemsToSkip).Take(_pageSize).ToList();
            return result;
        }
    }
}
