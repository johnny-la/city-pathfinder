using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all trees in the scene, including generation, updates, and data access
/// </summary>
public class TreeManager : MonoBehaviour
{
    [Header("Tree Generation Settings")]
    public int maxTrees = 100;
    public float minDistanceBetweenTrees = 5f;
    public float minDistanceFromBuildings = 8f;
    
    [Header("Tree Prefabs")]
    public GameObject[] treePrefabs;
    public GameObject defaultTreePrefab;
    
    [Header("Generation Area")]
    public Vector3 generationAreaCenter = Vector3.zero;
    public Vector2 generationAreaSize = new Vector2(400f, 400f);
    
    [Header("Tree Growth")]
    public bool enableTreeGrowth = true;
    public float growthUpdateInterval = 1f; // Update growth every second
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool autoGenerateOnStart = true;
    
    // Internal data
    private List<Tree> trees = new List<Tree>();
    private Dictionary<string, GameObject> treeGameObjects = new Dictionary<string, GameObject>();
    private float lastGrowthUpdate;
    private CityGenerator cityGenerator;
    
    // Events
    public System.Action<Tree> OnTreeAdded;
    public System.Action<Tree> OnTreeRemoved;
    public System.Action<Tree> OnTreeHealthChanged;
    
    void Start()
    {
        // Find the city generator
        cityGenerator = FindObjectOfType<CityGenerator>();
        
        if (autoGenerateOnStart)
        {
            GenerateTrees();
        }
        
        lastGrowthUpdate = Time.time;
    }
    
    void Update()
    {
        // Handle tree growth updates
        if (enableTreeGrowth && Time.time - lastGrowthUpdate >= growthUpdateInterval)
        {
            UpdateTreeGrowth();
            lastGrowthUpdate = Time.time;
        }
        
        // Handle manual tree generation
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateTrees();
        }
        
