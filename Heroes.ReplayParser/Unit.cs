using System;
using System.Collections.Generic;
using System.Linq;

namespace Heroes.ReplayParser
{
    public class Unit
    {
        public int UnitID { get; set; }
        public string Name { get; set; }
        public UnitGroup Group { get; set; }
        public TimeSpan TimeSpanBorn { get; set; }
        public TimeSpan? TimeSpanAcquired { get; set; }
        public TimeSpan? TimeSpanDied { get; set; }
        public int? Team { get; set; }
        public Player PlayerControlledBy { get; set; }
        public Player PlayerKilledBy { get; set; }
        public Unit UnitKilledBy { get; set; }
        public Point PointBorn { get; set; }
        public Point PointDied { get; set; }
        public List<Position> Positions { get; set; }
        public List<OwnerChangeEvent> OwnerChangeEvents { get; set; }

        public Unit()
        {
            Positions = new List<Position>();
            OwnerChangeEvents = new List<OwnerChangeEvent>();
        }

        public override string ToString()
        {
            return TimeSpanBorn + ": " + PointBorn + ": " + Name + ": " + PlayerControlledBy;
        }

        public static int GetUnitID(int unitIDIndex, int unitIDRecycle)
        {
            return unitIDIndex << 18 | unitIDRecycle;
        }

        public enum UnitGroup
        {
            Hero,
            HeroAbilityUse,
            Structures,
            MapObjective,
            MercenaryCamp,
            Minions,
            Miscellaneous,
            HeroTalentSelection,
            Unknown
        }

        public static readonly Dictionary<UnitGroup, string> UnitGroupFriendlyName = new Dictionary<UnitGroup, string> {
            { UnitGroup.Hero, "Heroes" },
            { UnitGroup.HeroAbilityUse, "Abilities" },
            { UnitGroup.Structures, "Structures" },
            { UnitGroup.MapObjective, "Map Objectives" },
            { UnitGroup.MercenaryCamp, "Mercenary Camps" },
            { UnitGroup.Minions, "Minions" },
            { UnitGroup.Miscellaneous, "Miscellaneous" },
            { UnitGroup.HeroTalentSelection, "Talent Selections" },
            { UnitGroup.Unknown, "Unknown" }
        };

        public static readonly Dictionary<string, bool> UnitGivesExpDictionary = new Dictionary<string, bool> {
            { "CatapultMinion", true },
            { "FootmanMinion", true },
            { "JungleGraveGolemDefender", true },
            { "JungleGraveGolemLaner", true },
            { "JunglePlantHorror", true },
            { "MercDefenderMeleeOgre", true },
            { "MercDefenderRangedOgre", true },
            { "MercDefenderSiegeGiant", true },
            { "MercLanerMeleeOgre", true },
            { "MercLanerRangedOgre", true },
            { "MercLanerSiegeGiant", true },
            { "PlantZombie", true },
            { "PlantZombieRanged", true },
            { "RangedMinion", true },
            { "SkeletalPirate", true },
            { "UnderworldBoss", true },
            { "UnderworldMinion", true },
            { "UnderworldRangedMinion", true },
            { "UnderworldSummonedBoss", true },
            { "WizardMinion", true }
        };

        public static readonly Dictionary<string, bool> UnitBornProvidesLocationForOwner = new Dictionary<string, bool> {
            { "AnubarakBeetleSpitBeetle", true },
            { "AnubarakCarrionSwarmBug", true },
            { "AnubarakCarrionSwarmMasteryBug", true },
            { "AzmodanDemon", true },
            { "DemonHunterCaltrop", true },
            { "ImpalingBladesZergling", true },
            { "MurkyRespawnEgg", true },
            { "SgtHammerMine", true },
            { "SgtHammerBullheadMine", true },
            { "StitchesStinkling", true },
            { "TinkerRockItTurret", true },
            { "WitchDoctorPlagueToad", true },
            { "ZagaraBaneling", true },
            { "ZagaraCreepTumor", true },
            { "ZagaraHydralisk", true },
            { "ZagaraMutalisk", true },
            { "ZeratulBlinkRelicUnit", true }
        };

        public static readonly Dictionary<string, bool> UnitOwnerChangeProvidesLocationForOwner = new Dictionary<string, bool> {
            { "ItemCannonball", true },
            { "ItemSeedPickup", true },
            { "ItemUnderworldPowerup", true },
            { "RavenLordTribute", true },
            { "RegenGlobe", true },
            { "RegenGlobeNeutral", true }
        };

