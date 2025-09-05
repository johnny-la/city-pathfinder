using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Advanced analytics and statistics for the tree logging system
/// </summary>
public class TreeAnalytics : MonoBehaviour
{
    [Header("Analytics Settings")]
    public bool enableRealTimeAnalytics = true;
    public float analyticsUpdateInterval = 5f;
    
    [Header("Historical Data")]
    public int maxHistoryEntries = 100;
    public bool trackGrowthHistory = true;
    public bool trackHealthHistory = true;
    
    // Analytics data
    private List<TreeSnapshot> treeHistory = new List<TreeSnapshot>();
    private Dictionary<TreeType, List<float>> growthRateHistory = new Dictionary<TreeType, List<float>>();
    private Dictionary<TreeType, List<float>> healthHistory = new Dictionary<TreeType, List<float>>();
    private float lastAnalyticsUpdate;
    
    // Current analytics
    public TreeAnalyticsData CurrentAnalytics { get; private set; }
    
    void Start()
    {
        CurrentAnalytics = new TreeAnalyticsData();
        lastAnalyticsUpdate = Time.time;
        
        // Initialize history dictionaries
        foreach (TreeType treeType in Enum.GetValues(typeof(TreeType)))
        {
            growthRateHistory[treeType] = new List<float>();
            healthHistory[treeType] = new List<float>();
        }
    }
    
    void Update()
    {
        if (enableRealTimeAnalytics && Time.time - lastAnalyticsUpdate >= analyticsUpdateInterval)
        {
            UpdateAnalytics();
            lastAnalyticsUpdate = Time.time;
        }
    }
    
