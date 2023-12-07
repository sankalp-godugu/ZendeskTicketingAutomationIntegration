﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ZenDeskAutomation.DataLayer.Interfaces;

namespace ZenDeskAutomation.DataLayer.Services
{
    /// <summary>
    /// Data Layer.
    /// </summary>
    public class DataLayer : IDataLayer
    {
        /// <summary>
        /// Executes a query and returns the collection of objects from the SQL database.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameters">The dictionary of SQL parameters to pass to the stored procedure.</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>The collection of objects returned from the executed query.</returns>
        public async Task<List<T>> ExecuteReader<T>(string procedureName, Dictionary<string, object> parameters, string connectionString, ILogger logger)
        {
            logger.LogInformation($"Started calling stored procedure {procedureName} with parameters: {string.Join(", ", parameters.Select(p => $"{p.Key} = {p.Value}"))}");
            List<T> list = new List<T>();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();
                try
                {
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.CommandTimeout = GetSqlCommandTimeout(logger);
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = procedureName;

                        if (parameters.Count > 0)
                        {
                            sqlCommand.Parameters.AddRange(GetSqlParameters(parameters).ToArray());
                        }
                        var dataReader = await sqlCommand.ExecuteReaderAsync();

                        list = DataReaderMapToList<T>(dataReader, logger);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"{procedureName} failed with exception: {ex.Message} for carrierId - {parameters["InsuranceCarrierId"]}");
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            logger.LogInformation($"Ended calling stored procedure {procedureName}");
            return list;
        }

        /// <summary>
        /// Inserts the data into the table.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="caseTicketId">Case ticket id</param>
        /// <param name="zenDeskTicketId">Zen desk ticket id.</param>
        /// <param name="logger">Logger</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Returns the collection of objects.</returns>
        public async Task<int> ExecuteNonQueryForCaseManagement(string procedureName, long? caseTicketId, long zenDeskTicketId, string connectionString, ILogger logger)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    command.Parameters.AddWithValue("@caseTicketId", caseTicketId);
                    command.Parameters.AddWithValue("@ZenDeskTicketID", zenDeskTicketId);

                    // Output parameter
                    SqlParameter resultParameter = new SqlParameter("@Result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultParameter);

                    try
                    {
                        await command.ExecuteNonQueryAsync();

                        // Retrieve the result code
                        int result = (int)resultParameter.Value;

                        // Log the result
                        logger.LogInformation($"UpdateZenDeskReferenceForMemberCaseTickets result: {result}");

                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error updating ZenDesk reference: {ex.Message}");
                        return -1;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Inserts the data into the table.
        /// </summary>
        /// <typeparam name="T">Generic parameter.</typeparam>
        /// <param name="procedureName">Procedure name.</param>
        /// <param name="orderChangeRequestId">orderChangeRequestId</param>
        /// <param name="zenDeskTicketId">Zen desk ticket id.</param>
        /// <param name="logger">Logger</param>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Returns the collection of objects.</returns>
        public async Task<int> ExecuteNonQueryForAdminPortal(string procedureName, long? orderChangeRequestId, long zenDeskTicketId, string connectionString, ILogger logger)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    command.Parameters.AddWithValue("@OrderChangeRequestID", orderChangeRequestId);
                    command.Parameters.AddWithValue("@ZenDeskTicketID", zenDeskTicketId);

                    // Output parameter
                    SqlParameter resultParameter = new SqlParameter("@Result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultParameter);

                    try
                    {
                        await command.ExecuteNonQueryAsync();

                        // Retrieve the result code
                        int result = (int)resultParameter.Value;

                        // Log the result
                        logger.LogInformation($"UpdateZenDeskReferenceForMemberCaseTickets result: {result}");

                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error updating ZenDesk reference: {ex.Message}");
                        return -1;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Gets the sql command timeout.
        /// </summary>
        /// <returns>Returns the sql command timeout.</returns>
        private static int GetSqlCommandTimeout(ILogger logger)
        {
            try
            {
                string sqlCommandTimeOut = Environment.GetEnvironmentVariable("SQLCommandTimeOut");
                if (!string.IsNullOrEmpty(sqlCommandTimeOut) && int.TryParse(sqlCommandTimeOut, out int parsedValue))
                {
                    return parsedValue;
                }
                else
                {
                    return 300;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"TimeOut with Exception: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Converts from data reader to a collection of generic objects.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="dataReader">The DataReader.</param>
        /// <returns>Returns the parsed object from the data reader.</returns>
        private static List<T> DataReaderMapToList<T>(IDataReader dataReader, ILogger logger)
        {
            List<T> list = new List<T>();
            try
            {
                T obj = default!;
                while (dataReader.Read())
                {
                    obj = Activator.CreateInstance<T>();
                    if (obj != null)
                    {
                        foreach (PropertyInfo property in obj.GetType().GetProperties())
                        {
                            if (property != null && !Equals(dataReader[property.Name], DBNull.Value))
                            {
                                property.SetValue(obj, dataReader[property.Name], null);
                            }
                        }
                        list.Add(obj);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured while parsing the List to table with Exception: {ex.Message}");
                return list;
            }
        }


        /// <summary>
        /// Gets the sql parameters.
        /// </summary>
        /// <param name="parameters">The dictionary of SQL parameters.</param>
        /// <returns>Returns the collection of sql parameters.</returns>
        private List<SqlParameter> GetSqlParameters(Dictionary<string, object> parameters)
        {
            return parameters.Select(sp => new SqlParameter(sp.Key, sp.Value)).ToList();
        }

        #endregion
    }
}
