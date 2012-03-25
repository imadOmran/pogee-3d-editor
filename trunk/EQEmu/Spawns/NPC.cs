﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

using EQEmu.Database;

namespace EQEmu.Spawns
{
    public class NPCAggregator : Database.ManageDatabase
    {
        private ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
        private List<NPC> _cache = new List<NPC>();
        private readonly MySqlConnection _connection;

        private TypeQueriesExtension _lookupByZone = null;

        public NPCAggregator(MySqlConnection connection, Database.QueryConfig config)
            : base(config)
        {
            if (connection == null) throw new Exception("must provide database connection");
            _connection = connection;

            _lookupByZone = _queries.ExtensionQueries.FirstOrDefault(x => x.Name == "LookupByZone");
        }

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
        }

        //this is kind of a pointless property... only used for reflection property setting in query
        public string FilterName
        {
            get;
            set;
        }

        public void ClearCache()
        {
            NPCs.Clear();
            _cache.Clear();
            NeedsDeleted.Clear();
            NeedsInserted.Clear();
        }

        public void AddNPC(NPC npc)
        {
            AddObject(npc);
            _npcs.Add(npc);

            if (_cache.FirstOrDefault(x => x.Id == npc.Id) != null) return;
            _cache.Add(npc);
        }

        public NPC CreateNPC()
        {
            return new NPC(_queryConfig);
        }

        public string GetSQL()
        {
            return GetQuery(_cache);
        }
        
        public void Lookup(string name)
        {
            FilterName = name;
            var sql = String.Format(Queries.SelectQuery,ResolveArgs(Queries.SelectArgs));            
            var results = Database.QueryHelper.RunQuery(_connection,sql);

            _npcs.Clear();

            foreach (var dictionary in results)
            {
                var npc = new NPC(_queryConfig);
                npc.SetProperties(Queries,dictionary);
                AddNPC(npc);
                npc.Created();
            }
        }