        // Handle tree removal
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveRandomTree();
        }
        
        // Handle adding a tree at camera position
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddTreeAtPosition(Camera.main.transform.position);
        }
    }
    
    /// <summary>
    /// Generates trees in the specified area, avoiding buildings
    /// </summary>
    public void GenerateTrees()
    {
        Debug.Log("Generating trees...");
        
        // Clear existing trees
        ClearAllTrees();
        
        int treesGenerated = 0;
        int maxAttempts = maxTrees * 10; // Prevent infinite loops
        int attempts = 0;
        
        while (treesGenerated < maxTrees && attempts < maxAttempts)
        {
            attempts++;
            
            // Generate random position within the generation area
            Vector3 randomPosition = GetRandomPositionInArea();
            
            // Check if position is valid (not too close to buildings or other trees)
            if (IsValidTreePosition(randomPosition))
            {
                // Create a new tree
                TreeType randomType = (TreeType)Random.Range(0, System.Enum.GetValues(typeof(TreeType)).Length);
                float randomHeight = Random.Range(3f, 8f);
                
                Tree newTree = new Tree(randomPosition, randomType, randomHeight);
                AddTree(newTree);
                treesGenerated++;
            }
        }
        
        Debug.Log($"Generated {treesGenerated} trees in {attempts} attempts.");
    }
    
    /// <summary>
    /// Adds a tree to the manager
    /// </summary>
    public void AddTree(Tree tree)
    {
        trees.Add(tree);
        
        // Create visual representation
        GameObject treeObject = CreateTreeGameObject(tree);
        if (treeObject != null)
        {
            treeGameObjects[tree.id] = treeObject;
        }
        
        OnTreeAdded?.Invoke(tree);
        Debug.Log($"Added {tree.treeType} tree at {tree.position}");
    }
    
    /// <summary>
    /// Adds a tree at a specific position
    /// </summary>
    public void AddTreeAtPosition(Vector3 position)
    {
        if (IsValidTreePosition(position))
        {
            TreeType randomType = (TreeType)Random.Range(0, System.Enum.GetValues(typeof(TreeType)).Length);
            float randomHeight = Random.Range(3f, 8f);
            
            Tree newTree = new Tree(position, randomType, randomHeight);
            AddTree(newTree);
        }
        else
        {
            Debug.LogWarning($"Cannot place tree at {position} - invalid position");
        }
    }
    
    /// <summary>
    /// Removes a tree by ID
    /// </summary>
    public bool RemoveTree(string treeId)
    {
        Tree tree = trees.FirstOrDefault(t => t.id == treeId);
        if (tree != null)
        {
            trees.Remove(tree);
            
            // Remove visual representation
            if (treeGameObjects.ContainsKey(treeId))
            {
                DestroyImmediate(treeGameObjects[treeId]);
                treeGameObjects.Remove(treeId);
            }
            
            OnTreeRemoved?.Invoke(tree);
            Debug.Log($"Removed tree {treeId}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Removes a random tree (for testing)
    /// </summary>
    public void RemoveRandomTree()
    {
        if (trees.Count > 0)
        {
            int randomIndex = Random.Range(0, trees.Count);
            RemoveTree(trees[randomIndex].id);
        }
    }
    
    /// <summary>
    /// Clears all trees
    /// </summary>
    public void ClearAllTrees()
    {
        // Destroy all game objects
        foreach (var kvp in treeGameObjects)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        
        treeGameObjects.Clear();
        trees.Clear();
        
        Debug.Log("Cleared all trees");
    }
    
    /// <summary>
    /// Updates tree growth and health
    /// </summary>
    private void UpdateTreeGrowth()
    {
        foreach (Tree tree in trees)
        {
            float previousHealth = tree.health;
            tree.UpdateTree(growthUpdateInterval);
            
            // Update visual representation if tree has grown
            if (treeGameObjects.ContainsKey(tree.id))
            {
                UpdateTreeGameObject(tree, treeGameObjects[tree.id]);
            }
            
            // Fire health changed event if health changed significantly
            if (Mathf.Abs(tree.health - previousHealth) > 1f)
            {
                OnTreeHealthChanged?.Invoke(tree);
            }
        }
    }
    
    /// <summary>
    /// Creates a GameObject representation of a tree
    /// </summary>
    private GameObject CreateTreeGameObject(Tree tree)
    {
        GameObject treePrefab = GetTreePrefab(tree.treeType);
        if (treePrefab == null)
        {
            Debug.LogWarning($"No prefab found for tree type {tree.treeType}");
            return null;
        }
        
        GameObject treeObject = Instantiate(treePrefab, tree.position, Quaternion.identity);
        treeObject.transform.parent = transform;
        treeObject.name = $"Tree_{tree.treeType}_{tree.id.Substring(0, 8)}";
        
        // Scale the tree based on its properties
        UpdateTreeGameObject(tree, treeObject);
        
        // Add a TreeComponent to link the visual to the data
        TreeComponent treeComponent = treeObject.GetComponent<TreeComponent>();
        if (treeComponent == null)
        {
            treeComponent = treeObject.AddComponent<TreeComponent>();
        }
        treeComponent.Initialize(tree);
        
        return treeObject;
    }
    
    /// <summary>
    /// Updates the visual representation of a tree
    /// </summary>
    private void UpdateTreeGameObject(Tree tree, GameObject treeObject)
    {
        if (treeObject == null) return;
        
        // Update scale based on tree size
        float scaleMultiplier = tree.height / 10f; // Assuming base prefab is ~10 units tall
        treeObject.transform.localScale = Vector3.one * scaleMultiplier;
        
        // Update color based on health
        Renderer renderer = treeObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color healthColor = Color.Lerp(Color.red, Color.green, tree.health / 100f);
            renderer.material.color = healthColor;
        }
    }
    
    /// <summary>
    /// Gets the appropriate prefab for a tree type
    /// </summary>
    private GameObject GetTreePrefab(TreeType treeType)
    {
        if (treePrefabs != null && treePrefabs.Length > (int)treeType)
        {
            return treePrefabs[(int)treeType];
        }
        
        return defaultTreePrefab;
    }
    
    /// <summary>
    /// Generates a random position within the generation area
    /// </summary>
    private Vector3 GetRandomPositionInArea()
    {
        float x = Random.Range(generationAreaCenter.x - generationAreaSize.x / 2,
                              generationAreaCenter.x + generationAreaSize.x / 2);
        float z = Random.Range(generationAreaCenter.z - generationAreaSize.y / 2,
                              generationAreaCenter.z + generationAreaSize.y / 2);
        
        return new Vector3(x, 0, z);
    }
    
    /// <summary>
    /// Checks if a position is valid for placing a tree
    /// </summary>
    private bool IsValidTreePosition(Vector3 position)
    {
        // Check distance from other trees
        foreach (Tree existingTree in trees)
        {
            if (Vector3.Distance(position, existingTree.position) < minDistanceBetweenTrees)
            {
                return false;
            }
        }
        
        // Check distance from buildings (if city generator exists)
        if (cityGenerator != null)
        {
            // Simple check - in a real implementation you'd check against actual building positions
            // For now, we'll use a basic grid-based check
            if (IsNearBuilding(position))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Simple check to see if position is near a building
    /// </summary>
    private bool IsNearBuilding(Vector3 position)
    {
        // This is a simplified check - in reality you'd want to check against actual building positions
        // For now, we'll assume buildings are on a grid and check if we're too close to grid points
        
        float cellSize = 20f; // Assuming same cell size as CityGenerator
        float gridX = Mathf.Round(position.x / cellSize) * cellSize;
        float gridZ = Mathf.Round(position.z / cellSize) * cellSize;
        
        Vector3 nearestGridPoint = new Vector3(gridX, 0, gridZ);
        return Vector3.Distance(position, nearestGridPoint) < minDistanceFromBuildings;
    }
    
    #region Public Data Access Methods
    
    /// <summary>
    /// Gets all trees
    /// </summary>
    public List<Tree> GetAllTrees()
    {
        return new List<Tree>(trees);
    }
    
    /// <summary>
    /// Gets a tree by ID
    /// </summary>
    public Tree GetTreeById(string id)
    {
        return trees.FirstOrDefault(t => t.id == id);
    }
    
    /// <summary>
    /// Gets trees within a specific area
    /// </summary>
    public List<Tree> GetTreesInArea(Vector3 center, float radius)
    {
        return trees.Where(t => Vector3.Distance(t.position, center) <= radius).ToList();
    }
    
    /// <summary>
    /// Gets trees of a specific type
    /// </summary>
    public List<Tree> GetTreesByType(TreeType treeType)
    {
        return trees.Where(t => t.treeType == treeType).ToList();
    }
    
    /// <summary>
    /// Gets healthy trees
    /// </summary>
    public List<Tree> GetHealthyTrees()
    {
        return trees.Where(t => t.isHealthy).ToList();
    }
    
    /// <summary>
    /// Gets unhealthy trees
    /// </summary>
    public List<Tree> GetUnhealthyTrees()
    {
        return trees.Where(t => !t.isHealthy).ToList();
    }
    
    /// <summary>
    /// Gets the total number of trees
    /// </summary>
    public int GetTreeCount()
    {
        return trees.Count;
    }
    
    #endregion
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw generation area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(generationAreaCenter, new Vector3(generationAreaSize.x, 1, generationAreaSize.y));
        
        // Draw tree positions
        Gizmos.color = Color.green;
        foreach (Tree tree in trees)
        {
            Gizmos.DrawWireSphere(tree.position, tree.radius);
        }
    }
}