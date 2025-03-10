using System;
using System.Collections.Generic;
using UnityEngine;

public enum SubGizmoDirection
{
    FORWARD = 1,
    BACKWARD = 2,
    LEFT = 3,
    RIGHT = 4
}

public enum SubGizmoCorner
{
    FRONT_TOP_LEFT = 5,
    FRONT_TOP_RIGHT = 6,
    FRONT_BOTTON_LEFT = 7,
    FRONT_BOTTON_RIGHT = 8,
    BACK_TOP_LEFT = 9,
    BACK_TOP_RIGHT = 10,
    BACK_BOTTOM_LEFT = 11,
    BACK_BOTTOM_RIGHT = 12,
}

public static class SubGizmoDirections
{
    public static Dictionary<SubGizmoDirection, Vector3> subGizmoDirLookup = new Dictionary<SubGizmoDirection, Vector3>()
    {
        { SubGizmoDirection.FORWARD, Vector3.forward },
        { SubGizmoDirection.BACKWARD, Vector3.back },
        { SubGizmoDirection.LEFT, Vector3.left},
        { SubGizmoDirection.RIGHT, Vector3.right }
    };

    public static Dictionary<int, Vector3> lookup = new Dictionary<int, Vector3>()
    {
        { (int)SubGizmoDirection.FORWARD, Vector3.forward },
        { (int)SubGizmoDirection.BACKWARD, Vector3.back },
        { (int)SubGizmoDirection.LEFT, Vector3.left },
        { (int)SubGizmoDirection.RIGHT, Vector3.right },
        { (int)SubGizmoCorner.FRONT_TOP_LEFT, Vector3.forward + Vector3.up + Vector3.left },
        { (int)SubGizmoCorner.FRONT_TOP_RIGHT, Vector3.forward + Vector3.up + Vector3.right },
        { (int)SubGizmoCorner.FRONT_BOTTON_LEFT, Vector3.forward + Vector3.down + Vector3.left },
        { (int)SubGizmoCorner.FRONT_BOTTON_RIGHT, Vector3.forward + Vector3.down + Vector3.right },
        { (int)SubGizmoCorner.BACK_TOP_LEFT, Vector3.back + Vector3.up + Vector3.left },
        { (int)SubGizmoCorner.BACK_TOP_RIGHT, Vector3.back + Vector3.up + Vector3.right },
        { (int)SubGizmoCorner.BACK_BOTTOM_LEFT, Vector3.back + Vector3.down + Vector3.left },
        { (int)SubGizmoCorner.BACK_BOTTOM_RIGHT, Vector3.back + Vector3.down + Vector3.right }
    };
}


public class SubGizmoMono : MonoBehaviour
{
    [Serializable]
    public class QuickDataClass
    {
        public float shownFloat;
        public float calculatedFloat;
        public List<Vector3> calculatedPositions = new List<Vector3>();
    }
    
    public float size = 5f;
    public float grabSize = 1f;
    
    public QuickDataClass[] quickDatas = new QuickDataClass[4];
}
