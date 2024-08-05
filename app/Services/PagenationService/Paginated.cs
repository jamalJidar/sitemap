
using app.Models;
using System.Linq.Dynamic.Core;

namespace SiteMap.Services.PagenationService
{
    public static  class Paginated<T> where T : class
    {
        public static PagedResult<T> GetPagedData<T>(IQueryable<T> query, int page, int pageSize)
        {
            var result = new PagedResult<T>
            {   CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            var skip = (page - 1) * pageSize;
            result.Queryable = query.Skip(skip).Take(pageSize);
            return result;
        }
        public static PagedResult<T> GetPagedData(IQueryable<T> query, int page, int pageSize)
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            var skip = (page - 1) * pageSize;
            result.Queryable = query.Skip(skip).Take(pageSize);
            return result;
        }
    }
}
