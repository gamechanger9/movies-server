using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task InsertPaginationParametersInHeaders<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            double amountOfRecords = await queryable.CountAsync();
            httpContext.Response.Headers.Append("total-records-count", amountOfRecords.ToString());
        }
    }
}
