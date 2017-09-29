using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EthicsControlScript : MonoBehaviour {

    public Text _textOne;
    public Text _textTwo;
    public Text _textThree;

    public GameObject _buttonOne;
    public GameObject _buttonTwo;
    public GameObject _buttonThree;
    public GameObject _buttonFour;
    public ScrollRect _scrollrect;

    public GameFlowScript _gameFlow;
	
    //these adjust which text and buttons are showing
	public void OnClickedContinueFirst()
    {
        _textOne.gameObject.SetActive(false);
        _textTwo.gameObject.SetActive(true);
        _buttonOne.SetActive(false);
        _buttonTwo.SetActive(true);

        //set the scrolling rectangle back to the top
        _scrollrect.verticalNormalizedPosition = 1;
    }

    public void OnClickedContinueSecond()
    {
        _textTwo.gameObject.SetActive(false);
        _textThree.gameObject.SetActive(true);
        _buttonTwo.SetActive(false);
        _buttonThree.SetActive(true);
        _buttonFour.SetActive(true);

        //set the scrolling rectangle back to the top
        _scrollrect.verticalNormalizedPosition = 1;
    }

    //these control if you should get the next level or close the game
    public void OnClickedAgree()
    {
        //move to the first game level
        _gameFlow.GetNextLevel();
    }

    public void OnClickedDisagree()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit ();
        #endif
    }
}
