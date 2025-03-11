using System;
using UnityEngine;

public class QuickMono : MonoBehaviour
{
    [Serializable]
    public class QuickMonoData
    {
        public bool quickBool;

        // [ShowIf("quickBool")]
        [Range(0f, 10f)]
        public float quickFloat;

        [ShowIf("quickBool")]
        public int quickInt;

        [ShowIf("quickBool")]
        public string quickString;
    }
    
    public QuickMonoData quickMonoData;
    
    public QuickMonoData[] quickMonoDataArray;
    
    public bool showAdvancedSettings;

    [ShowIf("showAdvancedSettings")]
    public float advancedValue;

    public int mode;

    [ShowIf("mode", 1)]
    public string mode1Setting;

    [ShowIf("mode", 2)]
    public string mode2Setting;
}
