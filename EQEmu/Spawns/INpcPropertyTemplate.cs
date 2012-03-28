using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    public interface INPCPropertyTemplate
    {
        void SetProperties(IEnumerable<NPC> npcs);
    }
}
