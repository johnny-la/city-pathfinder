using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Configuration system for tree generation and management parameters
/// Supports runtime loading/saving and preset configurations
/// </summary>
[CreateAssetMenu(fileName = "TreeConfiguration", menuName = "Tree System/Tree Configuration")]
public class TreeConfiguration : ScriptableObject
{
    [Header("Generation Settings")]
    [Range(10, 1000)]
    public int maxTrees = 100;
    
    [Range(1f, 20f)]
    public float minDistanceBetweenTrees = 5f;
    
    [Range(1f, 50f)]
    public float minDistanceFromBuildings = 8f;
    
    [Range(100f, 2000f)]
    public float generationAreaSize = 400f;
    
    [Header("Growth Parameters")]
    public bool enableTreeGrowth = true;
    
    [Range(0.1f, 10f)]
    public float growthUpdateInterval = 1f;
    
    [Range(0.1f, 5f)]
    public float globalGrowthMultiplier = 1f;
    
    [Header("Environmental Factors")]
    [Range(0f, 1f)]
    public float baseSunlightLevel = 0.7f;
    
    [Range(0f, 1f)]
    public float baseWaterLevel = 0.5f;
    
    [Range(0f, 1f)]
    public float environmentalVariation = 0.3f;
    
    [Header("Health System")]
    [Range(50f, 100f)]
    public float initialHealthRange = 90f;
    
    [Range(0.1f, 5f)]
    public float healthDecayRate = 1f;
    
    [Range(0.1f, 5f)]
    public float healthRecoveryRate = 1.5f;
    
    [Range(10f, 50f)]
    public float criticalHealthThreshold = 20f;
    
    [Header("Tree Type Probabilities")]
    [Range(0f, 1f)]
    public float oakProbability = 0.3f;
    
    [Range(0f, 1f)]
    public float pineProbability = 0.2f;
    
    [Range(0f, 1f)]
    public float birchProbability = 0.2f;
    
    [Range(0f, 1f)]
    public float mapleProbability = 0.15f;
    
    [Range(0f, 1f)]
    public float willowProbability = 0.15f;
    
    [Header("Logging Settings")]
    public bool enableAutoLogging = true;
    
    [Range(10f, 300f)]
    public float autoLogInterval = 60f;
    
    public bool enableConsoleLogging = true;
    public bool enableFileLogging = true;
    public bool enableCSVExport = true;
    
    [Header("Analytics Settings")]
    public bool enableAnalytics = true;
    
    [Range(1f, 60f)]
    public float analyticsUpdateInterval = 5f;
    
    [Range(10, 1000)]
    public int maxHistoryEntries = 100;
    
    [Header("Performance Settings")]
    public bool enableLOD = true;
    
    [Range(10f, 200f)]
    public float lodDistance1 = 50f;
    
    [Range(50f, 500f)]
    public float lodDistance2 = 150f;
    
    [Range(1, 10)]
    public int maxTreeUpdatesPerFrame = 5;
    
    /// <summary>
    /// Validates and normalizes tree type probabilities
    /// </summary>
    void OnValidate()
    {
        // Ensure probabilities sum to 1.0
        float totalProbability = oakProbability + pineProbability + birchProbability + 
                                mapleProbability + willowProbability;
        
        if (Mathf.Abs(totalProbability - 1f) > 0.01f && totalProbability > 0f)
        {
            oakProbability /= totalProbability;
            pineProbability /= totalProbability;
            birchProbability /= totalProbability;
            mapleProbability /= totalProbability;
            willowProbability /= totalProbability;
        }
    }
    
    /// <summary>
    /// Gets a random tree type based on configured probabilities
    /// </summary>
    public TreeType GetRandomTreeType()
    {
        float random = UnityEngine.Random.value;
        float cumulative = 0f;
        
        cumulative += oakProbability;
        if (random <= cumulative) return TreeType.Oak;
        
        cumulative += pineProbability;
        if (random <= cumulative) return TreeType.Pine;
        
        cumulative += birchProbability;
        if (random <= cumulative) return TreeType.Birch;
        
        cumulative += mapleProbability;
        if (random <= cumulative) return TreeType.Maple;
        
        return TreeType.Willow;
    }
    
    /// <summary>
    /// Applies this configuration to a TreeManager
    /// </summary>
    public void ApplyToTreeManager(TreeManager treeManager)
    {
        if (treeManager == null) return;
        
        treeManager.maxTrees = maxTrees;
        treeManager.minDistanceBetweenTrees = minDistanceBetweenTrees;
        treeManager.minDistanceFromBuildings = minDistanceFromBuildings;
        treeManager.generationAreaSize = new Vector2(generationAreaSize, generationAreaSize);
        treeManager.enableTreeGrowth = enableTreeGrowth;
        treeManager.growthUpdateInterval = growthUpdateInterval;
    }
    
