using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CompBioAnalyticsApi.DataAccess.Exceptions;
using CompBioAnalyticsApi.DataAccess.Extensions;
using CompBioAnalyticsApi.DataAccess.Models;
using CompBioAnalyticsApi.DataAccess.Repositories.Interfaces;
using CompBioAnalyticsApi.DataAccess.Services.Interfaces;
using Dapper;

namespace CompBioAnalyticsApi.DataAccess.Services
{
    public abstract class BaseDataService<T> : IDataService<T> where T : BaseModel
    {
        private const int MaxDynamicParamCount = 2000;

        private static readonly Lazy<Type> CurrentType = new Lazy<Type>(() => typeof(T));
        private static readonly Lazy<PropertyInfo[]> PublicPropertyInfo = new Lazy<PropertyInfo[]>(() => typeof(T).GetProperties());

        private readonly IRepository<T> _repository;
        private readonly IConnectionFactory _connectionFactory;

        protected BaseDataService(IRepository<T> repository, IConnectionFactory connectionFactory)
        {
            _repository = repository;
            _connectionFactory = connectionFactory;
        }

        private static (DynamicParameters dynamicParameters, StringBuilder queryBuilder) GetUpdateQuery(T input, StringBuilder queryBuilder = null, DynamicParameters dynamicParameters = null, string offset = null)
        {
            var propertyInfos = PublicPropertyInfo.Value.Where(x => x.Name != nameof(BaseModel.Id)).ToArray();
            if (dynamicParameters == null)
            {
                dynamicParameters = new DynamicParameters();
            }

            if (queryBuilder == null)
            {
                queryBuilder = new StringBuilder();
            }

            queryBuilder
                .AppendLine($"UPDATE [{CurrentType.Value.Name}] ")
                .AppendLine("SET ");

            for (var index = 0; index < propertyInfos.Length; index++)
            {
                var propertyInfo = propertyInfos[index];
                if (index > 0)
                {
                    queryBuilder.Append(" , ");
                }

                queryBuilder.AppendLine($"[{propertyInfo.Name}] = @{propertyInfo.Name}{offset}");
                dynamicParameters.Add($"@{propertyInfo.Name}{offset}", propertyInfo.GetValue(input));
            }

            queryBuilder.AppendLine($" FROM [{CurrentType.Value.Name}]");

            var paramName = $"{CurrentType.Value.Name}_PK_Id{offset}";
            queryBuilder.AppendLine($" WHERE [{nameof(BaseModel.Id)}] = @{paramName}")
                .AppendLine();
            dynamicParameters.Add(paramName, input.Id);
            return (dynamicParameters, queryBuilder);
        }

        private static (DynamicParameters, StringBuilder) GetInsertQuery(T input, StringBuilder queryBuilder = null, DynamicParameters dynamicParameters = null, string offset = null)
        {
            var propertyInfos = PublicPropertyInfo.Value.Where(x => x.Name != nameof(BaseModel.Id)).ToArray();

            if (dynamicParameters == null)
            {
                dynamicParameters = new DynamicParameters();
            }

            if (queryBuilder == null)
            {
                queryBuilder = new StringBuilder();
            }

            queryBuilder
                .AppendLine($" INSERT INTO [{CurrentType.Value.Name}] ")
                .AppendLine(" ( ")
                .AppendLine(string.Join(" , ", propertyInfos.Select(x => x.Name)))
                .AppendLine(" ) ")
                .AppendLine(" VALUES ")
                .AppendLine(" ( ");

            for (var index = 0; index < propertyInfos.Length; index++)
            {
                var propertyInfo = propertyInfos[index];
                if (index > 0)
                {
                    queryBuilder.Append(" , ");
                }

                queryBuilder.AppendLine($" @{propertyInfo.Name}{offset} ");
                dynamicParameters.Add($"@{propertyInfo.Name}{offset}", propertyInfo.GetValue(input));
            }

            queryBuilder.AppendLine(" ) ").AppendLine();

            return (dynamicParameters, queryBuilder);
        }

