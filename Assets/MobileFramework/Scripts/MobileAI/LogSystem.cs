#region Using Namespace
using UnityEngine;
using System.IO;
using System.Collections;
using MobileAIFramework.Gesture;
#endregion

namespace MobileAIFramework
{
	public class LogSystem : MonoBehaviour
	{
		//Instance of the class
		private static LogSystem s_instance;
		
		//File where the log are stored
		private const string FILE_NAME = "/log.maif";
		
		//Submission webpage
		private const string LOG_ADDRESS = "http://deadmansquestion.com/mobileai/save.php";
		
		//Connection available
		private bool m_isConnected;
		
		// Use this for initialization
		void Start()
		{
			if (s_instance == null)
				s_instance = this;
		
			this.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			DontDestroyOnLoad(this);
			
			//Check for internet connection
			m_isConnected = true;
			//StartCoroutine(CheckConnection());
			
			//If a file with logs exists, submit it online
			
			if (m_isConnected && File.Exists(Application.persistentDataPath + FILE_NAME))
				StartCoroutine(SubmitFile());
			
		}
		
		//Event fired if the component is destroyed
		void OnDestroy()
		{
			//SubmitReport();
		}
		
		//Submit the information of the GestureManager to the database
		static public IEnumerator SubmitLog()
		{
			WWWForm form = new WWWForm();
			form.AddField("data", GestureManager.GetLog());
			
			var link = new WWW(LOG_ADDRESS, form);
			
			yield return link;
		}
		
		//Check if there are files into the folder to submit to the website.
		//If so, it will submit information to the website and delete it.
		private IEnumerator CheckConnection()
		{		
			//Check for conenttivity
			Ping connectivity = new Ping("74.125.224.72"); //Ping www.google.com to test the connectivity
			
			while (!connectivity.isDone){}
				yield return new WaitForSeconds(0.1f);
			
			if (connectivity.time <= 2000f)
				m_isConnected = true;
		}
		
		//Submit the content of the log.maif file to the server.
		public IEnumerator SubmitFile()
		{
			//Read and submit every line in the file
			StreamReader file = new StreamReader(Application.persistentDataPath + FILE_NAME);
			
			WWWForm form = new WWWForm();
			form.AddField("data", file.ReadToEnd());
			
			var link = new WWW(LOG_ADDRESS, form);
			yield return link;
			
			file.Close();
			
			//File.Delete(Application.persistentDataPath + FILE_NAME);
		}
		
		//Save the log into a file.
		public void SaveFile()
		{
			StreamWriter file;
			string log = GestureManager.GetLog();
			
			//If the log is empty skip the serialization
			if (log != "{ \"info\":{} }")
			{
				if (File.Exists(Application.persistentDataPath + FILE_NAME))
					file = new StreamWriter(Application.persistentDataPath + FILE_NAME, true);
				else
					file = new StreamWriter(Application.persistentDataPath + FILE_NAME, false);
				
				file.WriteLine(log);
				file.Close();
				
				Debug.Log("Log File: " + Application.persistentDataPath + FILE_NAME);
			}
		}
		
		//Append the touch log to the file
		public static void AppendData()
		{
			StreamWriter file;
			string log = GestureManager.GetLog();
		
			file = new StreamWriter(Application.persistentDataPath + FILE_NAME, true);
			file.WriteLine(log);
			file.Close();
		}
		
		//Submit the log information to the server
		public static IEnumerator FromFileToDB()
		{
			//Read and submit every line in the file
			StreamReader file = new StreamReader(Application.persistentDataPath + FILE_NAME);
			
			WWWForm form = new WWWForm();
			form.AddField("data", file.ReadToEnd());
			
			var link = new WWW(LOG_ADDRESS, form);
			yield return link;
			
			file.Close();
		}
		
		//Saves the log information of the GestureManager into a file
		//If the application is closing, it will save the information in a file, otherwise it will submit these data to the website
		public void SubmitReport()
		{
			//Debug.Log("eccolo");
			/*
			if (!Application.isLoadingLevel)
			{
				SaveFile();
			}
			else
			{
				StartCoroutine(SubmitLog());
			}
			*/
		}
	}
}