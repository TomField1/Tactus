#region Using Statements
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework.Gesture
{
	public class VirtualThumbstick : VirtualControl
	{
		public event VCDirection OnDrag;
		
		// Use this for initialization
		void Start()
		{
			m_collider 	= (SphereCollider)GetComponent<Collider>();
			m_transform = transform;
			
			GestureManager.AddVirtualThumbstick(this);
		}
	
		//Fired when the control is dragged.
		//It is used on the VAnalog or VDPad
		void OnMouseDrag()
		{
			m_state = VControlState.Dragged;
			
			Ray ray = MobileAI.Instance.TargetCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (m_collider.Raycast(ray, out hit,  100000))
			{
				//Get informaion about where the click happened.
				Vector2 click = new Vector2(m_transform.position.x - hit.point.x, m_transform.position.z - hit.point.z);
				click = click.normalized;
				
				if (OnDrag != null)
					OnDrag(click);
			}
		}
	}
}