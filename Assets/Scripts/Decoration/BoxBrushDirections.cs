using System.Collections.Generic;
using UnityEngine;


public enum BoxBrushDirection
{
    FORWARD,
    BACKWARD,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public enum BoxBrushCornerType
{
    FRONT_TOP_LEFT,
    FRONT_TOP_RIGHT,
    FRONT_BOTTOM_LEFT,
    FRONT_BOTTOM_RIGHT,
    BACK_TOP_LEFT,
    BACK_TOP_RIGHT ,
    BACK_BOTTOM_LEFT ,
    BACK_BOTTOM_RIGHT
}

public enum BoxBrushEdge
{
    FRONT_TOP,
    FRONT_BOTTOM,
    FRONT_LEFT,
    FRONT_RIGHT,
    BACK_TOP,
    BACK_BOTTOM,
    BACK_LEFT,
    BACK_RIGHT,
    MID_TOP_RIGHT,
    MID_TOP_LEFT,
    MID_BOTTOM_RIGHT,
    MID_BOTTOM_LEFT
}

public static class BoxBrushDirections
{
    public static Dictionary<BoxBrushEdge, Vector3> edgeCenterLookup = new Dictionary<BoxBrushEdge, Vector3>()
    {
        { BoxBrushEdge.FRONT_TOP, Vector3.forward + Vector3.up },
        { BoxBrushEdge.FRONT_BOTTOM, Vector3.forward + Vector3.down },
        { BoxBrushEdge.FRONT_LEFT, Vector3.forward + Vector3.left },
        { BoxBrushEdge.FRONT_RIGHT, Vector3.forward + Vector3.right },
        
        { BoxBrushEdge.BACK_TOP, Vector3.back + Vector3.up },
        { BoxBrushEdge.BACK_BOTTOM, Vector3.back + Vector3.down },
        { BoxBrushEdge.BACK_LEFT, Vector3.back + Vector3.left },
        { BoxBrushEdge.BACK_RIGHT, Vector3.back + Vector3.right },
        
        { BoxBrushEdge.MID_TOP_RIGHT, Vector3.up + Vector3.right },
        { BoxBrushEdge.MID_TOP_LEFT, Vector3.up + Vector3.left },
        { BoxBrushEdge.MID_BOTTOM_RIGHT, Vector3.down + Vector3.right },
        { BoxBrushEdge.MID_BOTTOM_LEFT, Vector3.down + Vector3.left },
    };

    public static Dictionary<BoxBrushEdge, Vector3> edgeTangentLookup = new Dictionary<BoxBrushEdge, Vector3>()
    {
        { BoxBrushEdge.FRONT_TOP, Vector3.right },
        { BoxBrushEdge.FRONT_BOTTOM, Vector3.right },
        { BoxBrushEdge.FRONT_LEFT, Vector3.up },
        { BoxBrushEdge.FRONT_RIGHT, Vector3.up },
        
        { BoxBrushEdge.BACK_TOP, Vector3.left },
        { BoxBrushEdge.BACK_BOTTOM, Vector3.left },
        { BoxBrushEdge.BACK_LEFT, Vector3.up },
        { BoxBrushEdge.BACK_RIGHT, Vector3.up },
        
        { BoxBrushEdge.MID_TOP_RIGHT, Vector3.back },
        { BoxBrushEdge.MID_TOP_LEFT, Vector3.forward },
        { BoxBrushEdge.MID_BOTTOM_RIGHT, Vector3.back },
        { BoxBrushEdge.MID_BOTTOM_LEFT, Vector3.forward },
    };
    
    public static Dictionary<BoxBrushDirection, Vector3> faceDirLookup = new Dictionary<BoxBrushDirection, Vector3>()
    {
        { BoxBrushDirection.FORWARD, Vector3.forward },
        { BoxBrushDirection.BACKWARD, Vector3.back },
        { BoxBrushDirection.LEFT, Vector3.left },
        { BoxBrushDirection.RIGHT, Vector3.right },
        { BoxBrushDirection.UP, Vector3.up },
        { BoxBrushDirection.DOWN, Vector3.down }
    };
    
    public static Dictionary<BoxBrushDirection, Vector3> tangentLookup = new Dictionary<BoxBrushDirection, Vector3>()
    {
        { BoxBrushDirection.FORWARD, Vector3.up },
        { BoxBrushDirection.BACKWARD, Vector3.up },
        { BoxBrushDirection.LEFT, Vector3.up },
        { BoxBrushDirection.RIGHT, Vector3.up },
        { BoxBrushDirection.UP, Vector3.back },
        { BoxBrushDirection.DOWN, Vector3.forward }
    };
    
    public static Dictionary<BoxBrushDirection, Vector3> bitangentLookup = new Dictionary<BoxBrushDirection, Vector3>()
    {
        { BoxBrushDirection.FORWARD, Vector3.right },
        { BoxBrushDirection.BACKWARD, Vector3.left },
        { BoxBrushDirection.LEFT, Vector3.forward },
        { BoxBrushDirection.RIGHT, Vector3.back },
        { BoxBrushDirection.UP, Vector3.right },
        { BoxBrushDirection.DOWN, Vector3.right }
    };

    public static Dictionary<BoxBrushCornerType, Vector3> cornerNormalLookup = new Dictionary<BoxBrushCornerType, Vector3>()
    {
        { BoxBrushCornerType.FRONT_TOP_LEFT, Vector3.forward + Vector3.up + Vector3.left},
        { BoxBrushCornerType.FRONT_TOP_RIGHT, Vector3.forward + Vector3.up + Vector3.right},
        { BoxBrushCornerType.FRONT_BOTTOM_LEFT, Vector3.forward + Vector3.down + Vector3.left},
        { BoxBrushCornerType.FRONT_BOTTOM_RIGHT, Vector3.forward + Vector3.down + Vector3.right},
        { BoxBrushCornerType.BACK_TOP_LEFT , Vector3.back + Vector3.up + Vector3.left},
        { BoxBrushCornerType.BACK_TOP_RIGHT, Vector3.back + Vector3.up + Vector3.right},
        { BoxBrushCornerType.BACK_BOTTOM_LEFT, Vector3.back + Vector3.down + Vector3.left},
        { BoxBrushCornerType.BACK_BOTTOM_RIGHT, Vector3.back + Vector3.down + Vector3.right}
    };
    
    // FRONT_TOP_LEFT,
    // FRONT_TOP_RIGHT,
    // FRONT_BOTTOM_LEFT,
    // FRONT_BOTTOM_RIGHT,
    // BACK_TOP_LEFT,
    // BACK_TOP_RIGHT ,
    // BACK_BOTTOM_LEFT ,
    // BACK_BOTTOM_RIGHT
}