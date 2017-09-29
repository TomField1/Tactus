// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyScriptSimple : MonoBehaviour
{

    public GameObject _player;
    public GameManager _manager;

    [SerializeField]
    Material _normalMaterial;
    [SerializeField]
    Material _fleeMaterial;

    private NavMeshAgent _agent; //the navigation agent
    private float _timeLastCheckedNav;

    [SerializeField]
    MeshRenderer _renderer;

    public Vector3 _homePosition; //the start position of the enemy. it tries to return to here when it hits the player
    private bool _fallingBack; //is the enemy retreating
    private float _timeCaught; //when did it last catch the player?

    /// <summary>
    /// Setup the basic functions of the enemy
    /// </summary>
    void Start()
    {
        _timeLastCheckedNav = Time.fixedTime;
        _timeCaught = Time.fixedTime;
        _agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Frame Update for the enemy
    /// </summary>
    void Update()
    {
        //if not falling back and it has been more than half a second since the last time the target was set, set the target
        if(!_fallingBack && _timeLastCheckedNav+0.5 < Time.fixedTime)
        {
            UpdateNavTarget();
        }

        //if falling back and it has been more than six seconds since the last time we caught the target, stop falling back
        if(_fallingBack && _timeCaught + 6 < Time.fixedTime)
        {
            _fallingBack = false;
        }
    }

    public void MaterialIsFleeing(bool t)
    {
        if (t == true)
        {
            _renderer.material = _fleeMaterial;
        }
        else
        {
            _renderer.material = _normalMaterial;
        }
    }

    //if we catch the player
    public void CaughtPlayer()
    {
        //start falling back, try and get to the "home" position
        _fallingBack = true;
        _agent.destination = _homePosition;
        _timeCaught = Time.fixedTime;
    }

    //if we were caught by the player
    public void WasCaught()
    {
        //flash to show we took damage, and die
        StartCoroutine(flashAndDie());
    }

    IEnumerator flashAndDie()
    {
        _renderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        _renderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        _renderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        _renderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        _renderer.enabled = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// Every half second, run this to keep the bot chasing/fleeing from the player
    /// </summary>
    public void UpdateNavTarget()
    {

        _timeLastCheckedNav = Time.fixedTime;

        //if not in star mode chase the player
        if (_manager._focusMode == false)
        {
            _agent.destination = _player.transform.position;
            _renderer.material = _normalMaterial;
        }

        //if in star mode, run to the furthest corner from the player
        else
        {
            _renderer.material = _fleeMaterial;

            if (_player.transform.position.x >=12 && _player.transform.position.z >= 12)
            {
                _agent.destination = new Vector3(1,0,1);
            }
            else if (_player.transform.position.x <= 12 && _player.transform.position.z >= 12)
            {
                _agent.destination = new Vector3(23, 0, 1);
            }
            else if (_player.transform.position.x >= 12)
            {
                _agent.destination = new Vector3(1, 0, 23);
            }
            else
            {
                _agent.destination = new Vector3(23, 0, 23);
            }
        }
    }
}