# Tree Logging System for Pathfinder Unity Project

## Overview
A comprehensive tree logging and management system that generates, tracks, and logs trees within a Unity pathfinding environment. Trees are procedurally generated, avoid buildings, have realistic growth mechanics, and can be logged to multiple output formats.

## Features

### 🌳 Tree Management
- **Procedural Generation**: Automatically generates trees that avoid buildings and maintain proper spacing
- **5 Tree Types**: Oak, Pine, Birch, Maple, and Willow with unique characteristics
- **Real-time Growth**: Trees grow over time with health simulation based on environmental factors
- **Smart Placement**: Trees avoid buildings using grid-based collision detection

### 📊 Logging Capabilities
- **Multiple Output Formats**: Console, text files, and CSV exports
- **Comprehensive Data**: Position, type, health, age, environmental factors, and timestamps
- **Statistics**: Tree counts by type, health distribution, growth analytics
- **Auto-logging**: Configurable automatic logging intervals

### 🎮 Interactive Controls
- **Keyboard Shortcuts**:
  - `L` - Log all trees
  - `E` - Export to CSV
  - `C` - Console log only
  - `G` - Generate new trees
  - `R` - Remove random tree
  - `T` - Add tree at camera position
- **Click to Log**: Click individual trees to log their data
- **GUI Controls**: On-screen buttons for logging operations

## File Structure

```
Assets/Scripts/
├── Tree.cs                 # Core tree data structure
├── TreeManager.cs          # Tree generation and management
├── TreeLogger.cs           # Logging functionality
├── TreeComponent.cs        # Visual tree component
├── TreePrefabGenerator.cs  # Procedural tree generation
└── CityGenerator.cs        # Modified to integrate trees
```

## Usage

### Setup
1. Add `TreeManager` and `TreeLogger` components to GameObjects in your scene
2. Configure tree generation parameters in the TreeManager inspector
3. Ensure CityGenerator has `generateTrees` enabled and references the TreeManager

### Tree Generation
Trees are automatically generated when the city is created, or manually triggered:
```csharp
TreeManager treeManager = FindObjectOfType<TreeManager>();
treeManager.GenerateTrees();
```

### Logging Trees
Multiple ways to log tree data:
```csharp
TreeLogger logger = FindObjectOfType<TreeLogger>();
logger.LogAllTrees();           // All formats
logger.LogTreesToConsole();     // Console only
logger.ExportTreesToCSV();      // CSV only
```

### Accessing Tree Data
```csharp
TreeManager treeManager = FindObjectOfType<TreeManager>();
List<Tree> allTrees = treeManager.GetAllTrees();
List<Tree> healthyTrees = treeManager.GetHealthyTrees();
List<Tree> oakTrees = treeManager.GetTreesByType(TreeType.Oak);
```

## Tree Properties

Each tree contains comprehensive data:
- **Physical**: Position, height, radius
- **Biological**: Type, age, health, growth rate
- **Environmental**: Sunlight exposure, water level
- **Metadata**: Unique ID, planted date, last logged date

## Configuration

### TreeManager Settings
- `maxTrees`: Maximum number of trees to generate
- `minDistanceBetweenTrees`: Minimum spacing between trees
- `minDistanceFromBuildings`: Minimum distance from buildings
- `enableTreeGrowth`: Enable real-time growth simulation
- `growthUpdateInterval`: How often to update growth (seconds)

### TreeLogger Settings
- `enableConsoleLogging`: Log to Unity console
- `enableFileLogging`: Save to text files
- `enableCSVExport`: Export to CSV format
- `autoLogInterval`: Auto-logging frequency (seconds)
- `enableAutoLogging`: Enable automatic periodic logging

## Output Formats

### Console Log
Displays summary statistics and detailed tree information in Unity console.

### Text Files
Comprehensive reports saved to `Application.persistentDataPath/TreeLogs/`:
- Summary statistics
- Tree type distribution
- Individual tree details
- Timestamps and metadata

### CSV Export
Data-friendly format for analysis:
```csv
ID,Type,X,Y,Z,Height,Radius,Health,Age,IsHealthy,Sunlight,Water,PlantedDate,LastLoggedDate
```

## Tree Types & Characteristics

| Type   | Growth Rate | Max Height | Max Radius | Color Theme |
|--------|-------------|------------|------------|-------------|
| Oak    | 0.5         | 25m        | 8m         | Rich Green  |
| Pine   | 0.8         | 35m        | 6m         | Dark Green  |
| Birch  | 1.0         | 20m        | 5m         | Light Green |
| Maple  | 0.6         | 30m        | 10m        | Reddish     |
| Willow | 1.2         | 15m        | 12m        | Pale Green  |

## Events System

The TreeManager fires events for tree lifecycle:
```csharp
treeManager.OnTreeAdded += (tree) => Debug.Log($"Tree added: {tree.treeType}");
treeManager.OnTreeRemoved += (tree) => Debug.Log($"Tree removed: {tree.id}");
treeManager.OnTreeHealthChanged += (tree) => Debug.Log($"Health changed: {tree.health}%");
```

## Performance Considerations

- Trees update growth in batches every `growthUpdateInterval` seconds
- Visual updates only occur when tree properties change significantly
- Debug gizmos can be disabled for performance
- Procedural prefab generation is cached per tree type

## Troubleshooting

### No Trees Generated
- Check that `TreeManager.autoGenerateOnStart` is enabled
- Ensure `CityGenerator.generateTrees` is true
- Verify TreeManager is referenced in CityGenerator
- Check generation area size and tree limits

### Logging Not Working
- Verify TreeLogger component is in the scene
- Check file permissions for log directory
- Ensure TreeManager exists and has trees

### Trees Not Visible
- Check that tree prefabs or procedural generation is working
- Verify trees aren't placed underground (Y position)
- Check camera position and scene view

## Future Enhancements

- Seasonal changes and visual effects
- Tree diseases and environmental hazards
- Advanced collision detection with actual building meshes
- Tree interaction system (cutting, watering, etc.)
- Performance optimizations with LOD system
- Integration with weather systems