        private static int GetBatchSize(IReadOnlyCollection<T> inputArray)
        {
            if (inputArray.Count == 0 || PublicPropertyInfo.Value.Length == 0)
            {
                return 0;
            }

            var segmentSize = PublicPropertyInfo.Value.Length * inputArray.Count / MaxDynamicParamCount;
            return segmentSize;
        }

        private async Task BulkUpdateAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var queryBuilder = new StringBuilder();
            var dynamicParameters = new DynamicParameters();

            for (var index = 0; index < inputs.Count; index++)
            {
                var item = inputs[index];
                GetUpdateQuery(item, queryBuilder, dynamicParameters, index.ToString());
            }

            await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction));
        }

        private async Task BulkInsertAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var queryBuilder = new StringBuilder();
            var dynamicParameters = new DynamicParameters();

            for (var index = 0; index < inputs.Count; index++)
            {
                GetInsertQuery(inputs[index], queryBuilder, dynamicParameters, index.ToString());
            }

            await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction));
        }

        protected static StringBuilder GetSelectQueryBuilder()
        {
            var propertyInfos = PublicPropertyInfo.Value;
            var queryBuilder = new StringBuilder()
                .AppendLine("SELECT ");
            for (var index = 0; index < propertyInfos.Length; index++)
            {
                var propertyInfo = propertyInfos[index];
                if (index > 0)
                {
                    queryBuilder.Append(", ");
                }

                queryBuilder.AppendLine($" [{CurrentType.Value.Name}].[{propertyInfo.Name}] ");
            }

            queryBuilder.AppendLine($"FROM [{CurrentType.Value.Name}] ");
            return queryBuilder;
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                var queryBuilder = GetSelectQueryBuilder();

                var paramName = $"{CurrentType.Value.Name}_PK_Id";
                queryBuilder.AppendLine($" WHERE [{CurrentType.Value.Name}].[{nameof(BaseModel.Id)}] = @{paramName} ");
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(paramName, id);

                return await _repository.QuerySingleAsync(connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters));
            }
        }

        public virtual async Task<int> UpdateAsync(T input)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var rowsAffected = await UpdateAsync(input, transaction);
                    transaction.Commit();
                    return rowsAffected;
                }
            }
        }

        public virtual async Task<int> InsertAsync(T input)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var id = await InsertAsync(input, transaction);
                    transaction.Commit();
                    return id;
                }
            }
        }

        public virtual async Task UpdateAsync(IList<T> inputs)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    await UpdateAsync(inputs, transaction);
                    transaction.Commit();
                }
            }
        }

        public virtual async Task InsertAsync(IList<T> inputs)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    await InsertAsync(inputs, transaction);
                    transaction.Commit();
                }
            }
        }

        public virtual async Task<int> UpdateAsync(T input, IDbTransaction transaction)
        {
            var (dynamicParameters, queryBuilder) = GetUpdateQuery(input);
            var rowsAffected = await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction));
            return rowsAffected > 0 ? rowsAffected : throw new DatabaseOperationFailedException(queryBuilder.ToString(), input);
        }

        public virtual async Task<int> InsertAsync(T input, IDbTransaction transaction)
        {
            var (dynamicParameters, queryBuilder) = GetInsertQuery(input);
            var scopeIdentity = await _repository.ExecuteScalarAsync(transaction.Connection, new CommandDefinition(queryBuilder.AppendLine(" SELECT SCOPE_IDENTITY() ").ToString(), dynamicParameters, transaction));
            if (scopeIdentity <= 0)
            {
                throw new DatabaseOperationFailedException(queryBuilder.ToString(), input);
            }

            input.Id = scopeIdentity;
            return scopeIdentity;
        }

        public virtual async Task UpdateAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var inputArray = inputs as T[] ?? inputs.ToArray();

            foreach (var input in inputArray.Slice(GetBatchSize(inputArray)))
            {
                await BulkUpdateAsync(input, transaction);
            }
        }

        public virtual async Task InsertAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var inputArray = inputs as T[] ?? inputs.ToArray();

            foreach (var input in inputArray.Slice(GetBatchSize(inputArray)))
            {
                await BulkInsertAsync(input, transaction);
            }
        }
    }
}