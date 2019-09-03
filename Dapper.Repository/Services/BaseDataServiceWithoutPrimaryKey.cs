using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper.Repository.Exceptions;
using Dapper.Repository.Extensions;
using Dapper.Repository.Models;
using Dapper.Repository.Repositories.Interfaces;
using Dapper.Repository.Services.Interfaces;

namespace Dapper.Repository.Services
{
    public abstract class BaseDataServiceWithCompositePrimaryKey<T> : IDataServiceWithCompositePrimaryKey<T> where T : BaseModelWithCompositePrimaryKey
    {
        private const int MaxDynamicParamCount = 2000;

        private static readonly Lazy<Type> CurrentType = new Lazy<Type>(() => typeof(T));
        private static readonly Lazy<PropertyInfo[]> PublicPropertyInfo = new Lazy<PropertyInfo[]>(() => typeof(T).GetProperties());

        private readonly IRepository<T> _repository;
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _commandTimeout;

        protected BaseDataServiceWithCompositePrimaryKey(IRepository<T> repository, IConnectionFactory connectionFactory, int commandTimeout = 300)
        {
            _repository = repository;
            _connectionFactory = connectionFactory;
            _commandTimeout = commandTimeout;
        }

        private (DynamicParameters DynamicParameters, StringBuilder QueryBuilder) GetUpdateQuery(T input, StringBuilder queryBuilder = null, DynamicParameters dynamicParameters = null, string offset = null)
        {
            var propertyInfos = PublicPropertyInfo.Value.ToArray();
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

            return AddWhereConditions(dynamicParameters, queryBuilder, input, offset);
        }

        protected abstract (DynamicParameters dynamicParameters, StringBuilder queryBuilder) AddWhereConditions(DynamicParameters dynamicParameters, StringBuilder queryBuilder, T input, string offset);

        private static (DynamicParameters, StringBuilder) GetInsertQuery(T input, StringBuilder queryBuilder = null, DynamicParameters dynamicParameters = null, string offset = null)
        {
            var propertyInfos = PublicPropertyInfo.Value.ToArray();

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

        private async Task<int> BulkUpdateAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var queryBuilder = new StringBuilder();
            var dynamicParameters = new DynamicParameters();

            for (var index = 0; index < inputs.Count; index++)
            {
                var item = inputs[index];
                GetUpdateQuery(item, queryBuilder, dynamicParameters, index.ToString());
            }

            return await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction, _commandTimeout));
        }

        private async Task<int> BulkInsertAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var queryBuilder = new StringBuilder();
            var dynamicParameters = new DynamicParameters();

            for (var index = 0; index < inputs.Count; index++)
            {
                GetInsertQuery(inputs[index], queryBuilder, dynamicParameters, index.ToString());
            }

            return await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction, _commandTimeout));
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
                    var rowsInserted = await InsertAsync(input, transaction);
                    transaction.Commit();
                    return rowsInserted;
                }
            }
        }

        public virtual async Task<int> UpdateAsync(IList<T> inputs)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var rowsAffected = await UpdateAsync(inputs, transaction);
                    transaction.Commit();
                    return rowsAffected;
                }
            }
        }

        public virtual async Task<int> InsertAsync(IList<T> inputs)
        {
            using (var connection = _connectionFactory.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var rowsAffected = await InsertAsync(inputs, transaction);
                    transaction.Commit();
                    return rowsAffected;
                }
            }
        }

        public virtual async Task<int> UpdateAsync(T input, IDbTransaction transaction)
        {
            var (dynamicParameters, queryBuilder) = GetUpdateQuery(input);
            var rowsAffected = await _repository.ExecuteAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction, _commandTimeout));
            return rowsAffected > 0 ? rowsAffected : throw new DatabaseOperationFailedException(queryBuilder.ToString(), input);
        }

        public virtual async Task<int> InsertAsync(T input, IDbTransaction transaction)
        {
            var (dynamicParameters, queryBuilder) = GetInsertQuery(input);
            var rowsInserted = await _repository.ExecuteCommandAsync(transaction.Connection, new CommandDefinition(queryBuilder.ToString(), dynamicParameters, transaction, _commandTimeout));
            if (rowsInserted <= 0)
            {
                throw new DatabaseOperationFailedException(queryBuilder.ToString(), input);
            }

            return rowsInserted;
        }

        public virtual async Task<int> UpdateAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var inputArray = inputs as T[] ?? inputs.ToArray();
            var rowsAffected = 0;

            foreach (var input in inputArray.Slice(GetBatchSize(inputArray)))
            {
                rowsAffected += await BulkUpdateAsync(input, transaction);
            }

            return rowsAffected;
        }

        public virtual async Task<int> InsertAsync(IList<T> inputs, IDbTransaction transaction)
        {
            var inputArray = inputs as T[] ?? inputs.ToArray();
            var rowsAffected = 0;

            foreach (var input in inputArray.Slice(GetBatchSize(inputArray)))
            {
                rowsAffected += await BulkInsertAsync(input, transaction);
            }

            return rowsAffected;
        }
    }
}