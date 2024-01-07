namespace Catan;

using Microsoft.Xna.Framework;
using Type = Resources.Type;

internal static class Rules 
{
    public static readonly Resources.Collection SETTLEMENT_COST = new(){
        Brick = 1,
        Grain = 1,
        Lumber = 1,
        Ore = 1
    };

    public static readonly Resources.Collection ROAD_COST = new(){
        Brick = 1,
        Lumber = 1
    };

    public static readonly Resources.Collection CITY_COST = new(){
        Grain = 2,
        Ore = 3
    };

    public static readonly Resources.Collection DEVELOPMENT_CARD_COST = new(){
        Grain = 1,
        Ore = 1,
        Wool = 1
    };

    public const int MAX_HAND_SIZE = 7;

    public const int NUM_PLAYERS = 4;

    public const int BOARD_WIDTH = 5;

    public const int MAX_SETTLEMENTS = 5;
    public const int MAX_ROADS = 15;
    public const int MAX_CITIES = 4;

    public const int MAX_VICTORY_POINTS = 10;

    public static readonly Resources.Collection BANK_START = new(){
        Brick = 19,
        Grain = 19,
        Lumber = 19,
        Ore = 19,
        Wool = 19
    };

    public static readonly Type[] DEFAULT_RESOURCE_SPREAD = new Type[]
        {Type.Wool, Type.Grain, Type.Brick,
            Type.Wool, Type.Grain, Type.Ore, Type.Lumber,
                Type.Ore, Type.Lumber, Type.Empty, Type.Lumber, Type.Grain,
                    Type.Brick, Type.Wool, Type.Brick, Type.Grain,
                        Type.Lumber, Type.Wool, Type.Ore
    };

    public static readonly int[] DEFAULT_VALUE_SPREAD = new int[]
        {11, 6, 5, 
            5, 4, 3, 8, 
                8, 3, 11, 9, 
                    10, 4, 6, 12, 
                        9, 2, 10
    };

    public const int DEV_CARD_VICTORY_POINT_COUNT = 5;
    public const int DEV_CARD_KNIGHT_COUNT = 14;

    // NOTE: In base rules all progress typed dev cards have 2 copies, could be replaced with singluar PROGRESS_CARD_COUNT?
    public const int DEV_CARD_ROAD_BUILDING_COUNT = 0;
    public const int DEV_CARD_YEAR_OF_PLENTY_COUNT = 0;
    public const int DEV_CARD_MONOPOLY_COUNT = 0;

    public static Color GetPlayerIDColour(int playerID)
    {
        return playerID switch
        {
            0 => Color.Blue,
            1 => Color.Orange,
            2 => Color.White,
            3 => Color.Red,
            _ => Color.Black,
        };
    }
}