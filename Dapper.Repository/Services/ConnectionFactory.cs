using System;
using System.Data;
using System.Data.SqlClient;
using Dapper.Repository.Services.Interfaces;

namespace Dapper.Repository.Services
{
    /// <inheritdoc />
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Dapper.Repository.Services.ConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public ConnectionFactory(string connectionString) => _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentNullException(nameof(connectionString))
                : connectionString;

        /// <inheritdoc />
        public IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
