#region Using Namespace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework.Navigation.Pathfinding;
#endregion

namespace MobileAIFramework.Navigation
{
	public class NavGrid
	{
		//Number of rows and columns of which the grid is composed
		public int rows { private set; get; }
		public int columns { private set; get; }

		//The grid projected over the terrain and meshes
		private Vector3?[,] m_renderedGrid;

		//The boolean grid to hand the movement
		//private bool[,] m_inputGrid;
		private NavCell[,] m_inputGrid;
		public NavCell[,] InputGrid
		{
			get { return m_inputGrid; }
		}

		//Mask where to raycast to get the grid information
		private const int TERRAIN_LAYERMASK = 31;
		
		//List of all cell shape
		private Dictionary<int, int[]> m_cells;
		public Dictionary<int, int[]> Cells
		{
			get { return m_cells; }
		}
		
		//Grid Vertices information
		private List<Vector3> m_vertices;
		private List<Vector3> m_normals;
		private List<Vector2> m_uv;
		
		//Create an instance of the Base Grid Class
		public NavGrid(int rowCount, int columnsCount)
		{
			rows 		= rowCount;
			columns 	= columnsCount;
			
			//Initialize the list of cells
			m_cells = new Dictionary<int, int[]>();
		}

		//Sets the size of the grid
		public void SetSize(int rowCount, int columnsCount)
		{
			rows 		= rowCount;
			columns 	= columnsCount;
		}

		//Calculates the grid agains a given volume
		public Mesh BuildNavMesh(Vector3 center, Bounds volume, List<GameObject> nonBlocking)
		{
			//Create a temp Mesh instance that stores all the grid mesh information
			Mesh mesh = new Mesh();

			m_vertices 	= new List<Vector3>();
			m_normals 	= new List<Vector3>();
			m_uv 		= new List<Vector2>();
			
			List<int> 	index 		= new List<int>();

			//Initialize the grid with the correct size
			m_renderedGrid = new Vector3?[rows+1, columns+1];
			
			//Initialize the input grid
			m_inputGrid = new NavCell[rows, columns];
			
			float dx = volume.size.x / rows;
			float dz = volume.size.z / columns;
			
			Vector3 rayStart = volume.max;
			Vector3 rayEnd   = volume.min;
			
			//Copy the layer of the element to put them back again
			int[] layers = new int[nonBlocking.Count];
			for (int i = 0; i < nonBlocking.Count; ++i)
			{
				if(nonBlocking[i] != null)
				{
					layers [i] = nonBlocking [i].layer;
					nonBlocking[i].layer = TERRAIN_LAYERMASK;
				}
			}
			
			int cellIndex = 0;
			
			//Raycast every point on the grid to find the intersection with the terrain
			for (ushort i = 0; i <= rows; ++i)
			{
				rayStart.x = volume.min.x + (i * dx);
				rayEnd.x = rayStart.x;
				
				//Raycast the vertices of the grid and the grid information
				for (ushort j = 0; j <= columns; ++j)
				{
					rayStart.z 	= volume.min.z + (j * dz);
					rayEnd.z 	= rayStart.z;
				
					m_renderedGrid[i, j] = rayEnd;
					
					m_vertices.Add(m_renderedGrid[i, j].Value - center);
					m_normals.Add(Vector3.up);
					m_uv.Add(Vector2.zero);
					
					//Check the cell information
					RaycastHit rayHit = default(RaycastHit);
					
					if (i < rows && j < columns)
					{
						Vector3 centerStart = rayStart + new Vector3(dx/2, 0, dz/2);
						Vector3 centerEnd 	= rayEnd + new Vector3(dx/2, 0, dz/2);
					
						bool walkable = !Physics.Linecast(centerStart, centerEnd, out rayHit, ~(1 << TERRAIN_LAYERMASK));
						m_inputGrid[i, j] = new NavCell(cellIndex++, walkable, centerEnd);
					}
				}
			}
			
			//Restore the old layer information
			for (int i = 0; i < nonBlocking.Count; ++i)
			{
				if(nonBlocking [i] != null)
					nonBlocking [i].layer = layers[i];
			}
			//Create the incremental index to set the mesh
			int r = 0;
			int cellIdx = 0;

			//Create the indices for the grid mesh
			for (int i = 0; i < rows; ++i)
			{
				r = i * (columns + 1);

				for (int j = 0; j < columns; ++j)
				{
					int c = r + j;
					
					//Create the list of cells indices associated to the vertex and add it to the native mesh
					m_cells.Add(cellIdx, new int[] {c, c+1, c + columns + 1, c + columns + 1, c + 1, c + columns + 2 });
					
					//Iterate through the indices for every cell and add them to the mesh
					foreach (int idx in m_cells[cellIdx])
					{
						index.Add(idx);
					}
					
					//Increment the cell
					++cellIdx;
				}
			}

			mesh = new Mesh ();
			mesh.vertices 	= m_vertices.ToArray();

			//Creates a submesh, this will be used to draw with different color all the walkable or unwalkable zone
			mesh.SetIndices(index.ToArray(), MeshTopology.Triangles, 0);
			mesh.normals 	= m_normals.ToArray();
			mesh.uv 		= m_uv.ToArray ();

			return mesh;
		}

		//Checks in the grid if the cell is walkable or not.
		public bool IsValidLocation(int triangleIndex)
		{
			int i = (triangleIndex / 2) / columns;
			int j = (triangleIndex / 2) % rows;
			bool result = m_inputGrid [i, j].IsWalkable;

			return result;
		}

		public NavCell GetNavCell(int triangleIndex)
		{
			int i = (triangleIndex / 2) / columns;
			int j = (triangleIndex / 2) % rows;
			return m_inputGrid [i, j];
		}
		
		//Get grid id from coordinates
		public int GetSquareByCoord(int x, int y)
		{
			Debug.Log(y + x * columns);
			return (y + x * columns);
		}
		
		//Calculate the list of nodes from the point A to the point B
		public NavNode PathFinding(int a, int b)
		{
			MobileAI.StateSpaceSearch.SetNavGrid(this);
			return MobileAI.StateSpaceSearch.FindPath(a, b);
		}

		//Create a mesh that represent the pathfinding in the grid
		public Mesh GetPathMesh(NavNode path)
		{
			Mesh mesh = new Mesh();
			List<int> indices = new List<int>();
			
			while (path != null)
			{
				foreach (int vertId in m_cells[path.Id])
					indices.Add(vertId);
					
				path = path.Parent;
			}
			
			mesh.vertices 	= m_vertices.ToArray();
			mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
			mesh.normals 	= m_normals.ToArray ();
			mesh.uv 		= m_uv.ToArray ();
			
			return mesh;
		}

		//Draw the subdivision of the NavMesh
		public void DrawGrid()
		{
			Color hit = new Color(1f, 0f, 0f, 0.2f);

			for (ushort i = 0; i < rows; ++i)
			{
				Debug.DrawLine (m_renderedGrid[i, 0].Value, m_renderedGrid[i, columns].Value, hit);
				
				for (ushort j = 0; j < columns; ++j)
				{
					Debug.DrawLine (m_renderedGrid[0, j].Value, m_renderedGrid[rows, j].Value, hit);
				}
			}
		}
	}
}