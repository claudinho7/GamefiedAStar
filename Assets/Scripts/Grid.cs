using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour 
{
	// Display gizmos in the scene view
	public bool displayGridGizmos;
	
	// Layer mask used to identify obstacles
	public LayerMask unwalkableMask;
	// Size of the grid in world units
	public Vector2 gridWorldSize;
	// Radius of each node in the grid
	public float nodeRadius;
	
	// Terrain types and their corresponding movement penalties
	public TerrainType[] walkableRegions;
	// Penalty to add to movement cost when pathfinding around obstacles
	public int obstacleProximityPenalty = 10;
	// Dictionary that maps layer mask values to movement penalties
	private readonly Dictionary<int,int> _walkableRegionsDictionary = new Dictionary<int, int>();
	// Layer mask used to identify walkable areas (with penalties)
	private LayerMask _walkableMask;
	
	// 2D array that holds the nodes in the grid
	private GridNode[,] _grid;
	// Diameter of each node
	private float _nodeDiameter;
	// Size of the grid in nodes
	private int _gridSizeX, _gridSizeY;
	// Minimum and maximum movement penalties in the grid
	private int _penaltyMin = int.MaxValue;
	private int _penaltyMax = int.MinValue;

	private void Awake() 
	{
		// Calculate node diameter and grid size
		_nodeDiameter = nodeRadius*2;
		_gridSizeX = Mathf.RoundToInt(gridWorldSize.x/_nodeDiameter);
		_gridSizeY = Mathf.RoundToInt(gridWorldSize.y/_nodeDiameter);

		// Create a layer mask that includes all walkable terrain types
		foreach (TerrainType region in walkableRegions) 
		{
			_walkableMask.value |= region.terrainMask.value;
			// Add each walkable terrain type to the dictionary
			_walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value,2),region.terrainPenalty);
		}
	}

	public int MaxSize => _gridSizeX * _gridSizeY; 	// Maximum number of nodes in the grid

	// Create the grid
	public void CreateGrid() 
	{
		// Create a 2D array to hold the nodes
		_grid = new GridNode[_gridSizeX,_gridSizeY];
		// Calculate the position of the bottom-left node in the grid
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		// Loop through each node in the grid
		for (var x = 0; x < _gridSizeX; x ++) 
		{
			for (var y = 0; y < _gridSizeY; y ++) 
			{
				// Calculate the position of the node in world space
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.forward * (y * _nodeDiameter + nodeRadius);
				// Check if the node is walkable or not
				var walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));

				// Initialize movement penalty to zero
				int movementPenalty = 0;

				// Check if the node is part of a walkable terrain type
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				if (Physics.Raycast(ray,out var hit, 100, _walkableMask)) {
					_walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
				}

				if (!walkable) 
				{
					// If the node is un-walkable, increase its movement penalty by a certain amount
					movementPenalty += obstacleProximityPenalty;
				}
				// Create a new GridNode object and store it in the _grid array
				_grid[x,y] = new GridNode(walkable,worldPoint, x,y, movementPenalty);
			}
		}
		// Apply a blur to the movement penalty map to smooth out the values
		BlurPenaltyMap (3);
	}

	// This method applies a blur filter to the movement penalty values of each node in the grid.
	private void BlurPenaltyMap(int blurSize) 
	{
		// Calculate the size of the kernel used for blurring
		var kernelSize = blurSize * 2 + 1;
		var kernelExtents = (kernelSize - 1) / 2;

		// Create 2D arrays to store the horizontal and vertical passes of the penalty values.
		var penaltiesHorizontalPass = new int[_gridSizeX][];
		var penaltiesVerticalPass = new int[_gridSizeX][];
		
		for (var index = 0; index < _gridSizeX; index++)
		{
			penaltiesHorizontalPass[index] = new int[_gridSizeY];
			penaltiesVerticalPass[index] = new int[_gridSizeY];
		}

		// Calculate the horizontal pass of the penalty values.
		for (var y = 0; y < _gridSizeY; y++) 
		{
			for (var x = -kernelExtents; x <= kernelExtents; x++) 
			{
				// Clamp the sample index to avoid going out of bounds.
				var sampleX = Mathf.Clamp (x, 0, kernelExtents);
				penaltiesHorizontalPass[0][y] += _grid [sampleX, y].MovementPenalty;
			}

			for (var x = 1; x < _gridSizeX; x++) 
			{
				// Calculate the indices of the nodes to be removed and added in this pass.
				var removeIndex = Mathf.Max(0, x - kernelExtents - 1);
				var addIndex = Mathf.Min(_gridSizeX - 1, x + kernelExtents);
				
				// Update the penalty value for the current node in the horizontal pass.
				penaltiesHorizontalPass[x][y] = penaltiesHorizontalPass[x - 1][y] - _grid [removeIndex, y].MovementPenalty + _grid [addIndex, y].MovementPenalty;
			}
		}
		
		// Calculate the vertical pass of the penalty values.
		for (var x = 0; x < _gridSizeX; x++) 
		{
			for (var y = -kernelExtents; y <= kernelExtents; y++) 
			{
				// Clamp the sample index to avoid going out of bounds.
				var sampleY = Mathf.Clamp (y, 0, kernelExtents);
				penaltiesVerticalPass[x][0] += penaltiesHorizontalPass[x][sampleY];
			}
			// Calculate the final blurred penalty value for each node in the vertical pass.
			var blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x][0] / (kernelSize * kernelSize));
			// Update the MovementPenalty property of the node.
			_grid [x, 0].MovementPenalty = blurredPenalty;

			for (var y = 1; y < _gridSizeY; y++) {
				// Calculate the indices of the nodes to be removed and added in this pass.
				var removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
				var addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY-1);

				penaltiesVerticalPass[x][y] = penaltiesVerticalPass[x][y-1] - penaltiesHorizontalPass[x][removeIndex] + penaltiesHorizontalPass[x][addIndex];
				blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x][y] / (kernelSize * kernelSize));
				_grid [x, y].MovementPenalty = blurredPenalty;

				// Update the max and min penalty values if necessary.
				if (blurredPenalty > _penaltyMax) 
				{
					_penaltyMax = blurredPenalty;
				}
				if (blurredPenalty < _penaltyMin) 
				{
					_penaltyMin = blurredPenalty;
				}
			}
		}
	}

	// This function returns a list of the neighboring GridNodes for a given GridNode
	public List<GridNode> GetNeighbours(GridNode node) 
	{
		// Create an empty list of GridNodes to store the neighbors
		List<GridNode> neighbours = new List<GridNode>();

		// Loop through all the possible neighboring GridNodes
		for (var x = -1; x <= 1; x++) 
		{
			// Skip the current GridNode since it is not a neighbor
			for (var y = -1; y <= 1; y++) 
			{
				if (x == 0 && y == 0)
					continue;
				
				// Get the indices of the potential neighbor GridNode
				var checkX = node.GridX + x;
				var checkY = node.GridY + y;

				// Check if the potential neighbor GridNode is within the bounds of the grid
				if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY) 
				{	
					// Add the neighbor GridNode to the list of neighbors
					neighbours.Add(_grid[checkX,checkY]);
				}
			}
		}
		// Return the list of neighbors
		return neighbours;
	}


	// This function returns the GridNode that corresponds to a given world position
	public GridNode NodeFromWorldPoint(Vector3 worldPosition) 
	{
		// Calculate the percentage of the world position relative to the size of the grid
		var percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		var percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		
		// Clamp the percentages to ensure they are between 0 and 1
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		// Calculate the indices of the GridNode based on the percentages
		var x = Mathf.RoundToInt((_gridSizeX-1) * percentX);
		var y = Mathf.RoundToInt((_gridSizeY-1) * percentY);
		
		// Return the corresponding GridNode
		return _grid[x,y];
	}

	// This function draws gizmos in the scene view to visualize the grid
	private void OnDrawGizmos() 
	{
		// Draw a wire cube around the grid to represent its boundaries
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
		// Check if the grid has been initialized and if grid gizmos should be displayed
		if (_grid == null || !displayGridGizmos) return;
		// Loop through all the GridNodes in the grid and draw gizmos to represent them
		foreach (GridNode n in _grid) 
		{
			// Set the color of the gizmo based on the movement penalty of the GridNode
			Gizmos.color = Color.Lerp (Color.white, Color.black, Mathf.InverseLerp (_penaltyMin, _penaltyMax, n.MovementPenalty));
			// If the GridNode is not walkable, set the color to red
			Gizmos.color = (n.Walkable)?Gizmos.color:Color.red;
			// Draw a cube gizmo to represent the GridNode
			Gizmos.DrawCube(n.WorldPosition, Vector3.one * (_nodeDiameter));
		}
	}

	// This is a serializable class that represents a terrain type, which is used to set movement penalties for different types of terrain
	[System.Serializable]
	public class TerrainType 
	{
		public LayerMask terrainMask;
		public int terrainPenalty;
	}
}