        public static readonly Dictionary<string, UnitGroup> UnitGroupDictionary = new Dictionary<string, UnitGroup> {
            { "HeroAbathur", UnitGroup.Hero },
            { "HeroAnubarak", UnitGroup.Hero },
            { "HeroArthas", UnitGroup.Hero },
            { "HeroAzmodan", UnitGroup.Hero },
            { "HeroBaleog", UnitGroup.Hero },
            { "HeroBarbarian", UnitGroup.Hero },
            { "HeroChen", UnitGroup.Hero },
            { "HeroChenEarth", UnitGroup.Hero },
            { "HeroChenEarthConduit", UnitGroup.Hero },
            { "HeroChenFire", UnitGroup.Hero },
            { "HeroChenFireConduit", UnitGroup.Hero },
            { "HeroChenStorm", UnitGroup.Hero },
            { "HeroChenStormConduit", UnitGroup.Hero },
            { "HeroDemonHunter", UnitGroup.Hero },
            { "HeroDiablo", UnitGroup.Hero },
            { "HeroErik", UnitGroup.Hero },
            { "HeroFaerieDragon", UnitGroup.Hero },
            { "HeroFalstad", UnitGroup.Hero },
            { "HeroIllidan", UnitGroup.Hero },
            { "HeroJaina", UnitGroup.Hero },
            { "HeroKerrigan", UnitGroup.Hero },
            { "HeroL90ETC", UnitGroup.Hero },
            { "HeroLiLi", UnitGroup.Hero },
            { "HeroMalfurion", UnitGroup.Hero },
            { "HeroMuradin", UnitGroup.Hero },
            { "HeroMurky", UnitGroup.Hero },
            { "HeroNova", UnitGroup.Hero },
            { "HeroOlaf", UnitGroup.Hero },
            { "HeroRaynor", UnitGroup.Hero },
            { "HeroRehgar", UnitGroup.Hero },
            { "HeroSgtHammer", UnitGroup.Hero },
            { "HeroStitches", UnitGroup.Hero },
            { "HeroSylvanas", UnitGroup.Hero },
            { "HeroTassadar", UnitGroup.Hero },
            { "HeroThrall", UnitGroup.Hero },
            { "HeroTinker", UnitGroup.Hero },
            { "HeroTychus", UnitGroup.Hero },
            { "HeroTyrael", UnitGroup.Hero },
            { "HeroTyrande", UnitGroup.Hero },
            { "HeroUther", UnitGroup.Hero },
            { "HeroWitchDoctor", UnitGroup.Hero },
            { "HeroZagara", UnitGroup.Hero },
            { "HeroZeratul", UnitGroup.Hero },
            { "LongboatRaidBoat", UnitGroup.Hero },
            { "MurkyRespawnEgg", UnitGroup.Hero },
            
            { "ItemMULE", UnitGroup.HeroAbilityUse },
            { "ScoutingDrone", UnitGroup.HeroAbilityUse },
            { "TalentHealingWard", UnitGroup.HeroAbilityUse },

            { "AbathurEvolvedMonstrosity", UnitGroup.HeroAbilityUse },
            { "AbathurLocustAssaultStrain", UnitGroup.HeroAbilityUse },
            { "AbathurLocustBombardStrain", UnitGroup.HeroAbilityUse },
            { "AbathurLocustNest", UnitGroup.HeroAbilityUse },
            { "AbathurLocustNestItem", UnitGroup.HeroAbilityUse },
            { "AbathurLocustNormal", UnitGroup.HeroAbilityUse },
            { "AbathurSymbiote", UnitGroup.HeroAbilityUse },
            { "AbathurToxicNest", UnitGroup.HeroAbilityUse },
            { "AnubarakBeetleSpitBeetle", UnitGroup.HeroAbilityUse },
            { "AnubarakCarrionSwarmBug", UnitGroup.HeroAbilityUse },
            { "AnubarakCarrionSwarmMasteryBug", UnitGroup.HeroAbilityUse },
            { "AnubarakNewWebBlastCocoon", UnitGroup.HeroAbilityUse },
            { "AnubarakNewWebBlastCocoonFat", UnitGroup.HeroAbilityUse },
            { "ArthasGhoul", UnitGroup.HeroAbilityUse },
            { "AzmodanDemon", UnitGroup.HeroAbilityUse },
            { "AzmodanDemonicInvasionZombie", UnitGroup.HeroAbilityUse },
            { "AzmodanDemonLieutenant", UnitGroup.HeroAbilityUse },
            { "BaleogPlayAgainGhost", UnitGroup.HeroAbilityUse },
            { "BarbarianLeapArreatCraterCollision", UnitGroup.HeroAbilityUse },
            { "CloudSerpent", UnitGroup.HeroAbilityUse },
            { "DemonHunterCaltrop", UnitGroup.HeroAbilityUse },
            { "ErikPlayAgainGhost", UnitGroup.HeroAbilityUse },
            { "ElDruinsMightMasteryPathingBlocker", UnitGroup.HeroAbilityUse },
            { "EntanglingRootsRelicTreant", UnitGroup.HeroAbilityUse },
            { "Farsight", UnitGroup.HeroAbilityUse },
            { "ImpalingBladesZergling", UnitGroup.HeroAbilityUse },
            { "JainaWaterElemental", UnitGroup.HeroAbilityUse },
            { "KerriganUltralisk", UnitGroup.HeroAbilityUse },
            { "KerriganUltraliskTorrasqueEgg", UnitGroup.HeroAbilityUse },
            { "LostVikingsPlayAgainCairn", UnitGroup.HeroAbilityUse },
            { "MarchoftheMurlocMurloc", UnitGroup.HeroAbilityUse },
            { "MurkyPufferfish", UnitGroup.HeroAbilityUse },
            { "NovaHoloClone", UnitGroup.HeroAbilityUse },
            { "NovaHoloCloneCloaked", UnitGroup.HeroAbilityUse },
            { "OlafPlayAgainGhost", UnitGroup.HeroAbilityUse },
            { "RaynorRaynorsBanshee", UnitGroup.HeroAbilityUse },
            { "RehgarEarthbindTotem", UnitGroup.HeroAbilityUse },
            { "SgtHammerBullheadMine", UnitGroup.HeroAbilityUse },
            { "SgtHammerMine", UnitGroup.HeroAbilityUse },
            { "StitchesStinkling", UnitGroup.HeroAbilityUse },
            { "TassadarForceWall", UnitGroup.HeroAbilityUse },
            { "TassadarForceWallArtEndUnitLeft", UnitGroup.HeroAbilityUse },
            { "TassadarForceWallArtEndUnitRight", UnitGroup.HeroAbilityUse },
            { "TassadarForceWallArtUnitCenter", UnitGroup.HeroAbilityUse },
            { "TassadarForceWallArtUnitLeft", UnitGroup.HeroAbilityUse },
            { "TassadarForceWallArtUnitRight", UnitGroup.HeroAbilityUse },
            { "TinkerRockItTurret", UnitGroup.HeroAbilityUse },
            { "TinkerSalvageScrap", UnitGroup.HeroAbilityUse },
            { "TychusLaserDrill", UnitGroup.HeroAbilityUse },
            { "TyraelEldruinSword", UnitGroup.HeroAbilityUse },
            { "WitchDoctorCorpseSpider", UnitGroup.HeroAbilityUse },
            { "WitchDoctorGargantuan", UnitGroup.HeroAbilityUse },
            { "WitchDoctorPlagueToad", UnitGroup.HeroAbilityUse },
            { "WitchDoctorRavenousSouls", UnitGroup.HeroAbilityUse },
            { "WitchDoctorZombie", UnitGroup.HeroAbilityUse },
            { "ZagaraBaneling", UnitGroup.HeroAbilityUse },
            { "ZagaraBroodling", UnitGroup.HeroAbilityUse },
            { "ZagaraCreepTumor", UnitGroup.HeroAbilityUse },
            { "ZagaraDevouringMaw", UnitGroup.HeroAbilityUse },
            { "ZagaraHydralisk", UnitGroup.HeroAbilityUse },
            { "ZagaraMutalisk", UnitGroup.HeroAbilityUse },
            { "ZagaraNydusWorm", UnitGroup.HeroAbilityUse },
            { "ZagaraRoach", UnitGroup.HeroAbilityUse },
            { "ZeratulBlinkRelicUnit", UnitGroup.HeroAbilityUse },
            { "ZeratulVoidPrisonDome", UnitGroup.HeroAbilityUse },
            { "ZeratulVoidPrisonDomeProtectivePrison", UnitGroup.HeroAbilityUse },

            { "RegenGlobe", UnitGroup.HeroAbilityUse },
            { "RegenGlobeNeutral", UnitGroup.HeroAbilityUse },
            
            { "TownCannonTowerL2", UnitGroup.Structures },
            { "TownCannonTowerL2Standalone", UnitGroup.Structures },
            { "TownCannonTowerL3", UnitGroup.Structures },
            { "TownCannonTowerL3Standalone", UnitGroup.Structures },
            { "TownGateL2BLUR", UnitGroup.Structures },
            { "TownGateL215BLUR", UnitGroup.Structures },
            { "TownGateL215BRUL", UnitGroup.Structures },
            { "TownGateL2Vertical", UnitGroup.Structures },
            { "TownGateL2VerticalLeftVisionBlocked", UnitGroup.Structures },
            { "TownGateL2VerticalRightVisionBlocked", UnitGroup.Structures },
            { "TownGateL3BLUR", UnitGroup.Structures },
            { "TownGateL3BLURBRVisionBlocked", UnitGroup.Structures },
            { "TownGateL3BLURTLVisionBlocked", UnitGroup.Structures },
            { "TownGateL3BRUL", UnitGroup.Structures },
            { "TownGateL3BRULBLVisionBlocked", UnitGroup.Structures },
            { "TownGateL3BRULTRVisionBlocked", UnitGroup.Structures },
            { "TownGateL3Vertical", UnitGroup.Structures },
            { "TownGateL3VerticalLeftVisionBlocked", UnitGroup.Structures },
            { "TownGateL3VerticalRightVisionBlocked", UnitGroup.Structures },
            { "TownGateL315BRUL", UnitGroup.Structures },
            { "TownGateL315BLUR", UnitGroup.Structures },
            { "TownMoonwellL2", UnitGroup.Structures },
            { "TownMoonwellL3", UnitGroup.Structures },
            { "TownTownHallL2", UnitGroup.Structures },
            { "TownTownHallL3", UnitGroup.Structures },
            { "TownWallRadial14L3", UnitGroup.Structures },
            { "TownWallRadial15L3", UnitGroup.Structures },
            { "TownWallRadial16L2", UnitGroup.Structures },
            { "TownWallRadial17L2", UnitGroup.Structures },
            { "TownWallRadial17L3", UnitGroup.Structures },
            { "TownWallRadial18L2", UnitGroup.Structures },
            { "TownWallRadial18L3", UnitGroup.Structures },
            { "TownWallRadial19L2", UnitGroup.Structures },
            { "TownWallRadial19L3", UnitGroup.Structures },
            { "TownWallRadial20L3", UnitGroup.Structures },
            { "TownWallRadial21L3", UnitGroup.Structures },
            { "TownWallRadial2L3", UnitGroup.Structures },
            { "TownWallRadial3L3", UnitGroup.Structures },
            { "TownWallRadial4L2", UnitGroup.Structures },
            { "TownWallRadial4L3", UnitGroup.Structures },
            { "TownWallRadial5L2", UnitGroup.Structures },
            { "TownWallRadial5L3", UnitGroup.Structures },
            { "TownWallRadial6L2", UnitGroup.Structures },
            { "TownWallRadial6L3", UnitGroup.Structures },
            { "TownWallRadial7L2", UnitGroup.Structures },
            { "TownWallRadial7L3", UnitGroup.Structures },
            { "TownWallRadial8L3", UnitGroup.Structures },
            { "TownWallRadial9L3", UnitGroup.Structures },
            
            { "DocksTreasureChest", UnitGroup.MapObjective },
            { "DragonShireShrineMoon", UnitGroup.MapObjective },
            { "DragonShireShrineSun", UnitGroup.MapObjective },
            { "GhostShipBeacon", UnitGroup.MapObjective },
            { "ItemCannonball", UnitGroup.MapObjective },
            { "ItemSeedPickup", UnitGroup.MapObjective },
            { "ItemSoulPickup", UnitGroup.MapObjective },
            { "ItemSoulPickupFive", UnitGroup.MapObjective },
            { "ItemSoulPickupTwenty", UnitGroup.MapObjective },
            { "ItemUnderworldPowerup", UnitGroup.MapObjective },
            { "JunglePlantHorror", UnitGroup.MapObjective },
            { "LuxoriaTemple", UnitGroup.MapObjective },
            { "PlantHorrorOvergrowthPlant", UnitGroup.MapObjective },
            { "PlantZombie", UnitGroup.MapObjective },
            { "PlantZombieRanged", UnitGroup.MapObjective },
            { "RavenLordTribute", UnitGroup.MapObjective },
            { "RavenLordTributeWarning", UnitGroup.MapObjective },
            { "SkeletalPirate", UnitGroup.MapObjective },
            { "SoulCage", UnitGroup.MapObjective },
            { "SoulEater", UnitGroup.MapObjective },
            { "SoulEaterMinion", UnitGroup.MapObjective },
            { "TempleDefenderRanged", UnitGroup.MapObjective },
            { "TempleGuardianBoss", UnitGroup.MapObjective },
            { "UnderworldBoss", UnitGroup.MapObjective },
            { "UnderworldMinion", UnitGroup.MapObjective },
            { "UnderworldRangedMinion", UnitGroup.MapObjective },
            { "UnderworldSummonedBoss", UnitGroup.MapObjective },
            { "UnderworldSummonedBossBody", UnitGroup.MapObjective },
            { "VehicleDragon", UnitGroup.MapObjective },
            { "VehiclePlantHorror", UnitGroup.MapObjective },
            { "XelNagaWatchTower", UnitGroup.MapObjective },

            { "JungleGraveGolemDefender", UnitGroup.MercenaryCamp },
            { "JungleGraveGolemLaner", UnitGroup.MercenaryCamp },
            { "MercDefenderMeleeOgre", UnitGroup.MercenaryCamp },
            { "MercDefenderRangedOgre", UnitGroup.MercenaryCamp },
            { "MercDefenderSiegeGiant", UnitGroup.MercenaryCamp },
            { "MercLanerMeleeOgre", UnitGroup.MercenaryCamp },
            { "MercLanerRangedOgre", UnitGroup.MercenaryCamp },
            { "MercLanerSiegeGiant", UnitGroup.MercenaryCamp },

            { "CatapultMinion", UnitGroup.Minions },
            { "FootmanMinion", UnitGroup.Minions },
            { "RangedMinion", UnitGroup.Minions },
            { "WizardMinion", UnitGroup.Minions },
            
            { "CampOwnershipFlag", UnitGroup.Miscellaneous },
            { "DocksPirateCaptain", UnitGroup.Miscellaneous },
            { "DragonballCaptureBeacon", UnitGroup.Miscellaneous },
            { "EntanglingRootsUnit", UnitGroup.Miscellaneous },
            { "FertileSoil", UnitGroup.Miscellaneous },
            { "GardensDragonShrineTargetMoon", UnitGroup.Miscellaneous },
            { "GardensDragonShrineTargetSun", UnitGroup.Miscellaneous },
            { "GroundHole", UnitGroup.Miscellaneous },
            { "GroundHoleCamera", UnitGroup.Miscellaneous },
            { "GroundHoleVision", UnitGroup.Miscellaneous },
            { "HangingSharkBreakable", UnitGroup.Miscellaneous },
            { "HangingSharkBreakable_L", UnitGroup.Miscellaneous },
            { "HarrisonJones", UnitGroup.Miscellaneous },
            { "HarrisonJonesWell", UnitGroup.Miscellaneous },
            { "HealingPadSearchUnit", UnitGroup.Miscellaneous },
            { "HeroLostVikingsController", UnitGroup.Miscellaneous },
            { "HoleLadderDown", UnitGroup.Miscellaneous },
            { "HoleLadderUp", UnitGroup.Miscellaneous },
            { "InvisibleBeacon", UnitGroup.Miscellaneous },
            { "JungleBattleship", UnitGroup.Miscellaneous },
            { "JungleCampIconUnit", UnitGroup.Miscellaneous },
            { "JungleCampIconUnitTeamColor", UnitGroup.Miscellaneous },
            { "KingsCore", UnitGroup.Miscellaneous },
            { "MarchoftheMurlocsInvisibleDummy", UnitGroup.Miscellaneous },
            { "MercDefenderSiegeGiantOrientDummy", UnitGroup.Miscellaneous },
            { "PlantHorrorOvergrowthPlantDummyUnit", UnitGroup.Miscellaneous },
            { "RecallDestination", UnitGroup.Miscellaneous },
            { "SgtHammerBluntForceGunInvisibleOrbitalDummy", UnitGroup.Miscellaneous },
            { "SgtHammerConcussiveBlastScrap", UnitGroup.Miscellaneous },
            { "SgtHammerConcussiveBlastScrapCenter", UnitGroup.Miscellaneous },
            { "Storm_Critter_KingsCrest_Peasant_Vendor_A", UnitGroup.Miscellaneous },
            { "Storm_Critter_KingsCrest_Peasant_Vendor_B", UnitGroup.Miscellaneous },
            { "Storm_Critter_KingsCrest_Peasant_Vendor_D", UnitGroup.Miscellaneous },
            { "StormCrab", UnitGroup.Miscellaneous },
            { "StormGameStartPathingBlocker", UnitGroup.Miscellaneous },
            { "StormGameStartPathingBlockerDiagonal", UnitGroup.Miscellaneous },
            { "StormKingsCrestDestructibleBarrel", UnitGroup.Miscellaneous },
            { "StormKingsCrestSharkJawUnit", UnitGroup.Miscellaneous },
            { "StormPig", UnitGroup.Miscellaneous },
            { "SuicideUnitXPDummy", UnitGroup.Miscellaneous },
            { "TempleChampionTornado", UnitGroup.Miscellaneous },
            { "TempleDefenderPathingBlocker", UnitGroup.Miscellaneous },
            { "TownMercCampCaptureBeacon", UnitGroup.Miscellaneous },
            { "UnderworldMineEntranceMinimapIcon", UnitGroup.Miscellaneous },
            { "WatchTowerCaptureBeacon", UnitGroup.Miscellaneous },
            { "WitchDoctorPlagueToadRelic", UnitGroup.Miscellaneous },
            
            { "AbathurLocustSwarm", UnitGroup.HeroTalentSelection },
            { "BarbarianFerociousHealingItem", UnitGroup.HeroTalentSelection },
            { "Envenom", UnitGroup.HeroTalentSelection },
            { "FlashoftheStorms", UnitGroup.HeroTalentSelection },
            { "IcyVeinsUnit", UnitGroup.HeroTalentSelection },
            { "ImprovedIceBlockUnit", UnitGroup.HeroTalentSelection },
            { "JainaArcanePower", UnitGroup.HeroTalentSelection },
            { "ProtectiveShield", UnitGroup.HeroTalentSelection },
            { "RaynorRaidersRecruitment", UnitGroup.HeroTalentSelection },
            { "ScoutingDroneItem", UnitGroup.HeroTalentSelection },
            { "ShamanHealingWard", UnitGroup.HeroTalentSelection },
            { "SearingAttacks", UnitGroup.HeroTalentSelection },
            { "TalentBerserk", UnitGroup.HeroTalentSelection },
            { "TalentBloodForBlood", UnitGroup.HeroTalentSelection },
            { "TalentBucketBribe", UnitGroup.HeroTalentSelection },
            { "TalentBucketCalldownMule", UnitGroup.HeroTalentSelection },
            { "TalentBucketClairvoyance", UnitGroup.HeroTalentSelection },
            { "TalentBucketCleanse", UnitGroup.HeroTalentSelection },
            { "TalentBucketHealingWard", UnitGroup.HeroTalentSelection },
            { "TalentBucketIceBlock", UnitGroup.HeroTalentSelection },
            { "TalentBucketPromote", UnitGroup.HeroTalentSelection },
            { "TalentBucketShrinkRay", UnitGroup.HeroTalentSelection },
            { "TalentBucketSprint", UnitGroup.HeroTalentSelection },
            { "TalentBucketStormShield", UnitGroup.HeroTalentSelection },
            { "TalentFirstAid", UnitGroup.HeroTalentSelection },
            { "TalentHardenedShield", UnitGroup.HeroTalentSelection },
            { "TalentInfestItem", UnitGroup.HeroTalentSelection },
            { "TalentOverdrive", UnitGroup.HeroTalentSelection },
            { "TalentRewind", UnitGroup.HeroTalentSelection },
            { "TalentStoneskin", UnitGroup.HeroTalentSelection },
            { "TychusSearingAttacks", UnitGroup.HeroTalentSelection },
            { "TyrandeSearingArrows", UnitGroup.HeroTalentSelection },
            { "UtherBenedictionItem", UnitGroup.HeroTalentSelection },
            { "ZagaraRapidIncubationItem", UnitGroup.HeroTalentSelection },
            { "ZeratulVorpalBladeItem", UnitGroup.HeroTalentSelection }
        };

