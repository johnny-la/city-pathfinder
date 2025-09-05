using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Level of Detail (LOD) system for optimized tree rendering performance
/// </summary>
public class TreeLODSystem : MonoBehaviour
{
    [Header("LOD Settings")]
    public bool enableLOD = true;
    
    [Header("Distance Thresholds")]
    [Range(10f, 200f)]
    public float highDetailDistance = 50f;
    
    [Range(50f, 500f)]
    public float mediumDetailDistance = 150f;
    
    [Range(100f, 1000f)]
    public float lowDetailDistance = 300f;
    
    [Header("Performance Settings")]
    [Range(1, 50)]
    public int maxTreeUpdatesPerFrame = 10;
    
    [Range(0.1f, 2f)]
    public float lodUpdateInterval = 0.5f;
    
    [Header("Culling Settings")]
    public bool enableFrustumCulling = true;
    public bool enableOcclusionCulling = false;
    
    [Header("Detail Levels")]
    public TreeLODSettings highDetailSettings;
    public TreeLODSettings mediumDetailSettings;
    public TreeLODSettings lowDetailSettings;
    
    [Header("Debug")]
    public bool showLODGizmos = false;
    public bool showPerformanceStats = true;
    
    // Internal data
    private Dictionary<string, TreeLODData> treeLODData = new Dictionary<string, TreeLODData>();
    private Queue<string> treeUpdateQueue = new Queue<string>();
    private Camera playerCamera;
    private float lastLODUpdate;
    private TreeManager treeManager;
    
    // Performance tracking
    private int treesUpdatedThisFrame;
    private int totalTreesManaged;
    private int highDetailTrees;
    private int mediumDetailTrees;
    private int lowDetailTrees;
    private int culledTrees;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        treeManager = FindObjectOfType<TreeManager>();
        
        // Initialize default LOD settings if not configured
        InitializeDefaultLODSettings();
        
        // Subscribe to tree manager events
        if (treeManager != null)
        {
            treeManager.OnTreeAdded += RegisterTree;
            treeManager.OnTreeRemoved += UnregisterTree;
        }
        