    /// <summary>
    /// Applies this configuration to a TreeLogger
    /// </summary>
    public void ApplyToTreeLogger(TreeLogger treeLogger)
    {
        if (treeLogger == null) return;
        
        treeLogger.enableAutoLogging = enableAutoLogging;
        treeLogger.autoLogInterval = autoLogInterval;
        treeLogger.enableConsoleLogging = enableConsoleLogging;
        treeLogger.enableFileLogging = enableFileLogging;
        treeLogger.enableCSVExport = enableCSVExport;
    }
    
    /// <summary>
    /// Applies this configuration to TreeAnalytics
    /// </summary>
    public void ApplyToTreeAnalytics(TreeAnalytics analytics)
    {
        if (analytics == null) return;
        
        analytics.enableRealTimeAnalytics = enableAnalytics;
        analytics.analyticsUpdateInterval = analyticsUpdateInterval;
        analytics.maxHistoryEntries = maxHistoryEntries;
    }
    
    /// <summary>
    /// Creates a preset configuration for forest environments
    /// </summary>
    public static TreeConfiguration CreateForestPreset()
    {
        var config = CreateInstance<TreeConfiguration>();
        config.name = "Forest Preset";
        
        // Dense forest settings
        config.maxTrees = 200;
        config.minDistanceBetweenTrees = 3f;
        config.minDistanceFromBuildings = 5f;
        
        // Higher growth rates
        config.globalGrowthMultiplier = 1.5f;
        config.baseSunlightLevel = 0.6f; // Less sunlight due to canopy
        config.baseWaterLevel = 0.8f; // More water in forest
        
        // Forest tree distribution
        config.oakProbability = 0.4f;
        config.pineProbability = 0.3f;
        config.birchProbability = 0.15f;
        config.mapleProbability = 0.1f;
        config.willowProbability = 0.05f;
        
        return config;
    }
    
    /// <summary>
    /// Creates a preset configuration for park environments
    /// </summary>
    public static TreeConfiguration CreateParkPreset()
    {
        var config = CreateInstance<TreeConfiguration>();
        config.name = "Park Preset";
        
        // Moderate density for parks
        config.maxTrees = 80;
        config.minDistanceBetweenTrees = 8f;
        config.minDistanceFromBuildings = 12f;
        
        // Well-maintained conditions
        config.baseSunlightLevel = 0.8f;
        config.baseWaterLevel = 0.7f;
        config.initialHealthRange = 95f;
        
        // Diverse tree distribution
        config.oakProbability = 0.25f;
        config.pineProbability = 0.15f;
        config.birchProbability = 0.25f;
        config.mapleProbability = 0.25f;
        config.willowProbability = 0.1f;
        
        return config;
    }
    
    /// <summary>
    /// Creates a preset configuration for urban environments
    /// </summary>
    public static TreeConfiguration CreateUrbanPreset()
    {
        var config = CreateInstance<TreeConfiguration>();
        config.name = "Urban Preset";
        
        // Sparse urban trees
        config.maxTrees = 50;
        config.minDistanceBetweenTrees = 12f;
        config.minDistanceFromBuildings = 15f;
        
        // Challenging urban conditions
        config.baseSunlightLevel = 0.5f; // Buildings block sunlight
        config.baseWaterLevel = 0.4f; // Less natural water
        config.healthDecayRate = 1.5f; // Urban stress
        config.initialHealthRange = 70f;
        
        // Hardy urban trees
        config.oakProbability = 0.2f;
        config.pineProbability = 0.1f;
        config.birchProbability = 0.3f; // Hardy
        config.mapleProbability = 0.3f; // Common urban tree
        config.willowProbability = 0.1f;
        
        return config;
    }
    
    /// <summary>
    /// Saves configuration to JSON file
    /// </summary>
    public void SaveToFile(string filePath)
    {
        try
        {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"Tree configuration saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save tree configuration: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads configuration from JSON file
    /// </summary>
    public static TreeConfiguration LoadFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var config = CreateInstance<TreeConfiguration>();
                JsonUtility.FromJsonOverwrite(json, config);
                Debug.Log($"Tree configuration loaded from: {filePath}");
                return config;
            }
            else
            {
                Debug.LogWarning($"Configuration file not found: {filePath}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load tree configuration: {e.Message}");
            return null;
        }
    }
}