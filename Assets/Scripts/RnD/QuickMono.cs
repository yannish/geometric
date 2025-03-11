using UnityEngine;

public class QuickMono : MonoBehaviour
{
    public bool showAdvancedSettings;

    [ShowIf("showAdvancedSettings")]
    public float advancedValue;

    public int mode;

    [ShowIf("mode", 1)]
    public string mode1Setting;

    [ShowIf("mode", 2)]
    public string mode2Setting;
}