    /// <summary>
    /// Updates all analytics data
    /// </summary>
    public void UpdateAnalytics()
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null) return;
        
        List<Tree> allTrees = treeManager.GetAllTrees();
        CurrentAnalytics = CalculateAnalytics(allTrees);
        
        // Store historical snapshot
        if (trackGrowthHistory || trackHealthHistory)
        {
            StoreHistoricalData(allTrees);
        }
        
        // Fire analytics updated event
        TreeEventSystem.TriggerAnalyticsUpdatedEvent(CurrentAnalytics);
        
        Debug.Log($"Analytics Updated: {allTrees.Count} trees analyzed");
    }
    
    /// <summary>
    /// Calculates comprehensive analytics for a list of trees
    /// </summary>
    private TreeAnalyticsData CalculateAnalytics(List<Tree> trees)
    {
        var analytics = new TreeAnalyticsData();
        analytics.timestamp = DateTime.Now;
        analytics.totalTrees = trees.Count;
        
        if (trees.Count == 0) return analytics;
        
        // Basic statistics
        analytics.healthyTrees = trees.Count(t => t.isHealthy);
        analytics.unhealthyTrees = trees.Count - analytics.healthyTrees;
        analytics.averageHealth = trees.Average(t => t.health);
        analytics.averageHeight = trees.Average(t => t.height);
        analytics.averageAge = trees.Average(t => t.age);
        
        // Health distribution
        analytics.criticalHealthTrees = trees.Count(t => t.health < 25f);
        analytics.poorHealthTrees = trees.Count(t => t.health >= 25f && t.health < 50f);
        analytics.goodHealthTrees = trees.Count(t => t.health >= 50f && t.health < 75f);
        analytics.excellentHealthTrees = trees.Count(t => t.health >= 75f);
        
        // Tree type distribution
        analytics.treeTypeDistribution = trees.GroupBy(t => t.treeType)
                                             .ToDictionary(g => g.Key, g => g.Count());
        
        // Growth statistics
        analytics.averageGrowthRate = trees.Average(t => t.growthRate);
        analytics.fastestGrowingTree = trees.OrderByDescending(t => t.growthRate).FirstOrDefault();
        analytics.slowestGrowingTree = trees.OrderBy(t => t.growthRate).FirstOrDefault();
        
        // Size statistics
        analytics.tallestTree = trees.OrderByDescending(t => t.height).FirstOrDefault();
        analytics.shortestTree = trees.OrderBy(t => t.height).FirstOrDefault();
        analytics.largestTree = trees.OrderByDescending(t => t.radius).FirstOrDefault();
        analytics.smallestTree = trees.OrderBy(t => t.radius).FirstOrDefault();
        
        // Environmental statistics
        analytics.averageSunlightExposure = trees.Average(t => t.sunlightExposure);
        analytics.averageWaterLevel = trees.Average(t => t.waterLevel);
        
        // Age distribution
        analytics.saplings = trees.Count(t => t.age < 30);
        analytics.youngTrees = trees.Count(t => t.age >= 30 && t.age < 365);
        analytics.matureTrees = trees.Count(t => t.age >= 365 && t.age < 1825); // 5 years
        analytics.oldTrees = trees.Count(t => t.age >= 1825);
        
        // Density analysis
        if (trees.Count > 1)
        {
            var distances = new List<float>();
            for (int i = 0; i < trees.Count; i++)
            {
                for (int j = i + 1; j < trees.Count; j++)
                {
                    distances.Add(Vector3.Distance(trees[i].position, trees[j].position));
                }
            }
            analytics.averageTreeDistance = distances.Average();
            analytics.closestTreeDistance = distances.Min();
            analytics.farthestTreeDistance = distances.Max();
        }
        
        return analytics;
    }
    
    /// <summary>
    /// Stores historical data for trend analysis
    /// </summary>
    private void StoreHistoricalData(List<Tree> trees)
    {
        var snapshot = new TreeSnapshot
        {
            timestamp = DateTime.Now,
            totalTrees = trees.Count,
            averageHealth = trees.Average(t => t.health),
            averageHeight = trees.Average(t => t.height)
        };
        
        treeHistory.Add(snapshot);
        
        // Maintain history size limit
        if (treeHistory.Count > maxHistoryEntries)
        {
            treeHistory.RemoveAt(0);
        }
        
        // Track growth and health by type
        foreach (TreeType treeType in Enum.GetValues(typeof(TreeType)))
        {
            var treesOfType = trees.Where(t => t.treeType == treeType).ToList();
            if (treesOfType.Count > 0)
            {
                float avgGrowth = treesOfType.Average(t => t.growthRate);
                float avgHealth = treesOfType.Average(t => t.health);
                
                growthRateHistory[treeType].Add(avgGrowth);
                healthHistory[treeType].Add(avgHealth);
                
                // Maintain history size
                if (growthRateHistory[treeType].Count > maxHistoryEntries)
                    growthRateHistory[treeType].RemoveAt(0);
                if (healthHistory[treeType].Count > maxHistoryEntries)
                    healthHistory[treeType].RemoveAt(0);
            }
        }
    }
    
    /// <summary>
    /// Gets growth trend for a specific tree type
    /// </summary>
    public List<float> GetGrowthTrend(TreeType treeType)
    {
        return growthRateHistory.ContainsKey(treeType) ? 
               new List<float>(growthRateHistory[treeType]) : 
               new List<float>();
    }
    
    /// <summary>
    /// Gets health trend for a specific tree type
    /// </summary>
    public List<float> GetHealthTrend(TreeType treeType)
    {
        return healthHistory.ContainsKey(treeType) ? 
               new List<float>(healthHistory[treeType]) : 
               new List<float>();
    }
    
    /// <summary>
    /// Gets historical snapshots
    /// </summary>
    public List<TreeSnapshot> GetHistoricalData()
    {
        return new List<TreeSnapshot>(treeHistory);
    }
    
    /// <summary>
    /// Generates a comprehensive analytics report
    /// </summary>
    public string GenerateAnalyticsReport()
    {
        if (CurrentAnalytics == null)
        {
            UpdateAnalytics();
        }
        
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== TREE ANALYTICS REPORT ===");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Analysis Time: {CurrentAnalytics.timestamp:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        // Population Overview
        report.AppendLine("POPULATION OVERVIEW:");
        report.AppendLine($"Total Trees: {CurrentAnalytics.totalTrees}");
        report.AppendLine($"Healthy: {CurrentAnalytics.healthyTrees} ({(CurrentAnalytics.healthyTrees * 100.0 / CurrentAnalytics.totalTrees):F1}%)");
        report.AppendLine($"Unhealthy: {CurrentAnalytics.unhealthyTrees} ({(CurrentAnalytics.unhealthyTrees * 100.0 / CurrentAnalytics.totalTrees):F1}%)");
        report.AppendLine();
        
        // Health Distribution
        report.AppendLine("HEALTH DISTRIBUTION:");
        report.AppendLine($"Critical (0-25%): {CurrentAnalytics.criticalHealthTrees}");
        report.AppendLine($"Poor (25-50%): {CurrentAnalytics.poorHealthTrees}");
        report.AppendLine($"Good (50-75%): {CurrentAnalytics.goodHealthTrees}");
        report.AppendLine($"Excellent (75-100%): {CurrentAnalytics.excellentHealthTrees}");
        report.AppendLine();
        
        // Age Distribution
        report.AppendLine("AGE DISTRIBUTION:");
        report.AppendLine($"Saplings (0-30 days): {CurrentAnalytics.saplings}");
        report.AppendLine($"Young (30-365 days): {CurrentAnalytics.youngTrees}");
        report.AppendLine($"Mature (1-5 years): {CurrentAnalytics.matureTrees}");
        report.AppendLine($"Old (5+ years): {CurrentAnalytics.oldTrees}");
        report.AppendLine();
        
        // Tree Type Distribution
        report.AppendLine("TREE TYPE DISTRIBUTION:");
        foreach (var kvp in CurrentAnalytics.treeTypeDistribution)
        {
            report.AppendLine($"{kvp.Key}: {kvp.Value} trees");
        }
        report.AppendLine();
        
        // Averages
        report.AppendLine("AVERAGE STATISTICS:");
        report.AppendLine($"Health: {CurrentAnalytics.averageHealth:F1}%");
        report.AppendLine($"Height: {CurrentAnalytics.averageHeight:F2}m");
        report.AppendLine($"Age: {CurrentAnalytics.averageAge:F0} days");
        report.AppendLine($"Growth Rate: {CurrentAnalytics.averageGrowthRate:F2}");
        report.AppendLine($"Sunlight Exposure: {CurrentAnalytics.averageSunlightExposure:F2}");
        report.AppendLine($"Water Level: {CurrentAnalytics.averageWaterLevel:F2}");
        report.AppendLine();
        
        // Notable Trees
        report.AppendLine("NOTABLE TREES:");
        if (CurrentAnalytics.tallestTree != null)
            report.AppendLine($"Tallest: {CurrentAnalytics.tallestTree.treeType} at {CurrentAnalytics.tallestTree.height:F2}m");
        if (CurrentAnalytics.fastestGrowingTree != null)
            report.AppendLine($"Fastest Growing: {CurrentAnalytics.fastestGrowingTree.treeType} ({CurrentAnalytics.fastestGrowingTree.growthRate:F2})");
        
        return report.ToString();
    }
}

