using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ApplicationCore.DataServices
{
    public interface IDataService : INotifyPropertyChanged
    {        
        EQEmu.Database.Configuration DBConfiguration
        {
            get;
            set;
        }
    }
}
