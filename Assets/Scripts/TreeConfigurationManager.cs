using UnityEngine;
using System.IO;

/// <summary>
/// Manages tree configuration loading, saving, and application at runtime
/// </summary>
public class TreeConfigurationManager : MonoBehaviour
{
    [Header("Configuration")]
    public TreeConfiguration currentConfiguration;
    
    [Header("Auto-Apply")]
    public bool autoApplyOnStart = true;
    public bool autoFindComponents = true;
    
    [Header("Runtime Controls")]
    public bool enableRuntimeConfigChanges = true;
    
    private TreeManager treeManager;
    private TreeLogger treeLogger;
    private TreeAnalytics treeAnalytics;
    private string configDirectory;
    
    void Start()
    {
        // Set up configuration directory
        configDirectory = Path.Combine(Application.persistentDataPath, "TreeConfigs");
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }
        
        // Find components if auto-find is enabled
        if (autoFindComponents)
        {
            FindComponents();
        }
        
        // Load default configuration if none is assigned
        if (currentConfiguration == null)
        {
            LoadDefaultConfiguration();
        }
        
        // Apply configuration on start
        if (autoApplyOnStart && currentConfiguration != null)
        {
            ApplyCurrentConfiguration();
        }
    }
    
    void Update()
    {
        // Runtime configuration controls
        if (enableRuntimeConfigChanges)
        {
            HandleRuntimeInput();
        }
    }
    
    /// <summary>
    /// Finds tree system components automatically
    /// </summary>
    private void FindComponents()
    {
        if (treeManager == null)
            treeManager = FindObjectOfType<TreeManager>();
        
        if (treeLogger == null)
            treeLogger = FindObjectOfType<TreeLogger>();
        
        if (treeAnalytics == null)
            treeAnalytics = FindObjectOfType<TreeAnalytics>();
    }
    
    /// <summary>
    /// Handles runtime input for configuration changes
    /// </summary>
    private void HandleRuntimeInput()
    {
        // F1-F3 for preset loading
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadPreset("Forest");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadPreset("Park");
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadPreset("Urban");
        }
        
        // Ctrl+S to save current configuration
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            SaveCurrentConfiguration();
        }
        
        // Ctrl+L to load configuration
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            LoadConfigurationDialog();
        }
    }
    
    /// <summary>
    /// Applies the current configuration to all tree system components
    /// </summary>
    public void ApplyCurrentConfiguration()
    {
        if (currentConfiguration == null)
        {
            Debug.LogWarning("No tree configuration assigned!");
            return;
        }
        
        FindComponents(); // Ensure components are found
        
        // Apply to TreeManager
        if (treeManager != null)
        {
            currentConfiguration.ApplyToTreeManager(treeManager);
            Debug.Log("Configuration applied to TreeManager");
        }
        
        // Apply to TreeLogger
        if (treeLogger != null)
        {
            currentConfiguration.ApplyToTreeLogger(treeLogger);
            Debug.Log("Configuration applied to TreeLogger");
        }
        
        // Apply to TreeAnalytics
        if (treeAnalytics != null)
        {
            currentConfiguration.ApplyToTreeAnalytics(treeAnalytics);
            Debug.Log("Configuration applied to TreeAnalytics");
        }
        
        Debug.Log($"Tree configuration '{currentConfiguration.name}' applied successfully!");
    }
    
    /// <summary>
    /// Loads a preset configuration
    /// </summary>
    public void LoadPreset(string presetName)
    {
        TreeConfiguration preset = null;
        
        switch (presetName.ToLower())
        {
            case "forest":
                preset = TreeConfiguration.CreateForestPreset();
                break;
            case "park":
                preset = TreeConfiguration.CreateParkPreset();
                break;
            case "urban":
                preset = TreeConfiguration.CreateUrbanPreset();
                break;
            default:
                Debug.LogWarning($"Unknown preset: {presetName}");
                return;
        }
        
        if (preset != null)
        {
            currentConfiguration = preset;
            ApplyCurrentConfiguration();
            Debug.Log($"Loaded {presetName} preset configuration");
        }
    }
    
    /// <summary>
    /// Saves the current configuration to file
    /// </summary>
    public void SaveCurrentConfiguration()
    {
        if (currentConfiguration == null)
        {
            Debug.LogWarning("No configuration to save!");
            return;
        }
        
        string fileName = $"{currentConfiguration.name}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        string filePath = Path.Combine(configDirectory, fileName);
        
        currentConfiguration.SaveToFile(filePath);
    }
    
    /// <summary>
    /// Loads default configuration (creates one if needed)
    /// </summary>
    private void LoadDefaultConfiguration()
    {
        string defaultPath = Path.Combine(configDirectory, "default_tree_config.json");
        
        currentConfiguration = TreeConfiguration.LoadFromFile(defaultPath);
        
        if (currentConfiguration == null)
        {
            // Create default configuration
            currentConfiguration = ScriptableObject.CreateInstance<TreeConfiguration>();
            currentConfiguration.name = "Default Configuration";
            
            // Save it for future use
            currentConfiguration.SaveToFile(defaultPath);
            Debug.Log("Created default tree configuration");
        }
    }
    
    /// <summary>
    /// Simple dialog for loading configurations (placeholder for full UI)
    /// </summary>
    private void LoadConfigurationDialog()
    {
        // In a full implementation, this would show a file dialog
        // For now, we'll just try to load the most recent config
        string[] configFiles = Directory.GetFiles(configDirectory, "*.json");
        
        if (configFiles.Length > 0)
        {
            // Load the most recent file
            string mostRecent = configFiles[configFiles.Length - 1];
            var loadedConfig = TreeConfiguration.LoadFromFile(mostRecent);
            
            if (loadedConfig != null)
            {
                currentConfiguration = loadedConfig;
                ApplyCurrentConfiguration();
            }
        }
        else
        {
            Debug.Log("No configuration files found to load");
        }
    }
    
    /// <summary>
    /// Creates a configuration from current system state
    /// </summary>
    public TreeConfiguration CreateConfigurationFromCurrentState()
    {
        var config = ScriptableObject.CreateInstance<TreeConfiguration>();
        config.name = $"Current State {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        
        FindComponents();
        
        // Extract settings from TreeManager
        if (treeManager != null)
        {
            config.maxTrees = treeManager.maxTrees;
            config.minDistanceBetweenTrees = treeManager.minDistanceBetweenTrees;
            config.minDistanceFromBuildings = treeManager.minDistanceFromBuildings;
            config.enableTreeGrowth = treeManager.enableTreeGrowth;
            config.growthUpdateInterval = treeManager.growthUpdateInterval;
            
            if (treeManager.generationAreaSize.x > 0)
            {
                config.generationAreaSize = treeManager.generationAreaSize.x;
            }
        }
        
        // Extract settings from TreeLogger
        if (treeLogger != null)
        {
            config.enableAutoLogging = treeLogger.enableAutoLogging;
            config.autoLogInterval = treeLogger.autoLogInterval;
            config.enableConsoleLogging = treeLogger.enableConsoleLogging;
            config.enableFileLogging = treeLogger.enableFileLogging;
            config.enableCSVExport = treeLogger.enableCSVExport;
        }
        
        // Extract settings from TreeAnalytics
        if (treeAnalytics != null)
        {
            config.enableAnalytics = treeAnalytics.enableRealTimeAnalytics;
            config.analyticsUpdateInterval = treeAnalytics.analyticsUpdateInterval;
            config.maxHistoryEntries = treeAnalytics.maxHistoryEntries;
        }
        
        return config;
    }
    
    void OnGUI()
    {
        if (!enableRuntimeConfigChanges) return;
        
        // Configuration control panel
        GUILayout.BeginArea(new Rect(220, 10, 250, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Tree Configuration");
        
        if (currentConfiguration != null)
        {
            GUILayout.Label($"Current: {currentConfiguration.name}");
        }
        else
        {
            GUILayout.Label("No configuration loaded");
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Presets:");
        
        if (GUILayout.Button("Forest (F1)"))
        {
            LoadPreset("Forest");
        }
        
        if (GUILayout.Button("Park (F2)"))
        {
            LoadPreset("Park");
        }
        
        if (GUILayout.Button("Urban (F3)"))
        {
            LoadPreset("Urban");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Save Config (Ctrl+S)"))
        {
            SaveCurrentConfiguration();
        }
        
        if (GUILayout.Button("Load Config (Ctrl+L)"))
        {
            LoadConfigurationDialog();
        }
        
        if (GUILayout.Button("Apply Current"))
        {
            ApplyCurrentConfiguration();
        }
        
        if (GUILayout.Button("Create from Current State"))
        {
            currentConfiguration = CreateConfigurationFromCurrentState();
            Debug.Log("Configuration created from current system state");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}