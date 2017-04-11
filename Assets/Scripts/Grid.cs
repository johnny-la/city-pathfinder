using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public bool displayGrid;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

    //Variables for processing grid for HPA
    public int clusterSize;
    public List<Node>[] clusterNodes { get; set; }
    public int numberOfRows { get; set; }
    Path pathClass;
    public float entranceDistance;


    void Start() {
        //Create the grid
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		CreateGrid();

        //Process the grid
        numberOfRows = gridSizeX / clusterSize;
        clusterNodes = new List<Node>[numberOfRows * numberOfRows];
        pathClass = GetComponent<Path>();
        PreProcessGrid();

        
        //Tells Path class that grid is done and it can begin to pathfind
        pathClass.gridDone();
    }

    //Creates a grid of nodes according to a specified size
	void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		//Vector3 worldBottomLeft = transform.position - Vector3.right * (gridWorldSize.x/2 + nodeRadius) - Vector3.forward * (gridWorldSize.y/2+ nodeRadius);
		Vector3 worldBottomLeft = transform.position - Vector3.right * (gridWorldSize.x/2) - Vector3.forward * (gridWorldSize.y/2);
		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x,y] = new Node(walkable,worldPoint,x,y);
			}
		}
	}

    
    //Divides the grid into square clusters and desginates nodes that are at the boundary of those clusters
    void PreProcessGrid()
    {
        //Initialize array to store all nodes belonging to a cluster

        int currentCluster = 0;

        for (int j = 0; j < clusterNodes.Length; j++) clusterNodes[j] = new List<Node>();

        //For each cluster:
        for (int row = 0; row < numberOfRows; row++)
        {
            
            for (int col = 0; col < numberOfRows; col++)
            {
                //Figure out which nodes can be considered gateways to other clusters (entrances)

                //west

                bool entranceCheck = false;
                int lastExplored=0;

                //A border is the size of a cluster side
                for (int i = 0; i < clusterSize; i++)
                {

                    //No need to look at nodes at the end of the board 
                    if (col == 0) break;

                    //Check to see if node is walkable
                    Node temp = grid[row * clusterSize + i, col * clusterSize];

                    //Explored nodes have already been added to their cluster so no need to continue
                    if (temp.exploredWE) continue;

                    if (temp.walkable)
                    {

                        //Check to see if its symmetric counterpart is also walkable. If so, we have an entrance point!
                        temp.exploredWE = true;
                        Node symm = grid[row * clusterSize + i, col * clusterSize - 1];
                        if (symm.exploredWE) continue;

                        //If his counterpart is walkable save its position
                        if (symm.walkable) lastExplored = i;

                        //We are trying to find the largest entrance possible
                        //After we find an end of an entrance we continue looking until we are stuck due to an unwalkable node or symmetric counterpart node
                        //If we are stuck we make the last elligible entrance point we visited our end of the entrance
                        //When an end of an entrance is found add our node and its counterpart to their proper clusters
                        if (symm.walkable && (!entranceCheck))
                        {
                            entranceCheck = true;
                            AddToCluster(currentCluster, temp, symm, false, false);
                        }
                        else if (!symm.walkable && entranceCheck)
                        {
                            Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize - 1];
                            Node temp2 = grid[row * clusterSize + i, col * clusterSize];
                            entranceCheck = false;
                            AddToCluster(currentCluster, temp2, symm2, false, false);
                        }

                    }
                    //We are trying to find the largest entrance possible
                    //After we find an end of an entrance we continue looking until we are stuck due to an unwalkable node or symmetric counterpart node
                    //If we are stuck we make the last elligible entrance point we visited our end of the entrance
                    //When an end of an entrance is found add our node and its counterpart to their proper clusters
                    else if (entranceCheck)
                    {
                        Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize - 1];
                        Node temp2 = grid[row * clusterSize + i, col * clusterSize];
                        entranceCheck = false;
                        AddToCluster(currentCluster, temp2, symm2, false, false);


                    }
                    //We are trying to find the largest entrance possible
                    //After we find an end of an entrance we continue looking until we are stuck due to an unwalkable node or symmetric counterpart node
                    //If we are stuck we make the last elligible entrance point we visited our end of the entrance
                    //When an end of an entrance is found add our node and its counterpart to their proper clusters
                    //This also applied if we reached the end of the cluster and haven't found the other end of the entrance yet
                    if (entranceCheck && i == clusterSize - 1)
                    {
                        Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize - 1];
                        Node temp2 = grid[row * clusterSize + i, col * clusterSize];
                        entranceCheck = false;
                        AddToCluster(currentCluster, temp2, symm2, false, false);
                    }
                }

                //Do the same thing for all of the other borders

                //east

                entranceCheck = false;
                lastExplored = 0;
                for (int i = 0; i < clusterSize; i++)
                {
                        if (col == numberOfRows - 1) break;

                        Node temp = grid[row * clusterSize + i, col * clusterSize + clusterSize - 1];
                        if (temp.walkable)
                        {
                            temp.exploredWE = true;
                            Node symm = grid[row * clusterSize + i, col * clusterSize + clusterSize];
                            if (symm.exploredWE) continue;

                            if (symm.walkable) lastExplored = i;

                            if (symm.walkable && (!entranceCheck))
                            {
                                entranceCheck = true;
                                AddToCluster(currentCluster, temp, symm, false, true);
                        }
                            else if (!symm.walkable && entranceCheck)
                            {
                                Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize];
                                Node temp2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize - 1];
                                entranceCheck = false;
                                AddToCluster(currentCluster, temp2, symm2, false, true);
                        }
                        }
                        else if (entranceCheck)
                        {
                            Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize];
                            Node temp2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize - 1];
                            entranceCheck = false;
                            AddToCluster(currentCluster, temp2, symm2, false, true);
                    }
                        if (entranceCheck && i == clusterSize - 1)
                        {
                            Node symm2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize];
                            Node temp2 = grid[row * clusterSize + lastExplored, col * clusterSize + clusterSize - 1];
                            entranceCheck = false;
                            AddToCluster(currentCluster, temp2, symm2, false, true);
                        }

                }

                //North and south use different explore checks than west and east borders since explored nodes can act as an entrance on both x and y axis.
                
                //north
                lastExplored = 0;
                for (int i = 0; i < clusterSize; i++)
                {
                    if (row == numberOfRows - 1) break;
                    Node temp = grid[row * clusterSize + clusterSize - 1, col * clusterSize + i];
                    if (temp.walkable)
                    {
                        temp.exploredNS = true;
                        Node symm = grid[row * clusterSize + clusterSize, col * clusterSize + i];
                        if (symm.exploredNS) continue;

                        if (symm.walkable) lastExplored = i;

                        if (symm.walkable && (!entranceCheck))
                        {
                            entranceCheck = true;
                            AddToCluster(currentCluster, temp, symm, true, false);
                        }
                        else if (!symm.walkable && entranceCheck)
                        {
                            Node symm2 = grid[row * clusterSize + clusterSize, col * clusterSize + lastExplored];
                            Node temp2 = grid[row * clusterSize + clusterSize - 1, col * clusterSize + lastExplored];
                            entranceCheck = false;
                            AddToCluster(currentCluster, temp2, symm2, true, false);
                        }
                    }
                    else if ((!temp.walkable )&& entranceCheck)
                    {
                        Node symm2 = grid[row * clusterSize + clusterSize, col * clusterSize + lastExplored];
                        Node temp2 = grid[row * clusterSize + clusterSize - 1, col * clusterSize + lastExplored];
                        entranceCheck = false;
                        AddToCluster(currentCluster, temp2, symm2, true, false);
                    }

                    if (entranceCheck && i == clusterSize - 1)
                    {
                        Node symm2 = grid[row * clusterSize + clusterSize, col * clusterSize + lastExplored];
                        Node temp2 = grid[row * clusterSize + clusterSize - 1, col * clusterSize + lastExplored];
                        entranceCheck = false;
                        AddToCluster(currentCluster, temp2, symm2, true, false);
                    }
                }
                //south
                lastExplored = 0;
                for (int i = 0; i < clusterSize; i++)
                {
                    if(row == 0) break;
                    Node temp = grid[row * clusterSize, col * clusterSize + i];
                    if (temp.walkable)
                    {
                        temp.exploredNS = true;
                        Node symm = grid[row * clusterSize -1 , col * clusterSize + i];
                        if (symm.exploredNS) continue;

                        if (symm.walkable) lastExplored = i;

                        if (symm.walkable && (!entranceCheck))
                        {
                            entranceCheck = true;
                            AddToCluster(currentCluster, temp, symm, true, true);
                        }
                        else if (!symm.walkable && entranceCheck)
                        {
                            Node symm2 = grid[row * clusterSize - 1, col * clusterSize + lastExplored];
                            Node temp2 = grid[row * clusterSize, col * clusterSize + lastExplored];
                            entranceCheck = false;

                            AddToCluster(currentCluster, temp2, symm2, true, true);
                        }
                    }
                    else if ((!temp.walkable) && entranceCheck)
                    {
                        Node symm2 = grid[row * clusterSize - 1, col * clusterSize + lastExplored];
                        Node temp2 = grid[row * clusterSize, col * clusterSize + lastExplored];
                        entranceCheck = false;

                        AddToCluster(currentCluster, temp2, symm2, true, true);
                    }
                    if ( entranceCheck && i == clusterSize - 1)
                    {

                        Node symm2 = grid[row * clusterSize - 1, col * clusterSize + lastExplored];
                        Node temp2 = grid[row * clusterSize, col * clusterSize + lastExplored];
                        entranceCheck = false;

                        AddToCluster(currentCluster, temp2, symm2, true, true);

                    } 
                }
                
                currentCluster++;
            }
            
        }

        //Entrances are done, now for each cluster we join any pair of its entrances together if they can be connected

        for (int i = 1; i < numberOfRows*numberOfRows; i++)
        {
            foreach(Node n in clusterNodes[i])
            {
                foreach (Node e in clusterNodes[i])
                {
                    //Entrances must be different
                    if (n.Equals(e) || n.hasNeighbour(e)) continue;

                    //See if the two entrances are connected
                    List<Node> temp = pathClass.FindPath(n.worldPosition, e.worldPosition, false);

                    if (temp != null)
                    {
                        //They are connected, so make them neighbours

                        //Calculate edge cost between them
                        Node currentNode = n;
                        float tempDistance = 0.0f;
                        for (int f = 1; f < temp.Count; f++)
                        {

                            if (currentNode.gridX != temp[f].gridX && currentNode.gridY != temp[f].gridY)
                            {
                                tempDistance = tempDistance + 1.414f;
                            } else
                            {
                                tempDistance = tempDistance + 1.0f;
                            }
                            currentNode = temp[f];
                        }

                        //Make them neighbours by adding them to the other's neighbour collection and edge dictionary
                        Edge tempEdge = new Edge(n, e);
                        tempEdge.path = temp;
                        tempEdge.distance = tempDistance;
                        n.AddEdge(e, tempEdge);

                        //Edges are ordered from start to finish, so reverse its path of nodes when assigning it to the other node
                        temp.Reverse();
                        
                        Edge tempEdge2 = new Edge(e, n);
                        tempEdge2.path = temp;
                        tempEdge2.distance = tempDistance;
                        e.AddEdge(e, tempEdge2);

                        
                        
                    }
                }
 
            }

        }

    }

    //Makes 2 nodes an entrance
    void AddToCluster (int clusterIndex, Node a, Node b, bool vertChoice, bool horizChoice)
    {
        //Calculate which cluster to add the nodes to
        int clusterDifference = 0;
        if (!vertChoice && !horizChoice) //west
        {
            clusterDifference = -1;
        }
        else if (!vertChoice && horizChoice) //east
        {
            clusterDifference = +1;
        }
        else if (vertChoice && !horizChoice) //north
        {
            clusterDifference = 6;
        }
        else //south
        {
            clusterDifference = -6;
        }

        //Add the nodes to their clusters
        //Designate them as an entrance node and explored
        clusterNodes[clusterIndex].Add(a);
        clusterNodes[clusterIndex + clusterDifference].Add(b);
        b.exploredNS = true;
        b.clusterIndex = clusterIndex + clusterDifference;
        b.inCluster = true;
        a.exploredNS = true;
        a.clusterIndex = clusterIndex;
        a.inCluster = true;

        //Make the two nodes neighbours by adding an edge between them
        Edge tempEdge = new Edge(a, b);
        Edge symmEdge = new Edge(b, a);
        tempEdge.path.Add(a);
        tempEdge.path.Add(b);
        symmEdge.path.Add(b);
        symmEdge.path.Add(a);
        tempEdge.distance = entranceDistance;
        symmEdge.distance = entranceDistance;
        a.AddEdge(b, tempEdge);
        b.AddEdge(a, symmEdge);

    }

    //Get neighbouring nodes on the grid, used for AStar mainly
	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0) continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		/*int[] xValues = new int[4]{-1,1,0,0};
		int[] yValues = new int[4]{0,0,1,-1};
		for (int x = 0; x < 4; x++) {
			int checkX = node.gridX + xValues[x];
			int checkY = node.gridY + yValues[x];

			if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
				neighbours.Add(grid[checkX,checkY]);
			}
		}
		 */
		return neighbours;
	}
	

    //Converts a worldpoint to the most suitable node on the grid
	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x/2 + nodeRadius) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2 + nodeRadius) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		return grid[x,y];
	}

	public List<Node> pathAStar;
    public List<Node> pathHPA;

   
    //Draws the grid for debugging purposes and to show the path found by HPA and AStar
    void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));

        //Display grid with colour coded cluster nodes and unwalkable nodes
		if (grid != null && displayGrid) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable)?Color.white:Color.red;
                if (n.inCluster) Gizmos.color = Color.cyan;	
                Gizmos.DrawCube(n.worldPosition + Vector3.down*0.25f, Vector3.one * (nodeDiameter-0.1f) + Vector3.down*0.25f);
			}
		}

        //Draw AStar path
		if (pathAStar != null) {
			foreach (Node n in pathAStar) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}

        //Draw HPA path
        //Path given by the HPA pathfinding function only gives path through the abstract graph
        //Therefore, we need to also color the nodes that serve as imtermediaries between these nodes, hence why we stored them in the edges
        if (pathHPA != null)
        {
            for (int i = 0; i < pathHPA.Count-1; i++)
            {
                Node n = pathHPA[i];
                Gizmos.color = Color.green;
                Gizmos.DrawCube(n.worldPosition + Vector3.up, Vector3.one * (nodeDiameter - .1f));
                
                Node nextnode = pathHPA[i + 1];
                Edge nextnodeEdge = n.edges[nextnode];
                
                foreach (Node e in nextnodeEdge.path)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(e.worldPosition + Vector3.up, Vector3.one * (nodeDiameter - .1f));
                }
                
            }
        }
    }

    //Calculate total distance by summing up all edge costs instead of using the FgCost
    public float total = 0.0f;
    public void getTotalDistance()
    {
        if (pathHPA != null)
        {
            for (int i = 0; i < pathHPA.Count - 1; i++)
            {
                Node n = pathHPA[i];
                Node nextnode = pathHPA[i + 1];
                Edge nextnodeEdge = n.edges[nextnode];
                total = total + nextnodeEdge.distance;
 

            }
        }
    }
}
