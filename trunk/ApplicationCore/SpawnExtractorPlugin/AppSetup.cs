using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

using ApplicationCore.Setup;

namespace SpawnExtractorPlugin
{
    class AppSetup : IAppSetup
    {
        public void ConfigureContainer(Microsoft.Practices.Unity.IUnityContainer container)
        {
            var vm = container.Resolve<SpawnExtractorTabViewModel>();
            container.RegisterInstance(vm);            
        }
    }
}
