using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

namespace ApplicationCore.Setup
{
    public interface IAppSetup
    {
        void ConfigureContainer(IUnityContainer container);
    }
}
