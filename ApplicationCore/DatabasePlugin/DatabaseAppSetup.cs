using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Practices.Unity;

using MySql.Data.MySqlClient;

using ApplicationCore.Setup;

using EQEmu.Database;

namespace DatabasePlugin
{
    public class DatabaseAppSetup : IAppSetup
    {
        public void InitialConfiguration(Microsoft.Practices.Unity.IUnityContainer container)
        {
            Configuration conf = new Configuration();
            MySqlConnection connection = GetDatabaseConnection(ref conf);
            if (connection != null)
            {
                container.RegisterInstance<MySqlConnection>(connection);
            }
            else
            {
                container.RegisterInstance<MySqlConnection>(new MySqlConnection());
                System.Windows.MessageBox.Show("Could not create database connection, configure connection.xml and restart");
            }

            container.RegisterInstance<EQEmu.Database.Configuration>(conf);
            XDocument doc;
            EQEmu.Database.QueryConfig config = null;
#if EQEMU
                doc = XDocument.Load("eqemu-config.xml");
#else
            doc = XDocument.Load("config.xml");
#endif
            config = EQEmu.Database.QueryConfig.Create(doc.Root);
            container.RegisterInstance<EQEmu.Database.QueryConfig>(config);
        }

        public void FinalConfiguration(Microsoft.Practices.Unity.IUnityContainer container)
        {
        }

        private MySqlConnection GetDatabaseConnection(ref EQEmu.Database.Configuration configuration)
        {
            XmlSerializer serializer =
                new XmlSerializer(typeof(EQEmu.Database.Configuration));
            MySql.Data.MySqlClient.MySqlConnection conn = null;

            var confPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "EQEmu/connection.xml");

            //check for a configuration file in the user directory first
            if (!File.Exists(confPath))
            {
                confPath = "./connection.xml";
                if (!File.Exists(confPath))
                {
                    return null;
                }
            }

            EQEmu.Database.Configuration conf;

            using (XmlReader reader = XmlReader.Create(confPath))
            {
                conf = (EQEmu.Database.Configuration)serializer.Deserialize(reader);
                string connStr = string.Format("server={0};user={1};database={2};port={3};password={4};ConnectionTimeout={5};",
                    conf.Host, conf.User, conf.Database, conf.Port, conf.Password, conf.ConnectionTimeout);
                conn = new MySql.Data.MySqlClient.MySqlConnection(connStr);
                try
                {
                    conn.Open();
                }
                catch (System.Exception e)
                {
                    conn.Close();

                    System.Windows.MessageBox.Show(confPath + "\nCould not open database connection\n" + e.Message);
                }
            }

            if (configuration != null && conf != null)
            {
                configuration = conf;
            }

            return conn;
        }
    }
}
