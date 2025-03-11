using UnityEngine;

public class Dummy : MonoBehaviour
{
    [Clamp(0, 1000)]
    public int clampedInt;
    
    [Clamp(0, 1000)]
    public float clampedFloat;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
