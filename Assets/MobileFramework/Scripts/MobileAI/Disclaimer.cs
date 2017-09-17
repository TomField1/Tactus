#region Using Statements
using System.IO;
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework
{
	public class Disclaimer
	{
		//Is the diclaimer window shown?
		private bool m_show;
		public bool IsVisible
		{
			get { return m_show; }
			set { m_show = value; }
		}
		
		//TableFlip games or University of Derby logo
		private Texture2D m_logo;
		private string m_disclaimerText;
		
		//URL for the contents
		private const string DISCLAIMER_URL = "http://deadmansquestion.com/mobileai/disclaimer.php";
		private const string LOGO_URL 		= "http://deadmansquestion.com/mobileai/tfg.png";
		
		//Size of the disclaimer window
		private float m_windowWidth;
		private float m_windowHeigth;
		
		private bool m_isConnected;
		
		//Current scroll position
		private Vector2 m_scrollPosition;
		
		//Create an instance of the Disclaimer class
		public Disclaimer()
		{
			m_show = true;
		}
		
		//Draws the Disclaimer window at screen
		public void Show()
		{
			if (!m_show)
				return;
				
			m_isConnected = false;
		
			//Calculate and place the window in the middle of the screen
			m_windowWidth  = (int)(Screen.width * 0.6);
			m_windowHeigth = (int)(Screen.height * 0.6);
			
			Rect position = new Rect((Screen.width - m_windowWidth)/2, (Screen.height - m_windowHeigth)/2, m_windowWidth, m_windowHeigth);
			
			//Draw the window
			GUI.Window(0, position, DrawContent, "Disclaimer");
		}
		
		//Disclaimer window
		void DrawContent(int windowID)
		{
			int buttonWidth  = (int)(m_windowWidth * 0.15);
			int buttonHeight = (int)(m_windowHeigth * 0.1);
			
			Rect textRect = new Rect(10, 80, m_windowWidth - 20, m_windowHeigth - buttonHeight - 85);
			
			m_scrollPosition = GUI.BeginScrollView(textRect, m_scrollPosition, new Rect(0, 0, textRect.width - 20, 600));
			GUI.Label(new Rect(0, 0, 440, 600), m_disclaimerText);
			GUI.EndScrollView();
			
			if (m_logo != null)
				GUI.DrawTexture(new Rect(m_windowWidth/2 - 200, 20, 400, 60), m_logo, ScaleMode.ScaleToFit);
			
			Rect acceptButton  = new Rect(m_windowWidth/2 - buttonWidth - 10, m_windowHeigth - buttonHeight - 10, buttonWidth, buttonHeight);
			Rect declineButton = new Rect(m_windowWidth/2 + 10, m_windowHeigth - buttonHeight - 10, buttonWidth, buttonHeight);
			
			if (GUI.Button(acceptButton, "Accept"))
				m_show = false;
			
			if (GUI.Button(declineButton, "Decline"))
				Application.LoadLevel(-1);
		}
		
		private IEnumerator CheckConnection()
		{		
			//Check for conenttivity
			Ping connectivity = new Ping("74.125.224.72"); //Ping www.google.com to test the connectivity
			
			while (!connectivity.isDone){}
			yield return new WaitForSeconds(0.1f);
			
			if (connectivity.time <= 2000f)
				m_isConnected = true;
		}
		
		//Download the disclaimer and logo from internet
		public IEnumerator DownloadContents()
		{
			//Load the logo.. and update it in case
			LoadResources();
			
			CheckConnection();
			
			if (m_isConnected)
			{
				yield return 1;
				
				WWW www = new WWW(DISCLAIMER_URL);
				yield return www;
				m_disclaimerText = www.text;
				
				www = new WWW(LOGO_URL);
				yield return www;
				m_logo = www.texture;
				
				SaveResources();
			}
		}
		
		//Save the logo into a png file
		private void SaveResources()
		{
			//Saves the logo
			var bytes	= m_logo.EncodeToPNG();
			var file  	= File.Open(Application.persistentDataPath + "/Logo.png", FileMode.Create);
			var binary 	= new BinaryWriter(file);
			binary.Write(bytes);
			file.Close();
			
			//Save the disclaimer
			byte[] text = new byte[m_disclaimerText.Length * sizeof(char)];
			System.Buffer.BlockCopy(m_disclaimerText.ToCharArray(), 0, text, 0, text.Length);
			
			file  	= File.Open(Application.persistentDataPath + "/Disclaimer.txt", FileMode.Create);
			binary 	= new BinaryWriter(file);
			binary.Write(text);
			file.Close();
		}
		
		//Save the logo into a png file
		private void LoadResources()
		{
			//Load the logo
			string path = Application.persistentDataPath + "/Logo.png";
			if (System.IO.File.Exists(path))
			{
				var bytes = System.IO.File.ReadAllBytes(path);
				m_logo = new Texture2D(1, 1);
				m_logo.LoadImage(bytes);
			}
			
			//Load the disclaimer
			path = Application.persistentDataPath + "/Disclaimer.txt";
			if (System.IO.File.Exists(path))
			{				
				var bytes = System.IO.File.ReadAllBytes(path);
				char[] chars = new char[bytes.Length / sizeof(char)];
				System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
				m_disclaimerText = new string(chars);
			}
		}
	}
}