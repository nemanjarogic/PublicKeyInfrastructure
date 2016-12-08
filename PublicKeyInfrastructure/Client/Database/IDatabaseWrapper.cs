using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database
{
    public interface IDatabaseWrapper
    {
        void CreateDatabase(string dbName);
        void ConnectToDatabase(string dbname);
        void CreateTable(string tableName);
        void InsertToTable(string tableName, string serviceName);
        void ListAllRecordsFromTable(string tableName);
        void DropDatabase(string dbName);
    }
}
