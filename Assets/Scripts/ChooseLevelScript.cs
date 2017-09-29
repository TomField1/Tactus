using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseLevelScript : MonoBehaviour {

    /// <summary>
    /// This script manages the ChooseLevelScene
    /// </summary>

    GameFlowScript _flowScript;

	// Use this for initialization
	void Start () {
        _flowScript = FindObjectOfType<GameFlowScript>();
    }
	
	//For each of the buttons, set the correct control scheme as next and load the scene
	public void ClickedPlayTilt()
    {
        _flowScript.controlSchemeToSet = GameManager.ControlScheme.Tilt;
        SceneManager.LoadScene(1);
    }
    public void ClickedPlaySwipe()
    {
        _flowScript.controlSchemeToSet = GameManager.ControlScheme.Swipe;
        SceneManager.LoadScene(1);
    }
    public void ClickedPlayTap()
    {
        _flowScript.controlSchemeToSet = GameManager.ControlScheme.Tap;
        SceneManager.LoadScene(1);
    }

    //if opted to quit, quit
    public void ClickedQuit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit ();
        #endif
    }
}
