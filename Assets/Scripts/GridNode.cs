// GridNode class represents a single node in a grid that is used in AStar algorithm.
// It implements the IHeapItem interface, which means that it needs a HeapIndex property and a CompareTo method.

using UnityEngine;

public class GridNode : IHeapItem<GridNode> {

    // Is the node walkable or not
    public readonly bool Walkable;

    // Position of the node in the world
    public Vector3 WorldPosition;

    // X and Y position of the node in the grid
    public readonly int GridX;
    public readonly int GridY;

    // Movement penalty for traversing the node, used to avoid certain areas
    public int MovementPenalty;

    // Cost to reach this node from the starting node
    public int GCost;

    // Heuristic cost to reach the target node from this node
    public int HCost;

    // The parent node of this node in the path
    public GridNode Parent;

    // The HeapIndex property is used to keep track of the node's position in the heap
    public int HeapIndex { get; set; }

    // Constructor for creating a new GridNode
    public GridNode(bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty) {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = gridX;
        GridY = gridY;
        MovementPenalty = penalty;
    }

    // FCost is the sum of GCost and HCost and real cost of movement
    private int FCost => GCost + HCost;

    // CompareTo method is used to compare two nodes based on their FCost and HCost
    // If multiple nodes have the same FCost will use HCost to decide which one to follow
    public int CompareTo(GridNode nodeToCompare) {
        var compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0) {
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        return -compare;
    }
}