/// <summary>
/// Comprehensive analytics data structure
/// </summary>
[System.Serializable]
public class TreeAnalyticsData
{
    public DateTime timestamp;
    public int totalTrees;
    public int healthyTrees;
    public int unhealthyTrees;
    
    // Health distribution
    public int criticalHealthTrees;
    public int poorHealthTrees;
    public int goodHealthTrees;
    public int excellentHealthTrees;
    
    // Age distribution
    public int saplings;
    public int youngTrees;
    public int matureTrees;
    public int oldTrees;
    
    // Averages
    public float averageHealth;
    public float averageHeight;
    public float averageAge;
    public float averageGrowthRate;
    public float averageSunlightExposure;
    public float averageWaterLevel;
    
    // Tree type distribution
    public Dictionary<TreeType, int> treeTypeDistribution = new Dictionary<TreeType, int>();
    
    // Notable trees
    public Tree tallestTree;
    public Tree shortestTree;
    public Tree largestTree;
    public Tree smallestTree;
    public Tree fastestGrowingTree;
    public Tree slowestGrowingTree;
    
    // Spatial analysis
    public float averageTreeDistance;
    public float closestTreeDistance;
    public float farthestTreeDistance;
}

/// <summary>
/// Historical snapshot of tree data
/// </summary>
[System.Serializable]
public class TreeSnapshot
{
    public DateTime timestamp;
    public int totalTrees;
    public float averageHealth;
    public float averageHeight;
}