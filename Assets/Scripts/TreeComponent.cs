using UnityEngine;

/// <summary>
/// Component that links a GameObject to a Tree data object
/// </summary>
public class TreeComponent : MonoBehaviour
{
    [Header("Tree Reference")]
    public Tree treeData;
    
    [Header("Visual Effects")]
    public bool showHealthBar = true;
    public float healthBarHeight = 2f;
    
    private Renderer treeRenderer;
    private Camera playerCamera;
    
    void Start()
    {
        treeRenderer = GetComponent<Renderer>();
        playerCamera = Camera.main;
    }
    
    void Update()
    {
        // Update visual representation based on tree data
        if (treeData != null)
        {
            UpdateVisualRepresentation();
        }
    }
    
    /// <summary>
    /// Initializes the component with tree data
    /// </summary>
    public void Initialize(Tree tree)
    {
        treeData = tree;
        UpdateVisualRepresentation();
    }
    
    /// <summary>
    /// Updates the visual representation based on tree data
    /// </summary>
    private void UpdateVisualRepresentation()
    {
        if (treeData == null) return;
        
        // Update position (in case tree data position changed)
        transform.position = treeData.position;
        
        // Update color based on health
        if (treeRenderer != null)
        {
            Color baseColor = GetBaseColorForTreeType(treeData.treeType);
            Color healthColor = Color.Lerp(Color.red, baseColor, treeData.health / 100f);
            
            // Add some variation based on environmental factors
            float brightness = 0.5f + (treeData.sunlightExposure * 0.3f) + (treeData.waterLevel * 0.2f);
            healthColor *= brightness;
            
            treeRenderer.material.color = healthColor;
        }
    }
    
    /// <summary>
    /// Gets the base color for a tree type
    /// </summary>
    private Color GetBaseColorForTreeType(TreeType treeType)
    {
        switch (treeType)
        {
            case TreeType.Oak:
                return new Color(0.4f, 0.8f, 0.2f); // Rich green
            case TreeType.Pine:
                return new Color(0.2f, 0.6f, 0.3f); // Dark green
            case TreeType.Birch:
                return new Color(0.6f, 0.9f, 0.4f); // Light green
            case TreeType.Maple:
                return new Color(0.8f, 0.4f, 0.2f); // Reddish (autumn colors)
            case TreeType.Willow:
                return new Color(0.5f, 0.8f, 0.6f); // Pale green
            default:
                return Color.green;
        }
    }
    
    /// <summary>
    /// Called when the tree is clicked
    /// </summary>
    void OnMouseDown()
    {
        if (treeData != null)
        {
            // Log this specific tree
            TreeLogger logger = FindObjectOfType<TreeLogger>();
            if (logger != null)
            {
                logger.LogTreeById(treeData.id);
            }
            
            // Also log to console for immediate feedback
            Debug.Log($"Clicked on tree: {treeData.treeType} (ID: {treeData.id})");
            Debug.Log($"Health: {treeData.health:F1}%, Height: {treeData.height:F2}m");
        }
    }
    
    /// <summary>
    /// Draws health bar and tree info in the scene
    /// </summary>
    void OnDrawGizmos()
    {
        if (treeData == null || !showHealthBar) return;
        
        // Draw health bar above the tree
        Vector3 healthBarPos = transform.position + Vector3.up * (treeData.height + healthBarHeight);
        
        // Background bar
        Gizmos.color = Color.red;
        Gizmos.DrawCube(healthBarPos, new Vector3(2f, 0.2f, 0.1f));
        
        // Health bar
        Gizmos.color = Color.green;
        float healthPercent = treeData.health / 100f;
        Gizmos.DrawCube(healthBarPos - Vector3.right * (1f - healthPercent), 
                       new Vector3(2f * healthPercent, 0.2f, 0.1f));
        
        // Tree info text would go here in a real implementation
        // Unity's Gizmos don't support text, so this would need a custom editor or UI system
    }
    
    /// <summary>
    /// Draws GUI information when selected
    /// </summary>
    void OnGUI()
    {
        if (treeData == null || playerCamera == null) return;
        
        // Only show GUI if this tree is close to the camera
        float distanceToCamera = Vector3.Distance(transform.position, playerCamera.transform.position);
        if (distanceToCamera > 20f) return;
        
        // Convert world position to screen position
        Vector3 screenPos = playerCamera.WorldToScreenPoint(transform.position + Vector3.up * treeData.height);
        
        // Only draw if tree is in front of camera
        if (screenPos.z > 0)
        {
            // Flip Y coordinate for GUI
            screenPos.y = Screen.height - screenPos.y;
            
            // Draw tree info
            string info = $"{treeData.treeType}\\nHealth: {treeData.health:F0}%\\nAge: {treeData.age}d";
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = treeData.isHealthy ? Color.green : Color.red;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 40, 100, 60), info, style);
        }
    }
}