        public void LookupByZone(string zone)
        {
            if (_lookupByZone != null)
            {
                var sql = String.Format(_lookupByZone.SelectQuery, zone);
                var results = QueryHelper.RunQuery(_connection, sql);

                _npcs.Clear();

                foreach (var dict in results)
                {
                    var npc = new NPC(_queryConfig);
                    npc.SetProperties(Queries, dict);
                    AddNPC(npc);
                    npc.Created();
                }
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class NPC : Database.DatabaseObject
    {
        public static void SetNPCProperties(ref NPC npc,ZoneEntryStruct data)
        {
            npc.Name = data.SpawnName;
            npc.Level = data.Level;
            npc.Id = (int)data.SpawnID;
            npc.Size = data.Size;
            npc.LastName = data.LastName;

            npc.Gender = (TypeGender)data.Gender;
            npc.Class = (NPCClass)data.Class;
            npc.Race = (TypeRace)data.Race;
            npc.BodyType = (TypeBodyType)data.BodyType;

            npc.WalkSpeed = data.WalkSpeed;
            npc.RunSpeed = data.RunSpeed;

            npc.Face = data.Face;
            npc.Findable = data.Findable;

            npc.DMeleeTexture1 = data.MeleeTexture1;
            npc.DMeleeTexture2 = data.MeleeTexture2;

            npc.LuclinBeard = data.Beard;
            npc.LuclinEyeColor = data.EyeColor1;
            npc.LuclinEyeColor2 = data.EyeColor2;
            npc.LuclinHairColor = data.HairColor;
            npc.LuclinHairStyle = data.HairStyle;

            npc.Texture = data.EquipChest2;
            npc.HelmTexture = data.Helm;

            npc.ArmorTintBlue = data.ArmorTintBlue;
            npc.ArmorTintGreen = data.ArmorTintGreen;
            npc.ArmorTintRed = data.ArmorTintRed;

            npc.DrakkinDetails = data.DrakkinDetails;
            npc.DrakkinHeritage = data.DrakkinHeritage;
            npc.DrakkinTattoo = data.DrakkinTattoo;
        }

        public enum TypeBodyType
        {
            Unknown0 = 0,
            Humanoid = 1,
            Lycanthrop = 2,
            Undead = 3,
            Giant = 4,
            Construct = 5,
            Extraplanar = 6,
            Magical = 7,
            SummonedUndead = 8,
            BaneGiant = 9,
            Unknown10 = 10,
            NoTarget = 11,
            Vampire = 12,
            Atenha_Ra = 13,
            Greater_Akheva = 14,
            Khati_Sha = 15,
            Seru = 16,
            Unknown17 = 17,
            Unknown18 = 18,
            Zek = 19,
            Luggald = 20,
            Animal = 21,
            Insect = 22,
            Monster = 23,
            Summoned = 24,
            Plant = 25,
            Dragon = 26,
            Summoned2 = 27,
            Summoned3 = 28,
            Dragon29 = 29,
            VeliousDragon = 30,
            Familiar = 31,
            Dragon3 = 32,
            Boxes = 33,
            Muramite = 34,
            Unknown35 = 35,
            Unknown36 = 36,
            Unknown37 = 37,
            Unknown38 = 38,
            Unknown39 = 39,
            Unknown40 = 40,
            Unknown41 = 41,
            Unknown42 = 42,
            Unknown43 = 43,
            Unknown44 = 44,
            Unknown45 = 45,
            Unknown46 = 46,
            Unknown47 = 47,
            Unknown48 = 48,
            Unknown49 = 49,
            Unknown50 = 50,
            Unknown51 = 51,
            Unknown52 = 52,
            Unknown53 = 53,
            Unknown54 = 54,
            Unknown55 = 55,
            Unknown56 = 56,
            Unknown57 = 57,
            Unknown58 = 58,
            Unknown59 = 59,
            NoTarget2 = 60,
            Unknown61 = 61,
            Unknown62 = 62,
            SwarmPet = 63,
            Unknown64 = 64,
            Trap = 65,
            InvisMan = 66,
            Special = 67,
            Unknown68 = 68
        }
        public enum TypeRace
        {
                   Doug = 0,
                   Human = 1,
                   Barbarian = 2,
                   Erudite = 3,
                   WoodElf = 4,
                   HighElf = 5,
                   DarkElf = 6,
                   HalfElf = 7,
                   Dwarf = 8,
                   Troll = 9,
                   Ogre = 10,
                   Halfling = 11,
                   Gnome = 12,
                   Aviak = 13,
                   Werewolf = 14,
                   Brownie = 15,
                   Centaur = 16,
                   Golem = 17,
                   GiantCyclops = 18,
                   Trakanon = 19,
                   VenrilSathir4arm = 20,
                   EvilEye = 21,
                   Beetle = 22,
                   Kerran = 23,
                   Fish = 24,
                   Fairy = 25,
                   Froglok = 26,
                   FroglokGhoul = 27,
                   Fungusman = 28,
                   Gargoyle = 29,
                   Gasbag = 30,
                   GelatinousCube = 31,
                   Ghost = 32,
                   Ghoul = 33,
                   GiantBat = 34,
                   GiantEel = 35,
                   GiantRat = 36,
                   GiantSnake = 37,
                   GiantSpider = 38,
                   Gnoll = 39,
                   Goblin = 40,
                   Gorilla = 41,
                   Wolf = 42,
                   Bear = 43,
                   FreeportGuard = 44,
                   DemiLich = 45,
                   Imp = 46,
                   Griffin = 47,
                   Kobold = 48,
                   LavaDragon = 49,
                   Lion = 50,
                   LizardMan = 51,
                   Mimic = 52,
                   Minotaur = 53,
                   Orc = 54,
                   HumanBeggar = 55,
                   Pixie = 56,
                   Dracnid = 57,
                   SolusekRo = 58,
                   Bloodgill = 59,
                   Skeleton = 60,
                   Shark = 61,
                   Tunare = 62,
                   Tiger = 63,
                   Treant = 64,
                   Vampire = 65,
                   StatueofRallosZek = 66,
                   HighpassCitizen = 67,
                   Tentacle = 68,
                   Wisp = 69,
                   Zombie = 70,
                   QeynosCitizen = 71,
                   Ship = 72,
                   Launch = 73,
                   Piranha = 74,
                   Elemental = 75,
                   Puma = 76,
                   NeriakCitizen = 77,
                   EruditeCitizen = 78,
                   Bixie = 79,
                   ReanimatedHand = 80,
                   RivervaleCitizen = 81,
                   Scarecrow = 82,
                   Skunk = 83,
                   SnakeElemental = 84,
                   Spectre = 85,
                   Sphinx = 86,
                   Armadillo = 87,
                   ClockworkGnome = 88,
                   Drake = 89,
                   HalasCitizen = 90,
                   Alligator = 91,
                   GrobbCitizen = 92,
                   OggokCitizen = 93,
                   KaladimCitizen = 94,
                   CazicThule = 95,
                   Cockatrice = 96,
                   DaisyMan = 97,
                   ElfVampire = 98,
                   Denizen = 99,
                   Dervish = 100,
                   Efreeti = 101,
                   FroglokTadpole = 102,
                   PhinigelAutropos = 103,
                   Leech = 104,
                   Swordfish = 105,
                   Felguard = 106,
                   Mammoth = 107,
                   EyeofZomm = 108,
                   Wasp = 109,
                   Mermaid = 110,
                   Harpie = 111,
                   Fayguard = 112,
                   Drixie = 113,
                   GhostShip = 114,
                   Clam = 115,
                   SeaHorse = 116,
                   DwarfGhost = 117,
                   EruditeGhost = 118,
                   Sabertooth = 119,
                   WolfElemental = 120,
                   Gorgon = 121,
                   DragonSkeleton = 122,
                   Innoruuk = 123,
                   Unicorn = 124,
                   Pegasus = 125,
                   Djinn = 126,
                   InvisibleMan = 127,
                   Iksar = 128,
                   Scorpion = 129,
                   VahShir = 130,
                   Sarnak = 131,
                   Draglock = 132,
                   Lycanthrope = 133,
                   Mosquito = 134,
                   Rhino = 135,
                   Xalgoz = 136,
                   KunarkGoblin = 137,
                   Yeti = 138,
                   IksarCitizen = 139,
                   ForestGiant = 140,
                   Boat = 141,
                   MinorIllusion = 142,
                   TreeIllusion = 143,
                   Burynai = 144,
                   Goo = 145,
                   SpectralSarnak = 146,
                   SpectralIksar = 147,
                   KunarkFish = 148,
                   IksarScorpion = 149,
                   Erollisi = 150,
                   Tribunal = 151,
                   Bertoxxulous = 152,
                   Bristlebane = 153,
                   FayDrake = 154,
                   SarnakSkeleton = 155,
                   Ratman = 156,
                   Wyvern = 157,
                   Wurm = 158,
                   Devourer = 159,
                   IksarGolem = 160,
                   IksarSkeleton = 161,
                   ManEatingPlant = 162,
                   Raptor = 163,
                   SarnakGolem = 164,
                   WaterDragon = 165,
                   IksarHand = 166,
                   Succulent = 167,
                   Holgresh = 168,
                   Brontotherium = 169,
                   SnowDervish = 170,
                   DireWolf = 171,
                   Manticore = 172,
                   Totem = 173,
                   ColdSpectre = 174,
                   EnchantedArmor = 175,
                   SnowBunny = 176,
                   Walrus = 177,
                   RockGemMan = 178,
                   Unknown179 = 179,
                   Unknown180 = 180,
                   YakMan = 181,
                   Faun = 182,
                   Coldain = 183,
                   VeliousDragon = 184,
                   Hag = 185,
                   Hippogriff = 186,
                   Siren = 187,
                   FrostGiant = 188,
                   StormGiant = 189,
                   Otterman = 190,
                   WalrusMan = 191,
                   ClockworkDragon = 192,
                   Abhorrent = 193,
                   SeaTurtle = 194,
                   BlackandWhiteDragon = 195,
                   GhostDragon = 196,
                   RonnieTest = 197,
                   PrismaticDragon = 198,
                   Shiknar = 199,
                   Rockhopper = 200,
                   Underbulk = 201,
                   Grimling = 202,
                   VacuumWorm = 203,
                   EvanTest = 204,
                   KahliShah = 205,
                   Owlbear = 206,
                   RhinoBeetle = 207,
                   Vampyre = 208,
                   EarthElemental = 209,
                   AirElemental = 210,
                   WaterElemental = 211,
                   FireElemental = 212,
                   WetfangMinnow = 213,
                   ThoughtHorror = 214,
                   Tegi = 215,
                   Horse = 216,
                   Shissar = 217,
                   FungalFiend = 218,
                   VampireVolatalis = 219,
                   StoneGrabber = 220,
                   ScarletCheetah = 221,
                   Zelniak = 222,
                   Lightcrawler = 223,
                   Shade = 224,
                   Sunflower = 225,
                   KhatiSha = 226,
                   Shrieker = 227,
                   Galorian = 228,
                   Netherbian = 229,
                   Akhevan = 230,
                   SpireSpirit = 231,
                   SonicWolf = 232,
                   GroundShaker = 233,
                   VahShirSkeleton = 234,
                   MutantHumanoid = 235,
                   LordInquisitorSeru = 236,
                   Recuso = 237,
                   VahShirKing = 238,
                   VahShirGuard = 239,
                   TeleportMan = 240,
                   Lujein = 241,
                   Naiad = 242,
                   Nymph = 243,
                   Ent = 244,
                   Wrinnfly = 245,
                   Coirnav = 246,
                   SolusekRo247 = 247,
                   ClockworkGolem = 248,
                   ClockworkBrain = 249,
                   SpectralBanshee = 250,
                   GuardofJustice = 251,
                   PoMCastle = 252,
                   DiseaseBoss = 253,
                   SolusekRoGuard = 254,
                   BertoxxulousNew = 255,
                   TribunalNew = 256,
                   TerrisThule = 257,
                   Vegerog = 258,
                   Crocodile = 259,
                   Bat = 260,
                   Slarghilug = 261,
                   Tranquilion = 262,
                   TinSoldier = 263,
                   NightmareWraith = 264,
                   Malarian = 265,
                   KnightofPestilence = 266,
                   Lepertoloth = 267,
                   BubonianBoss = 268,
                   BubonianUnderling = 269,
                   Pusling = 270,
                   WaterMephit = 271,
                   Stormrider = 272,
                   JunkBeast = 273,
                   BrokenClockwork = 274,
                   GiantClockwork = 275,
                   ClockworkBeetle = 276,
                   NightmareGoblin = 277,
                   Karana = 278,
                   BloodRaven = 279,
                   NightmareGargoyle = 280,
                   MouthofInsanity = 281,
                   SkeletalHorse = 282,
                   Saryrn = 283,
                   FenninRo = 284,
                   Tormentor = 285,
                   NecromancerPriest = 286,
                   Nightmare = 287,
                   NewRallosZek = 288,
                   VallonZek = 289,
                   TallonZek = 290,
                   AirMephit = 291,
                   EarthMephit = 292,
                   FireMephit = 293,
                   NightmareMephit = 294,
                   Zebuxoruk = 295,
                   MithanielMarr = 296,
                   KnightmareRider = 297,
                   RatheCouncilman = 298,
                   Xegony = 299,
                   DemonFiend = 300,
                   TestObject = 301,
                   LobsterMonster = 302,
                   Phoenix = 303,
                   Quarm = 304,
                   NewBear = 305,
                   EarthGolem = 306,
                   IronGolem = 307,
                   StormGolem = 308,
                   AirGolem = 309,
                   WoodGolem = 310,
                   FireGolem = 311,
                   WaterGolem = 312,
                   VeiledGargoyle = 313,
                   Lynx = 314,
                   Squid = 315,
                   Frog = 316,
                   FlyingSerpent = 317,
                   TacticsSoldier = 318,
                   ArmoredBoar = 319,
                   Djinni = 320,
                   Boar = 321,
                   KnightofMarr = 322,
                   ArmorofMarr = 323,
                   NightmareKnight = 324,
                   RallosOgre = 325,
                   Arachnid = 326,
                   CrystalArachnid = 327,
                   TowerModel = 328,
                   Portal = 329,
                   Froglok330 = 330,
                   TrollCrewMember = 331,
                   PirateDeckhand = 332,
                   BrokenSkullPirate = 333,
                   PirateGhost = 334,
                   OnearmedPirate = 335,
                   SpiritmasterNadox = 336,
                   BrokenSkullTaskmaster = 337,
                   GnomePirate = 338,
                   DarkElfPirate = 339,
                   OgrePirate = 340,
                   HumanPirate = 341,
                   EruditePirate = 342,
                   Frog343 = 343,
                   UndeadPirate = 344,
                   LuggaldWorker = 345,
                   LuggaldSoldier = 346,
                   LuggaldDisciple = 347,
                   Drogmor = 348,
                   FroglokSkeleton = 349,
                   UndeadFroglok = 350,
                   KnightofHate = 351,
                   WarlockofHate = 352,
                   Highborn = 353,
                   HighbornDiviner = 354,
                   HighbornCrusader = 355,
                   Chokidai = 356,
                   UndeadChokidai = 357,
                   UndeadVeksar = 358,
                   UndeadVampire = 359,
                   Vampire360 = 360,
                   RujarkianOrc = 361,
                   BoneGolem = 362,
                   Synarcana = 363,
                   SandElf = 364,
                   MasterVampire = 365,
                   MasterOrc = 366,
                   NewSkeleton = 367,
                   CryptCreeper = 368,
                   NewGoblin = 369,
                   BurrowerBug = 370,
                   FroglokGhost = 371,
                   Vortex = 372,
                   Shadow = 373,
                   GolemBeast = 374,
                   WatchfulEye = 375,
                   Box = 376,
                   Barrel = 377,
                   Chest = 378,
                   Vase = 379,
                   FrozenTable = 380,
                   WeaponRack = 381,
                   Coffin = 382,
                   SkullandBones = 383,
                   Jester = 384,
                   TaelosianNative = 385,
                   TaelosianEvoker = 386,
                   TaelosianGolem = 387,
                   TaelosianWolf = 388,
                   TaelosianAmphibianCreature = 389,
                   TaelosianMountainBeast = 390,
                   TaelosianStonemite = 391,
                   UkunWarHound = 392,
                   IxtCentaur = 393,
                   IkaavSnakewoman = 394,
                   Aneuk = 395,
                   KyvHunter = 396,
                   NocSprayblood = 397,
                   RatukBrute = 398,
                   Ixt = 399,
                   Huvul = 400,
                   MastruqWarfiend = 401,
                   Mastruq = 402,
                   Taelosian = 403,
                   Ship404 = 404,
                   NewGolem = 405,
                   OverlordMataMuram = 406,
                   Lightingwarrior = 407,
                   Succubus = 408,
                   Bazu = 409,
                   Feran = 410,
                   Pyrilen = 411,
                   Chimera = 412,
                   Dragorn = 413,
                   Murkglider = 414,
                   Rat = 415,
                   Bat416 = 416,
                   Gelidran = 417,
                   Discordling = 418,
                   Girplan = 419,
                   Minotaur420 = 420,
                   DragornBox = 421,
                   RunedOrb = 422,
                   DragonBones = 423,
                   MuramiteArmorPile = 424,
                   CrystalShard = 425,
                   Portal426 = 426,
                   CoinPurse = 427,
                   RockPile = 428,
                   MurkgliderEggSack = 429,
                   Drake430 = 430,
                   Dervish431 = 431,
                   Drake432 = 432,
                   Goblin433 = 433,
                   Kirin = 434,
                   Dragon = 435,
                   Basilisk = 436,
                   Dragon437 = 437,
                   Dragon438 = 438,
                   Puma439 = 439,
                   Spider = 440,
                   SpiderQueen = 441,
                   AnimatedStatue = 442,
                   Unknown443 = 443,
                   Unknown444 = 444,
                   DragonEgg = 445,
                   DragonStatue = 446,
                   LavaRock = 447,
                   AnimatedStatue448 = 448,
                   SpiderEggSack = 449,
                   LavaSpider = 450,
                   LavaSpiderQueen = 451,
                   Dragon452 = 452,
                   Giant = 453,
                   Werewolf454 = 454,
                   Kobold455 = 455,
                   Sporali = 456,
                   Gnomework = 457,
                   Orc458 = 458,
                   Corathus = 459,
                   Coral = 460,
                   Drachnid = 461,
                   DrachnidCocoon = 462,
                   FungusPatch = 463,
                   Gargoyle464 = 464,
                   Witheran = 465,
                   DarkLord = 466,
                   Shiliskin = 467,
                   Snake = 468,
                   EvilEye469 = 469,
                   Minotaur470 = 470,
                   Zombie471 = 471,
                   ClockworkBoar = 472,
                   Fairy473 = 473,
                   Witheran474 = 474,
                   AirElemental475 = 475,
                   EarthElemental476 = 476,
                   FireElemental477 = 477,
                   WaterElemental478 = 478,
                   Alligator479 = 479,
                   Bear480 = 480,
                   ScaledWolf = 481,
                   Wolf482 = 482,
                   SpiritWolf = 483,
                   Skeleton484 = 484,
                   Spectre485 = 485,
                   Bolvirk = 486,
                   Banshee = 487,
                   Banshee488 = 488,
                   Elddar = 489,
                   ForestGiant490 = 490,
                   BoneGolem491 = 491,
                   Horse492 = 492,
                   Pegasus493 = 493,
                   ShamblingMound = 494,
                   Scrykin = 495,
                   Treant496 = 496,
                   Vampire497 = 497,
                   AyonaeRo = 498,
                   SullonZek = 499,
                   Banner = 500,
                   Flag = 501,
                   Rowboat = 502,
                   BearTrap = 503,
                   ClockworkBomb = 504,
                   DynamiteKeg = 505,
                   PressurePlate = 506,
                   PufferSpore = 507,
                   StoneRing = 508,
                   RootTentacle = 509,
                   RunicSymbol = 510,
                   SaltpetterBomb = 511,
                   FloatingSkull = 512,
                   SpikeTrap = 513,
                   Totem514 = 514,
                   Web = 515,
                   WickerBasket = 516,
                   NightmareUnicorn = 517,
                   Horse518 = 518,
                   NightmareUnicorn519 = 519,
                   Bixie520 = 520,
                   Centaur521 = 521,
                   Drakkin = 522,
                   Giant523 = 523,
                   Gnoll524 = 524,
                   Griffin525 = 525,
                   GiantShade = 526,
                   Harpy = 527,
                   Mammoth528 = 528,
                   Satyr = 529,
                   Dragon530 = 530,
                   Dragon531 = 531,
                   DynLeth = 532,
                   Boat533 = 533,
                   WeaponRack534 = 534,
                   ArmorRack = 535,
                   HoneyPot = 536,
                   JumJumBucket = 537,
                   Plant538 = 538,
                   Plant539 = 539,
                   Plant540 = 540,
                   Toolbox = 541,
                   WineCask = 542,
                   StoneJug = 543,
                   ElvenBoat = 544,
                   GnomishBoat = 545,
                   BarrelBargeShip = 546,
                   Goo547 = 547,
                   Goo548 = 548,
                   Goo549 = 549,
                   MerchantShip = 550,
                   PirateShip = 551,
                   GhostShip552 = 552,
                   Banner553 = 553,
                   Banner554 = 554,
                   Banner555 = 555,
                   Banner556 = 556,
                   Banner557 = 557,
                   Aviak558 = 558,
                   Beetle559 = 559,
                   Gorilla560 = 560,
                   Kedge = 561,
                   Kerran562 = 562,
                   Shissar563 = 563,
                   Siren564 = 564,
                   Sphinx565 = 565,
                   Human566 = 566,
                   Campfire = 567,
                   Brownie568 = 568,
                   Dragon569 = 569,
                   Exoskeleton = 570,
                   Ghoul571 = 571,
                   ClockworkGuardian = 572,
                   Mantrap = 573,
                   Minotaur574 = 574,
                   Scarecrow575 = 575,
                   Shade576 = 576,
                   Rotocopter = 577,
                   TentacleTerror = 578,
                   Wereorc = 579,
                   Worg = 580,
                   Wyvern581 = 581,
                   Chimera582 = 582,
                   Kirin583 = 583,
                   Puma584 = 584,
                   Boulder = 585,
                   Banner586 = 586,
                   ElvenGhost = 587,
                   HumanGhost = 588,
                   Chest589 = 589,
                   Chest590 = 590,
                   Crystal = 591,
                   Coffin592 = 592,
                   GuardianCPU = 593,
                   Worg594 = 594,
                   Mansion = 595,
                   FloatingIsland = 596,
                   Cragslither = 597,
                   Wrulon = 598,
                   Unknown599 = 599,
                   Unknown600 = 600,
                   Unknown601 = 601,
                   Burynai602 = 602,
                   Frog603 = 603,
                   Dracolich = 604,
                   IksarGhost = 605,
                   IksarSkeleton606 = 606,
                   Mephit = 607,
                   Muddite = 608,
                   Raptor609 = 609,
                   Sarnak610 = 610,
                   Scorpion611 = 611,
                   Tsetsian = 612,
                   Wurm613 = 613,
                   Balrog = 614,
                   HydraCrystal = 615,
                   CrystalSphere = 616,
                   Gnoll617 = 617,
                   Sokokar = 618,
                   StonePylon = 619,
                   DemonVulture = 620,
                   Wagon = 621,
                   GodofDiscord = 622,
                   WrulonMount = 623,
                   Unknown624 = 624,
                   SokokarMount = 625,
                   Unknown626 = 626,
                   SokokarMount627 = 627,
                   Unknown628 = 628,
                   Unknown629 = 629,
                   Unknown630 = 630,
                   Unknown631 = 631,
                   Unknown632 = 632,
                   Unknown633 = 633,
                   Unknown634 = 634,
                   Unknown635 = 635,
                   Unknown636 = 636,
                   Unknown637 = 637,
                   Unknown638 = 638,
                   Unknown639 = 639,
                   Unknown640 = 640,
                   Unknown641 = 641,
                   Unknown642 = 642,
                   Unknown643 = 643,
                   Unknown644 = 644,
                   Unknown645 = 645,
                   Unknown646 = 646,
                   Unknown647 = 647,
                   Unknown648 = 648,
                   Unknown649 = 649,
                   Unknown650 = 650,
                   Unknown651 = 651
        }
        public enum NPCClass
        {
            Soldier = 0,
            Warrior = 1,
            Cleric = 2,
            Paladin = 3,
            Ranger = 4,
            ShadowKnight = 5,
            Druid = 6,
            Monk = 7,
            Bard = 8,
            Rogue = 9,
            Shaman = 10,
            Necromancer = 11,
            Wizard = 12,
            Magician = 13,
            Enchanter = 14,
            Beastlord = 15,
            Berserker = 16,
            Banker = 17,
            Unknown18 = 18,
            Unknown19 = 19,
            WarriorGM = 20,
            ClericGM = 21,
            PaladinGM = 22,
            RangerGM = 23,
            ShadowKnightGM = 24,
            DruidGM = 25,
            MonkGM = 26,
            BardGM = 27,
            RogueGM = 28,
            ShamanGM = 29,
            NecromancerGM = 30,
            WizardGM = 31,
            MagicianGM = 32,
            EnchanterGM = 33,
            BeastlordGM = 34,
            BerserkerGM = 35,
            Unknown36 = 36,
            Unkown37 = 37,
            Unknown38 = 38,
            Unknown39 = 39,
            Banker40 = 40,
            Shopkeeper = 41,
            Unknown42,Unknown43,Unknown44,Unknown45,Unknown46,Unknown47,Unknown48,
            Unknown49,Unknown50,Unknown51,Unknown52,Unknown53,Unknown54,Unknown55,
            Unknown56,Unknown57,Unknown58,
            DiscordMerchant = 59,
            LDoNAdventureRecruiter = 60,
            LDoNAdventureMerchant = 61,
            LDoNObject = 62,
            TributeMaster = 63,
            GuildTributeMaster = 64,
            Unknown65 = 65,
            GuildBanker = 66,
            NorrathKeepersMerchant = 67,
            DarkReignMerchant = 68,
            Unknown69 = 69,
            Unknown70,
            MercenaryLiaison = 71                   
        }
        public enum TypeGender
        {
            Male = 0,
            Female = 1,
            It = 2,
            Unknown = 9
        }

        private string _name;
        private string _lastName;

        private int _level;
        private int _id;
        private int _lootTableId;
        private int _hp;
        private int _minDamage;
        private int _maxDamage;
        private int _texture;
        private int _helmTexture;
        private int _version;
        private int _face;
        private int _luclinHairStyle;
        private int _luclinHairColor;
        private int _luclinEyeColor;
        private int _luclinEyeColor2;
        private int _luclinBeard;        
        private uint _dMeleeTexture1;
        private uint _dMeleeTexture2;
        private int _armorTintRed;
        private int _armorTintGreen;
        private int _armorTintBlue;
        private uint _drakkinHeritage;
        private uint _drakkinTattoo;
        private uint _drakkinDetails;
        private int _aggroRadius;
        private int _npcFactionId;
        private int _npcSpellsId;
        
        private float _size;
        private float _runSpeed;
        private float _walkSpeed;

        private bool _findable;

        private TypeGender _gender;
        private NPCClass _class;
        private TypeRace _race;
        private TypeBodyType _bodyType;

        public NPC(Database.QueryConfig conf)
            : base(conf)
        {

        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Dirtied();
            }
        }

        public TypeBodyType BodyType
        {
            get { return _bodyType; }
            set
            {
                _bodyType = value;
                Dirtied();
            }
        }
    

        public TypeRace Race
        {
            get { return _race; }
            set {
                _race = value;
                Dirtied();
            }
        }

        public NPCClass Class
        {
            get { return _class; }
            set
            {
                _class = value;
                Dirtied();
            }
        }

        public TypeGender Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                Dirtied();
            }
        }

