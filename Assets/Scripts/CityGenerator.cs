using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{
    [TooltipAttribute("The width and height of the city")]
    public float citySize = 500;

    [TooltipAttribute("The width and height of a cell")]
    public float cellSize = 20;
    
    [TooltipAttribute("A 1x1x1 block that can be scaled to a building")]
    public GameObject buildingPrefab;

    [TooltipAttribute("The maximum height of a building in the city")]
    public float maxBuildingHeight = 50;

    [TooltipAttribute("The number of octaves used in the perlin noise generation")]
    public int octaves = 4;

    // The perlin noise generator used to determine building heights
    private PerlinNoise perlinNoise;

    // The coordinates of the top-left of the city
    private Vector3 topLeftPosition;
    // The width/height of a building
    private float buildingSize;

    public void Start()
    {
        // Compute the dimensions of the city
        Rows = (int)(citySize / cellSize);
        Columns = Rows;

        // Cache the top-left position of the city
        topLeftPosition = transform.position - new Vector3(citySize/2,0,citySize/2);
        buildingSize = buildingPrefab.transform.localScale.x;

        // Create the perlin noise generator
        perlinNoise = new PerlinNoise(octaves);

        GenerateCity();
    }
    
    /// <summary>
    /// Uses perlin noise to generate a city
    /// </summary>
    public void GenerateCity()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Vector3 buildingPosition = getBuildingPosition(row,col);
                float buildingHeight = perlinNoise.getHeight(buildingPosition.x,
                                                             buildingPosition.z,
                                                             maxBuildingHeight);
                
                CreateBuilding(buildingPosition,buildingHeight);
            }
        }
    }

    /// <summary>
    /// Creates a building at the given position, with the given height
    private void CreateBuilding(Vector3 position, float height)
    {
        // Create the building
        GameObject buildingObject = GameObject.Instantiate(buildingPrefab);
        // Set its coordinates and height
        buildingObject.transform.position = position + Vector3.up*(height/2);
        buildingObject.transform.parent = transform;
        Vector3 currentScale = buildingObject.transform.localScale;
        buildingObject.transform.localScale = new Vector3(currentScale.x,height,currentScale.z);
    }

    /// <summary>
    /// Returns the postion of a building at the given coordinates
    /// </summary>
    private Vector3 getBuildingPosition(int row, int col)
    {
        float maxSalt = (cellSize-buildingSize)/2;
        // Add random salt to make the city interesting
        float xOffset = col * cellSize + Random.Range(-maxSalt,maxSalt);
        float yOffset = row * cellSize + Random.Range(-maxSalt,maxSalt);

        return topLeftPosition + new Vector3(xOffset,0,yOffset);
    }

    /// <summary>
    /// The number of rows in the Perlin Noise generation
    /// </summary>
    public int Rows
    {
        get; private set;
    }

    /// <summary>
    /// The number of columns in the Perlin Noise generation
    /// </summary>
    public int Columns
    {
        get; private set;
    }
}