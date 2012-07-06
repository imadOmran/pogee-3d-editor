using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

namespace ApplicationCore.Setup
{
    public interface IAppSetup
    {
        void InitialConfiguration(IUnityContainer container);
        //this will be called after every module has executed their InitialConfiguration method
        void FinalConfiguration(IUnityContainer container);
    }
}
