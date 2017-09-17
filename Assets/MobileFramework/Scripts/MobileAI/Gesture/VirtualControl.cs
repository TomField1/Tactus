#region Using Statements
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework.Gesture
{
	public class VirtualControl : MonoBehaviour
	{
		public enum VControlState
		{
			Pressed,
			Dragged,
			Released
		}
	
		//Store a reference to the transfork for the MonoBehaviour
		protected Transform m_transform;
		
		//The current state of the control
		protected VControlState m_state;
		public VControlState State
		{
			get { return m_state; }
		}
		
		//The id of the component
		public int _id;
		
		//Collisionbox to detect touches information
		protected SphereCollider m_collider;
		
		//Event handler and delegate for the Touch event
		public delegate void VCClick();
		public delegate void VCDirection(Vector2 thumbstick);
	
		// Use this for initialization
		void Start()
		{			
			tag = "VirtualControls";
		}
	}
}