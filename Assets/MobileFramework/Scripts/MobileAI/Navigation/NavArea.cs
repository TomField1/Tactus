#region Using Namespace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework.Navigation.Pathfinding;
#endregion

namespace MobileAIFramework.Navigation
{
	public class NavArea : MonoBehaviour
	{
		public enum NavColliderType
		{
			Walkable,
			Obstacle
		}

		//The mesh filter used to draw the mesh in the scene
		private MeshFilter m_meshFilter;

		//The NavGrid associated to the mesh
		private NavGrid m_grid;
		
		public Material _material;

		//The touch volume
		public Bounds volume { private set; get; }
		
		//The input mesh
		public Mesh map { private set; get; }

		//NavMesh that rapresents the collision mesh used to calculate if the player can or cannot move
		void Start ()
		{

		}

		//Set the volume of the NavMesh
		public void SetVolume(float width, float height, float depth)
		{
			volume = new Bounds(transform.position, new Vector3(width, height, depth));
		}

		//Create the grid with the input sizes
		public void CreateGrid(int rows, int columns)
		{
			m_grid = new NavGrid(rows, columns);
		}

		//Initializes the NavMesh from a file or at loadtime
		public void Initialize(string navMeshFile, List<GameObject> nonBlocking)
		{
			name = "MobileAIFramework";

			if (m_meshFilter == null)
				m_meshFilter = GetComponent<MeshFilter>();

			//Build the grid obstacles information
			//m_grid.BuildGrid (transform.position, volume, nonBlocking);
			
			//Build the mesh information
			//m_meshFilter.mesh = m_grid.BuildNavMesh(transform.position, volume);
			m_meshFilter.mesh = m_grid.BuildNavMesh(transform.position, volume, nonBlocking);
			GetComponent<MeshCollider>().sharedMesh = m_meshFilter.mesh;

			gameObject.layer = 31;
		}

		public NavCell GetNavCell(int triangleIndex)
		{
			return m_grid.GetNavCell(triangleIndex);
		}
		
		//Checks in the grid if the cell is walkable or not.
		public bool IsValidLocation(int id)
		{
			return m_grid.IsValidLocation(id);
		}
		
		//Pathfinding method.
		//Works as interface between the core and the base grid
		public NavNode PathFinding(int startTriangle, int endTriangle)
		{
			return m_grid.PathFinding(startTriangle, endTriangle);
		}
		
		//Get the mesh associated with the path
		public Mesh GetPathMesh(NavNode path)
		{
			return m_grid.GetPathMesh(path);
		}

		public void RefreshGrid(string navMeshFile, List<GameObject> nonBlocking)
		{
			m_meshFilter.mesh = m_grid.BuildNavMesh(transform.position, volume, nonBlocking);
		}

		//Draw the debug graphics for the Mesh and Grid
		public void DebugDraw()
		{
			m_grid.DrawGrid();
		}

		public NavGrid NavigationGrid
		{
			get {return m_grid;}
		}
	}
}