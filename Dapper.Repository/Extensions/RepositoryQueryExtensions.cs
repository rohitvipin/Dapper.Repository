using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper.Repository.Repositories.Interfaces;

namespace Dapper.Repository.Extensions
{
    /// <summary>
    /// Easy to use extension methods for repository classes
    /// </summary>
    public static class RepositoryQueryExtensions
    {
        /// <summary>
        /// Gets a single record.
        /// If not found throws exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        public static async Task<T> QuerySingleAsync<T>(this IRepository<T> repository, IDbConnection connection, CommandDefinition commandDefinition) where T : class
            => await repository.GetSingleAsync(connection, commandDefinition);

        /// <summary>
        /// Gets a single record.
        /// If not found returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        public static async Task<T> QuerySingleOrDefaultAsync<T>(this IRepository<T> repository, IDbConnection connection, CommandDefinition commandDefinition) where T : class
            => await repository.GetSingleOrDefaultAsync(connection, commandDefinition);

        /// <summary>
        /// Gets all the records that matches the query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAsync<T>(this IRepository<T> repository, IDbConnection connection, CommandDefinition commandDefinition) where T : class
            => await repository.GetAsync(connection, commandDefinition);

        /// <summary>
        /// Executes the query and returns the number of rows affected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync<T>(this IRepository<T> repository, IDbConnection connection, CommandDefinition commandDefinition) where T : class
            => await repository.ExecuteCommandAsync(connection, commandDefinition);

        /// <summary>
        /// Executes the query and returns the single value returned by the scalar method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteScalarAsync<T>(this IRepository<T> repository, IDbConnection connection, CommandDefinition commandDefinition) where T : class
            => await repository.GetScalarResultAsync(connection, commandDefinition);
    }
}