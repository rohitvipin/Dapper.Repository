using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper.Repository.Models;

namespace Dapper.Repository.Repositories.Interfaces
{
    /// <summary>
    /// Base interface for any repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : BaseModel
    {
        /// <summary>
        /// Gets a single record.
        /// If not found throws exception
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        Task<T> GetSingleAsync(IDbConnection connection, CommandDefinition commandDefinition);

        /// <summary>
        /// Gets a single record.
        /// If not found returns null
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        Task<T> GetSingleOrDefaultAsync(IDbConnection connection, CommandDefinition commandDefinition);

        /// <summary>
        /// Gets all the records that matches the query
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync(IDbConnection connection, CommandDefinition commandDefinition);

        /// <summary>
        /// Executes the query and returns the number of rows affected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        Task<int> ExecuteCommandAsync(IDbConnection connection, CommandDefinition commandDefinition);


        /// <summary>
        /// Executes the query and returns the number of rows affected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandDefinition"></param>
        /// <returns></returns>
        Task<int> GetScalarResultAsync(IDbConnection connection, CommandDefinition commandDefinition);
    }
}