using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

namespace EQEmu.Database
{
    public static class QueryHelper
    {
        public static List< Dictionary<string,object> >RunQuery(MySqlConnection connection, string query)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            var reader = command.ExecuteReader();

            var results = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                var fields = reader.FieldCount;
                var dict = new Dictionary<string, object>();

                for (int i = 0; i < fields; i++)
                {
                    dict[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(dict);
            }

            reader.Close();

            return results;
        }
    }
}
