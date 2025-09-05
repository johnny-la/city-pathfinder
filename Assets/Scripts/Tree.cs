using UnityEngine;
using System;

/// <summary>
/// Represents a tree in the pathfinding environment
/// </summary>
[Serializable]
public class Tree
{
    [Header("Tree Properties")]
    public Vector3 position;
    public TreeType treeType;
    public float height;
    public float radius;
    public float health;
    public int age;
    public string id;
    
    [Header("Growth Properties")]
    public float growthRate;
    public float maxHeight;
    public float maxRadius;
    
    [Header("Environmental")]
    public bool isHealthy;
    public float sunlightExposure;
    public float waterLevel;
    
    public DateTime plantedDate;
    public DateTime lastLoggedDate;
    
    /// <summary>
    /// Constructor for creating a new tree
    /// </summary>
    public Tree(Vector3 pos, TreeType type, float initialHeight = 5f)
    {
        position = pos;
        treeType = type;
        height = initialHeight;
        radius = initialHeight * 0.3f; // Radius proportional to height
        health = 100f;
        age = 0;
        id = System.Guid.NewGuid().ToString();
        
        // Set growth properties based on tree type
        SetGrowthProperties();
        
        // Environmental defaults
        isHealthy = true;
        sunlightExposure = UnityEngine.Random.Range(0.5f, 1.0f);
        waterLevel = UnityEngine.Random.Range(0.3f, 0.8f);
        
        plantedDate = DateTime.Now;
        lastLoggedDate = DateTime.Now;
    }
    
    /// <summary>
    /// Sets growth properties based on tree type
    /// </summary>
    private void SetGrowthProperties()
    {
        switch (treeType)
        {
            case TreeType.Oak:
                growthRate = 0.5f;
                maxHeight = 25f;
                maxRadius = 8f;
                break;
            case TreeType.Pine:
                growthRate = 0.8f;
                maxHeight = 35f;
                maxRadius = 6f;
                break;
            case TreeType.Birch:
                growthRate = 1.0f;
                maxHeight = 20f;
                maxRadius = 5f;
                break;
            case TreeType.Maple:
                growthRate = 0.6f;
                maxHeight = 30f;
                maxRadius = 10f;
                break;
            case TreeType.Willow:
                growthRate = 1.2f;
                maxHeight = 15f;
                maxRadius = 12f;
                break;
        }
    }
    
    /// <summary>
    /// Updates the tree's growth and health
    /// </summary>
    public void UpdateTree(float deltaTime)
    {
        // Age the tree
        age += Mathf.RoundToInt(deltaTime);
        
        // Grow the tree if healthy and not at max size
        if (isHealthy && height < maxHeight)
        {
            float growthThisFrame = growthRate * deltaTime * (sunlightExposure * waterLevel);
            height = Mathf.Min(height + growthThisFrame, maxHeight);
            radius = Mathf.Min(radius + growthThisFrame * 0.3f, maxRadius);
        }
        
        // Update health based on environmental factors
        UpdateHealth();
    }
    
    /// <summary>
    /// Updates tree health based on environmental conditions
    /// </summary>
    private void UpdateHealth()
    {
        float healthChange = 0f;
        
        // Good conditions improve health
        if (sunlightExposure > 0.7f && waterLevel > 0.5f)
        {
            healthChange += 1f;
        }
        // Poor conditions decrease health
        else if (sunlightExposure < 0.3f || waterLevel < 0.2f)
        {
            healthChange -= 2f;
        }
        
        health = Mathf.Clamp(health + healthChange, 0f, 100f);
        isHealthy = health > 20f;
    }
    
    /// <summary>
    /// Returns a formatted string with tree information for logging
    /// </summary>
    public string GetLogInfo()
    {
        return $"Tree ID: {id}\n" +
               $"Type: {treeType}\n" +
               $"Position: {position}\n" +
               $"Height: {height:F2}m\n" +
               $"Radius: {radius:F2}m\n" +
               $"Health: {health:F1}%\n" +
               $"Age: {age} days\n" +
               $"Healthy: {isHealthy}\n" +
               $"Sunlight: {sunlightExposure:F2}\n" +
               $"Water: {waterLevel:F2}\n" +
               $"Planted: {plantedDate:yyyy-MM-dd HH:mm:ss}\n" +
               $"Last Logged: {lastLoggedDate:yyyy-MM-dd HH:mm:ss}";
    }
    
    /// <summary>
    /// Returns a CSV formatted string for data export
    /// </summary>
    public string GetCSVData()
    {
        return $"{id},{treeType},{position.x:F2},{position.y:F2},{position.z:F2}," +
               $"{height:F2},{radius:F2},{health:F1},{age},{isHealthy}," +
               $"{sunlightExposure:F2},{waterLevel:F2}," +
               $"{plantedDate:yyyy-MM-dd HH:mm:ss},{lastLoggedDate:yyyy-MM-dd HH:mm:ss}";
    }
    
    /// <summary>
    /// Updates the last logged date
    /// </summary>
    public void MarkAsLogged()
    {
        lastLoggedDate = DateTime.Now;
    }
}

/// <summary>
/// Different types of trees available
/// </summary>
public enum TreeType
{
    Oak,
    Pine,
    Birch,
    Maple,
    Willow
}