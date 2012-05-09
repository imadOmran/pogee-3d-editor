using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using MySql.Data.MySqlClient;

namespace RoamArea.Loader.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var doc = XDocument.Load("config.xml");
            var config = EQEmu.Database.QueryConfig.Create(doc.Root);
            EQEmu.Database.Configuration conf = new EQEmu.Database.Configuration();
            MySqlConnection connection = GetDatabaseConnection(ref conf);

            var roamarea = new EQEmu.RoamAreas.ZoneRoamAreasDatabase("misty",connection,config);
        }

        private static MySqlConnection GetDatabaseConnection(ref EQEmu.Database.Configuration configuration)
        {
            XmlSerializer serializer =
                new XmlSerializer(typeof(EQEmu.Database.Configuration));
            MySql.Data.MySqlClient.MySqlConnection conn = null;

            if (!File.Exists("./connection.xml")) return null;
            EQEmu.Database.Configuration conf;

            using (XmlReader reader = XmlReader.Create("./connection.xml"))
            {
                conf = (EQEmu.Database.Configuration)serializer.Deserialize(reader);
                string connStr = string.Format("server={0};user={1};database={2};port={3};password={4};",
                    conf.Host, conf.User, conf.Database, conf.Port, conf.Password);
                conn = new MySql.Data.MySqlClient.MySqlConnection(connStr);
            }

            if (configuration != null && conf != null)
            {
                configuration = conf;
            }

            return conn;
        }
    }
}
