using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ColorPick
{
    private const string swatchPath = "ColorSwatches/swatches";

    static ColorPick()
    {
        swatches = Resources.Load(swatchPath, typeof(ColorSwatches)) as ColorSwatches;
    }

    private static ColorSwatches swatches;
    public static ColorSwatches Swatches
    {
        get
        {
            if (swatches == null)
                swatches = Resources.Load(swatchPath, typeof(ColorSwatches)) as ColorSwatches;
            return swatches;
        }
    }
}