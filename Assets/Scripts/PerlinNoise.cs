using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
	// Used to generate varying perlin noise
	private int seed;

	// The higher the number, the more iterations in the perlin noise
	private int octaves;

	public PerlinNoise(int octaves)
	{
		this.octaves = octaves;

		seed = Random.Range(-1000000,1000000);
	}

	/// <summary>
	/// Returns the height of the point at the given coordinates
	/// </summary>
	public float getHeight(float x, float y, float maxHeight)
	{
		float result = 0;

		// Iterate through each octave
		for (int octave = octaves; octave > 0; octave--)
		{
			// Computes the size of a cell in the perlin noise
			int cellSize = octave*2;

			// Find the cell coordinates of the point
			int xIndex = (int)(x / cellSize);
			int yIndex = (int)(y / cellSize);

			// Find the extra offet into the cell
			float xOffset = (x % (cellSize)) / ((float)cellSize);
			float yOffset = (y % (cellSize)) / ((float)cellSize);
			
			// Compute perlin noise values in the chosen cell
			float topLeft = getNoise(xIndex, yIndex, maxHeight);
			float topRight = getNoise(xIndex+1, yIndex, maxHeight);
			float bottomLeft = getNoise(xIndex, yIndex+1, maxHeight);
			float bottomRight = getNoise(xIndex+1, yIndex+1, maxHeight);

			// Linearly interpolate to find the height value at the given point
			float yLeftLerp = Mathf.Lerp(topLeft,bottomLeft,yOffset);
			float yRightLerp = Mathf.Lerp(topRight,bottomRight,yOffset);

			float height = Mathf.Lerp(yLeftLerp, yRightLerp, xOffset);
			result += height;

			// Move to the next octave
			maxHeight /= 2f;
			maxHeight = Mathf.Clamp(maxHeight,1,maxHeight);
		}

		return result;
	}

	/// <summary>
	/// Returns a deterministic noise value at the given point
	/// </summary>
	private float getNoise(int x, int y, float max)
	{
		return Mathf.Abs(((x+y*65535+seed)^5) % (int)max);
	}
}
