using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework;
using MobileAIFramework.Navigation;

public class GameManager : MonoBehaviour {

	public static int Score = 0; //the player's score
	public static float TimeValue = 60; //the time for a single round
    public static float TimeDebug = 10; //the time for a single round if in debug mode

    GameFlowScript _flowScript;
    TrackingManager _trackingManager;

	[SerializeField] PlayerScriptSimple _player;

    //various text fields on the UI
	[SerializeField] Text _score;
	[SerializeField] Text _health;
    [SerializeField]
    Text _tutorialText;

    //has the game ended?
    bool _gameOver = false;
    
    //the enum for the possible control schemes, and the current one we're using
    public enum ControlScheme { Tap, Swipe, Tilt, }
    public ControlScheme controls;

    //are we in debug mode?
    public bool _debugMode;

    //the UI panel that pops up at the end of the game
    public GameObject _endUIPanel;

    public bool _focusMode = false; //are we in focus mode?
    public float _timeLeft = TimeValue; //how much time is left?

    // Use this for initialization
    void Start()
    {
        //set basics
        _flowScript = FindObjectOfType<GameFlowScript>();
        _trackingManager = FindObjectOfType<TrackingManager>();
        _timeLeft = TimeDebug;

        //if not in debug mode, use non-debug values
        if (!_debugMode)
        {
            controls = _flowScript.controlSchemeToSet;
            _timeLeft = TimeValue;
        }

        //freeze the game until the player clicks to start it
        Time.timeScale = 0;

        //prevent the screen from timing out or rotating
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Portrait;

        //get the camera, and zoom it out so the player can see everything
        Camera mainCam = FindObjectOfType<Camera>();
        while(mainCam.WorldToScreenPoint(new Vector3(24,40,24)).x > Screen.width)
        {
            mainCam.orthographicSize = mainCam.orthographicSize + 0.5f;
        }

        SetTutorialText();
    }
    /// <summary>
    /// Set the tutorial text for the game
    /// </summary>
    void SetTutorialText()
    {
        //if this is the first time playing, create a string for the number of times played
        int p = _flowScript.playCount + 1;

        string s;

        if (!_flowScript.firstTime)
        {
            s = null;
        }
        else
        {
            s = " (" + p + "/3)";
        }
        //depending on what the control scheme is, show the correct tutorial text
        switch (controls)
        {
            case GameManager.ControlScheme.Swipe:
                _tutorialText.text = "<b>Swipe" + s + "</b>\n" +
                    "\n" +
                    "Swipe the screen to change the character's movement direction.";
                FindObjectOfType<SwipeManager>().StartCheckingSwipes(); //also if it's swipes, start listening for swipes
                break;
            case GameManager.ControlScheme.Tap:
                _tutorialText.text = "<b>Tap" + s + "</b>\n" +
                     "\n" +
                     "Tap the screen at any point: the character will navigate to that point.";
                break;
            case GameManager.ControlScheme.Tilt:
                _tutorialText.text = "<b>Tilt" + s + "</b>\n" +
                    "\n" +
                    "Tilt the screen to change the character's movement direction.";
                break;

            default:
                break;
        }
    }
    public void StartGame()
    {
        //unpause the game
        Time.timeScale = 1;
        _trackingManager.AddEvent(new EventStart(controls)); //add the "start" event
    }
	
	// Update is called once per frame
	void Update() 
	{

        //update the UI
		_score.text = "Score: " + Score;
        _health.text = "Health: " + _player._health;

        //change the time left
        _timeLeft = _timeLeft - Time.deltaTime;

        //if the player runs out of time, game over
		if(_timeLeft <= 0.0f && !_gameOver)
		{
            GameOver();
		}
	}

    /// <summary>
    /// happens when the game finishes. shows the End Game panel and adds the Game End event
    /// </summary>
    public void GameOver()
    {
        //add the events for how the player lost
        if(_player._health <= 0)
        {
            _endUIPanel.GetComponentInChildren<Text>().text = "You died!\n \n Score: " + Score;
            _trackingManager.AddEvent(new EventDied(Time.fixedTime, this.gameObject.transform.position, Score));
        }
        else
        {
            _endUIPanel.GetComponentInChildren<Text>().text = "Score: " + Score;
            _trackingManager.AddEvent(new EventTimeEnd(Time.fixedTime, this.gameObject.transform.position, Score, _player._health));
        }
        _trackingManager.AddEvent(new EventEnd());

        //note that the game is over and stop the player moving
        _gameOver = true;
        _player.gameObject.SetActive(false);

        //set up the UI
        _endUIPanel.SetActive(true);
    }

    /// <summary>
    /// Ends the game. if in debug mode writes the XML file, if not gets the next level. used after checking should load next level not upload
    /// </summary>
    public void EndGameMoveOn()
    {
        print("end game clicked");
        //end of the round, player has clicked continue
        if (!_debugMode)
        {
            _flowScript.GetNextLevel();
        }
        else
        {
            //if in debug mode write the event list file
            _trackingManager.XMLWriteEventList(controls);
        }
    }

    /// <summary>
    /// Writes the XML file from the tracking manager. TODO: make it upload it, make it return an error, make it wipe the events list.
    /// </summary>
    public void WriteAndUploadFile(Text errorText)
    {
        _trackingManager.XMLWriteEventList(controls);
        bool b = _trackingManager.Upload();

        if (b)
        {
            //if upload works
            _trackingManager.ClearList();
            EndGameMoveOn();
        }
        else
        {
            //if upload fails
            //set the string and let players try again
            errorText.text = "Something went wrong with the upload: check your network connection and try again.";
        }
    }

    /// <summary>
    /// wipes the event list and continues without uploading
    /// </summary>
    public void ContinueSkipUpload()
    {
        _trackingManager.ClearList();
        EndGameMoveOn();
    }

    public void SetPlayerResponseScores(int control, int challenge, int fun)
    {
        _trackingManager.AddEvent(new EventResponseScore(control, challenge, fun));
    }

    //set the game score
    public void SetScore(int s)
    {
        Score = s;
    }

    //set whether or not the player is in focus mode
    public void SetFocusMode(bool focus)
    {
        _focusMode = focus;
    }
}