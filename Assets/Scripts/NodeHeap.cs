using UnityEngine;
using System.Collections;
using System;

//A Heap to hold Nodes
public class FNodeHeap
{

    Node[] items;
    int currentItemCount;

    public FNodeHeap(int maxSize)
    {
        items = new Node[maxSize];
        currentItemCount = 0;
    }

    //Add a Node to the Heap
    public void Add(Node item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    //Removes the item with the highest priority
    //Replaces it with the last item in heap, then restores the heap property
    public Node RemoveFirst()
    {
        Node first = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return first;
    }

    //Restore heap property if Node item's fCost has been modified
    public void UpdateItem(Node item)
    {
        SortUp(item);
    }

    //Gets size of heap
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    //Returns true if heap contains Node, else false
    public bool Contains(Node item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //Keep swapping Node item with its children until its fcost is less than both of its children
    void SortDown(Node item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            //Check if node has a child
            if (childIndexLeft < currentItemCount)
            {

                //If yes, child to swap is the one with the lowest fcost
                swapIndex = childIndexLeft;

                //Check if right child exists
                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].FCompareTo(items[childIndexRight]) > 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                //Swap current node if its fcost is higher
                if (item.FCompareTo(items[swapIndex]) > 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else {
                    return;
                }

            }
            else {
                return;
            }

        }
    }

    //Keep swapping Node item with its parent until its fcost is more than its parent
    void SortUp(Node item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            Node parentItem = items[parentIndex];
            if (item.FCompareTo(parentItem) < 0)
            {
                Swap(item, parentItem);
            }
            else {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    //Swap nodes having different heap indexes
    void Swap(Node itemA, Node itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        //Switch heap indexes
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

