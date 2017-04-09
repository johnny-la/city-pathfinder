using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Edge {
    Node endpoint1;
    Node endpoint2;

    //Stores distance between two nodes
    public float distance { get; set; }

    //List of nodes in between path from one enpoint to another. Includes start node and includes end node
    public List<Node> path { get; set; }

    public Edge(Node start, Node end)
    {
        endpoint1 = start;
        endpoint2 = end;
        path = new List<Node>();
    }
}
