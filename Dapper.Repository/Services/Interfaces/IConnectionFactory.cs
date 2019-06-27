using System.Data;

namespace Dapper.Repository.Services.Interfaces
{
    /// <summary>
    /// Connection factory.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        IDbConnection GetConnection();  
    }
}
