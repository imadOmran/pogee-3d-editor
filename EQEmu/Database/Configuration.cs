using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Database
{
    public class Configuration
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Database { get; set; }
        public string Port { get; set; }
        public string PEQEditorUrl { get; set; }
    }
}
