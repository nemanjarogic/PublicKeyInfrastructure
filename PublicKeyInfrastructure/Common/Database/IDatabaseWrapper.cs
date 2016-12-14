using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database
{
    /// <summary>
    /// Interface for creating, accessing and editing database on client side.
    /// </summary>
    public interface IDatabaseWrapper
    {
        /// <summary>
        /// Name of the table. One table for current client session.
        /// </summary>
        string TheTableName { get; set; }

        /// <summary>
        /// Name of the database. One database for current client session.
        /// </summary>
        string TheDatabaseName { get; set; }


        /// <summary>
        /// Creates database for client.
        /// </summary>
        /// <param name="dbName">Name of database</param>
        void CreateDatabase(string dbName);

        /// <summary>
        /// Connect to the existing database.
        /// </summary>
        void ConnectToDatabase();

        /// <summary>
        /// Creates table in existing database.
        /// </summary>
        /// <param name="tableName">Name of new table.</param>
        void CreateTable(string tableName);

        /// <summary>
        /// Inserts new record into clients table.
        /// </summary>
        /// <param name="serviceName">Name of the connected client.</param>
        void InsertToTable(string serviceName);

        /// <summary>
        /// Writes all records from clients table.
        /// </summary>
        void ListAllRecordsFromTable();

        /// <summary>
        /// Close database connection and remove database from file system.
        /// </summary>
        void DropDatabase();
    }
}
