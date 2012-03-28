using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQEmu.Spawns
{
    /*
     *  This file is copied into the plugins directory on build
     * 
     *  On application launch this file is compiled as an assembly and loaded into the runtime
     *  These templates can therefore be modified without having to recompile the source, C# as a scripting language basically
     */

    class CastersTemplate : INPCPropertyTemplate
    {
        public void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                switch (npc.Class)
                {
                    case NPC.NPCClass.Cleric:
                        npc.SpellsId = 1;
                        break;
                    case NPC.NPCClass.Shaman:
                        npc.SpellsId = 6;
                        break;
                    case NPC.NPCClass.Wizard:
                        npc.SpellsId = 2;
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            return "Default Casters Template";
        }
    }

    class LDONNormalToHighRisk : INPCPropertyTemplate
    {
        private LDONNpcTemplate _normal = new LDONNpcTemplate();

        public void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                if (npc.Level < 20)
                {
                    npc.Level += 3;
                }
            }
            _normal.SetProperties(npcs);
        }

        public override string ToString()
        {
            return "LDON - Convert Normal to High Risk";
        }
    }

    class LDONRujTemplate : INPCPropertyTemplate
    {
        public void SetProperties(IEnumerable<NPC> npcs)
        {

            foreach (var npc in npcs)
            {
                switch (npc.Race)
                {
                    case NPC.TypeRace.RujarkianOrc:
                    case NPC.TypeRace.NewBear:
                    case NPC.TypeRace.Boar:
                    case NPC.TypeRace.Wolf:
                        npc.FactionId = 722;
                        break;
                    case NPC.TypeRace.NewGoblin:
                        npc.FactionId = 721;
                        break;
                    default:
                        break;                    
                }
            }

        }

        public override string ToString()
        {
            return "LDON - Rujarkian Hills";
        }
    }

    class LDONNpcTemplate : INPCPropertyTemplate
    {
        public virtual void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                if (npc.Race == NPC.TypeRace.Box || npc.Class == NPC.NPCClass.LDoNObject)
                {
                    npc.HealthPoints = 5000;
                    continue;
                }

                switch (npc.Level)
                {
                    case 17:                        
                        npc.HealthPoints = 720;
                        npc.MinDamage = 2;
                        npc.MaxDamage = 30;
                        break;
                    case 18:
                        npc.HealthPoints = 800;
                        npc.MinDamage = 2;
                        npc.MaxDamage = 34;
                        break;
                    case 19:
                        if (npc.Name.Contains("#"))
                        {
                            npc.HealthPoints = 940;
                            npc.MinDamage = 5;
                            npc.MaxDamage = 45;
                        }
                        else
                        {
                            npc.HealthPoints = 880;
                            npc.MinDamage = 2;
                            npc.MaxDamage = 38;
                        }
                        break;
                    //high risks
                    case 20:
                        npc.HealthPoints = 800;
                        npc.MinDamage = 2;
                        npc.MaxDamage = 38;
                        break;
                    case 21:
                        npc.HealthPoints = 880;
                        npc.MinDamage = 3;
                        npc.MaxDamage = 40;
                        break;
                    case 22:
                        if (npc.Name.Contains("#"))
                        {
                            npc.HealthPoints = 1020;
                            npc.MinDamage = 5;
                            npc.MaxDamage = 50;
                        }
                        else
                        {
                            npc.HealthPoints = 940;
                            npc.MinDamage = 3;
                            npc.MaxDamage = 42;
                        }
                        break;
                    default:
                        break;
                }

                if (npc.Level <= 20)
                {
                    npc.AggroRadius = 55;
                }

                //high risks
                if (npc.Level >= 20 && npc.Level <= 22)
                {
                    npc.AggroRadius = 65;
                }
            }
        }

        public override string ToString()
        {
            return "LDON Template";
        }
    }
}
