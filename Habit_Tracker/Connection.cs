using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habit_Tracker
{
    class Connection
    {
        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection("Data Source=HabitDb.db");

            connection.Open();

            return connection;
        }
    }
}

