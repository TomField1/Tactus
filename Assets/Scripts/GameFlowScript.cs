using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class manages the flow of the game - how to switch between scenes
/// </summary>
public class GameFlowScript : MonoBehaviour {

    //the control scene that the main level should use the next time it's loaded
    public GameManager.ControlScheme controlSchemeToSet;

    //is this the first time through each set of schemes?
    public bool firstTime = false;

    //how many times has the player played this latest scheme?
    public int playCount = 0;

    //list of control schemes and our current place in it
    int currentSchemeNum;
    GameManager.ControlScheme[] schemeList;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this.gameObject);
        firstTime = true;
        GenerateRandomSchemeList();
	}

    /// <summary>
    /// Get a random-ordered list of control schemes
    /// </summary>
    public void GenerateRandomSchemeList()
    {
        //create the array and make a dummy ordering
        schemeList = new GameManager.ControlScheme[3];
        schemeList[0] = GameManager.ControlScheme.Tap;
        schemeList[1] = GameManager.ControlScheme.Swipe;
        schemeList[2] = GameManager.ControlScheme.Tilt;

        //shuffle it
        int n = schemeList.Length;
        while (n > 1)
        {
            n--;
            int k = (int)Mathf.Ceil(Random.Range(-1.01f, n));
            GameManager.ControlScheme value = schemeList[k];
            schemeList[k] = schemeList[n];
            schemeList[n] = value;
        }
    }

    /// <summary>
    /// Choose and load the next level
    /// </summary>
    public void GetNextLevel()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        //if current level is main menu, set control scheme to the first in the list and load the main level
        if (sceneIndex == 0)
        {
            controlSchemeToSet = schemeList[0];
            currentSchemeNum = 0;
            SceneManager.LoadScene(1);
        }


        //if the current level is the game scene
        else if(sceneIndex == 1)
        {
            //if this is the first/second/third time they've played, play the same gamemode
            if (playCount < 2)
            {
                playCount++;
                SceneManager.LoadScene(1);
                return;
            }

            //if they've already played it three times, the gamemode will automatically load the upload function, so we dont worry about that here

            
            //if first time is true and control scheme is not last
            //then load the game with the next control scheme
            if (firstTime == true && controlSchemeToSet != schemeList[2])
            {
                playCount = 0; //reset the play count so we play this next one three times
                currentSchemeNum++; // we're now on the next control scheme
                controlSchemeToSet = schemeList[currentSchemeNum];
                SceneManager.LoadScene(1);

            }
            //if first time is false or the control scheme is set to the last, load choices level
            else 
            {
                firstTime = false;
                SceneManager.LoadScene(2);
            }
        }
        
        //if current level is choices level, that will set control scheme, load main level
        else if (sceneIndex == 2)
        {
            //set the playcount to 2 so the game treats it as if they've played three times and lets them upload it
            playCount = 2;
            SceneManager.LoadScene(1);
        }

        //if we get here something is wrong
        else
        {
            print("you dun goofed sceneID = " + sceneIndex);
        }
    }
}
