using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Microsoft.Practices.Unity;

namespace ApplicationCore.DataServices
{
    abstract public class DataServiceBase : IDataService
    {
        public DataServiceBase()
        {
            _instances++;
        }

        private static int _instances = 0;

        private EQEmu.Database.Configuration _configuration = null;
        [OptionalDependency]
        public EQEmu.Database.Configuration DBConfiguration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
            }
        }

        private EQEmu.Database.QueryConfig _typeQueryConfig = null;
        [OptionalDependency]
        public EQEmu.Database.QueryConfig TypeQueryConfig
        {
            get { return _typeQueryConfig; }
            set
            {
                _typeQueryConfig = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
