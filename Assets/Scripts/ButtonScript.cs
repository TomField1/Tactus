using UnityEngine;
using System;
using System.Collections;

public class ButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	public void ExitGame()
	{
		Application.Quit();
	}

	/// <summary>
	/// Restarts the game.
	/// </summary>
	public void RestartGame()
	{
		GameManager.RunCount = 1;
		GameManager.Score = 0;
		GameManager.TimeValue = 50;
		Destroy(GameObject.Find("MobileAIFramework"));
		Application.LoadLevel(4);
	}

	/// <summary>
	/// Loads the main level.
	/// </summary>
	public void LoadMainLevel()
	{
		GameManager.RunCount = 1;
		GameManager.Score = 0;
		GameManager.TimeValue = 50;
		Destroy(GameObject.Find("MobileAIFramework"));
		Application.LoadLevel(4);
    }

	/// <summary>
	/// Quits to menu.
	/// </summary>
	public void QuitToMenu()
	{
		Destroy(GameObject.Find("MobileAIFramework"));
		Application.LoadLevel(0);
	}

	/// <summary>
	/// Loads the next level
	/// </summary>
	public void NextLevel(int levelID)
	{
		GameManager.TimeValue = 50;
		GameManager.RunCount += 1;
		Destroy(GameObject.Find("MobileAIFramework"));
		Application.LoadLevel(levelID);
	}
}