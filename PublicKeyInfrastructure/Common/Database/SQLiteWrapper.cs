using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Client.Database
{
    /// <summary>
    /// Wrapper for SQLite database. Implements <see cref="IDatabaseWrapper"/>.
    /// </summary>
    public class SQLiteWrapper : IDatabaseWrapper
    {
        /// <summary>
        /// Database conenction.
        /// </summary>
        SQLiteConnection m_dbConnection;

        public string TheTableName { get; set; }

        public string TheDatabaseName { get; set; }

        /// <summary>
        /// Creates database in file system if not exists.
        /// </summary>
        /// <param name="dbName">Name of the database. Sets <see cref="TheDatabaseName"/> property.</param>
        public void CreateDatabase(string dbName)
        {
            if (!File.Exists(dbName + ".sqlite"))
            {
                SQLiteConnection.CreateFile(dbName + ".sqlite");
            }
            TheDatabaseName = dbName;
        }

        /// <summary>
        /// Instantiates and opent database connection.
        /// </summary>
        public void ConnectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=" + TheDatabaseName + ".sqlite;Version=3;");
            m_dbConnection.Open();
        }

        /// <summary>
        /// Creates and executes sql query for creating table. Sets <see cref="TheTableName"/> property.
        /// </summary>
        /// <param name="tableName"></param>
        public void CreateTable(string tableName)
        {
            string sql = "drop table if exists "+tableName+
                         "; create table if not exists " + tableName + " (Id INTEGER  PRIMARY KEY, TimeStamp DATETIME, ConnectedTo varchar(20))";/*yyyy-MM-dd HH:mm:ss*/
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            TheTableName = tableName;
        }

        /// <summary>
        /// Creates and executes sql query for inserting new record into table.
        /// </summary>
        /// <param name="serviceName"></param>
        public void InsertToTable(string serviceName)
        {
            DateTime timeNow = DateTime.Now;
            string sql = "insert into " + TheTableName + " (Id, TimeStamp, ConnectedTo) values (NULL, '" + timeNow.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + serviceName + "')";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets all records from database, parses and writes them in console.
        /// </summary>
        public void ListAllRecordsFromTable()
        {
            string sql = "select * from " + TheTableName;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            Console.WriteLine("Id \t TimeStamp \t\t ConnectedTo");
            Console.WriteLine("_____________________________________________");
            while (reader.Read())
                Console.WriteLine(reader["Id"] + "\t" + reader["TimeStamp"] + "\t" + reader["ConnectedTo"]);     
        }

        /// <summary>
        /// Closes database connections. Removes database from file system.
        /// </summary>
        public void DropDatabase()
        {
            if (File.Exists(TheDatabaseName + ".sqlite"))
            {
                m_dbConnection.Close();
                File.Delete(TheDatabaseName + ".sqlite");
            }
        }
    }
}
