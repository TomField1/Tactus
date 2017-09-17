#region Using Statements
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework.Gesture
{
	public class VirtualButton : VirtualControl
	{
		public event VCClick OnClick;
		
		// Use this for initialization
		void Start()
		{
			m_collider 	= (SphereCollider)collider;
			m_transform = transform;
			
			GestureManager.AddVirtualButton(this);
		}
		
		//Determines if the control has been focused
		void OnMouseDown()
		{
			m_state = VControlState.Pressed;
		}
		
		//It checks whenever the mouse is clicking on the button
		void OnMouseUpAsButton()
		{
			m_state = VControlState.Released;
		
			Debug.Log("Virtual button " + _id + " clicked!");
		
			if (OnClick != null)
				OnClick();
		}
	}
}