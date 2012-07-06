using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

using ApplicationCore.Setup;

namespace SpawnGroupPlugin
{
    class SpawnGroupAppSetup : IAppSetup
    {
        public void InitialConfiguration(IUnityContainer container)
        {
            var vm = container.Resolve<SpawnGroupEditTabViewModel>();
            container.RegisterInstance(vm);   
        }

        public void FinalConfiguration(IUnityContainer container)
        {            
        }
    }
}
