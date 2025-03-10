using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ColorSwatches")]
public class ColorSwatches : ScriptableObject
{
	[Header("DECORATION:")]
	public ColorReference boxBrushWire;
	public ColorReference boxBrushCenter;
	
	[Header("DEBUG DRAW:")]
    public ColorReference salmon;
    public ColorReference sunset;
    public ColorReference rust;
    public ColorReference hotBlue;
}