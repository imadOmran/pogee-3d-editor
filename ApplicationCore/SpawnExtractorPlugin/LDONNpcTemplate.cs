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

    class CastersTemplate : INpcPropertyTemplate
    {
        public void SetProperties(IEnumerable<Npc> npcs)
        {
            foreach (var npc in npcs)
            {
                switch (npc.Class)
                {
                    case Npc.NPCClass.Cleric:
                    case Npc.NPCClass.ClericGM:
                        npc.SpellsId = 1;
                        break;
                    case Npc.NPCClass.Shaman:
                    case Npc.NPCClass.ShamanGM:
                        npc.SpellsId = 6;
                        break;
                    case Npc.NPCClass.Wizard:
                    case Npc.NPCClass.WizardGM:
                        npc.SpellsId = 2;
                        break;
                    case Npc.NPCClass.Enchanter:
                    case Npc.NPCClass.EnchanterGM:
                        npc.SpellsId = 5;
                        break;
                    case Npc.NPCClass.Magician:
                    case Npc.NPCClass.MagicianGM:
                        npc.SpellsId = 4;
                        break;
                    case Npc.NPCClass.Necromancer:
                    case Npc.NPCClass.NecromancerGM:
                        npc.SpellsId = 3;
                        break;
                    case Npc.NPCClass.ShadowKnight:
                    case Npc.NPCClass.ShadowKnightGM:
                        npc.SpellsId = 9;
                        break;
                    case Npc.NPCClass.Paladin:
                    case Npc.NPCClass.PaladinGM:
                        npc.SpellsId = 8;
                        break;
                    case Npc.NPCClass.Ranger:
                    case Npc.NPCClass.RangerGM:
                        npc.SpellsId = 10;
                        break;
                    case Npc.NPCClass.Druid:
                    case Npc.NPCClass.DruidGM:
                        npc.SpellsId = 7;
                        break;
                    case Npc.NPCClass.Beastlord:
                    case Npc.NPCClass.BeastlordGM:
                        npc.SpellsId = 12;
                        break;
                    case Npc.NPCClass.Bard:
                    case Npc.NPCClass.BardGM:
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

    class LDONNormalToHighRisk : INpcPropertyTemplate
    {
        private LDONNpcTemplate _normal = new LDONNpcTemplate();

        public void SetProperties(IEnumerable<Npc> npcs)
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

    class LDONGukTemplate : INpcPropertyTemplate
    {
        public void SetProperties(IEnumerable<Npc> npcs)
        {
            foreach (var npc in npcs)
            {
                switch (npc.BodyType)
                {
                    case Npc.TypeBodyType.Undead:
                        break;
                }

                switch (npc.Race)
                {
                    case Npc.TypeRace.FroglokGhost:
                    case Npc.TypeRace.UndeadFroglok:
                    case Npc.TypeRace.WatchfulEye:
                    case Npc.TypeRace.Froglok:
                        npc.FactionId = 725;
                        break;

                    case Npc.TypeRace.Spider:
                    case Npc.TypeRace.Goo:
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

    class LDONRujTemplate : INpcPropertyTemplate
    {
        public void SetProperties(IEnumerable<Npc> npcs)
        {

            foreach (var npc in npcs)
            {
                switch (npc.Race)
                {
                    case Npc.TypeRace.RujarkianOrc:
                    case Npc.TypeRace.NewBear:
                    case Npc.TypeRace.Boar:
                    case Npc.TypeRace.Wolf:
                        npc.FactionId = 722;
                        break;
                    case Npc.TypeRace.NewGoblin:
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

    class LDONNpcTemplate : INpcPropertyTemplate
    {
        public virtual void SetProperties(IEnumerable<Npc> npcs)
        {
            foreach (var npc in npcs)
            {
                if (npc.Race == Npc.TypeRace.InvisibleMan)
                {
                    continue;
                }
                if (npc.Race == Npc.TypeRace.Box || npc.Class == Npc.NPCClass.LDoNObject)
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

    class LDONConvert18To28Template : INpcPropertyTemplate
    {
        private INpcPropertyTemplate _base = new LDONNpc28Template();

        public void SetProperties(IEnumerable<Npc> npcs)
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


    class LDONNpc28Template : INpcPropertyTemplate
    {
        public virtual void SetProperties(IEnumerable<Npc> npcs)
        {
            foreach (var npc in npcs)
            {
                if (npc.Race == Npc.TypeRace.InvisibleMan)
                {
                    continue;
                }

                if (npc.Race == Npc.TypeRace.Box || npc.Class == Npc.NPCClass.LDoNObject)
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