        private static readonly int[] HeroDeathTimersByTeamLevelInSeconds = new[] {
            9,  //  1
            10, //  2
            11, //  3
            12, //  4
            13, //  5
            14, //  6
            16, //  7
            18, //  8
            20, //  9
            23, // 10
            26, // 11
            29, // 12
            32, // 13
            36, // 14
            40, // 15
            44, // 16
            50, // 17
            56, // 18
            62, // 19
            65, // 20
            65, // 21
            65, // 22
            65, // 23
            65, // 24
            65, // 25
            65, // 26
            65, // 27
            65, // 28
            65, // 29
            65  // 30
        };

        private static readonly int[] HeroDeathTimersByTeamLevelInSecondsForTalentLevels = new[] {
            9,  //  1
            12, //  4
            16, //  7
            23, // 10
            32, // 13
            44, // 16
            65  // 20
        };

        public static void ParseUnitData(Replay replay)
        {
            // Get array of units from 'UnitBornEvent'
            replay.Units = replay.TrackerEvents.Where(i => i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitBornEvent).Select(i => new Unit {
                UnitID = Unit.GetUnitID((int)i.Data.dictionary[0].vInt.Value, (int)i.Data.dictionary[1].vInt.Value),
                Name = i.Data.dictionary[2].blobText,
                Group = Unit.UnitGroupDictionary.ContainsKey(i.Data.dictionary[2].blobText) ? Unit.UnitGroupDictionary[i.Data.dictionary[2].blobText] : Unit.UnitGroup.Unknown,
                TimeSpanBorn = i.TimeSpan,
                Team = i.Data.dictionary[3].vInt.Value == 11 || i.Data.dictionary[3].vInt.Value == 12 ? (int)i.Data.dictionary[3].vInt.Value - 11
                    : i.Data.dictionary[3].vInt.Value > 0 && i.Data.dictionary[3].vInt.Value <= 10 ? replay.Players[i.Data.dictionary[3].vInt.Value - 1].Team
                    : (int?)null, 
                PlayerControlledBy = i.Data.dictionary[3].vInt.Value > 0 && i.Data.dictionary[3].vInt.Value <= 10 ? replay.Players[i.Data.dictionary[3].vInt.Value - 1] : null,
                PointBorn = new Point { X = (int)i.Data.dictionary[5].vInt.Value, Y = (int)i.Data.dictionary[6].vInt.Value } })
                .ToList();

            // Add in information on unit deaths from 'UnitDiedEvent'
            var unitsDictionary = replay.Units.ToDictionary(i => i.UnitID, i => i);
            foreach (var unitDiedEvent in replay.TrackerEvents.Where(i => i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitDiedEvent).Select(i => new {
                UnitID = Unit.GetUnitID((int)i.Data.dictionary[0].vInt.Value, (int)i.Data.dictionary[1].vInt.Value),
                TimeSpanDied = i.TimeSpan,
                PlayerIDKilledBy = i.Data.dictionary[2].optionalData != null ? (int)i.Data.dictionary[2].optionalData.vInt.Value : (int?)null,
                PointDied = new Point { X = (int)i.Data.dictionary[3].vInt.Value, Y = (int)i.Data.dictionary[4].vInt.Value },
                UnitKilledBy = i.Data.dictionary[5].optionalData != null ? unitsDictionary[Unit.GetUnitID((int)i.Data.dictionary[5].optionalData.vInt.Value, (int)i.Data.dictionary[6].optionalData.vInt.Value)] : null }))
            {
                var unitThatDied = unitsDictionary[unitDiedEvent.UnitID];
                unitThatDied.TimeSpanDied = unitDiedEvent.TimeSpanDied;
                unitThatDied.PlayerKilledBy = unitDiedEvent.PlayerIDKilledBy.HasValue && unitDiedEvent.PlayerIDKilledBy.Value > 0 && unitDiedEvent.PlayerIDKilledBy.Value <= 10 ? replay.Players[unitDiedEvent.PlayerIDKilledBy.Value - 1] : null;
                unitThatDied.PointDied = unitDiedEvent.PointDied;
                unitThatDied.UnitKilledBy = unitDiedEvent.UnitKilledBy;

                // Sometimes 'PlayerIDKilledBy' will be outside of the range of players (1-10)
                // Minions that are killed by other minions or towers will have the 'team' that killed them in this field (11 or 12)
                // Some other units have interesting values I don't fully understand yet.  For example, 'ItemCannonball' (the coins on Blackheart's Bay) will have 0 or 15 in this field.  I'm guessing this is also which team acquires them, which may be useful
                // Other map objectives may also have this.  I'll look into this more in the future.
                /* if (unitDiedEvent.PlayerIDKilledBy.HasValue && unitThatDied.PlayerKilledBy == null)
                    Console.WriteLine(""); */
            }

            // Add in information on unit ownership changes from 'UnitOwnerChangeEvent' (For example, players grabbing regen globes or a player grabbing a Garden Terror)
            foreach (var unitOwnerChangeEvent in replay.TrackerEvents.Where(i => i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitOwnerChangeEvent).Select(i => new {
                UnitID = Unit.GetUnitID((int)i.Data.dictionary[0].vInt.Value, (int)i.Data.dictionary[1].vInt.Value),
                TimeSpanOwnerChanged = i.TimeSpan,
                Team = i.Data.dictionary[2].vInt.Value == 11 || i.Data.dictionary[2].vInt.Value == 12 ? (int)i.Data.dictionary[2].vInt.Value - 11 : (int?)null,
                PlayerNewOwner = i.Data.dictionary[2].vInt.Value > 0 && i.Data.dictionary[2].vInt.Value <= 10 ? replay.Players[i.Data.dictionary[2].vInt.Value - 1] : null }))
                    unitsDictionary[unitOwnerChangeEvent.UnitID].OwnerChangeEvents.Add(new OwnerChangeEvent {
                        TimeSpanOwnerChanged = unitOwnerChangeEvent.TimeSpanOwnerChanged,
                        Team = unitOwnerChangeEvent.Team ?? (unitOwnerChangeEvent.PlayerNewOwner != null ? unitOwnerChangeEvent.PlayerNewOwner.Team : (int?)null),
                        PlayerNewOwner = unitOwnerChangeEvent.PlayerNewOwner });

            // For simplicity, I set extra fields on units that are not initially controlled by a player, and only have one owner change event
            foreach (var unitWithOneOwnerChange in replay.Units.Where(i => i.OwnerChangeEvents.Count() == 1 && i.PlayerControlledBy == null))
            {
                var singleOwnerChangeEvent = unitWithOneOwnerChange.OwnerChangeEvents.Single();
                if (singleOwnerChangeEvent.PlayerNewOwner != null)
                {
                    unitWithOneOwnerChange.PlayerControlledBy = singleOwnerChangeEvent.PlayerNewOwner;
                    unitWithOneOwnerChange.TimeSpanAcquired = singleOwnerChangeEvent.TimeSpanOwnerChanged;
                    unitWithOneOwnerChange.OwnerChangeEvents.Clear();
                }
            }

            // Add in information from the 'UnitPositionEvent'
            // We need to go through the replay file in order because unit IDs are recycled
            var activeUnits = new Dictionary<int, Unit>();
            foreach (var unitPositionEvent in replay.TrackerEvents.Where(i => i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitBornEvent || i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitPositionsEvent).OrderBy(i => i.TimeSpan))
                if (unitPositionEvent.TrackerEventType == ReplayTrackerEvents.TrackerEventType.UnitBornEvent)
                    activeUnits[(int)unitPositionEvent.Data.dictionary[0].vInt.Value] = unitsDictionary[Unit.GetUnitID((int)unitPositionEvent.Data.dictionary[0].vInt.Value, (int)unitPositionEvent.Data.dictionary[1].vInt.Value)];
                else
                {
                    var currentUnitIndex = (int)unitPositionEvent.Data.dictionary[0].vInt.Value;
                    for (var i = 0; i < unitPositionEvent.Data.dictionary[1].array.Length; i++)
                    {
                        currentUnitIndex += (int)unitPositionEvent.Data.dictionary[1].array[i++].vInt.Value;
                        activeUnits[currentUnitIndex].Positions.Add(new Position {
                            TimeSpan = unitPositionEvent.TimeSpan,
                            Point = new Point {
                                X = (int)unitPositionEvent.Data.dictionary[1].array[i++].vInt.Value,
                                Y = (int)unitPositionEvent.Data.dictionary[1].array[i].vInt.Value } });
                    }
                }

            // Add an array of Hero units to each player
            // Currently I'm only getting single heroes (Lost Vikings not yet supported)
            var earlyGameTimeSpan = new TimeSpan(0, 0, 10);
            var heroUnitsDictionary = replay.Players.Where(i => replay.Units.Count(j => j.TimeSpanBorn < earlyGameTimeSpan && j.PlayerControlledBy == i && j.Name.StartsWith("Hero")) == 1).ToDictionary(i => i, i => replay.Units.Single(j => j.TimeSpanBorn < earlyGameTimeSpan && j.PlayerControlledBy == i && j.Name.StartsWith("Hero")));
            foreach (var player in replay.Players)
                if (heroUnitsDictionary.ContainsKey(player))
                    player.HeroUnits = new[] { heroUnitsDictionary[player] };

            // Add derived hero positions from associated unit born/acquired/died info
            // These are accurate positions: Picking up regen globes, spawning Locusts, etc

            // For Abathur locusts, we need to make sure they aren't spawning from a locust nest (Level 20 talent)
            var abathurLocustUnits = replay.Units.Where(i => i.Name == "AbathurLocustNormal" || i.Name == "AbathurLocustAssaultStrain" || i.Name == "AbathurLocustBombardStrain").ToList();
            if (abathurLocustUnits.Any() && replay.Units.Any(i => i.Name == "AbathurLocustNest"))
            {
                var abathurLocustNests = replay.Units.Where(i => i.Name == "AbathurLocustNest");
                foreach (var abathurLocustUnit in abathurLocustUnits.ToArray())
                    if (abathurLocustNests.Any(i => i.TimeSpanBorn <= abathurLocustUnit.TimeSpanBorn && (!i.TimeSpanDied.HasValue || i.TimeSpanDied >= abathurLocustUnit.TimeSpanBorn) && i.PointBorn.DistanceTo(abathurLocustUnit.PointBorn) <= 3))
                        abathurLocustUnits.Remove(abathurLocustUnit);
            }

            foreach (var unit in replay.Units.Where(i => Unit.UnitBornProvidesLocationForOwner.ContainsKey(i.Name) || i.Group == Unit.UnitGroup.HeroTalentSelection).Union(abathurLocustUnits).Where(i => heroUnitsDictionary.ContainsKey(i.PlayerControlledBy)))
                heroUnitsDictionary[unit.PlayerControlledBy].Positions.Add(new Position { TimeSpan = unit.TimeSpanBorn, Point = unit.PointBorn });

            foreach (var unit in replay.Units.Where(i => Unit.UnitOwnerChangeProvidesLocationForOwner.ContainsKey(i.Name) && i.PlayerControlledBy != null).Where(i => heroUnitsDictionary.ContainsKey(i.PlayerControlledBy)))
                heroUnitsDictionary[unit.PlayerControlledBy].Positions.Add(new Position { TimeSpan = unit.TimeSpanAcquired.Value, Point = unit.PointBorn });

            // Use 'CCmdUpdateTargetUnitEvent' to find an accurate location of units targeted
            // Excellent for finding frequent, accurate locations of heroes during team fights
            foreach (var updateTargetUnitEvent in replay.GameEvents.Where(i => i.eventType == GameEventType.CCmdUpdateTargetUnitEvent))
                if (replay.Units.Any(i => i.UnitID == (int)updateTargetUnitEvent.data.array[2].unsignedInt.Value))
                    replay.Units.Single(i => i.UnitID == (int)updateTargetUnitEvent.data.array[2].unsignedInt.Value).Positions.Add(new Position {
                        TimeSpan = updateTargetUnitEvent.TimeSpan,
                        Point = Point.FromEventFormat(
                            updateTargetUnitEvent.data.array[6].array[0].unsignedInt.Value,
                            updateTargetUnitEvent.data.array[6].array[1].unsignedInt.Value) });
            

            // Add in 'accurate' positions for each player's death, which sends them to their spawn point
            // Special Exceptions:
            // Uther: Level 20 respawn talent: Doesn't display the death animation when respawning, so probably doesn't count as a death in this situation.  This is actually probably the best situation for us
            // Diablo: Fast respawn if he has enough souls.  Not yet able to detect when this occurs
            // Murky: Respawns to his egg if his egg is alive when he dies
            // Lost Vikings: Individual Vikings spawn 25% faster per their trait, and 50% faster with a talent, but currently we aren't able to track their deaths individually
            foreach (var player in replay.Players.Where(i => i.HeroUnits.Length == 1 && i.Deaths.Length > 0))
            {
                var fullTimerDeaths = new List<TimeSpan>();
                if (player.HeroUnits[0].Name == "HeroMurky")
                {
                    // Gather a list of the eggs Murky has placed throughout the game
                    var murkyEggs = replay.Units.Where(i => i.PlayerControlledBy == player && i.Name == "MurkyRespawnEgg").OrderBy(i => i.TimeSpanBorn).ToArray();
                    var currentEggIndex = 0;
                    foreach (var murkyDeath in player.Deaths)
                    {
                        // If Murky respawns at the egg, it will be 5 seconds after his death
                        var murkyRespawnFromEggTimeSpan = murkyDeath.Add(TimeSpan.FromSeconds(5));
                        for (; currentEggIndex < murkyEggs.Length; currentEggIndex++)
                        {
                            if (murkyRespawnFromEggTimeSpan > murkyEggs[currentEggIndex].TimeSpanDied && currentEggIndex < murkyEggs.Length + 1)
                                continue;

                            // Check to see if there is an egg alive when Murky would respawn
                            if (murkyRespawnFromEggTimeSpan >= murkyEggs[currentEggIndex].TimeSpanBorn && (!murkyEggs[currentEggIndex].TimeSpanDied.HasValue || murkyRespawnFromEggTimeSpan <= murkyEggs[currentEggIndex].TimeSpanDied.Value))
                                for (; murkyRespawnFromEggTimeSpan >= murkyDeath; murkyRespawnFromEggTimeSpan = murkyRespawnFromEggTimeSpan.Add(TimeSpan.FromSeconds(-1)))
                                    player.HeroUnits[0].Positions.Add(new Position { TimeSpan = murkyRespawnFromEggTimeSpan, Point = murkyEggs[currentEggIndex].PointBorn, IsEstimated = false });
                            else
                                // Murky did not respawn at egg - give him the normal death timer
                                fullTimerDeaths.Add(murkyDeath);
                            break;
                        }
                    }
                }
                else
                    fullTimerDeaths.AddRange(player.Deaths);


                // Normal death timer deaths
                // This is all deaths for most heroes, and Murky deaths if he didn't respawn from his egg
                if (fullTimerDeaths.Count != 0)
                {
                    // Add a 'Position' at the player spawn when the death occurs
                    player.HeroUnits[0].Positions.AddRange(fullTimerDeaths.Select(i => new Position { TimeSpan = i, Point = player.HeroUnits[0].PointBorn, IsEstimated = false }));

                    // Add a 'Position' at the player spawn when the hero respawns
                    if (player.HeroUnits[0].Name == "HeroDiablo")
                        // Currently not able to tell if Diablo has a fast respawn - because of this we just always assume he does respawn quickly
                        player.HeroUnits[0].Positions.AddRange(fullTimerDeaths.Select(i => new Position { TimeSpan = i.Add(TimeSpan.FromSeconds(5)), Point = player.HeroUnits[0].PointBorn, IsEstimated = false }));
                    else
                    {
                        var currentTeamLevelMilestoneIndex = 1;
                        foreach (var playerDeath in fullTimerDeaths)
                            for (; currentTeamLevelMilestoneIndex < replay.TeamLevelMilestones[player.Team].Length; currentTeamLevelMilestoneIndex++)
                            {
                                Position spawnPosition = null;
                                if (playerDeath < replay.TeamLevelMilestones[player.Team][currentTeamLevelMilestoneIndex])
                                    spawnPosition = new Position { TimeSpan = playerDeath.Add(TimeSpan.FromSeconds(HeroDeathTimersByTeamLevelInSecondsForTalentLevels[currentTeamLevelMilestoneIndex - 1])), Point = player.HeroUnits[0].PointBorn, IsEstimated = false };
                                else if (currentTeamLevelMilestoneIndex == replay.TeamLevelMilestones[player.Team].Length - 1)
                                    spawnPosition = new Position { TimeSpan = playerDeath.Add(TimeSpan.FromSeconds(HeroDeathTimersByTeamLevelInSecondsForTalentLevels[currentTeamLevelMilestoneIndex])), Point = player.HeroUnits[0].PointBorn, IsEstimated = false };

                                if (spawnPosition != null)
                                {
                                    var deathTimeSpan = playerDeath;
                                    while (deathTimeSpan < spawnPosition.TimeSpan)
                                    {
                                        // Add a 'Position' at the player spawn for every second the player is dead, to make sure we don't add 'estimated' positions during this time
                                        player.HeroUnits[0].Positions.Add(new Position { TimeSpan = deathTimeSpan, Point = player.HeroUnits[0].PointBorn, IsEstimated = false });
                                        deathTimeSpan = deathTimeSpan.Add(TimeSpan.FromSeconds(1));
                                    }
                                    player.HeroUnits[0].Positions.Add(spawnPosition);
                                    break;
                                }
                            }
                    }
                }                

                player.HeroUnits[0].Positions = player.HeroUnits[0].Positions.OrderBy(i => i.TimeSpan).ToList();
            }

            // Estimate Hero positions from CCmdEvent and CCmdUpdateTargetPointEvent (Movement points)
            {
                // List of Hero units (Excluding heroes with multiple units like Lost Vikings - not sure how to handle those)
                // This is different from the above dictionary in that it excludes Abathur if he chooses the clone hero talent
                // It's okay to not estimate Abathur's position, as he rarely moves and we also get an accurate position each time he spawns a locust
                heroUnitsDictionary = replay.Players.Where(i => replay.Units.Count(j => j.PlayerControlledBy == i && j.Name.StartsWith("Hero")) == 1).ToDictionary(i => i, i => replay.Units.Single(j => j.PlayerControlledBy == i && j.Name.StartsWith("Hero")));

                // This is a list of 'HeroUnit', 'TimeSpan', and 'EventPosition' for each CCmdEvent where ability data is null and a position is included
                var heroCCmdEventLists = replay.GameEvents.Where(i =>
                    i.eventType == GameEventType.CCmdEvent &&
                    i.data.array[1] == null &&
                    i.data.array[2] != null &&
                    i.data.array[2].array.Length == 3 &&
                    heroUnitsDictionary.ContainsKey(i.player)).Select(i => new {
                        HeroUnit = heroUnitsDictionary[i.player],
                        Position = new Position { TimeSpan = i.TimeSpan, Point = Point.FromEventFormat(i.data.array[2].array[0].unsignedInt.Value, i.data.array[2].array[1].unsignedInt.Value), IsEstimated = true } })
                        .GroupBy(i => i.HeroUnit)
                        .Select(i => new {
                            HeroUnit = i.Key,
                            // Take the latest applicable CCmdEvent or CCmdUpdateTargetPointEvent if there are more than one in a second
                            Positions = i.Select(j => j.Position).Union(replay.GameEvents.Where(j => j.player == i.Key.PlayerControlledBy && j.eventType == GameEventType.CCmdUpdateTargetPointEvent).Select(j => new Position { TimeSpan = j.TimeSpan, Point = Point.FromEventFormat(j.data.array[0].unsignedInt.Value, j.data.array[1].unsignedInt.Value), IsEstimated = true })).GroupBy(j => (int)j.TimeSpan.TotalSeconds).Select(j => j.OrderByDescending(k => k.TimeSpan).First()).OrderBy(j => j.TimeSpan).ToArray() });
                
                const double PlayerSpeedUnitsPerSecond = 5.0;
                foreach (var heroCCmdEventList in heroCCmdEventLists)
                {
                    // Estimate the hero unit travelling to each intended destination
                    // Only save one position per second, and prefer accurate positions
                    // Heroes can have a lot more positions, and probably won't be useful more frequently than this
                    var heroTargetLocationArray = heroCCmdEventList.HeroUnit.Positions.Union(new[] { new Position { TimeSpan = heroCCmdEventList.HeroUnit.TimeSpanBorn, Point = heroCCmdEventList.HeroUnit.PointBorn } }).Union(heroCCmdEventList.Positions).GroupBy(i => (int)i.TimeSpan.TotalSeconds).Select(i => i.OrderBy(j => j.IsEstimated).First()).OrderBy(i => i.TimeSpan).ToArray();
                    var currentEstimatedPosition = heroTargetLocationArray[0];
                    for (var i = 0; i < heroTargetLocationArray.Length - 1; i++)
                        if (!heroTargetLocationArray[i + 1].IsEstimated)
                            currentEstimatedPosition = heroTargetLocationArray[i + 1];
                        else
                        {
                            var percentageOfDistanceTravelledToTargetLocation = (heroTargetLocationArray[i + 1].TimeSpan - currentEstimatedPosition.TimeSpan).TotalSeconds * PlayerSpeedUnitsPerSecond / currentEstimatedPosition.Point.DistanceTo(heroTargetLocationArray[i + 1].Point);
                            currentEstimatedPosition = new Position {
                                TimeSpan = heroTargetLocationArray[i + 1].TimeSpan,
                                Point = percentageOfDistanceTravelledToTargetLocation >= 1
                                    ? heroTargetLocationArray[i + 1].Point
                                    : new Point {
                                        X = (int)((heroTargetLocationArray[i + 1].Point.X - currentEstimatedPosition.Point.X) * percentageOfDistanceTravelledToTargetLocation + currentEstimatedPosition.Point.X),
                                        Y = (int)((heroTargetLocationArray[i + 1].Point.Y - currentEstimatedPosition.Point.Y) * percentageOfDistanceTravelledToTargetLocation + currentEstimatedPosition.Point.Y) },
                                IsEstimated = true };
                            heroCCmdEventList.HeroUnit.Positions.Add(currentEstimatedPosition);
                        }
                    heroCCmdEventList.HeroUnit.Positions = heroCCmdEventList.HeroUnit.Positions.OrderBy(i => i.TimeSpan).ToList();
                }
            }

            foreach (var unit in replay.Units.Where(i => i.Positions.Any()))
            {
                // Save no more than one position event per second per unit
                unit.Positions = unit.Positions.GroupBy(i => (int)i.TimeSpan.TotalSeconds).Select(i => i.OrderBy(j => j.IsEstimated).First()).OrderBy(i => i.TimeSpan).ToList();

                // If this is a Hero unit, adjust the 'PointDied' and 'TimeSpanDied' to the last position
                // Currently Hero units stop receiving tracker event updates after their first death
                if (unit.Group == Unit.UnitGroup.Hero)
                {
                    var finalPosition = unit.Positions.Last();
                    unit.PointDied = finalPosition.Point;
                    unit.TimeSpanDied = finalPosition.TimeSpan;
                }
            }

            // Add 'estimated' minion positions based on their fixed pathing
            // Without these positions, minions can appear to travel through walls straight across the map
            // These estimated positions are actually quite accurate, as minions always follow a path connecting each fort/keep in their lane
            var numberOfStructureTiers = replay.Units.Where(i => i.Name.StartsWith("TownTownHall")).Select(i => i.Name).Distinct().Count();
            var uniqueTierName = replay.Units.First(i => i.Name.StartsWith("TownTownHall")).Name;
            var numberOfLanes = replay.Units.Count(i => i.Name == uniqueTierName && i.Team == 0);
            var minionWayPoints = replay.Units.Where(i => i.Name.StartsWith("TownTownHall")).Select(j => j.PointBorn).OrderBy(j => j.X).Skip(numberOfLanes).OrderByDescending(j => j.X).Skip(numberOfLanes).OrderBy(j => j.Y);
            for (var team = 0; team <= 1; team++)
            {
                // Gather all minion units for this team
                var minionUnits = replay.Units.Where(i => i.Team == team && i.Group == Unit.UnitGroup.Minions).ToArray();

                // Each wave spawns together, but not necessarily from top to bottom
                // We will figure out what order the lanes are spawning in, and order by top to bottom later on
                var unitsPerLaneTemp = new List<Unit>[numberOfLanes];
                for (var i = 0; i < unitsPerLaneTemp.Length; i++)
                    unitsPerLaneTemp[i] = new List<Unit>();
                var minionLaneOrderMinions = minionUnits.Where(i => i.Name == "WizardMinion").Take(numberOfLanes).ToArray();
                var minionLaneOrder = new List<Tuple<int, int>>();
                for (var i = 0; i < numberOfLanes; i++)
                    minionLaneOrder.Add(new Tuple<int, int>(i, minionLaneOrderMinions[i].PointBorn.Y));
                minionLaneOrder = minionLaneOrder.OrderBy(i => i.Item2).ToList();

                // Group minion units by lane
                var currentIndex = 0;
                var minionUnitsPerWave = 7;
                while (currentIndex < minionUnits.Length)
                    for (var i = 0; i < unitsPerLaneTemp.Length; i++)
                        for (var j = 0; j < minionUnitsPerWave; j++)
                        {
                            if (currentIndex == minionUnits.Length)
                                break;
                            unitsPerLaneTemp[i].Add(minionUnits[currentIndex++]);

                            // CatapultMinions don't seem to spawn exactly with their minion wave, which is strange
                            // For now I will leave them out of this, which means they may appear to travel through walls
                            if (currentIndex < minionUnits.Length && minionUnits[currentIndex].Name == "CatapultMinion")
                                currentIndex++;
                        }

                // Order the lanes by top to bottom
                var unitsPerLane = unitsPerLaneTemp.ToArray();
                for (var i = 0; i < unitsPerLane.Length; i++)
                    unitsPerLane[i] = unitsPerLaneTemp[minionLaneOrder[i].Item1];

                for (var i = 0; i < numberOfLanes; i++)
                {
                    // For each lane, take the forts in that lane, and see if the minions in that lane walked beyond this
                    var currentLaneUnitsToAdjust = unitsPerLane[i].Where(j => j.Positions.Any() || j.TimeSpanDied.HasValue);
                    var currentLaneWaypoints = minionWayPoints.Skip(numberOfStructureTiers * i).Take(numberOfStructureTiers);
                    if (team == 0)
                        currentLaneWaypoints = currentLaneWaypoints.OrderBy(j => j.X);
                    else
                        currentLaneWaypoints = currentLaneWaypoints.OrderByDescending(j => j.X);

                    foreach (var laneUnit in currentLaneUnitsToAdjust)
                    {
                        var isLaneUnitModified = false;
                        var beginningPosition = new Position { TimeSpan = laneUnit.TimeSpanBorn, Point = laneUnit.PointBorn };
                        var firstLaneUnitPosition = laneUnit.Positions.Any()
                            ? laneUnit.Positions.First()
                            : new Position { TimeSpan = laneUnit.TimeSpanDied.Value, Point = laneUnit.PointDied };
                        foreach (var laneWaypoint in currentLaneWaypoints)
                            if ((team == 0 && firstLaneUnitPosition.Point.X > laneWaypoint.X) || team == 1 && firstLaneUnitPosition.Point.X < laneWaypoint.X)
                            {
                                var leg1Distance = beginningPosition.Point.DistanceTo(laneWaypoint);
                                var newPosition = new Position {
                                    TimeSpan = beginningPosition.TimeSpan + TimeSpan.FromSeconds((long)((firstLaneUnitPosition.TimeSpan - beginningPosition.TimeSpan).TotalSeconds * (leg1Distance / (leg1Distance + laneWaypoint.DistanceTo(firstLaneUnitPosition.Point))))),
                                    Point = laneWaypoint };
                                laneUnit.Positions.Add(newPosition);
                                beginningPosition = newPosition;
                                isLaneUnitModified = true;
                            }
                            else
                                break;
                        if (isLaneUnitModified)
                            laneUnit.Positions = laneUnit.Positions.OrderBy(j => j.TimeSpan).ToList();
                    }
                }
            }

            // Remove 'duplicate' positions that don't tell us anything
            foreach (var unit in replay.Units.Where(i => i.Positions.Count >= 3))
            {
                var unitPositions = unit.Positions.ToArray();
                for (var i = 1; i < unitPositions.Length - 1; i++)
                    if (unitPositions[i].Point.X == unitPositions[i - 1].Point.X
                        && unitPositions[i].Point.Y == unitPositions[i - 1].Point.Y
                        && unitPositions[i].Point.X == unitPositions[i + 1].Point.X
                        && unitPositions[i].Point.Y == unitPositions[i + 1].Point.Y)
                        unit.Positions.Remove(unitPositions[i]);
            }
        }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static Point FromEventFormat(ulong x, ulong y)
        {
            /* var Z = ((z & 0x80000000) == 0) ? -1d : 1d;
            Z *= (z & 0x7fffffff) / 4096d; */

            return new Point { X = (int)(x / 4096d), Y = (int)(y / 4096d) };
        }

        public double DistanceTo(Point p)
        {
            var a = X - p.X;
            var b = Y - p.Y;
            return Math.Sqrt(a * a + b * b);
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }
    }

    public class Position
    {
        public TimeSpan TimeSpan { get; set; }
        public Point Point { get; set; }
        public bool IsEstimated { get; set; }

        public Position()
        {
            IsEstimated = false;
        }

        public override string ToString()
        {
            return TimeSpan + ": " + IsEstimated + ": " + Point;
        }
    }

    public class OwnerChangeEvent
    {
        public TimeSpan TimeSpanOwnerChanged { get; set; }
        public int? Team { get; set; }
        public Player PlayerNewOwner { get; set; }
    }
}
