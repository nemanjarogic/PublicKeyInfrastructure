using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database
{
    public interface IDatabaseWrapper
    {
        string TheTableName { get; set; }
        string TheDatabaseName { get; set; }

        void CreateDatabase(string dbName);
        void ConnectToDatabase();
        void CreateTable(string tableName);
        void InsertToTable(string serviceName);
        void ListAllRecordsFromTable();
        void DropDatabase();
    }
}
