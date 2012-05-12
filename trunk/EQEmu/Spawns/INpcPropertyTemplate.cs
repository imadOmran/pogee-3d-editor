using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    public interface INpcPropertyTemplate
    {
        void SetProperties(IEnumerable<Npc> npcs);
    }
}
