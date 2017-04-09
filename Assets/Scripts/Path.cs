using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

//This class does pathfinding for us
public class Path : MonoBehaviour {

    Grid grid;
    public Transform start;
    public Transform end;

    public float HPADistance = 0.0f;
    public float AStarDistance = 0.0f;

    void Awake() {
        grid = GetComponent<Grid>();
    }

    void Start() {

    }

    public void gridDone()
    {

        grid = GetComponent<Grid>();
        grid.pathAStar = FindPath(start.position, end.position, true);
        grid.pathHPA = FindPathHPA(start.position, end.position);
        grid.getTotalDistance();
        print("AStar path has distance of " + AStarDistance);
        print("HPA path has distance of " + HPADistance);

    }


    //Finds a path between two points using A*, returns the path of nodes to get there
    public List<Node> FindPath(Vector3 startPos, Vector3 endPos, bool stopWatch) {

        Stopwatch timer = new Stopwatch();
        if (stopWatch) {
            timer.Start();
        }

        //Get the proper nodes from the world positions
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node endNode = grid.NodeFromWorldPoint(endPos);

        //Use a heap to store our nodes yet to be explored
        FNodeHeap openSet = new FNodeHeap(200);
        HashSet<Node> closedSet = new HashSet<Node>();

        //Start exploring with our start node
        openSet.Add(startNode);
        startNode.Reset();

        //While there are still nodes to be explored
        while (openSet.Count > 0) {

            //Get the most promising node, add it to our list of already explored nodes
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            //If we reach our destination return our path
            if (currentNode == endNode) {
                if (stopWatch)
                {
                    timer.Stop();
                    print("AStar Path Found in " + timer.ElapsedMilliseconds + " ms");
                    AStarDistance = currentNode.FgCost;
                }
                return RetracePath(startNode, endNode);
            }

            //For each neighbour:
            foreach (Node neighbour in grid.GetNeighbours(currentNode)) {

                //Skip nodes we can't walk on or are already explored.
                if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                //Calculate edgecost based on what kind of neighbour we have
                float edgeCost;
                if (currentNode.gridX != neighbour.gridX && currentNode.gridY != neighbour.gridX)
                {
                    edgeCost = 1.414f;
                }
                else
                {
                    edgeCost = 1.0f;
                }

                //Calculate the new gCost
                float newCostToNeighbour = currentNode.FgCost + edgeCost;

                //If neighbour has not be explored yet or we obtained a better route to it, update the node's gCost and hCost, as well as its parent
                if (newCostToNeighbour < neighbour.FgCost || !openSet.Contains(neighbour)) {
                    neighbour.FgCost = newCostToNeighbour;
                    neighbour.FhCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    //Add to open set if not already explored
                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                    else {
                        //Else update our heap
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        return null;

    }

    //Gives us a list of paths from the start to end Node. Assumes that path to endNode already found
    List<Node> RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        //Keep going to the node's parents until we reach the start
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }

    //Does HPA pathfinding. Assumes that the grid has already been prepared
    //Returns a path of nodes connected on the abstract graph (not by the grid)
    public List<Node> FindPathHPA(Vector3 startPos, Vector3 endPos) {
        //connect endNode and startNode to graph

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node endNode = grid.NodeFromWorldPoint(endPos);

        //Find out what cluster they are in
        int startClusterIndex = (startNode.gridX / grid.clusterSize) * grid.numberOfRows + (startNode.gridY / grid.clusterSize);
        int endClusterIndex = (endNode.gridX / grid.clusterSize) * grid.numberOfRows + (endNode.gridY / grid.clusterSize);

        //Connect the start node and end node to every node in their respective cluster
        //Very similar to how it was done when preparing the grid
        foreach (Node n in grid.clusterNodes[startClusterIndex])
        {
            if (startNode.Equals(n) || startNode.hasNeighbour(n)) continue;

            List<Node> temp = FindPath(startNode.worldPosition, n.worldPosition, false);
            if (temp != null)
            {

                Node currentNode = startNode;

                float tempDistance = 0.0f;
                for (int f = 0; f < temp.Count; f++)
                {

                    if (currentNode.gridX != temp[f].gridX && currentNode.gridY != temp[f].gridY)
                    {
                        tempDistance = tempDistance + 1.414f;
                    }
                    else
                    {
                        tempDistance = tempDistance + 1.0f;
                    }
                    currentNode = temp[f];
                }

                Edge tempEdge = new Edge(startNode, n);
                tempEdge.path = temp;
                tempEdge.distance = tempDistance;
                startNode.AddEdge(n, tempEdge);

                temp.Reverse();

                Edge tempEdge2 = new Edge(n, startNode);
                tempEdge2.path = temp;
                tempEdge2.distance = tempDistance;
                n.AddEdge(startNode, tempEdge2);
            }
        }
        foreach (Node n in grid.clusterNodes[endClusterIndex])
        {
            if (endNode.Equals(n) || endNode.hasNeighbour(n)) continue;

            List<Node> temp = FindPath(endNode.worldPosition, n.worldPosition, false);
            if (temp != null)
            {
                Node currentNode = endNode;

                float tempDistance = 0.0f;
                for (int f = 0; f < temp.Count; f++)
                {
                    if (currentNode.gridX != temp[f].gridX && currentNode.gridY != temp[f].gridY)
                    {
                        tempDistance = tempDistance + 1.414f;
                    }
                    else
                    {
                        tempDistance = tempDistance + 1.0f;
                    }
                    currentNode = temp[f];
                }

                Edge tempEdge = new Edge(endNode, n);
                tempEdge.path = temp;
                tempEdge.distance = tempDistance;
                endNode.AddEdge(n, tempEdge);

                temp.Reverse();
                

                Edge tempEdge2 = new Edge(n, endNode);
                tempEdge2.path = temp;
                tempEdge2.distance = tempDistance;
                n.AddEdge(endNode, tempEdge2);
            }
        }
    

    //Now do A* on our new graph
    FNodeHeap openSet = new FNodeHeap(200);
    HashSet<Node> closedSet = new HashSet<Node>();
    openSet.Add(startNode);
    startNode.Reset();


	while (openSet.Count > 0) {
		Node currentNode = openSet.RemoveFirst();
        closedSet.Add(currentNode);

		if (currentNode == endNode) {
            HPADistance = currentNode.FgCost;
        	return RetracePath(startNode, endNode);
		}

		foreach (Node neighbour in currentNode.neighbours) {
			if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
			}

			float newCostToNeighbour = currentNode.FgCost + currentNode.distanceToNeighbour(neighbour);
				if (newCostToNeighbour < neighbour.FgCost || !openSet.Contains(neighbour)) {
					neighbour.FgCost = newCostToNeighbour;
					neighbour.FhCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour)){
						openSet.Add(neighbour);
					}
					else {
						openSet.UpdateItem(neighbour);
					}
				}
			}
		}

        return null;

}
    

   
//Heuristic for all our A* like searches, uses Euclidean Distance from the goal		
int GetDistance(Node nodeA, Node nodeB) {
	int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
	int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        /*if (dstX > dstY)
			return 14*dstY + 10*(dstX-dstY);
		return 14*dstX + 10*(dstY-dstX);

        */
        return dstX * dstX + dstY * dstY;
	}   
}

