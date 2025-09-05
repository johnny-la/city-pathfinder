using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

/// <summary>
/// Handles logging of tree data to various outputs
/// </summary>
public class TreeLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    public bool enableConsoleLogging = true;
    public bool enableFileLogging = true;
    public bool enableCSVExport = true;
    
    [Header("File Settings")]
    public string logDirectory = "TreeLogs";
    public string logFileName = "tree_log";
    public string csvFileName = "tree_data";
    
    [Header("Logging Frequency")]
    public float autoLogInterval = 60f; // Auto-log every 60 seconds
    public bool enableAutoLogging = true;
    
    private float lastAutoLogTime;
    private string fullLogDirectory;
    
    void Start()
    {
        // Set up logging directory
        fullLogDirectory = Path.Combine(Application.persistentDataPath, logDirectory);
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(fullLogDirectory))
        {
            Directory.CreateDirectory(fullLogDirectory);
            Debug.Log($"Created tree logging directory: {fullLogDirectory}");
        }
        
        lastAutoLogTime = Time.time;
    }
    
    void Update()
    {
        // Handle auto-logging
        if (enableAutoLogging && Time.time - lastAutoLogTime >= autoLogInterval)
        {
            LogAllTrees();
            lastAutoLogTime = Time.time;
        }
        
        // Handle manual logging input
        if (Input.GetKeyDown(KeyCode.L))
        {
            LogAllTrees();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExportTreesToCSV();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            LogTreesToConsole();
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            GenerateAnalyticsReport();
        }
    }
    
    /// <summary>
    /// Logs all trees using all enabled logging methods
    /// </summary>
    public void LogAllTrees()
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null)
        {
            Debug.LogWarning("TreeManager not found! Cannot log trees.");
            return;
        }
        
        List<Tree> trees = treeManager.GetAllTrees();
        
        if (trees.Count == 0)
        {
            Debug.Log("No trees found to log.");
            return;
        }
        
        Debug.Log($"Logging {trees.Count} trees...");
        
        if (enableConsoleLogging)
        {
            LogTreesToConsole(trees);
        }
        
        if (enableFileLogging)
        {
            LogTreesToFile(trees);
        }
        
        if (enableCSVExport)
        {
            ExportTreesToCSV(trees);
        }
        
        // Mark trees as logged
        foreach (Tree tree in trees)
        {
            tree.MarkAsLogged();
        }
    }
    
    /// <summary>
    /// Logs tree information to the Unity console
    /// </summary>
    public void LogTreesToConsole()
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null) return;
        
        LogTreesToConsole(treeManager.GetAllTrees());
    }
    
    private void LogTreesToConsole(List<Tree> trees)
    {
        Debug.Log("=== TREE LOGGING REPORT ===");
        Debug.Log($"Total Trees: {trees.Count}");
        Debug.Log($"Log Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Debug.Log("===========================");
        
        // Summary statistics
        var healthyTrees = trees.Where(t => t.isHealthy).Count();
        var averageHeight = trees.Average(t => t.height);
        var averageHealth = trees.Average(t => t.health);
        
        Debug.Log($"Healthy Trees: {healthyTrees}/{trees.Count}");
        Debug.Log($"Average Height: {averageHeight:F2}m");
        Debug.Log($"Average Health: {averageHealth:F1}%");
        
        // Log each tree type count
        var treeTypeCounts = trees.GroupBy(t => t.treeType)
                                 .ToDictionary(g => g.Key, g => g.Count());
        
        Debug.Log("Tree Type Distribution:");
        foreach (var kvp in treeTypeCounts)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        // Log individual tree details (first 5 trees to avoid spam)
        Debug.Log("\nDetailed Tree Information (First 5):");
        for (int i = 0; i < Mathf.Min(5, trees.Count); i++)
        {
            Debug.Log($"\n--- Tree {i + 1} ---");
            Debug.Log(trees[i].GetLogInfo());
        }
        
        if (trees.Count > 5)
        {
            Debug.Log($"\n... and {trees.Count - 5} more trees. Check log files for complete data.");
        }
    }
    
    /// <summary>
    /// Logs tree information to a text file
    /// </summary>
    private void LogTreesToFile(List<Tree> trees)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{logFileName}_{timestamp}.txt";
            string filePath = Path.Combine(fullLogDirectory, fileName);
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("=== TREE LOGGING REPORT ===");
                writer.WriteLine($"Total Trees: {trees.Count}");
                writer.WriteLine($"Log Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Application: {Application.productName}");
                writer.WriteLine($"Unity Version: {Application.unityVersion}");
                writer.WriteLine("===========================\n");
                
                // Summary statistics
                var healthyTrees = trees.Where(t => t.isHealthy).Count();
                var averageHeight = trees.Average(t => t.height);
                var averageHealth = trees.Average(t => t.health);
                var oldestTree = trees.OrderByDescending(t => t.age).FirstOrDefault();
                var tallestTree = trees.OrderByDescending(t => t.height).FirstOrDefault();
                
                writer.WriteLine("SUMMARY STATISTICS:");
                writer.WriteLine($"Healthy Trees: {healthyTrees}/{trees.Count} ({(healthyTrees * 100.0 / trees.Count):F1}%)");
                writer.WriteLine($"Average Height: {averageHeight:F2}m");
                writer.WriteLine($"Average Health: {averageHealth:F1}%");
                writer.WriteLine($"Oldest Tree: {oldestTree?.age} days ({oldestTree?.treeType})");
                writer.WriteLine($"Tallest Tree: {tallestTree?.height:F2}m ({tallestTree?.treeType})\n");
                
                // Tree type distribution
                var treeTypeCounts = trees.GroupBy(t => t.treeType)
                                         .ToDictionary(g => g.Key, g => g.Count());
                
                writer.WriteLine("TREE TYPE DISTRIBUTION:");
                foreach (var kvp in treeTypeCounts)
                {
                    writer.WriteLine($"  {kvp.Key}: {kvp.Value} trees");
                }
                writer.WriteLine();
                
                // Detailed tree information
                writer.WriteLine("DETAILED TREE INFORMATION:");
                writer.WriteLine(new string('=', 50));
                
                for (int i = 0; i < trees.Count; i++)
                {
                    writer.WriteLine($"\nTree #{i + 1}:");
                    writer.WriteLine(new string('-', 20));
                    writer.WriteLine(trees[i].GetLogInfo());
                }
            }
            
            Debug.Log($"Tree log saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save tree log to file: {e.Message}");
        }
    }
    
    /// <summary>
    /// Exports tree data to CSV format
    /// </summary>
    public void ExportTreesToCSV()
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null) return;
        
        ExportTreesToCSV(treeManager.GetAllTrees());
    }
    
    private void ExportTreesToCSV(List<Tree> trees)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{csvFileName}_{timestamp}.csv";
            string filePath = Path.Combine(fullLogDirectory, fileName);
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV header
                writer.WriteLine("ID,Type,X,Y,Z,Height,Radius,Health,Age,IsHealthy,Sunlight,Water,PlantedDate,LastLoggedDate");
                
                // Write tree data
                foreach (Tree tree in trees)
                {
                    writer.WriteLine(tree.GetCSVData());
                }
            }
            
            Debug.Log($"Tree data exported to CSV: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export tree data to CSV: {e.Message}");
        }
    }
    
    /// <summary>
    /// Logs a specific tree by ID
    /// </summary>
    public void LogTreeById(string treeId)
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null) return;
        
        Tree tree = treeManager.GetTreeById(treeId);
        if (tree != null)
        {
            Debug.Log("=== INDIVIDUAL TREE LOG ===");
            Debug.Log(tree.GetLogInfo());
            tree.MarkAsLogged();
        }
        else
        {
            Debug.LogWarning($"Tree with ID {treeId} not found.");
        }
    }
    
    /// <summary>
    /// Logs trees within a specific area
    /// </summary>
    public void LogTreesInArea(Vector3 center, float radius)
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager == null) return;
        
        List<Tree> treesInArea = treeManager.GetTreesInArea(center, radius);
        
        if (treesInArea.Count > 0)
        {
            Debug.Log($"=== TREES IN AREA (Center: {center}, Radius: {radius}) ===");
            LogTreesToConsole(treesInArea);
        }
        else
        {
            Debug.Log($"No trees found in area (Center: {center}, Radius: {radius})");
        }
    }
    
    void OnGUI()
    {
        // Simple UI for logging controls
        GUILayout.BeginArea(new Rect(10, 10, 200, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Tree Logging Controls");
        
        if (GUILayout.Button("Log All Trees (L)"))
        {
            LogAllTrees();
        }
        
        if (GUILayout.Button("Export to CSV (E)"))
        {
            ExportTreesToCSV();
        }
        
        if (GUILayout.Button("Console Log (C)"))
        {
            LogTreesToConsole();
        }
        
        if (GUILayout.Button("Analytics Report"))
        {
            GenerateAnalyticsReport();
        }
        
        GUILayout.Space(10);
        
        enableAutoLogging = GUILayout.Toggle(enableAutoLogging, "Auto Logging");
        enableConsoleLogging = GUILayout.Toggle(enableConsoleLogging, "Console Logging");
        enableFileLogging = GUILayout.Toggle(enableFileLogging, "File Logging");
        enableCSVExport = GUILayout.Toggle(enableCSVExport, "CSV Export");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Generates and logs an analytics report
    /// </summary>
    public void GenerateAnalyticsReport()
    {
        TreeAnalytics analytics = FindObjectOfType<TreeAnalytics>();
        if (analytics == null)
        {
            Debug.LogWarning("TreeAnalytics component not found! Add TreeAnalytics to generate reports.");
            return;
        }
        
        string report = analytics.GenerateAnalyticsReport();
        Debug.Log(report);
        
        // Also save to file if enabled
        if (enableFileLogging)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"tree_analytics_{timestamp}.txt";
                string filePath = Path.Combine(fullLogDirectory, fileName);
                
                File.WriteAllText(filePath, report);
                Debug.Log($"Analytics report saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save analytics report: {e.Message}");
            }
        }
    }