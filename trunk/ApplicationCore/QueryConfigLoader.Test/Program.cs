using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QueryConfigLoader.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var doc = XDocument.Load("config.xml");
            var config = EQEmu.Database.QueryConfig.Create(doc.Root);
        }
    }
}
