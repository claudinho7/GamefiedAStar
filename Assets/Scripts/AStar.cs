// This script is used to implement the A* algorithm for pathfinding in Unity.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class AStar : MonoBehaviour 
{
	// Reference to the PathRequestManager script and the Grid script.
	private PathRequestManager _requestManager;
	private Grid _grid;

	private void Awake() 
	{
		// Get the PathRequestManager and Grid components.
		_requestManager = GetComponent<PathRequestManager>();
		_grid = GetComponent<Grid>();
	}
	
	// Method to start finding a path, given the start and target positions.
	public void StartFindPath(Vector3 startPos, Vector3 targetPos) 
	{
		// Use a coroutine to find the path asynchronously.
		StartCoroutine(FindPath(startPos,targetPos));
	}

	// Coroutine to find the path asynchronously.
	private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) 
	{
		// Start a stopwatch to measure the time taken to find the path.
		Stopwatch sw = new Stopwatch();
		sw.Start();
		
		// Create an empty array to store the waypoints in the path, and a boolean to indicate whether a path was found.
		var waypoints = Array.Empty<Vector3>();
		var pathSuccess = false;
		
		// Get the GridNode corresponding to the start and target positions.
		GridNode startNode = _grid.NodeFromWorldPoint(startPos);
		GridNode targetNode = _grid.NodeFromWorldPoint(targetPos);
		startNode.Parent = startNode;
		
		// Check if both the start and target nodes are walkable.
		if (startNode.Walkable && targetNode.Walkable) 
		{
			// Create an open set and a closed set.
			Heap<GridNode> openSet = new Heap<GridNode>(_grid.MaxSize);
			HashSet<GridNode> closedSet = new HashSet<GridNode>();
			// Add the start node to the open set.
			openSet.Add(startNode);
			
			// Keep looping until the open set is empty.
			while (openSet.Count > 0) 
			{
				// Get the node in the open set with the lowest f cost.
				GridNode currentNode = openSet.RemoveFirst();
				// Add the current node to the closed set.
				closedSet.Add(currentNode);
				
				// If the current node is the target node, the path has been found.
				if (currentNode == targetNode) 
				{
					// Stop the stopwatch and print the time taken to find the path.
					sw.Stop();
					print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					// Set pathSuccess to true.
					pathSuccess = true;
					break;
				}
				// Loop through the neighbours of the current node.
				foreach (GridNode neighbour in _grid.GetNeighbours(currentNode)) 
				{
					// If the neighbour is obstacle or is in the closed set, skip to the next neighbour.
					if (!neighbour.Walkable || closedSet.Contains(neighbour)) 
					{
						continue;
					}
					// Calculate the new movement cost to the neighbour.
					var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
					
					// If the new movement cost to the neighbour is greater than or equal to the neighbour's current G cost and the neighbour is already in the open set, skip to the next neighbour.
					if (newMovementCostToNeighbour >= neighbour.GCost && openSet.Contains(neighbour)) continue;
					// Otherwise, update the neighbour's costs and parent node
					neighbour.GCost = newMovementCostToNeighbour;
					neighbour.HCost = GetDistance(neighbour, targetNode);
					neighbour.Parent = currentNode;
					
					// If the neighbour is not already in the open set, add it
					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
					// If the neighbour is already in the open set, update its position in the heap
					else 
						openSet.UpdateItem(neighbour);
				}
			}
		}
		yield return null;
		if (pathSuccess) 
		{
			waypoints = RetracePath(startNode,targetNode);
		}
		_requestManager.FinishedProcessingPath(waypoints,pathSuccess);
		
	}
	
	// This method retraces the path by starting at the end node and following its parent nodes back to the start node
	private static Vector3[] RetracePath(GridNode startNode, GridNode endNode) 
	{
		List<GridNode> path = new List<GridNode>();
		GridNode currentNode = endNode;
		
		while (currentNode != startNode) 
		{
			path.Add(currentNode);
			currentNode = currentNode.Parent;
		}
		// Simplify the path by removing nodes that are in a straight line
		Vector3[] waypoints = SimplifyPath(path);
		// Reverse the order of the waypoints so they go from start to end
		Array.Reverse(waypoints);
		return waypoints;
		
	}

	// This method simplifies the path by removing nodes that are in a straight line
	private static Vector3[] SimplifyPath(List<GridNode> path) 
	{
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;
		
		for (var i = 1; i < path.Count; i ++) 
		{
			Vector2 directionNew = new Vector2(path[i-1].GridX - path[i].GridX,path[i-1].GridY - path[i].GridY);
			// If the direction changes, add a waypoint
			if (directionNew != directionOld) 
			{
				waypoints.Add(path[i].WorldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	// This method calculates the distance between two nodes
	private static int GetDistance(GridNode nodeA, GridNode nodeB) 
	{
		var dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
		var dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
		
		// Use the diagonal distance heuristic for movement on both axes
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
}
