using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a node in the pathfinding graph
/// </summary>
public class Node : MonoBehaviour
{
    public Node[] neighbourList;
    // The node's neighbours
    private List<Node> neighbours = new List<Node>();

    // Caches the node's components
    private new Transform transform;

    public void Start()
    {
        transform = GetComponent<Transform>();

        if (neighbourList != null)
        {
            for (int i = 0; i < neighbourList.Length; i++)
            {
                neighbours.Add(neighbourList[i]);
            }
        }
    }

	/// <summary>
	/// Add the node to the list of neighbours
	/// </summary>
    public void AddNeighbour(Node node)
    {
        if (node == null) { return; }

        neighbours.Add(node);
    }

    /// <summary>
    /// Returns the node's position in world space
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// True if the node was visited in a pathfinding algorithm
    /// </summary>
    public bool Visited
    {
        get; set;
    }

    /// <summary>
    /// The node's neighbours
    /// </summary>
    public List<Node> Neighbours
    {
        get { return neighbours; }
    }

    public string ToString()
    {
        return GetPosition().ToString();
    }
}