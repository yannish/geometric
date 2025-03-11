using UnityEngine;

public class PrefabBoundsCalculator : MonoBehaviour
{
    public GameObject prefabAsset;
    public GameObject instancedPrefab;
    public bool checkAgainstMeshRenderers = false;
    public float calculatedPrefabSize;

    public Vector3 boundsSize;
    public Vector3 boundsExtents;

    [ContextMenu("Calculate Instance Bounds")]
    public void CalculateInstanceBounds() => CalculateBounds(instancedPrefab);

    [ContextMenu("Calculate Prefab Bounds")]
    public void CalculateBounds() => CalculateBounds(prefabAsset);

    public void CalculateBounds(GameObject gameObjectToCheck)
    {
       if(checkAgainstMeshRenderers)
           CalculateBoundsFromMeshRenderers(gameObjectToCheck);
       else
           CalculateBoundsFromColliders(gameObjectToCheck);
    }

    [ContextMenu("Report prefab Bounds")]
    public void Report() => ReportBoundsSize(prefabAsset);

    public void ReportBoundsSize(GameObject prefabToCheck)
    {
        if (prefabToCheck == null)
        {
            return;
        }
        
        var colliders = prefabToCheck.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            Debug.LogWarning($"col: {col.bounds.size}");
        }
        
        Debug.LogWarning($"checked : {colliders.Length}");
    }

    public void CalculateBoundsFromColliders(GameObject gameObjectToCheck)
    {
        if (gameObjectToCheck == null)
            return;

        var colliders = gameObjectToCheck.GetComponentsInChildren<Collider>();
        
        var bounds = new Bounds();
        var firstBound = true;
        
        foreach (var collider in colliders)
        {
            if (firstBound)
            {
                Debug.LogWarning($"setting first: {collider.bounds.size}");
                
                bounds = collider.bounds;
                firstBound = false;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }
        
        Debug.Log($"bounds done, checked against {colliders.Length}");
        
        boundsSize = bounds.size;
        boundsExtents = bounds.extents;
    }

    public void CalculateBoundsFromMeshRenderers(GameObject gameObjectToCheck)
    {
        if (gameObjectToCheck == null)
            return;

        var meshRenderers = gameObjectToCheck.GetComponentsInChildren<MeshRenderer>();
        
        var bounds = new Bounds();
        var firstBound = true;
        
        foreach (var meshRenderer in meshRenderers)
        {
            if (firstBound)
            {
                Debug.LogWarning($"setting first: {meshRenderer.bounds.size}");

                bounds = meshRenderer.bounds;
                firstBound = false;
            }
            else
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }
        }
        
        Debug.Log($"bounds done, checked against {meshRenderers.Length}");
        
        boundsSize = bounds.size;
        boundsExtents = bounds.extents;
    }
}
