using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    public interface INpcPropertyTemplate
    {
        void SetProperties(IEnumerable<Npc> npcs);
        string Name { get; }
        string Category { get; }
    }

    public static class NpcPropertyCategories
    {
        public static string General
        {
            get { return "General"; }
        }

        public static string LDoN
        {
            get { return "Lost Dungeons of Norrath"; }
        }
    }
}
