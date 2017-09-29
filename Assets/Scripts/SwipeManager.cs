using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{

    public float minSwipeDist, maxSwipeTime; bool couldBeSwipe;
    Vector2 startPos;
    float swipeStartTime;
    PlayerScriptSimple player;

    public LineRenderer _pathRenderer;

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<PlayerScriptSimple>();
    }

    /// <summary>
    /// Start checking swipe actions
    /// </summary>
    public void StartCheckingSwipes()
    {
        StartCoroutine(checkHorizontalSwipes());
        print("starting to check 2swipes");
    }


    IEnumerator checkHorizontalSwipes() //Coroutine, wich gets Started in "Start()" and runs over the whole game to check for swipes
    {
        while (true)
        { //Loop. Otherwise we wouldnt check continoulsy ;-)
            foreach (Touch touch in Input.touches)
            { //For every touch in the Input.touches - array...

                switch (touch.phase)
                {
                    case TouchPhase.Began: //The finger first touched the screen --> It could be(come) a swipe

                        print("2swipe start touch");
                        couldBeSwipe = true;

                        startPos = touch.position;  //Position where the touch started
                        swipeStartTime = Time.time; //The time it started
                        break;

                    case TouchPhase.Stationary: //Is the touch stationary? --> No swipe then!
                        //couldBeSwipe = false;
                        print("2swipe end stationary");
                        break;
                }
                float swipeTime = Time.time - swipeStartTime; //Time the touch stayed at the screen till now.
                Vector2 totalSwipe = (touch.position - startPos);
                float swipeDist = totalSwipe.magnitude; //Swipedistance


                if (couldBeSwipe && swipeTime < maxSwipeTime && swipeDist > minSwipeDist)
                {
                    // It's a swiiiiiiiiiiiipe!
                    couldBeSwipe = false; //<-- Otherwise this part would be called over and over again.

                    _pathRenderer.positionCount = 2;
                    _pathRenderer.SetPosition(0, startPos);
                    _pathRenderer.SetPosition(1, touch.position);
                    _pathRenderer.widthMultiplier = 1;


                    //print("2swipe ==========================================================================");

                    //if the difference in X is less than the difference in Y, the player is swiping vertically
                    if(Mathf.Abs(touch.position.x - startPos.x) > Mathf.Abs(touch.position.y - startPos.y))
                    {
                        if ((touch.position.x - startPos.x) > 0)
                        { //Swipe-direction, either 1 or -1.

                            //Right-swipe
                            player.Swiped(new Vector3(1, 0, 0));

                            print("2swipe RIGHT "+swipeDist+" ==========================================================================");

                        }
                        else
                        {

                            //Left-swipe
                            player.Swiped(new Vector3(-1, 0, 0));
                            print("2swipe LEFT " + swipeDist + " ==========================================================================");
                        }
                    }
                    else
                    {
                        if ((touch.position.y - startPos.y) > 0)
                        { //Swipe-direction, either 1 or -1.

                            //up-swipe
                            player.Swiped(new Vector3(0, 0, 1));
                            print("2swipe UP " + swipeDist + " ==========================================================================");

                        }
                        else
                        {

                            //down-swipe
                            player.Swiped(new Vector3(0, 0, -1));
                            print("2swipe DOWN " + swipeDist + " ==========================================================================");
                        }
                    }

                   
                    
                }
            }
            yield return null;
        }
    }
}
