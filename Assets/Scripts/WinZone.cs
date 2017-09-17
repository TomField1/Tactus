using UnityEngine;
using System.Collections;

public class WinZone : MonoBehaviour {

	bool m_isEnabled = false;


	public bool Enabled
	{
		get {return m_isEnabled;}
		set 
		{
			m_isEnabled = value;

			if(value == false)
			{
				renderer.enabled = false;
				collider.enabled = false;
			}
			else
			{
				renderer.enabled = true;
				collider.enabled = true;
			}
		}
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}


}
