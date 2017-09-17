#region Using Namespace
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework.Gesture;
using MobileAIFramework.Navigation;
using MobileAIFramework.Navigation.Pathfinding;
#endregion

namespace MobileAIFramework
{
	public class MobileAI : MonoBehaviour
	{
		//Debug event declaration
		public delegate void MAIArgs();
		public static MAIArgs OnDebug;
	
		//Static instance of the Core
		private static MobileAI s_instance;
		public static MobileAI Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new MobileAI();
					s_instance.Start();
				}
			
				return s_instance;
			}
		}
		
		//Determine if the grid and the map area have to be drawn
		[SerializeField][HideInInspector] private bool m_drawGrid = false;
			
		//Debug the area volume
		[SerializeField][HideInInspector] private bool m_drawMapArea = false;
		
		//Size of the volume map
		[SerializeField][HideInInspector] private Vector3 m_volumeSize = new Vector3(32, 32, 32);
		public Vector3 VolumeSize
		{
			get { return m_volumeSize; }
		}
		
		//Grid division
		[SerializeField][HideInInspector] private Vector2 m_gridSize = new Vector2(32, 32);

		//List of non blocking objects
		[SerializeField] public List<GameObject> m_nonBlockingObjects;
		
		//Camera where the touch needs to be calculated
		[SerializeField] [HideInInspector] private Camera m_targetCamera;
		public Camera TargetCamera
		{
			get { return m_targetCamera; }
		}
		
		//TrailPrefab used by the GestureManager
		[SerializeField][HideInInspector] private GameObject m_trailDebug;
		
		[SerializeField][HideInInspector] private bool m_drawTrail;
		public bool DrawTrail
		{
			get { return m_drawTrail; }
			
			set 
			{
				m_drawTrail = value;
				
				if (m_gestureManager != null)
					m_gestureManager.DebugTrail = m_drawTrail;
			}
		}
		
		[SerializeField][HideInInspector] private StateSpaceSearch m_algorithm;
		public static StateSpaceSearch StateSpaceSearch
		{
			get { return MobileAI.s_instance.m_algorithm; }
			set { MobileAI.s_instance.m_algorithm = value; }
		}

		bool m_touchEnabled = false;
		public bool TouchEnabled
		{
			get { return m_touchEnabled; }
			set { m_touchEnabled = value; }
		}

		//Touch Raycast information
		//public bool _dynamicObstacles = true;
		
		//The layer that contains obstacles
		//public int[] _obstacleLayerMask;
		
		//The NavMesh
		private NavArea m_navMesh;

		//The computations of the layermasks to ignore as obstacles
		private const int TERRAIN_LAYERMASK = 31;

		//The length of the ray for the raycast
		private const float RAY_LENGTH = 100;
		
		//Gesture Manager
		private GestureManager m_gestureManager;
		
		//Handles the communication with the server to store all the information for the log system
		private LogSystem m_logSystem;
		
		//Reference to the camera
		private CameraPathtrace m_camera;
		
		//Show the disclaimer for the data collection
		//private Disclaimer m_disclaimer;
		//public bool Disclaimer
		//{
		//	get { return m_disclaimer.IsVisible; }
		//}

		//Initialize the Touch framework engine.
		//The influence grid mey be camculated at execution or imported as file
		public void Awake()
		{
			s_instance = this;

			if (m_gridSize.x < 0)
				throw new UnityException("The number of rows for the grid mst be greater than zero");

			if (m_gridSize.y < 0)
				throw new UnityException("The number of columns for the grid mst be greater than zero");

			m_navMesh = (NavArea)(transform.GetComponent(typeof(NavArea)));
			
			//Set the volume of the Map
			if (m_volumeSize.x <= 0 || m_volumeSize.y  <= 0 || m_volumeSize.z <= 0)
				throw new UnityException("The volume size must be a positive value");
			
			m_navMesh.SetVolume (m_volumeSize.x, m_volumeSize.y, m_volumeSize.z);

			//Create the grid
			m_navMesh.CreateGrid((int)m_gridSize.x, (int)m_gridSize.y);

			//Initialize the NavMesh with a file
			m_navMesh.Initialize (string.Empty, m_nonBlockingObjects);
			
			//Assign the camera
			if (m_targetCamera == null)
				m_targetCamera = Camera.main;
			
			//Debug Camera
			m_targetCamera.gameObject.AddComponent("CameraPathtrace");
			m_camera = m_targetCamera.gameObject.GetComponent("CameraPathtrace") as CameraPathtrace;
			
			//Hooking the OnBeginDebug event
			m_camera.OnBeginDebug -= OnBeginDebug;
			m_camera.OnBeginDebug += OnBeginDebug;

			m_gestureManager = new GestureManager();
			m_gestureManager.DebugTrail = m_drawTrail;
			
			//Create the disclaimer window
			//m_disclaimer = new Disclaimer();
			//m_disclaimer.IsVisible = true;
			//StartCoroutine(m_disclaimer.DownloadContents());

			//Creates the log system
			m_logSystem = gameObject.AddComponent<LogSystem>();
			m_logSystem.transform.parent = null;
			
			if (m_trailDebug != null)
				m_gestureManager.SetTrailPrefab(m_trailDebug);
				
			m_algorithm = new AStar();
		}
		
		void Start()
		{
			//Awake();
		}
		
		//Before destroying the component saves all the information about the touch
		void OnDestroy()
		{
			//TODO Check for connection. If the connection is not available save the touch information to a file
			//If the component is destroyed again send the files to the server and the current session.
			
			//TODO UNCOMMENT THIS AFTER THE TEST
			//m_logSystem.SubmitReport();
		}

		//Update the Core logic to handle the touches
		void Update()
		{
			//if (m_disclaimer.IsVisible)
				//return;
			
			//Update the gesture manager
			m_gestureManager.Update();
		}

		/// <summary>
		/// Gets the triangle ID (Not the cell ID) of the cell at the given position.
		/// Useful for getting the cell in future with the triangle ID.
		/// </summary>
		public static int GetTriangleID(Vector3 mousePosition)
		{
			if(Instance.TargetCamera)
			{
				Ray ray = new Ray(Instance.TargetCamera.ScreenToWorldPoint(mousePosition), Vector3.down);
				RaycastHit hit = default(RaycastHit);
				
				if (Physics.Raycast(ray, out hit, RAY_LENGTH, 1 << TERRAIN_LAYERMASK))
				{
					if (hit.collider.name == MobileAI.Instance.m_navMesh.name)
					{
						return hit.triangleIndex;
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Gets the cell identifier.
		/// </summary>
		public static int GetCellId(Vector3 position)
		{
			Ray ray = new Ray(position, Vector3.down);
			
			RaycastHit startHit = default(RaycastHit);
			
			if (Physics.Raycast(ray, out startHit, RAY_LENGTH, 1 << TERRAIN_LAYERMASK))
			{
				if (startHit.collider.name == MobileAI.Instance.m_navMesh.name)
				{
					return startHit.triangleIndex >> 1;
				}
			}
			
			return -1;
		}

		/// <summary>
		/// Gets the cell using its triangle ID
		/// </summary>
		public static NavCell GetCell(int id)
		{
			return s_instance.m_navMesh.GetNavCell(id);
		}
		
		/// <summary>
		/// Determines if the cell is valid at the specified id.
		/// </summary>
		public static bool IsValidLocation(int id)
		{
			return s_instance.m_navMesh.IsValidLocation(id);
		}

		/// <summary>
		/// Get the shortest path from the starting position to the ending position.
		/// Both start and end position need to be on the NavMesh.
		/// </summary>
		public static NavNode Pathfind(Vector3 startPosition, Vector3 endPosition)
		{
			Ray ray = new Ray(startPosition, Vector3.down);
		
			RaycastHit startHit = default(RaycastHit);
			RaycastHit endHit = default(RaycastHit);
		
			if (Physics.Raycast(ray, out startHit, RAY_LENGTH, 1 << TERRAIN_LAYERMASK))
			{
				if (startHit.collider.name == MobileAI.Instance.m_navMesh.name)
				{
					ray = new Ray(endPosition, Vector3.down);
					
					if (Physics.Raycast(ray, out endHit, RAY_LENGTH, 1 << TERRAIN_LAYERMASK))
					{
						return MobileAI.Instance.m_navMesh.PathFinding(startHit.triangleIndex, endHit.triangleIndex);
					}
					else
					{
						throw new UnityException("The end position should be on the NavMesh");
					}
				}
				else
				{
					throw new UnityException("The start position should be on the NavMesh");
				}
			}
			
			return null;
		}

		/// <summary>
		/// Calculate the pathfinding for the gesture
		/// </summary>
		public static NavNode CalculatePath(Vector3 from, Gesture.Gesture gesture)
		{
			if(EventSystem.current.IsPointerOverGameObject(-1) || EventSystem.current.IsPointerOverGameObject(0))
			{
				return null;
			}

			Ray ray = new Ray(from, Vector3.down);
			RaycastHit startHit = default(RaycastHit);
		
			if (Physics.Raycast(ray, out startHit, RAY_LENGTH, 1 << TERRAIN_LAYERMASK))
			{
				//Gets the player position on the grid
				if (startHit.collider.name == MobileAI.Instance.m_navMesh.name)
					gesture.cellsHitten.Insert(0, startHit.triangleIndex);
				else
					return null;
			}

			//Store the last node path
			NavNode path = MobileAI.Instance.m_navMesh.PathFinding(gesture.cellsHitten[0], gesture.cellsHitten[1] << 1);
			
			for (int index = 1; index < gesture.cellsHitten.Count - 1;)
			{
				NavNode tmp = null;
				int tmpIndex = index;
			
				//Since the player may have clicked outside the bound area or over an object,
				//this code assured a path between the last valid location and the final location
				while (tmp == null)
				{
					if (++tmpIndex >= gesture.cellsHitten.Count)
						break;
				
					tmp = MobileAI.Instance.m_navMesh.PathFinding(gesture.cellsHitten[index]<< 1, gesture.cellsHitten[tmpIndex] << 1);
				}
				
				//If the algorithm found a possible path, then concatenate it
				if (tmp != null)
					path = NavNode.Concatenate(path, tmp);

				//Skip all the invalid index.
				index = tmpIndex;
			}
			
			return path;
		}

		/// <summary>
		/// Get the shortest path from the starting position to the mouse position.
		/// Both start and end position need to be on the NavMesh.
		/// DEPRECATED
		/// </summary>
		public static NavNode PathToMousePosition(Vector3 startPosition)
		{
			return MobileAI.Pathfind(startPosition, Instance.camera.ScreenToWorldPoint(Input.mousePosition));
		}

		/// <summary>
		/// Get the mesh associated to the path. This method can be only called inside the OnBeginDebug method.
		/// </summary>
		public static Mesh GetPathMesh(NavNode path)
		{
			return MobileAI.Instance.m_navMesh.GetPathMesh(path);
		}

		/// <summary>
		/// Draw the pathfinding. This method can be only called inside the OnBeginDebug method.
		/// </summary>
		public static void DrawPath(NavNode path)
		{
			Graphics.DrawMeshNow(MobileAI.Instance.m_navMesh.GetPathMesh(path), s_instance.transform.position, s_instance.transform.rotation);
		}
		
		//Handles all the invalid gesture
		private void InvalidGestureHandling()
		{
		}
		
		//Postdraw from the camera
		private void OnBeginDebug()
		{
			if (MobileAI.OnDebug != null)
				MobileAI.OnDebug();
		}

		/// <summary>
		/// Render the raycast from the screen to the grid
		/// </summary>
		public void OnDrawGizmos()
		{
			//Draw the map volume on the editor
			if (m_drawMapArea)
			{
				Gizmos.color = new Color (1f, 0f, 0f, .1f);
				Gizmos.DrawCube (transform.position, new Vector3 (m_volumeSize.x, m_volumeSize.y, m_volumeSize.z));
			}
			
			if (m_navMesh == null)
				return;
			
			if (m_drawGrid)
				m_navMesh.DebugDraw();
		}
		
		//Shows the disclaimer
		void OnGUI()
		{
			//m_disclaimer.Show();
		}
		/// <summary>
		/// Gets the nav mesh.
		/// </summary>
		/// <value>The nav mesh.</value>
		public NavArea NavMesh
		{
			get {return m_navMesh;}
		}

		/// <summary>
		/// Gets the size of the grid.
		/// </summary>
		/// <returns>The grid size.</returns>
		public Vector2 GetGridSize()
		{
			return m_gridSize;
		}

		/// <summary>
		/// Refreshs the grid.
		/// DEPRECATED
		/// </summary>
		public void RefreshGrid()
		{
			m_navMesh.RefreshGrid(string.Empty, m_nonBlockingObjects);
		}
	}
}