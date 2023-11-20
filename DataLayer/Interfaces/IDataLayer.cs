using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZenDeskAutomation.DataLayer.Interfaces
{
    // Interface for data layer.
    public interface IDataLayer
    {
        /// <summary>
        /// Executes a stored procedure and returns the result as a byte array.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="sqlParams">The dictionary of SQL parameters to pass to the stored procedure.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="logger">Logger</param>
        /// <returns>The result of the stored procedure as a byte array.</returns>
        Task<List<T>> ExecuteReader<T>(string procedureName, Dictionary<string, object> parameters, string connectionString, ILogger logger);


        /// <summary>
        /// Inserts the data into the table.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="caseTicketId">Case ticket id</param>
        /// <param name="zenDeskTicketId">Zen desk ticket id.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="logger">Logger</param>
        /// <returns>Returns the collection of objects.</returns>
        Task<int> ExecuteNonQuery(string procedureName, long caseTicketId, long zenDeskTicketId, string connectionString, ILogger logger);
    }
}
