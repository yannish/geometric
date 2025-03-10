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

public static class BoxBrushDirections
{
    public static Dictionary<BoxBrushDirection, Vector3> brushDirLookup = new Dictionary<BoxBrushDirection, Vector3>()
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
}