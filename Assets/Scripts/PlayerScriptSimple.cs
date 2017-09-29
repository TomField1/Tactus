using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerScriptSimple : MonoBehaviour
{

    [SerializeField]
    GameManager _gameManager;

    //tracking the current movement direction
    Vector3 _direction = new Vector3(0, 0, 1);

    //track the current tilt direction so we only try to change when the tilt orientation changes
    //(note its set to up at the start so all other directions - forward/back/left/right - work from the start.
    Vector3 _tiltDirection = Vector3.up;

    //current score and health
    int _score = 0;
    public int _health = 5;

    //the nav agent + pathrenderer
    private NavMeshAgent agent;
    public LineRenderer _pathRenderer;

    [SerializeField]
    Material _defaultMaterial;
    [SerializeField]
    Material _powerupMaterial;

    //when did focus mode start
    double _focusModeStartTime;
    float _focus = 0; //how much is left?

    //i never got this working: it tidies the nav mesh
    NavMeshPath tidiedPath;

    //the images for the focus bar
    [SerializeField]
    Image _focusBar;
    [SerializeField]
    Image _timeBar;

    [SerializeField]
    MeshRenderer _renderer;

    //the tracking manager for the game
    TrackingManager _trackingManager;

    // Use this for initialization
    //get the key components
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _trackingManager = FindObjectOfType<TrackingManager>();
    }

    void OnDestroy()
    {

    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// GENERAL STUFF /////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    // Update is called once per frame  
    void Update()
    {
        //run the relevant Update for the control scheme
        if (_gameManager.controls == GameManager.ControlScheme.Tap)
        {
            TapSchemeUpdate();
        }
        else if (_gameManager.controls == GameManager.ControlScheme.Swipe)
        {
            SwipeSchemeUpdate();
        }
        else if (_gameManager.controls == GameManager.ControlScheme.Tilt)
        {
            TiltSchemeUpdate();
        }

        //set the sizes of the time and focus bars
        _focusBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _focus*100);
        _timeBar.rectTransform.SetSizeWithCurrentAnchors(
                            RectTransform.Axis.Horizontal, _gameManager._timeLeft * 1000 / 60);

        //if health below 0, die and gameover
        if (_health <= 0)
        {
            _gameManager.GameOver();
            Destroy(gameObject);
        }

        //reduce focus mode time remaining, if its 0 end it
        if ((_gameManager._focusMode == true))
        {
            _focus = _focus - Time.deltaTime;
        }
        if (_focus <= 0)
        {
            EndFocusMode();
        }

        
    }

    /// <summary>
    /// Used when you collide with a thing
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter(Collider coll)
    {

        //if the collider belongs to a pickup
        if (coll.gameObject.tag == "Pickup")
        {
            ///Collided with pickup
            CollectPickup();

            // remove the pickup
            coll.gameObject.SetActive(false);
        }
        //if it's an enemy, you hit the enemy
        else if (coll.gameObject.tag == "Enemy")
        {
            HitEnemy(coll.gameObject);
        }
        //if it's a focus pickup, start focus mode
        else if (coll.gameObject.tag == "FocusPickup")
        {
            StartFocusMode();

            // remove the pickup
            coll.gameObject.SetActive(false);
        }
        else if (coll.gameObject.tag == "SpikeTrap")
        {
            HitSpikes();
        }
    }

    //start and end the powerup mode
    void StartFocusMode()
    {
        
        _gameManager.SetFocusMode(true);
        _focus = 10;
        _renderer.material = _powerupMaterial;
        _trackingManager.AddEvent(new EventPowerup(Time.fixedTime, this.gameObject.transform.position));
    }
    void EndFocusMode()
    {
        _gameManager.SetFocusMode(false);
        _renderer.material = _defaultMaterial;
    }

    void CollectPickup()
    {
        // increment the score
        _score++;

        //if in focus mode, incrememnt it again (+2 for each pickup total)
        if (_gameManager._focusMode == true)
        {
            _score++;
        }

        //set this score as the official score
        _gameManager.SetScore(_score);
    }

    void HitEnemy(GameObject enemy)
    {
        print("hit enemy");
        //what happens when you hit an enemy
        EnemyScriptSimple e = enemy.GetComponent<EnemyScriptSimple>();

        //if not in focus mode
        if (_gameManager._focusMode == false)
        {
            e.CaughtPlayer(); //the enemy reacts
            _health = _health - 1; //you take damage

            //flash your light red
            StartCoroutine(flash());

            //add the event
            _trackingManager.AddEvent(new EventDamaged(Time.fixedTime, this.gameObject.transform.position, _health, TrackingManager.DamageSource.Enemy));
        }
        else
        {
            e.WasCaught(); //you caught the enemy
            _score = _score + 50; //have some points

            //add the event
            _trackingManager.AddEvent(new EventCaughtEnemy(Time.fixedTime, this.gameObject.transform.position));
        }
    }

    void HitSpikes()
    {
        //if you hit the spikes and you're not in focus mode
        if (_gameManager._focusMode == false)
        {
            _health = _health - 1;//you take damage

            //flash to show we've taken damage
            StartCoroutine(flash());

            //add the event
            _trackingManager.AddEvent(new EventDamaged(Time.fixedTime, this.gameObject.transform.position, _health, TrackingManager.DamageSource.Spikes));
        }
    }

    /// <summary>
    /// This coroutine flashes the MeshRenderer of the player when it is damaged.
    /// </summary>
    /// <returns></returns>
    IEnumerator flash()
    {
        _renderer.enabled = false;
        yield return new WaitForSeconds(0.2f);
        _renderer.enabled = true;
        yield return new WaitForSeconds(0.2f);
        _renderer.enabled = false;
        yield return new WaitForSeconds(0.2f);
        _renderer.enabled = true;
        yield return new WaitForSeconds(0.2f);
        _renderer.enabled = false;
        yield return new WaitForSeconds(0.2f);
        _renderer.enabled = true;
    }


    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// TAP STUFF /////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// If in Tap mode, this is the regular Update function
    void TapSchemeUpdate()
    {
        //if the player clicks or taps
        if (Input.GetMouseButtonDown(0))
        {
            //get the position of the tap in the world and set it as the AI destination
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 100);

            agent.destination = hit.point;

            //add the event
            _trackingManager.AddEvent(new EventTap(Time.fixedTime, this.gameObject.transform.position, hit.point));

        }
        if (Input.GetMouseButtonUp(0))
        {
            //when the click/tap ends, set the pathrenderer to show the path and show it
            Vector3[] pathToShow = TidyPath(agent.path.corners);
            _pathRenderer.positionCount = pathToShow.Length;
            _pathRenderer.SetPositions(pathToShow);
            _pathRenderer.widthMultiplier = 1;
        }

        //when close to the destination, hide the path
        if (agent.remainingDistance < 0.5f)
        {
            _pathRenderer.widthMultiplier = 0;
        }
    }

    //this doesnt work so dont worry about it
    Vector3[] TidyPath(Vector3[] path)
    {
        int i = 0;

        Vector3[] tidiedPath = path;

        for (i = 0; i < tidiedPath.Length; i++)
        {
            tidiedPath[i].x = Mathf.RoundToInt(tidiedPath[i].x);
            tidiedPath[i].z = Mathf.RoundToInt(tidiedPath[i].z);

            //print("tidied to " + tidiedPath.corners[i].x + " " + tidiedPath.corners[i].z);
        }

        return tidiedPath;
    }


    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    /// SWIPE STUFF /////////////////////////////////////////////////////////////////////////////////
    /// ///////////////////////////////////////////////////////////////////////////////////////////////
    
    //This is the standard Update for the swipe scheme. Note that the SwipeManager handles most of this
    void SwipeSchemeUpdate()
    {

        //if control detected try to change direction
        if (Input.GetKeyDown("w"))
        {
            TryChangeDirection(new Vector3(0, 0, 1));
        }
        else if (Input.GetKeyDown("a"))
        {
            TryChangeDirection(new Vector3(-1, 0, 0));
        }
        else if (Input.GetKeyDown("s"))
        {
            TryChangeDirection(new Vector3(0, 0, -1));
        }
        else if (Input.GetKeyDown("d"))
        {
            TryChangeDirection(new Vector3(1, 0, 0));
        }
        
        //Debug.DrawLine(transform.position, transform.position + _direction, Color.red);


        //try and move in the current direction
        if (!Physics.Raycast(transform.position, _direction, 0.6f))
        {
            //if can move that way
            //move
            transform.position = transform.position + _direction / 20;

        }
        else
        {
            //if can't
            //dont
        }
    }

    /// <summary>
    /// Has the player Swiped in the SwipeManager?
    /// </summary>
    /// <param name="dir"></param>
    public void Swiped(Vector3 dir)
    {
        //if so try to change the direction to that of the swipe
        TryChangeDirection(dir);
    }

    bool TryChangeDirection(Vector3 dir)
    {
        //raycast in control direction
        if (!Physics.Raycast(transform.position, dir, 0.7f))
        {
            //if can move that way

            _trackingManager.AddEvent(new EventTryChangeDir(Time.fixedTime, this.gameObject.transform.position, _direction, dir, true));
            //change movement direction
            _direction = dir;
            return true;
        }
        else
        {
            //if can't

            _trackingManager.AddEvent(new EventTryChangeDir(Time.fixedTime, this.gameObject.transform.position, _direction, dir, false));
            //dont
            return false;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////// TILT STUFF //////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    //standard Tilt update
    void TiltSchemeUpdate()
    {
        //if the device is tilted sharply in any direction
        if(Mathf.Abs(Input.acceleration.x) > 0.3 || Mathf.Abs(Input.acceleration.y) > 0.3)
        {
            //if it's MORE tilted in X than Z
            if(Mathf.Abs(Input.acceleration.x) > Mathf.Abs(Input.acceleration.y))
            {

                //depending on direction, try and move that way
                if (Input.acceleration.x > 0)
                {
                    //this last if statement means that we only track an attempt to change direction when the orientation changes
                    if (_tiltDirection != Vector3.right) {
                        _tiltDirection = Vector3.right;
                        TryChangeDirection(Vector3.right);
                    }
                }
                else {
                    if (_tiltDirection != Vector3.left)
                    {
                        _tiltDirection = Vector3.left;
                        TryChangeDirection(Vector3.left);
                    }
                }
            }
            else
            {
                
                //depending on direction try to move that way
                if (Input.acceleration.y > 0)
                {
                    if (_tiltDirection != Vector3.forward)
                    {
                        _tiltDirection = Vector3.forward;
                        TryChangeDirection(Vector3.forward);
                    }
                }
                else
                {
                    if (_tiltDirection != Vector3.back)
                    {
                        _tiltDirection = Vector3.back;
                        TryChangeDirection(Vector3.back);
                    }
                }
            }
        }


        //debug computer controls
        if (Input.GetKeyDown("w"))
        {
            TryChangeDirection(new Vector3(0, 0, 1));
        }
        else if (Input.GetKeyDown("a"))
        {
            TryChangeDirection(new Vector3(-1, 0, 0));
        }
        else if (Input.GetKeyDown("s"))
        {
            TryChangeDirection(new Vector3(0, 0, -1));
        }
        else if (Input.GetKeyDown("d"))
        {
            TryChangeDirection(new Vector3(1, 0, 0));
        }

        //Debug.DrawLine(transform.position, transform.position + _direction, Color.red);

        //try and move in the current direction
        if (!Physics.Raycast(transform.position, _direction, 0.6f))
        {
            //if can move that way
            //move
            transform.position = transform.position + _direction / 15;
        }
        else
        {
            //if can't
            //dont
        }
    }
}