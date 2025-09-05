using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Comprehensive event system for tree lifecycle and interactions
/// </summary>
public class TreeEventSystem : MonoBehaviour
{
    [Header("Event Settings")]
    public bool enableEventLogging = true;
    public bool enableEventNotifications = true;
    
    [Header("Notification Settings")]
    public float notificationDuration = 3f;
    public int maxNotifications = 5;
    
    // Event delegates
    public static event Action<TreeEventData> OnTreeEvent;
    public static event Action<Tree> OnTreePlanted;
    public static event Action<Tree> OnTreeGrown;
    public static event Action<Tree> OnTreeHealthChanged;
    public static event Action<Tree> OnTreeDied;
    public static event Action<Tree> OnTreeRemoved;
    public static event Action<Tree, TreeInteractionType> OnTreeInteraction;
    public static event Action<TreeAnalyticsData> OnAnalyticsUpdated;
    
    // Notification system
    private Queue<TreeNotification> activeNotifications = new Queue<TreeNotification>();
    private List<TreeNotification> displayedNotifications = new List<TreeNotification>();
    
    void Start()
    {
        // Subscribe to tree manager events
        SubscribeToTreeManagerEvents();
        
        Debug.Log("Tree Event System initialized");
    }
    
    void Update()
    {
        // Update notifications
        UpdateNotifications();
    }
    
