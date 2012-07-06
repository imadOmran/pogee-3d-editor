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
            //eventually this should be it's own plugin - when the next plugin requires use of the manager
            var npcmanager = new EQEmu.Spawns.NpcPropertyTemplateManager();
            npcmanager.Templates = container.ResolveAll<EQEmu.Spawns.INpcPropertyTemplate>();
            container.RegisterInstance(npcmanager);

            var vm = container.Resolve<SpawnExtractorTabViewModel>();
            container.RegisterInstance(vm);         
        }
    }
}
