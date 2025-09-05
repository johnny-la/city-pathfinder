using UnityEngine;

/// <summary>
/// Utility class to procedurally generate simple tree prefabs at runtime
/// </summary>
public class TreePrefabGenerator : MonoBehaviour
{
    [Header("Tree Generation")]
    public Material trunkMaterial;
    public Material leavesMaterial;
    
    /// <summary>
    /// Creates a simple procedural tree prefab
    /// </summary>
    public static GameObject CreateSimpleTreePrefab(TreeType treeType)
    {
        GameObject treeRoot = new GameObject($"Tree_{treeType}");
        
        // Create trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.parent = treeRoot.transform;
        trunk.transform.localPosition = Vector3.up * 2.5f;
        trunk.transform.localScale = new Vector3(0.5f, 2.5f, 0.5f);
        
        // Create leaves/canopy
        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.name = "Leaves";
        leaves.transform.parent = treeRoot.transform;
        
        // Customize based on tree type
        switch (treeType)
        {
            case TreeType.Oak:
                leaves.transform.localPosition = Vector3.up * 6f;
                leaves.transform.localScale = Vector3.one * 4f;
                break;
            case TreeType.Pine:
                DestroyImmediate(leaves);
                leaves = CreatePineCanopy(treeRoot.transform);
                break;
            case TreeType.Birch:
                leaves.transform.localPosition = Vector3.up * 5.5f;
                leaves.transform.localScale = Vector3.one * 3f;
                trunk.transform.localScale = new Vector3(0.3f, 2.5f, 0.3f);
                break;
            case TreeType.Maple:
                leaves.transform.localPosition = Vector3.up * 6.5f;
                leaves.transform.localScale = Vector3.one * 4.5f;
                break;
            case TreeType.Willow:
                leaves.transform.localPosition = Vector3.up * 5f;
                leaves.transform.localScale = new Vector3(5f, 2f, 5f);
                break;
        }
        
        // Add materials
        Renderer trunkRenderer = trunk.GetComponent<Renderer>();
        if (trunkRenderer != null)
        {
            trunkRenderer.material.color = new Color(0.4f, 0.2f, 0.1f); // Brown
        }
        
        Renderer leavesRenderer = leaves.GetComponent<Renderer>();
        if (leavesRenderer != null)
        {
            leavesRenderer.material.color = GetTreeColor(treeType);
        }
        
        // Add collider for interaction
        SphereCollider collider = treeRoot.AddComponent<SphereCollider>();
        collider.radius = 2f;
        collider.center = Vector3.up * 3f;
        
        return treeRoot;
    }
    
    private static GameObject CreatePineCanopy(Transform parent)
    {
        GameObject canopy = new GameObject("PineCanopy");
        canopy.transform.parent = parent;
        
        // Create multiple cone layers for pine tree
        for (int i = 0; i < 3; i++)
        {
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = $"PineLayer_{i}";
            cone.transform.parent = canopy.transform;
            cone.transform.localPosition = Vector3.up * (4f + i * 1.5f);
            cone.transform.localScale = new Vector3(2f - i * 0.3f, 1f, 2f - i * 0.3f);
            
            // Make it more cone-like
            MeshFilter meshFilter = cone.GetComponent<MeshFilter>();
            // In a real implementation, you'd create a proper cone mesh here
        }
        
        return canopy;
    }
    
    private static Color GetTreeColor(TreeType treeType)
    {
        switch (treeType)
        {
            case TreeType.Oak:
                return new Color(0.2f, 0.6f, 0.1f);
            case TreeType.Pine:
                return new Color(0.1f, 0.4f, 0.2f);
            case TreeType.Birch:
                return new Color(0.4f, 0.8f, 0.3f);
            case TreeType.Maple:
                return new Color(0.8f, 0.4f, 0.1f);
            case TreeType.Willow:
                return new Color(0.3f, 0.7f, 0.4f);
            default:
                return Color.green;
        }
    }
}