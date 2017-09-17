#region Using Statements
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace MobileAIFramework.Gesture
{
	//Gesture type enum
	public enum GestureType
	{
		Tap,
		Hold,
		Released,
		Drag,
		Swipe
	}

	//Gesture information structure.

	/// <summary>
	/// Contains the information about the player's touch, such as the position, deltaposition etc
	/// </summary>
	public class Gesture
	{
		public int 	fingerId;		//Id of the finger
		
		public char valid;
		public Vector3 	position;		//Position of the touch
		public Vector3  deltaPosition;	//Difference of position from the starting touch position
		public float 	deltaTime;		//Total time of the gesture
		public float	gameTime;
		public GestureType phase;		//Additional inforamtion about the gesture
		public string details;
		public List<int> cellsHitten;	//List of all point of the cells hitten by the gesture
		
		public bool ended;				//Identifies when tha touch ends
	}


	/// <summary>
	/// Manages all of the gestures currently active, and passes their information to the log
	/// </summary>
	public class GestureManager
	{
		//GestureManager Instance
		private static GestureManager s_instance;
		public static GestureManager Instance
		{
			get
			{
				if (s_instance == null)
					s_instance = new GestureManager();
					
				return s_instance;
			}
			set
			{
				s_instance = value;

			}
		}
	
		//Delegate for gesture events
		//public delegate void GestureHandler(GestureInfo info);
		public delegate void GestureHandler(Gesture info);
		/// <summary>
		/// Event rise when the display is touched once.
		/// </summary>
		public static event GestureHandler OnTap;

		/// <summary>
		/// Event rise when the the finger is holding.
		/// </summary>
		public static event GestureHandler OnHold;

		/// <summary>
		/// Event rise when the finger is dragging.
		/// </summary>
		public static event GestureHandler OnDrag;

		/// <summary>
		/// Event triggered when a hold event ends.
		/// </summary>
		public static event GestureHandler OnRelease;

		/// <summary>
		/// Event that fires when at the end of a fast drag.
		/// </summary>
		public static event GestureHandler OnSwipe;

		//Event that fires when there are two fast drags towards each other
		//public static event GestureHandler OnPinch;
		
		//The input had no methods linked to it
		public static event GestureHandler OnInvalidGesture;
		public static event GestureHandler OnValidGesture;
		
		//private bool m_isHolding;
		private GestureType m_gestureType;
		
		//Consider the click as hold after ... seconds
		private const float CONSIDER_HOLD_AFTER  = 0.3f;
		private const float SWIPE_TIME_THRESHOLD = 1f;
		private const float DRAG_THRESHOLD 		 = 2f;
		
		//List of gesture to store all the touches information
		private List<Gesture> m_gestures;
		private List<Gesture> m_log;
		
		//Debug Features
		private GameObject m_trailPrefab;
		private Dictionary<int, GameObject> m_trails;
		
		//List of the virtual controls
		private Dictionary<int, VirtualButton> m_virtualButtons;
		private Dictionary<int, VirtualThumbstick> m_virtualThumbsticks;
		
		//
		private string m_enqueuedLogInfo;

		/// <summary>
		/// Gets or sets the enqueued log info.
		/// </summary>
		/// <value>The enqueued log info.</value>
		public string EnqueuedLogInfo
		{
			get { return m_enqueuedLogInfo; }
			set { m_enqueuedLogInfo = value; }
		}
		
		//are the trails for the drag to be drawn?
		private bool m_debugTrail;

		/// <summary>
		/// Gets or sets a value indicating whether the debug trail is rendered.
		/// </summary>
		public bool DebugTrail 
		{
			get { return m_debugTrail; }
			set { m_debugTrail = value; }
		}

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy()
		{

		}

		/// <summary>
		/// Create an instance of the Gesture Manager. This manager broadcasts message to the MobileAI and objects that subscribe the events.
		/// </summary>
		public GestureManager()
		{
			//m_gestures.Clear();
			//Initialize the instance of the class
			s_instance = this;
		
			//Initializes the gesture list
			m_gestures = new List<Gesture>();
			
			//Create the log list
			m_log = new List<Gesture>();
			
			//Trails used to debug the finger movent over a drag;
			m_trails = new Dictionary<int, GameObject>();
			
			OnInvalidGesture -= InvalidGesture;
			OnInvalidGesture += InvalidGesture;
			
			OnValidGesture -= ValidGesture;
			OnValidGesture += ValidGesture;
			
			m_virtualButtons 	 = new Dictionary<int, VirtualButton>();
			m_virtualThumbsticks = new Dictionary<int, VirtualThumbstick>();

			m_gestures.Clear();

		}

		/// <summary>
		/// Add a virtual button to the list of virtual buttons
		/// </summary>
		/// <param name="button">Button.</param>
		public static void AddVirtualButton(VirtualButton button)
		{
			if (s_instance.m_virtualButtons.ContainsKey(button._id))
			{
				throw new UnityException("Another virtual button with the same ID already exists.");
			}
			else
			{
				s_instance.m_virtualButtons.Add(button._id, button);
			}
		}

		/// <summary>
		/// Add a virtual thumbstick to the list of virtual thumbsticks
		/// </summary>
		/// <param name="thumbstick">Thumbstick.</param>
		public static void AddVirtualThumbstick(VirtualThumbstick thumbstick)
		{
			if (s_instance.m_virtualThumbsticks.ContainsKey(thumbstick._id))
			{
				throw new UnityException("Another virtual thumbstick with the same ID already exists.");
			}
			else
			{
				s_instance.m_virtualThumbsticks.Add(thumbstick._id, thumbstick);
			}
		}

		/// <summary>
		/// Get a Virtual Button by ID
		/// </summary>
		public static VirtualButton GetVirtualButton(int id)
		{
			if (!s_instance.m_virtualButtons.ContainsKey(id))
				throw new UnityException("The virtual button with the id does not exist");
				
			return s_instance.m_virtualButtons[id];
		}

		/// <summary>
		/// Get a Virtual Thumbstick by ID
		/// </summary>
		public static VirtualThumbstick GetVirtualThumbstick(int id)
		{
			if (!s_instance.m_virtualThumbsticks.ContainsKey(id))
				throw new UnityException("The virtual thumbstick with the id does not exist");
				
			return s_instance.m_virtualThumbsticks[id];
		}

		/// <summary>
		/// Sets the prefab used to debug the trails
		/// </summary>
		public void SetTrailPrefab(GameObject trailPrefab)
		{
			m_trailPrefab = trailPrefab;
		}

		/// <summary>
		/// Create a prefab used for the trail
		/// </summary>
		private void PrefabTrail()
		{
			m_trailPrefab = new GameObject();
			TrailRenderer tRenderer = (TrailRenderer)m_trailPrefab.AddComponent<TrailRenderer>();
			tRenderer.time = .2f;
			tRenderer.startWidth = .5f;
			tRenderer.endWidth 	 = .2f;
		}

		/// <summary>
		/// Updates the Gesture Manager
		/// </summary>
		public void Update()
		{
			//Get the information about the touches happening on the screen
			if (Input.touchCount > 0)
			{
				foreach (Touch touch in Input.touches)
				{
					Gesture gesture;
					
					//If it is within the list count, get the positional gesture
					if (touch.fingerId < m_gestures.Count)
					{
						gesture = m_gestures[touch.fingerId];
					}
					else
					{
						gesture	= new Gesture();
						gesture.cellsHitten = new List<int>();
					}
					
					//Set the gesture parameters
					gesture.fingerId = touch.fingerId;
					gesture.position = touch.position;
					gesture.deltaPosition 	+= new Vector3(touch.deltaPosition.x, touch.deltaPosition.y);
					gesture.deltaTime 		+= Time.deltaTime;
					gesture.gameTime 		= Time.time;
					gesture.details 		= string.Empty;
					gesture.ended = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
					
					if (touch.fingerId < m_gestures.Count)
						m_gestures[touch.fingerId] = gesture;
					else
						m_gestures.Add(gesture);
				}
			}
			else
			{
				Gesture gesture;
			
				if (m_gestures.Count == 0 && Input.GetMouseButtonDown(0))
				{
					gesture = new Gesture();
					gesture.fingerId = 0;
					gesture.phase 	 = GestureType.Tap;
					gesture.position = Input.mousePosition;
					gesture.deltaPosition 	= Vector3.zero;
					gesture.cellsHitten 	= new List<int>();
					gesture.deltaTime 		= 0;
					gesture.gameTime 		= Time.time;
					gesture.details 		= string.Empty;
					gesture.ended = false;
					
					m_gestures.Add(gesture);
				}
				else if (m_gestures.Count == 1)
				{
					gesture = m_gestures[0];
					gesture.deltaPosition = Input.mousePosition - gesture.position;
					gesture.position 	  = Input.mousePosition;
					gesture.deltaTime 	  += Time.deltaTime;
					
					if (Input.GetMouseButtonUp(0))
						gesture.ended = true;
						
					m_gestures[0] = gesture;
				}
			}

			//Updates the gestures information and rise the events
			for (int i = 0; i < m_gestures.Count; ++i)
			{
				Gesture currentGesture = m_gestures[i];
				
				if (currentGesture.phase == GestureType.Released)
					continue;
				
				//Check if the gesture is in a cell. If so, add the cell to the list only if the cell is not the current one.
				int triangleId = MobileAI.GetTriangleID(currentGesture.position) >> 1;
				
				if (triangleId != -1)
				{
					if (currentGesture.cellsHitten.Count == 0)
					{
						currentGesture.cellsHitten.Add(triangleId);
					}
					else
					{
						if (triangleId != currentGesture.cellsHitten[currentGesture.cellsHitten.Count - 1])
						{
							currentGesture.cellsHitten.Add(triangleId);
						}
					}
				}
				
				//Check if the drag or hold event has to be fired
				if (currentGesture.phase != GestureType.Drag)
				{
					//Consider the finger position
					if (currentGesture.deltaPosition.magnitude >= DRAG_THRESHOLD)
					{
						currentGesture.phase = GestureType.Drag;
						
						if (OnDrag != null)
						{
							OnDrag(currentGesture);
						}
						else if (OnInvalidGesture != null)
						{
							OnInvalidGesture(currentGesture);
						}
						
						m_log.Add(currentGesture);
						
						//Add the trail for the finger
						if (m_debugTrail)
						{
							if (m_trailPrefab == null)
								PrefabTrail();
						
							GameObject trail = (GameObject)GameObject.Instantiate(m_trailPrefab, currentGesture.position, Quaternion.identity);
							m_trails.Add(currentGesture.fingerId, trail);
						}
						
						Debug.Log("Drag");
					}
					else if (currentGesture.deltaTime >= CONSIDER_HOLD_AFTER && currentGesture.phase != GestureType.Hold)
					{
						currentGesture.phase = GestureType.Hold;
						
						if (OnHold != null)
						{
							OnHold(currentGesture);
							
							if (OnValidGesture != null)
								OnValidGesture(currentGesture);
						}
						else if (OnInvalidGesture != null)
						{
							OnInvalidGesture(currentGesture);
						}
						
						m_log.Add(currentGesture);
						
						Debug.Log("Hold");
					}
				}
				else if (m_debugTrail)
				{
					//m_trailRenderer.transform.position = currentGesture.position;
					//Update the trial for the dragged finger
					if(MobileAI.Instance.TargetCamera)
					{
						Vector3 trailPosition = MobileAI.Instance.TargetCamera.ScreenToWorldPoint(currentGesture.position);//new Vector3(currentGesture.position.x, 0, currentGesture.position.y));
						trailPosition.y -= 10;
						
						if (m_trails.ContainsKey(currentGesture.fingerId) != false)
						m_trails[currentGesture.fingerId].transform.position = trailPosition;
					}
				}
			
				//The input state change
				if (currentGesture.ended)
				{
					if (currentGesture.phase == GestureType.Tap)
					{
						if (OnTap != null)
						{
							OnTap(currentGesture);
							
							if (OnValidGesture != null)
								OnValidGesture(currentGesture);
						}
						else if (OnInvalidGesture != null)
						{
							OnInvalidGesture(currentGesture);
						}
						
						m_log.Add(currentGesture);

						Debug.Log("Click");
					}
					else
					{
						if (currentGesture.phase == GestureType.Drag && currentGesture.deltaTime < SWIPE_TIME_THRESHOLD)
						{
							if (OnSwipe != null)
							{
								OnSwipe(currentGesture);
								
								if (OnValidGesture != null)
									OnValidGesture(currentGesture);
							}
							else if (OnInvalidGesture != null)
							{
								OnInvalidGesture(currentGesture);
							}
							
							m_log.Add(currentGesture);
								
							Debug.Log("Swipe");
						}
						else
						{
							if(currentGesture.phase != GestureType.Hold)
							{
								if (OnRelease != null)
								{
									OnRelease(currentGesture);
									
									if (OnValidGesture != null)
										OnValidGesture(currentGesture);
								}
								else if (OnInvalidGesture != null)
								{
									OnInvalidGesture(currentGesture);
								}
								
								m_log.Add(currentGesture);
								
								Debug.Log("Release");
							}						
						}
					}
					
					//Remove the current gesture and continue
					//currentGesture.phase = GestureType.Released;
					currentGesture.ended = true;
				}
				
				m_gestures[i] = currentGesture;
			}
			
			//Iterate the gesture list backwards to eliminate the resolved touch information
			for (int i = m_gestures.Count - 1; i >= 0; --i)
			{
				if (m_gestures[i].ended)
				{
					m_gestures.RemoveAt(i);
					
					if (m_trails.ContainsKey(i) && m_debugTrail)
					{
						GameObject.Destroy(m_trails[i], 1);
						m_trails.Remove(i);
					}
				}
				else
				{
					break;
				}
			}
		}

		/// <summary>
		/// Get the touch information of a finger on the screen 
		/// </summary>
		public static Gesture GetTouch(int fingerId)
		{
			return Instance.m_gestures[fingerId];
		}

		/// <summary>
		/// Get the number of touches on the screen
		/// </summary>
		public static int GetTouchCount()
		{
			return Instance.m_gestures.Count;
		}

		/// <summary>
		/// Method called whenever a registered input happens
		/// </summary>
		public void ValidGesture(Gesture info)
		{
			info.valid = 'y';
		}

		/// <summary>
		/// Method called whenever an unregistered input happens
		/// </summary>
		public void InvalidGesture(Gesture info)
		{
			info.valid = 'n';
		}

		/// <summary>
		/// Get the JSON of the log list
		/// </summary>
		public static string GetLog()
		{
			s_instance.m_trails.Clear();
			s_instance.m_gestures.Clear();
		
			string json = "{ \"header\": " + s_instance.m_enqueuedLogInfo + " , ";
			
			json += "\"info\":[";
		
			foreach (Gesture cg in s_instance.m_log)
			{
				json += " {\"fingerId\" : \"" + cg.fingerId.ToString() + "\", ";
				json += "\"valid\" : \"" 	+ cg.valid.ToString() + "\", ";
				json += "\"position\" : \"" + cg.position.x.ToString() + "," + cg.position.y.ToString() + "," + cg.position.z.ToString() + "\", ";
				json += "\"deltaPosition\" : \"" + cg.deltaPosition.x.ToString() + "," + cg.deltaPosition.y.ToString() + "," + cg.deltaPosition.z.ToString() + "\", ";
				json += "\"deltaTime\" : \"" + cg.deltaTime.ToString() + "\", ";
				json += "\"gameTime\" : \"" + cg.gameTime.ToString() + "\", ";
				json += "\"GestureType\" : \"" + cg.phase.ToString() + "\", ";
				json += "\"details\" : " + cg.details + " },";
			}
			
			json = json.Remove(json.Length - 1);
			json += "]}";
			
			return json;
		}
	}

}