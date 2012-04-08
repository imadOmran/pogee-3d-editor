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
                    case NPC.NPCClass.ClericGM:
                        npc.SpellsId = 1;
                        break;
                    case NPC.NPCClass.Shaman:
                    case NPC.NPCClass.ShamanGM:
                        npc.SpellsId = 6;
                        break;
                    case NPC.NPCClass.Wizard:
                    case NPC.NPCClass.WizardGM:
                        npc.SpellsId = 2;
                        break;
                    case NPC.NPCClass.Enchanter:
                    case NPC.NPCClass.EnchanterGM:
                        npc.SpellsId = 5;
                        break;
                    case NPC.NPCClass.Magician:
                    case NPC.NPCClass.MagicianGM:
                        npc.SpellsId = 4;
                        break;
                    case NPC.NPCClass.Necromancer:
                    case NPC.NPCClass.NecromancerGM:
                        npc.SpellsId = 3;
                        break;
                    case NPC.NPCClass.ShadowKnight:
                    case NPC.NPCClass.ShadowKnightGM:
                        npc.SpellsId = 9;
                        break;
                    case NPC.NPCClass.Paladin:
                    case NPC.NPCClass.PaladinGM:
                        npc.SpellsId = 8;
                        break;
                    case NPC.NPCClass.Ranger:
                    case NPC.NPCClass.RangerGM:
                        npc.SpellsId = 10;
                        break;
                    case NPC.NPCClass.Druid:
                    case NPC.NPCClass.DruidGM:
                        npc.SpellsId = 7;
                        break;
                    case NPC.NPCClass.Beastlord:
                    case NPC.NPCClass.BeastlordGM:
                        npc.SpellsId = 12;
                        break;
                    case NPC.NPCClass.Bard:
                    case NPC.NPCClass.BardGM:
                        npc.SpellsId = 11;
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
                if (npc.Level < 25)
                {
                    npc.Level += 3;
                }
            }
            _normal.SetProperties(npcs);
        }

        public override string ToString()
        {
            return "LDON - Convert Level ~18 to High Risk";
        }
    }

    class LDONGukTemplate : INPCPropertyTemplate
    {
        public void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                switch (npc.BodyType)
                {
                    case NPC.TypeBodyType.Undead:
                        break;
                }

                switch (npc.Race)
                {
                    case NPC.TypeRace.FroglokGhost:
                    case NPC.TypeRace.UndeadFroglok:
                    case NPC.TypeRace.WatchfulEye:
                    case NPC.TypeRace.Froglok:
                        npc.FactionId = 725;
                        break;

                    case NPC.TypeRace.Spider:
                    case NPC.TypeRace.Goo:
                        npc.FactionId = 79;
                        break;
                }

                if (npc.Name.ToLower().Contains("witness"))
                {
                    npc.FactionId = 904;
                }
                else if (npc.Name.ToLower().Contains("guktan"))
                {
                    npc.FactionId = 725;
                }
            }
        }

        public override string ToString()
        {
            return "LDON - Guk";
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
                if (npc.Race == NPC.TypeRace.InvisibleMan)
                {
                    continue;
                }
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

                    //normal assasinate level 
                    case 23:
                        npc.HealthPoints = 1083;
                        npc.MinDamage = 4;
                        npc.MaxDamage = 53;
                        break;

                    case 25:
                    //high risk assasinate
                    case 26:
                        //hp estimate
                        npc.HealthPoints = 1625;
                        npc.MinDamage = 14;
                        npc.MaxDamage = 57;
                        break;

                    default:
                        npc.HealthPoints = 500;
                        break;
                }

                if (npc.Level <= 20)
                {
                    npc.AggroRadius = 55;
                }

                //high risks
                if (npc.Level >= 20 && npc.Level <= 26)
                {
                    npc.AggroRadius = 65;
                }
            }
        }

        public override string ToString()
        {
            return "LDON - Teir1 (15-24) Template";
        }
    }

    class LDONConvert18To28Template : INPCPropertyTemplate
    {
        private INPCPropertyTemplate _base = new LDONNpc28Template();

        public void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                npc.Level += 8;
            }
            _base.SetProperties(npcs);
        }

        public override string ToString()
        {
            return "LDON - Convert Level 18 to Level 28";
        }
    }


    class LDONNpc28Template : INPCPropertyTemplate
    {
        public virtual void SetProperties(IEnumerable<NPC> npcs)
        {
            foreach (var npc in npcs)
            {
                if (npc.Race == NPC.TypeRace.InvisibleMan)
                {
                    continue;
                }

                if (npc.Race == NPC.TypeRace.Box || npc.Class == NPC.NPCClass.LDoNObject)
                {
                    npc.HealthPoints = 5000;
                    continue;
                }

                switch (npc.Level)
                {
                    case 25:
                        npc.HealthPoints = 950;
                        npc.MaxDamage = 47;
                        npc.MinDamage = 4;
                        break;
                    case 26:
                        npc.HealthPoints = 1050;
                        npc.MaxDamage = 49;
                        npc.MinDamage = 4;
                        break;
                    case 27:
                        if (npc.Name.Contains("#"))
                        {
                            npc.HealthPoints = 1300;
                            npc.MinDamage = 4;
                            npc.MinDamage = 57;
                        }
                        else
                        {
                            npc.HealthPoints = 1150;
                            npc.MinDamage = 4;
                            npc.MaxDamage = 51;
                        }
                        break;
                    case 31:
                        npc.HealthPoints = 1800;
                        npc.MinDamage = 14;
                        npc.MaxDamage = 72;
                        break;
                }

                npc.AggroRadius = 55;
            }
        }

        public override string ToString()
        {
            return "LDON - (28) Template";
        }
    }
}
