#region Using Statements
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework.Navigation
{
	public class CameraPathtrace : MonoBehaviour
	{
		public delegate void CameraArgs();
		public CameraArgs OnBeginDebug;
	
		// Use this for initialization
		void Start()
		{
			DontDestroyOnLoad(this);
		}
		
		// Update is called once per frame
		void Update()
		{
		}
		
		//The render process is finished
		void OnPostRender()
		{
			if (OnBeginDebug != null)
				OnBeginDebug();
		}
	}
}