        public float Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Dirtied();
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                Dirtied();
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                Dirtied();
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Dirtied();
            }
        }

        public int HealthPoints
        {
            get { return _hp; }
            set
            {
                _hp = value;
                Dirtied();
            }
        }

        public int MaxDamage
        {
            get { return _maxDamage; }
            set
            {
                _maxDamage = value;
                Dirtied();
            }
        }

        public int MinDamage
        {
            get { return _minDamage; }
            set
            {
                _minDamage = value;
                Dirtied();
            }
        }

        public int LootTableId
        {
            get { return _lootTableId; }
            set
            {
                _lootTableId = value;
                Dirtied();
            }
        }

        public int SpellsId
        {
            get { return _npcSpellsId; }
            set
            {
                _npcSpellsId = value;
                Dirtied();
            }
        }

        public int FactionId
        {
            get { return _npcFactionId; }
            set
            {
                _npcFactionId = value;
                Dirtied();
            }
        }

        public int AggroRadius
        {
            get { return _aggroRadius; }
            set
            {
                _aggroRadius = value;
                Dirtied();
            }
        }

        public float RunSpeed
        {
            get { return _runSpeed; }
            set
            {
                _runSpeed = value;
                Dirtied();
            }
        }

        public float WalkSpeed
        {
            get { return _walkSpeed; }
            set
            {
                _walkSpeed = value;
                Dirtied();
            }
        }

        public int Texture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                Dirtied();
            }
        }

        public int HelmTexture
        {
            get { return _helmTexture; }
            set
            {
                _helmTexture = value;
                Dirtied();
            }
        }

        public int Version
        {
            get { return _version; }
            set
            {
                _version = value;
                Dirtied();
            }
        }

        public int Face
        {
            get { return _face; }
            set
            {
                _face = value;
                Dirtied();
            }
        }

        public int LuclinHairStyle
        {
            get { return _luclinHairStyle; }
            set
            {
                _luclinHairStyle = value;
                Dirtied();
            }
        }

        public int LuclinHairColor
        {
            get { return _luclinHairColor; }
            set {
                _luclinHairColor = value;
                Dirtied();
            }
        }

        public int LuclinEyeColor
        {
            get { return _luclinEyeColor; }
            set
            {
                _luclinEyeColor = value;
                Dirtied();
            }
        }

        public int LuclinEyeColor2
        {
            get { return _luclinEyeColor2; }
            set
            {
                _luclinEyeColor2 = value;
                Dirtied();
            }
        }

        public int LuclinBeard
        {
            get { return _luclinBeard; }
            set
            {
                _luclinBeard = value;
                Dirtied();
            }
        }

        public bool Findable
        {
            get { return _findable; }
            set
            {
                _findable = value;
                Dirtied();
            }
        }

        public uint DMeleeTexture1
        {
            get { return _dMeleeTexture1; }
            set
            {
                _dMeleeTexture1 = value;
                Dirtied();
            }
        }

        public uint DMeleeTexture2
        {
            get { return _dMeleeTexture2; }
            set
            {
                _dMeleeTexture2 = value;
                Dirtied();
            }
        }

        public int ArmorTintRed
        {
            get { return _armorTintRed; }
            set
            {
                _armorTintRed = value;
                Dirtied();
            }
        }

        public int ArmorTintGreen
        {
            get { return _armorTintGreen; }
            set
            {
                _armorTintGreen = value;
                Dirtied();
            }
        }

        public int ArmorTintBlue
        {
            get { return _armorTintBlue; }
            set
            {
                _armorTintBlue = value;
                Dirtied();
            }
        }

        public uint DrakkinHeritage
        {
            get { return _drakkinHeritage; }
            set
            {
                _drakkinHeritage = value;
                Dirtied();
            }
        }

        public uint DrakkinTattoo
        {
            get { return _drakkinTattoo; }
            set
            {
                _drakkinTattoo = value;
                Dirtied();
            }
        }

        public uint DrakkinDetails
        {
            get { return _drakkinDetails; }
            set
            {
                _drakkinDetails = value;
                Dirtied();
            }
        }
    }

    public class ZoneEntryStruct
    {
        public ZoneEntryStruct()
        {
            SlotColour = new UInt32[9];
            Equipment = new UInt32[9];
        }

        public string SpawnName;
        public UInt32 SpawnID;
        public bool Findable;
        public Byte Level;
        public Byte IsNPC;
        public uint Showname;
        public uint TargetableWithHotkey;
        public uint Targetable;
        public uint ShowHelm;
        public uint Gender;
        public byte OtherData;
        public string DestructableString1;
        public string DestructableString2;
        public string DestructableString3;
        public UInt32 DestructableUnk1;
        public UInt32 DestructableUnk2;
        public UInt32 DestructableID1;
        public UInt32 DestructableID2;
        public UInt32 DestructableID3;
        public UInt32 DestructableID4;
        public UInt32 DestructableUnk3;
        public UInt32 DestructableUnk4;
        public UInt32 DestructableUnk5;
        public UInt32 DestructableUnk6;
        public UInt32 DestructableUnk7;
        public UInt32 DestructableUnk8;
        public UInt32 DestructableUnk9;
        public byte DestructableByte;
        public float Size;
        public byte Face;
        public float WalkSpeed;
        public float RunSpeed;
        public UInt32 Race;
        public byte PropCount;
        public UInt32 BodyType;
        public byte HairColor;
        public byte BeardColor;
        public byte EyeColor1;
        public byte EyeColor2;
        public byte HairStyle;
        public byte Beard;
        public UInt32 DrakkinHeritage;
        public UInt32 DrakkinTattoo;
        public UInt32 DrakkinDetails;
        public UInt32 Deity;
        public byte Class;
        public byte EquipChest2;
        public byte Helm;
        public string LastName;
        public UInt32 PetOwnerID;
        public float YPos;
        public float Heading;
        public float XPos;
        public float ZPos;
        public UInt32[] SlotColour;
        public byte ArmorTintRed;
        public byte ArmorTintGreen;
        public byte ArmorTintBlue;
        public UInt32 MeleeTexture1;
        public UInt32 MeleeTexture2;
        public UInt32[] Equipment;
        public string Title;
        public string Suffix;
        public byte IsMercenary;
        public UInt32 Padding5;
        public UInt32 Padding7;
        public UInt32 Padding26;

    }
}
