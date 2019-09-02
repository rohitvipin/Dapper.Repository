using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapper.Repository.Repositories.Interfaces;

namespace Dapper.Repository.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Base class implementing the basic methods from <see cref="T:ChatMessage.Repository.Interfaces.IRepository`1" />
    /// This class internally invokes the dapper static extension methods.
    /// There is no business logic in this class only relaying methods. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <inheritdoc />
        public virtual async Task<T> GetSingleAsync(IDbConnection connection, CommandDefinition commandDefinition) => await connection.QuerySingleAsync<T>(commandDefinition);

        /// <inheritdoc />
        public virtual async Task<T> GetSingleOrDefaultAsync(IDbConnection connection, CommandDefinition commandDefinition) => await connection.QuerySingleOrDefaultAsync<T>(commandDefinition);

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> GetAsync(IDbConnection connection, CommandDefinition commandDefinition) => await connection.QueryAsync<T>(commandDefinition);

        public virtual async Task<int> ExecuteCommandAsync(IDbConnection connection, CommandDefinition commandDefinition) => await connection.ExecuteAsync(commandDefinition);

        public virtual async Task<int> GetScalarResultAsync(IDbConnection connection, CommandDefinition commandDefinition) => await connection.ExecuteScalarAsync<int>(commandDefinition);
    }
}