    /// <summary>
    /// Subscribes to TreeManager events
    /// </summary>
    private void SubscribeToTreeManagerEvents()
    {
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager != null)
        {
            treeManager.OnTreeAdded += HandleTreeAdded;
            treeManager.OnTreeRemoved += HandleTreeRemoved;
            treeManager.OnTreeHealthChanged += HandleTreeHealthChanged;
        }
    }
    
    /// <summary>
    /// Handles tree added events
    /// </summary>
    private void HandleTreeAdded(Tree tree)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.TreePlanted,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"New {tree.treeType} tree planted at {tree.position}"
        };
        
        FireTreeEvent(eventData);
        OnTreePlanted?.Invoke(tree);
        
        ShowNotification($"🌱 {tree.treeType} planted!", NotificationType.Success);
    }
    
    /// <summary>
    /// Handles tree removed events
    /// </summary>
    private void HandleTreeRemoved(Tree tree)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.TreeRemoved,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"{tree.treeType} tree removed from {tree.position}"
        };
        
        FireTreeEvent(eventData);
        OnTreeRemoved?.Invoke(tree);
        
        ShowNotification($"🪓 {tree.treeType} removed", NotificationType.Warning);
    }
    
    /// <summary>
    /// Handles tree health changed events
    /// </summary>
    private void HandleTreeHealthChanged(Tree tree)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.HealthChanged,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"{tree.treeType} health changed to {tree.health:F1}%"
        };
        
        FireTreeEvent(eventData);
        OnTreeHealthChanged?.Invoke(tree);
        
        // Show notification for critical health changes
        if (tree.health < 25f && tree.isHealthy)
        {
            ShowNotification($"⚠️ {tree.treeType} critical health!", NotificationType.Error);
        }
        else if (tree.health > 75f && !tree.isHealthy)
        {
            ShowNotification($"💚 {tree.treeType} recovering!", NotificationType.Success);
        }
    }
    
    /// <summary>
    /// Fires a tree event
    /// </summary>
    public static void FireTreeEvent(TreeEventData eventData)
    {
        OnTreeEvent?.Invoke(eventData);
        
        TreeEventSystem eventSystem = FindObjectOfType<TreeEventSystem>();
        if (eventSystem != null && eventSystem.enableEventLogging)
        {
            eventSystem.LogEvent(eventData);
        }
    }
    
    /// <summary>
    /// Logs an event to the console
    /// </summary>
    private void LogEvent(TreeEventData eventData)
    {
        string logMessage = $"[Tree Event] {eventData.timestamp:HH:mm:ss} - {eventData.eventType}: {eventData.message}";
        
        switch (eventData.eventType)
        {
            case TreeEventType.TreeDied:
            case TreeEventType.TreeRemoved:
                Debug.LogWarning(logMessage);
                break;
            case TreeEventType.HealthCritical:
                Debug.LogError(logMessage);
                break;
            default:
                Debug.Log(logMessage);
                break;
        }
    }
    
    /// <summary>
    /// Shows a notification to the player
    /// </summary>
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (!enableEventNotifications) return;
        
        var notification = new TreeNotification
        {
            message = message,
            type = type,
            timestamp = Time.time,
            duration = notificationDuration
        };
        
        activeNotifications.Enqueue(notification);
        
        // Limit active notifications
        while (activeNotifications.Count > maxNotifications)
        {
            activeNotifications.Dequeue();
        }
    }
    
    /// <summary>
    /// Updates the notification system
    /// </summary>
    private void UpdateNotifications()
    {
        // Move notifications from queue to display list
        while (activeNotifications.Count > 0 && displayedNotifications.Count < maxNotifications)
        {
            displayedNotifications.Add(activeNotifications.Dequeue());
        }
        
        // Remove expired notifications
        for (int i = displayedNotifications.Count - 1; i >= 0; i--)
        {
            if (Time.time - displayedNotifications[i].timestamp > displayedNotifications[i].duration)
            {
                displayedNotifications.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Triggers tree growth event
    /// </summary>
    public static void TriggerTreeGrowthEvent(Tree tree, float previousHeight)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.TreeGrown,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"{tree.treeType} grew from {previousHeight:F1}m to {tree.height:F1}m",
            additionalData = new Dictionary<string, object>
            {
                ["previousHeight"] = previousHeight,
                ["currentHeight"] = tree.height,
                ["growthAmount"] = tree.height - previousHeight
            }
        };
        
        FireTreeEvent(eventData);
        OnTreeGrown?.Invoke(tree);
    }
    
    /// <summary>
    /// Triggers tree death event
    /// </summary>
    public static void TriggerTreeDeathEvent(Tree tree)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.TreeDied,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"{tree.treeType} tree died at age {tree.age} days"
        };
        
        FireTreeEvent(eventData);
        OnTreeDied?.Invoke(tree);
        
        TreeEventSystem eventSystem = FindObjectOfType<TreeEventSystem>();
        if (eventSystem != null)
        {
            eventSystem.ShowNotification($"💀 {tree.treeType} died!", NotificationType.Error);
        }
    }
    
    /// <summary>
    /// Triggers tree interaction event
    /// </summary>
    public static void TriggerTreeInteractionEvent(Tree tree, TreeInteractionType interactionType)
    {
        var eventData = new TreeEventData
        {
            eventType = TreeEventType.TreeInteraction,
            tree = tree,
            timestamp = DateTime.Now,
            message = $"Player {interactionType} {tree.treeType} tree"
        };
        
        FireTreeEvent(eventData);
        OnTreeInteraction?.Invoke(tree, interactionType);
    }
    
    /// <summary>
    /// Triggers analytics updated event
    /// </summary>
    public static void TriggerAnalyticsUpdatedEvent(TreeAnalyticsData analytics)
    {
        OnAnalyticsUpdated?.Invoke(analytics);
        
        TreeEventSystem eventSystem = FindObjectOfType<TreeEventSystem>();
        if (eventSystem != null && eventSystem.enableEventLogging)
        {
            Debug.Log($"[Analytics] Updated: {analytics.totalTrees} trees, {analytics.averageHealth:F1}% avg health");
        }
    }
    
    void OnGUI()
    {
        if (!enableEventNotifications || displayedNotifications.Count == 0) return;
        
        // Display notifications
        float yOffset = 10f;
        foreach (var notification in displayedNotifications)
        {
            Color backgroundColor = GetNotificationColor(notification.type);
            
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;
            style.fontSize = 14;
            
            float remainingTime = notification.duration - (Time.time - notification.timestamp);
            float alpha = Mathf.Clamp01(remainingTime / 0.5f); // Fade out in last 0.5 seconds
            
            backgroundColor.a = alpha;
            style.normal.background = CreateColorTexture(backgroundColor);
            
            GUI.Box(new Rect(Screen.width - 320, yOffset, 300, 30), notification.message, style);
            yOffset += 35f;
        }
    }
    
    /// <summary>
    /// Gets the color for a notification type
    /// </summary>
    private Color GetNotificationColor(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success:
                return new Color(0.2f, 0.8f, 0.2f, 0.8f);
            case NotificationType.Warning:
                return new Color(0.8f, 0.6f, 0.2f, 0.8f);
            case NotificationType.Error:
                return new Color(0.8f, 0.2f, 0.2f, 0.8f);
            default:
                return new Color(0.3f, 0.3f, 0.8f, 0.8f);
        }
    }
    
    /// <summary>
    /// Creates a solid color texture for GUI backgrounds
    /// </summary>
    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        TreeManager treeManager = FindObjectOfType<TreeManager>();
        if (treeManager != null)
        {
            treeManager.OnTreeAdded -= HandleTreeAdded;
            treeManager.OnTreeRemoved -= HandleTreeRemoved;
            treeManager.OnTreeHealthChanged -= HandleTreeHealthChanged;
        }
    }
}

/// <summary>
/// Tree event data structure
/// </summary>
[System.Serializable]
public class TreeEventData
{
    public TreeEventType eventType;
    public Tree tree;
    public DateTime timestamp;
    public string message;
    public Dictionary<string, object> additionalData;
}

/// <summary>
/// Types of tree events
/// </summary>
public enum TreeEventType
{
    TreePlanted,
    TreeGrown,
    TreeDied,
    TreeRemoved,
    HealthChanged,
    HealthCritical,
    TreeInteraction,
    TreeLogged,
    AnalyticsUpdated
}

/// <summary>
/// Types of tree interactions
/// </summary>
public enum TreeInteractionType
{
    Clicked,
    Watered,
    Pruned,
    Harvested,
    Inspected
}

/// <summary>
/// Notification types for the UI
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Notification data structure
/// </summary>
[System.Serializable]
public class TreeNotification
{
    public string message;
    public NotificationType type;
    public float timestamp;
    public float duration;
}