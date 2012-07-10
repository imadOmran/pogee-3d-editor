using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

using ApplicationCore.Setup;

namespace SpawnExtractorPlugin
{
    class SpawnExtractorAppSetup : IAppSetup
    {
        public void InitialConfiguration(IUnityContainer container)
        {            
        }

        public void FinalConfiguration(IUnityContainer container)
        {
            var npcmanager = container.Resolve<EQEmu.Spawns.NpcPropertyTemplateManager>();
            npcmanager.Templates = container.ResolveAll<EQEmu.Spawns.INpcPropertyTemplate>();

            //register it just in case resolve created a new instance of the manager
            //as opposed to one being already registered with the container
            container.RegisterInstance(npcmanager);

            var vm = container.Resolve<SpawnExtractorTabViewModel>();
            container.RegisterInstance(vm);         
        }
    }
}
