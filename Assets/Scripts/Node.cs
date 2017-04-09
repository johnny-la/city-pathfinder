using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The unit by which all pathfinding is done. Defines walkable or unwalkable space
public class Node {

    //Variables for preprocessing the grid
    public bool walkable; 
    public bool inCluster { get; set; }
    public int clusterIndex { get; set; }

    //Used in finding entrances
    public bool exploredNS { get; set; }
    public bool exploredWE { get; set; }

    //For use when trying to get adjacent nodes in a grid
    public Vector3 worldPosition;
	public int gridX;
	public int gridY;

    //Variables for pathfinding
    public float FgCost;
    public float FhCost;

	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;

    public List<Node> neighbours = new List<Node>();

    public Dictionary<Node, Edge> edges = new Dictionary<Node, Edge>();


    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
        clusterIndex = -1;
        exploredNS = false;
        exploredWE = false;
	}

    //Adds node and an edge to neighbour collection and dictionary respectively, if not already added
    public void AddEdge(Node n, Edge e)
    {
        if (!edges.ContainsKey(n)) { 
            neighbours.Add(n);
            edges.Add(n, e);
        }
    }

    //Checks if neighbour has alreayd been added to current node
    public bool hasNeighbour(Node neighbour)
    {
        return neighbours.Contains(neighbour);
    }

    //Gets distance from current node to a neighbour
    public float distanceToNeighbour(Node neighbour)
    {
        return edges[neighbour].distance;
    }

    //Resets all info changed from pathfinding just in case
    public void Reset()
    {
        gCost = 0;
        hCost = 0;
        heapIndex = 0;
        parent = null;
        FgCost = 0;
        FhCost = 0;
    }

    //F costs in int and float form
	public int fCost {
		get {
			return gCost + hCost;
		}
	}

    public float FfCost
    {
        get
        {
            return FgCost + FhCost;
        }
    }

    //Index for sorting purposes when stored in the heap
    public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	//If current node has less fcost than nodeToCompare, return a negative
	//else if more, return a positive
	//if same, compare hcosts as a tiebreaker
	public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return compare;
    }

    //If current node has less fcost than nodeToCompare, return a negative
    //else if more, return a positive
    //if same, compare hcosts as a tiebreaker
    public float FCompareTo(Node nodeToCompare)
    {
        float compare = FfCost.CompareTo(nodeToCompare.FfCost);
        if (compare == 0)
        {
            compare = FhCost.CompareTo(nodeToCompare.FhCost);
        }
        return compare;
    }
}