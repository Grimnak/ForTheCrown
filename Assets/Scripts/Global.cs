using System;

namespace Capstone
{
    public static class Global
    {
        public static bool oneLogoCycleDone = false;
        public static readonly string menuScene = "MainMenuScene";
        public static readonly string gameScene = "GameScene";
        public static readonly string exitScene = "ExitScene";
        public static int rngSeed = -1;
        public static string activeGameMode = "None";
        public static int activeGameMap = -1;
        
        /// <summary>
        /// This struct indicates the map that the player(s) are actively playing on.
        /// </summary>
        public struct ActiveGameMap
        {
            public const int map1 = 1;
            public const int map2 = 2;
            public const int map3 = 3;
        }

        /// <summary>
        /// This struct indicates the mode that the player(s) are actively playing.
        /// </summary>
        public struct ActiveGameMode
        {
            public const string training1 = "Training1";
            public const string training2 = "Training2";
            public const string training3 = "Training3";
            public const string training4 = "Training4";
            public const string endlessMode = "Endless";
            public const string none = "None";
        }

        /// <summary>
        /// Reset the active game map and game mode to the default values.
        /// </summary>
        public static void ResetGameState()
        {
            activeGameMap = -1;
            activeGameMode = "None";
        }

        /// <summary>
        /// This struct holds tag strings used throughout the game.
        /// </summary>
        public struct Tags
        {
            public const string occupied = "Occupied";
            public const string selected = "Selected Tile";
            public const string active = "Active Tile";
            public const string inactive = "Inactive Tile";
            public const string staging = "Staging";
            public const string map = "Map";
            public const string damage = "Damage Popup";
        }

        /// <summary>
        /// This struct holds data related to map 1's creation and setup.
        /// </summary>
        public struct Map1Data
        {
            public const int gridLength = 11;
            public const int gridWidth = 11;
            public const int tileDimension = 6;
            public const int healingStructureTileIdx = 60;
            public static readonly int[] obstacleTileIdxs = { 52, 56, 64, 68 };
        }

        /// <summary>
        /// This struct holds data related to map 2's creation and setup.
        /// </summary>
        public struct Map2Data
        {
            public const int gridLength = 6;
            public const int gridWidth = 13;
            public const int tileDimension = 6;
            public const int attackRangeStructureTileIdx = 36;
            public const int movementRangeStructureTileIdx = 41;
            public static readonly int[] obstacleTileIdxs = { 25, 28, 49, 52 };
        }

        /// <summary>
        /// This struct holds data related to map 3's creation and setup.
        /// </summary>
        public struct Map3Data
        {
            public const int gridLength = 21;
            public const int gridWidth = 18;
            public const int tileDimension = 6;
            public static readonly int[] obstacleTileIdxs = { 58, 62 };
        }

        /// <summary>
        /// This struct indicates the unit idx for a given unit.
        /// </summary>
        public struct UnitIdx
        {
            public const int Knight = 0;
            public const int Archer = 1;
            public const int Cleric = 2;
            public const int Siege = 3;
            public const int Horseman = 4;
        }

        /// <summary>
        /// This struct indicates the population point value for a given unit.
        /// </summary>
        public struct UnitPopulationCost
        {
            public const int knight = 2;
            public const int archer = 2;
            public const int cleric = 3;
            public const int horseman = 3;
            public const int siege = 4;
        }

        /// <summary>
        /// Determine the active game map's tile grid length.
        /// </summary>
        /// <returns>The active game map's tile grid length.</returns>
        public static int LookUpActiveMapLength()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map1:
                    return Map1Data.gridLength;
                case ActiveGameMap.map2:
                    return Map2Data.gridLength;
                case ActiveGameMap.map3:
                    return Map3Data.gridLength;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's tile grid width.
        /// </summary>
        /// <returns>The active game map's tile grid width.</returns>
        public static int LookUpActiveMapWidth()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map1:
                    return Map1Data.gridWidth;
                case ActiveGameMap.map2:
                    return Map2Data.gridWidth;
                case ActiveGameMap.map3:
                    return Map3Data.gridWidth;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's tile dimension.
        /// </summary>
        /// <returns>The active game map's tile dimension.</returns>
        public static int LookUpActiveMapTileDimension()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map1:
                    return Map1Data.tileDimension;
                case ActiveGameMap.map2:
                    return Map2Data.tileDimension;
                case ActiveGameMap.map3:
                    return Map3Data.tileDimension;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's healing structure location, if it exists.
        /// </summary>
        /// <returns>If the healing structure exists, the active game map's tile index on which the healing structure resides; else -1.</returns>
        public static int LookUpActiveMapHealingStructureLocation()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map1:
                    return Map1Data.healingStructureTileIdx;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's attack range structure location, if it exists.
        /// </summary>
        /// <returns>If the attack range structure exists, the active game map's tile index on which the attack range structure resides; else -1.</returns>
        public static int LookUpActiveMapAttackRangeStructureLocation()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map2:
                    return Map2Data.attackRangeStructureTileIdx;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's movement range structure location, if it exists.
        /// </summary>
        /// <returns>If the movement range structure exists, the active game map's tile index on which the movement range structure resides; else -1.</returns>
        public static int LookUpActiveMapMovementRangeStructureLocation()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map2:
                    return Map2Data.movementRangeStructureTileIdx;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determine the active game map's obstacle tile locations, if obstacles exist.
        /// </summary>
        /// <returns>If obstacles exist, an array containing the active game map's tile indices on which the obstacles reside; else an empty array.</returns>
        public static int[] LookUpActiveMapObstacleLocationsArray()
        {
            switch (activeGameMap)
            {
                case ActiveGameMap.map1:
                    return Map1Data.obstacleTileIdxs;
                case ActiveGameMap.map2:
                    return Map2Data.obstacleTileIdxs;
                case ActiveGameMap.map3:
                    return Map3Data.obstacleTileIdxs;
                default:
                    return Array.Empty<int>();
            }
        }

        /// <summary>
        /// Determine the population cost of a particular unit type.
        /// </summary>
        /// <param name="name">The name of the unit, acquired using the unitType property in its UnitController.</param>
        /// <returns>The population cost of the unit.</returns>
        public static int LookUpPopulationCost(string name)
        {
            switch (name)
            {
                case "Knight":
                    return UnitPopulationCost.knight;
                case "Archer":
                    return UnitPopulationCost.archer;
                case "Cleric":
                    return UnitPopulationCost.cleric;
                case "Siege":
                    return UnitPopulationCost.siege;
                case "Horseman":
                    return UnitPopulationCost.horseman;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Determine the unit idx of a particular unit type.
        /// </summary>
        /// <param name="name">The name of the unit, acquired using the unitType property in its UnitController.</param>
        /// <returns>The unit idx of the unit.</returns>
        public static int LookUpUnitIdx(string name)
        {
            name = name.ToLower();
            switch (name)
            {
                case "knight":
                    return UnitIdx.Knight;
                case "archer":
                    return UnitIdx.Archer;
                case "cleric":
                    return UnitIdx.Cleric;
                case "siege":
                    return UnitIdx.Siege;
                case "horseman":
                    return UnitIdx.Horseman;
                default:
                    return 0;
            }
        }
    }
}
