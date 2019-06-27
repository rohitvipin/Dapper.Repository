using System;
using System.Data;
using System.Data.SqlClient;
using CompBioAnalyticsApi.DataAccess.Services.Interfaces;

namespace CompBioAnalyticsApi.DataAccess.Services
{
    /// <inheritdoc />
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CompBioAnalyticsApi.DataAccess.Services.ConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public ConnectionFactory(string connectionString) => _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentNullException(nameof(connectionString))
                : connectionString;

        /// <inheritdoc />
        public IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
