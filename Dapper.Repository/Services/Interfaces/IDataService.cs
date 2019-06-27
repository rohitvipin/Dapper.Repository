using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CompBioAnalyticsApi.DataAccess.Models;

namespace CompBioAnalyticsApi.DataAccess.Services.Interfaces
{
    public interface IDataService<T> where T : BaseModel
    {
        /// <summary>
        /// Gets the requested type by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Object of specified type</returns>
        Task<T> GetByIdAsync(int id);


        /// <summary>
        /// Updates the given record into the database
        /// </summary>
        /// <param name="input">Object to be updated</param>
        /// <returns>Number of records updated</returns>
        Task<int> UpdateAsync(T input);

        /// <summary>
        /// Inserts the given record into the table
        /// </summary>
        /// <param name="input">Object to be inserted</param>
        /// <returns>Primary key</returns>
        Task<int> InsertAsync(T input);

        /// <summary>
        /// Updates a list of objects
        /// </summary>
        /// <param name="inputs">Objects to be updated</param>
        /// <returns></returns>
        Task UpdateAsync(IList<T> inputs);

        /// <summary>
        /// Inserts a list of objects
        /// </summary>
        /// <param name="inputs">Objects to be inserted</param>
        /// <returns></returns>
        Task InsertAsync(IList<T> inputs);

        /// <summary>
        /// Updates the given record into the database
        /// </summary>
        /// <param name="input">Object to be updated</param>
        /// <param name="transaction">Transaction to be used, if null creates a new connection and transaction</param>
        /// <returns>Number of records updated</returns>
        Task<int> UpdateAsync(T input, IDbTransaction transaction);

        /// <summary>
        /// Inserts the given record into the table
        /// </summary>
        /// <param name="input">Object to be inserted</param>
        /// <param name="transaction">Transaction to be used, if null creates a new connection and transaction</param>
        /// <returns>Primary key</returns>
        Task<int> InsertAsync(T input, IDbTransaction transaction);

        /// <summary>
        /// Updates a list of objects
        /// </summary>
        /// <param name="inputs">Objects to be updated</param>
        /// <param name="transaction">Transaction to be used, if null creates a new connection and transaction</param>
        /// <returns></returns>
        Task UpdateAsync(IList<T> inputs, IDbTransaction transaction);

        /// <summary>
        /// Inserts a list of objects
        /// </summary>
        /// <param name="inputs">Objects to be inserted</param>
        /// <param name="transaction">Transaction to be used, if null creates a new connection and transaction</param>
        /// <returns></returns>
        Task InsertAsync(IList<T> inputs, IDbTransaction transaction);
    }
}