        lastLODUpdate = Time.time;
        Debug.Log("Tree LOD System initialized");
    }
    
    void Update()
    {
        if (!enableLOD || playerCamera == null) return;
        
        treesUpdatedThisFrame = 0;
        
        // Update LOD levels periodically
        if (Time.time - lastLODUpdate >= lodUpdateInterval)
        {
            UpdateLODLevels();
            lastLODUpdate = Time.time;
        }
        
        // Process tree updates in batches
        ProcessTreeUpdates();
    }
    
    /// <summary>
    /// Initializes default LOD settings
    /// </summary>
    private void InitializeDefaultLODSettings()
    {
        if (highDetailSettings == null)
        {
            highDetailSettings = new TreeLODSettings
            {
                renderTrunk = true,
                renderLeaves = true,
                renderDetails = true,
                shadowCasting = UnityEngine.Rendering.ShadowCastingMode.On,
                shadowReceiving = true,
                meshQuality = 1f,
                textureQuality = 1f,
                updateFrequency = 1f
            };
        }
        
        if (mediumDetailSettings == null)
        {
            mediumDetailSettings = new TreeLODSettings
            {
                renderTrunk = true,
                renderLeaves = true,
                renderDetails = false,
                shadowCasting = UnityEngine.Rendering.ShadowCastingMode.On,
                shadowReceiving = false,
                meshQuality = 0.7f,
                textureQuality = 0.7f,
                updateFrequency = 0.5f
            };
        }
        
        if (lowDetailSettings == null)
        {
            lowDetailSettings = new TreeLODSettings
            {
                renderTrunk = true,
                renderLeaves = false,
                renderDetails = false,
                shadowCasting = UnityEngine.Rendering.ShadowCastingMode.Off,
                shadowReceiving = false,
                meshQuality = 0.3f,
                textureQuality = 0.5f,
                updateFrequency = 0.1f
            };
        }
    }
    
    /// <summary>
    /// Registers a tree with the LOD system
    /// </summary>
    public void RegisterTree(Tree tree)
    {
        if (treeLODData.ContainsKey(tree.id)) return;
        
        var lodData = new TreeLODData
        {
            tree = tree,
            currentLODLevel = TreeLODLevel.High,
            lastUpdateTime = Time.time,
            isVisible = true,
            isCulled = false
        };
        
        treeLODData[tree.id] = lodData;
        treeUpdateQueue.Enqueue(tree.id);
        totalTreesManaged++;
    }
    
    /// <summary>
    /// Unregisters a tree from the LOD system
    /// </summary>
    public void UnregisterTree(Tree tree)
    {
        if (treeLODData.ContainsKey(tree.id))
        {
            treeLODData.Remove(tree.id);
            totalTreesManaged--;
        }
    }
    
    /// <summary>
    /// Updates LOD levels for all trees based on distance to camera
    /// </summary>
    private void UpdateLODLevels()
    {
        if (playerCamera == null) return;
        
        Vector3 cameraPosition = playerCamera.transform.position;
        Plane[] frustumPlanes = null;
        
        if (enableFrustumCulling)
        {
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        }
        
        // Reset counters
        highDetailTrees = 0;
        mediumDetailTrees = 0;
        lowDetailTrees = 0;
        culledTrees = 0;
        
        foreach (var kvp in treeLODData)
        {
            var lodData = kvp.Value;
            var tree = lodData.tree;
            
            // Calculate distance to camera
            float distance = Vector3.Distance(cameraPosition, tree.position);
            
            // Frustum culling
            bool inFrustum = true;
            if (enableFrustumCulling && frustumPlanes != null)
            {
                Bounds treeBounds = new Bounds(tree.position, Vector3.one * tree.radius * 2);
                inFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, treeBounds);
            }
            
            // Determine LOD level
            TreeLODLevel newLODLevel;
            bool shouldCull = !inFrustum || distance > lowDetailDistance;
            
            if (shouldCull)
            {
                newLODLevel = TreeLODLevel.Culled;
                culledTrees++;
            }
            else if (distance <= highDetailDistance)
            {
                newLODLevel = TreeLODLevel.High;
                highDetailTrees++;
            }
            else if (distance <= mediumDetailDistance)
            {
                newLODLevel = TreeLODLevel.Medium;
                mediumDetailTrees++;
            }
            else
            {
                newLODLevel = TreeLODLevel.Low;
                lowDetailTrees++;
            }
            
            // Update LOD data
            lodData.distanceToCamera = distance;
            lodData.isVisible = inFrustum;
            lodData.isCulled = shouldCull;
            
            // If LOD level changed, queue for update
            if (newLODLevel != lodData.currentLODLevel)
            {
                lodData.currentLODLevel = newLODLevel;
                treeUpdateQueue.Enqueue(tree.id);
            }
        }
    }
    
    /// <summary>
    /// Processes tree visual updates in batches
    /// </summary>
    private void ProcessTreeUpdates()
    {
        while (treeUpdateQueue.Count > 0 && treesUpdatedThisFrame < maxTreeUpdatesPerFrame)
        {
            string treeId = treeUpdateQueue.Dequeue();
            
            if (treeLODData.ContainsKey(treeId))
            {
                UpdateTreeVisuals(treeLODData[treeId]);
                treesUpdatedThisFrame++;
            }
        }
    }
    
    /// <summary>
    /// Updates the visual representation of a tree based on its LOD level
    /// </summary>
    private void UpdateTreeVisuals(TreeLODData lodData)
    {
        if (treeManager == null) return;
        
        // Find the tree's GameObject
        GameObject treeObject = FindTreeGameObject(lodData.tree.id);
        if (treeObject == null) return;
        
        TreeLODSettings settings = GetLODSettings(lodData.currentLODLevel);
        
        // Update visibility
        bool shouldBeVisible = lodData.currentLODLevel != TreeLODLevel.Culled;
        if (treeObject.activeSelf != shouldBeVisible)
        {
            treeObject.SetActive(shouldBeVisible);
        }
        
        if (!shouldBeVisible) return;
        
        // Update renderers
        Renderer[] renderers = treeObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // Update shadow casting
            renderer.shadowCastingMode = settings.shadowCasting;
            renderer.receiveShadows = settings.shadowReceiving;
            
            // Update material properties based on LOD
            if (renderer.material != null)
            {
                // Adjust texture quality (simplified - in practice you'd swap materials)
                Color currentColor = renderer.material.color;
                currentColor.a = settings.textureQuality;
                renderer.material.color = currentColor;
            }
        }
        
        // Update specific components based on LOD level
        UpdateTreeComponents(treeObject, settings);
        
        lodData.lastUpdateTime = Time.time;
    }
    
    /// <summary>
    /// Updates specific tree components based on LOD settings
    /// </summary>
    private void UpdateTreeComponents(GameObject treeObject, TreeLODSettings settings)
    {
        // Find trunk and leaves
        Transform trunk = treeObject.transform.Find("Trunk");
        Transform leaves = treeObject.transform.Find("Leaves") ?? treeObject.transform.Find("PineCanopy");
        
        // Update trunk visibility
        if (trunk != null)
        {
            trunk.gameObject.SetActive(settings.renderTrunk);
        }
        
        // Update leaves visibility
        if (leaves != null)
        {
            leaves.gameObject.SetActive(settings.renderLeaves);
        }
        
        // Scale based on mesh quality (simplified LOD)
        if (settings.meshQuality < 1f)
        {
            Vector3 scale = treeObject.transform.localScale;
            float targetScale = Mathf.Lerp(0.5f, 1f, settings.meshQuality);
            treeObject.transform.localScale = Vector3.one * targetScale;
        }
    }
    
    /// <summary>
    /// Gets LOD settings for a specific level
    /// </summary>
    private TreeLODSettings GetLODSettings(TreeLODLevel level)
    {
        switch (level)
        {
            case TreeLODLevel.High:
                return highDetailSettings;
            case TreeLODLevel.Medium:
                return mediumDetailSettings;
            case TreeLODLevel.Low:
                return lowDetailSettings;
            default:
                return lowDetailSettings;
        }
    }
    
    /// <summary>
    /// Finds a tree GameObject by tree ID
    /// </summary>
    private GameObject FindTreeGameObject(string treeId)
    {
        if (treeManager == null) return null;
        
        // This would need to be implemented in TreeManager
        // For now, we'll search by name pattern
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        return allObjects.FirstOrDefault(obj => obj.name.Contains(treeId.Substring(0, 8)));
    }
    
    /// <summary>
    /// Gets performance statistics
    /// </summary>
    public TreeLODStats GetPerformanceStats()
    {
        return new TreeLODStats
        {
            totalTrees = totalTreesManaged,
            highDetailTrees = highDetailTrees,
            mediumDetailTrees = mediumDetailTrees,
            lowDetailTrees = lowDetailTrees,
            culledTrees = culledTrees,
            treesUpdatedThisFrame = treesUpdatedThisFrame,
            queuedUpdates = treeUpdateQueue.Count
        };
    }
    
    void OnDrawGizmos()
    {
        if (!showLODGizmos || playerCamera == null) return;
        
        Vector3 cameraPos = playerCamera.transform.position;
        
        // Draw LOD distance spheres
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cameraPos, highDetailDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraPos, mediumDetailDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cameraPos, lowDetailDistance);
        
        // Draw tree LOD levels
        foreach (var kvp in treeLODData)
        {
            var lodData = kvp.Value;
            
            switch (lodData.currentLODLevel)
            {
                case TreeLODLevel.High:
                    Gizmos.color = Color.green;
                    break;
                case TreeLODLevel.Medium:
                    Gizmos.color = Color.yellow;
                    break;
                case TreeLODLevel.Low:
                    Gizmos.color = Color.red;
                    break;
                case TreeLODLevel.Culled:
                    Gizmos.color = Color.gray;
                    break;
            }
            
            Gizmos.DrawWireCube(lodData.tree.position, Vector3.one * 2f);
        }
    }
    
    void OnGUI()
    {
        if (!showPerformanceStats) return;
        
        var stats = GetPerformanceStats();
        
        GUILayout.BeginArea(new Rect(480, 10, 200, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Tree LOD Performance");
        GUILayout.Label($"Total Trees: {stats.totalTrees}");
        GUILayout.Label($"High Detail: {stats.highDetailTrees}");
        GUILayout.Label($"Medium Detail: {stats.mediumDetailTrees}");
        GUILayout.Label($"Low Detail: {stats.lowDetailTrees}");
        GUILayout.Label($"Culled: {stats.culledTrees}");
        GUILayout.Label($"Updates/Frame: {stats.treesUpdatedThisFrame}");
        GUILayout.Label($"Queued: {stats.queuedUpdates}");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void OnDestroy()
    {
        if (treeManager != null)
        {
            treeManager.OnTreeAdded -= RegisterTree;
            treeManager.OnTreeRemoved -= UnregisterTree;
        }
    }
}

/// <summary>
/// LOD settings for different detail levels
/// </summary>
[System.Serializable]
public class TreeLODSettings
{
    public bool renderTrunk = true;
    public bool renderLeaves = true;
    public bool renderDetails = true;
    public UnityEngine.Rendering.ShadowCastingMode shadowCasting = UnityEngine.Rendering.ShadowCastingMode.On;
    public bool shadowReceiving = true;
    
    [Range(0.1f, 1f)]
    public float meshQuality = 1f;
    
    [Range(0.1f, 1f)]
    public float textureQuality = 1f;
    
    [Range(0.1f, 1f)]
    public float updateFrequency = 1f;
}

/// <summary>
/// LOD data for individual trees
/// </summary>
public class TreeLODData
{
    public Tree tree;
    public TreeLODLevel currentLODLevel;
    public float distanceToCamera;
    public bool isVisible;
    public bool isCulled;
    public float lastUpdateTime;
}

/// <summary>
/// LOD levels for trees
/// </summary>
public enum TreeLODLevel
{
    High,
    Medium,
    Low,
    Culled
}

/// <summary>
/// Performance statistics for the LOD system
/// </summary>
public struct TreeLODStats
{
    public int totalTrees;
    public int highDetailTrees;
    public int mediumDetailTrees;
    public int lowDetailTrees;
    public int culledTrees;
    public int treesUpdatedThisFrame;
    public int queuedUpdates;
}