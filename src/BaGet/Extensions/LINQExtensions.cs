using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Extensions
{
    internal static class LINQExtensions
    {
        /// <summary>
        /// LINQ extension in order to apply the Where fluent API also for the async case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static async Task<IAsyncEnumerable<T>> WhereAsync<T>(this IQueryable<T> source, Func<T, bool> filter)
            where T : class
        {
            return await Task.FromResult(source.Where(filter).ToAsyncEnumerable());
        }

        /// <summary>
        /// LINQ extension in order to apply the include fluent API to include more than one navigation properties at the same time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        internal static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, params string[] includes)
            where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query;
        }

        /// <summary>
        /// LINQ extension in order to apply the include fluent API to include more than one navigation properties at the same time for the async case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        internal static async Task<IQueryable<T>> IncludeMultipleAsync<T>(this IQueryable<T> query, params string[] includes)
            where T : class
        {
            if (includes != null)
            {
                query = await Task.FromResult(includes.Aggregate(query, (current, include) => current.Include(include)));
            }

            return query;
        }
    }
}