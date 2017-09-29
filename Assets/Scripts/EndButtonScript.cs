using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndButtonScript : MonoBehaviour {

    /// <summary>
    /// This script controls the buttons at the end of the main scene.
    /// </summary>

    public GameObject _scorePanel;
    public GameObject _uploadPanel; //the panel for the upload process
    public GameObject _endPanel;
    GameManager _gameManager;
    public Text _errorText;

    int fun = 0;
    int challenge = 0;
    int control = 0;

    // Use this for initialization
    void Start () {
        _gameManager = FindObjectOfType<GameManager>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    //////////////////////////////////////////////////////////////////////////////////////////
    //// END ROUND ///////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This happens when the player clicks the "continue" button after seeing the score.
    /// It checks whether it should show the upload or move on panel and does that
    /// </summary>
    public void OnClickedEndButton()
    {
        if(_gameManager._debugMode == true)
        {
            _gameManager.EndGameMoveOn();
            return;
        }
        if (FindObjectOfType<GameFlowScript>().playCount == 2)
        {
            //the player has played this round three times
            //prepare for them to set score and move on
            _scorePanel.SetActive(true);
            _endPanel.SetActive(false);
        }
        else
        {
            _gameManager.EndGameMoveOn();
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ////// SCORES ///////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////

    //these functions set the scores for the fun, control and challenge given the players' response
    public void SetControlScore(int value)
    {
        control = value;
    }
    public void SetChallengeScore(int value)
    {
        challenge = value;
    }
    public void SetFunScore(int value)
    {
        fun = value;
    }

    //Continue with the game after scoring.
    public void ScoreContinueClicked()
    {
        _gameManager.SetPlayerResponseScores(control, challenge, fun);
        _scorePanel.SetActive(false);
        _uploadPanel.SetActive(true);
    }


    /// ///////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////
    /// UPLOAD ////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////
    //these just trigger the correct actions
    //note that the upload one passes the error text so that it can be changed if there's an error
    public void OnClickedUpload()
    {
        _gameManager.WriteAndUploadFile(_errorText);
    }
    public void OnClickedContinue()
    {
        _gameManager.ContinueSkipUpload();
    }
}
