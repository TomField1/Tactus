using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
	bool _deployed = false;
	float _timer = 0;

	PlayerScriptSimple _player;
	
	// Use this for initialization
	public void Start ()
	{
		_player = GameObject.Find("Player").GetComponent<PlayerScriptSimple>();	
	}
	
	// Update is called once per frame
	public void Update ()
	{
		_timer += Time.deltaTime;

        //if more than a second has passed, switch states
		if(_timer >= 1)
		{
			Effect();
			_timer = 0;
		}
	}

	/// <summary>
    /// Switches state of spike trap. Sets location and Deployed check.
    /// </summary>
	public void Effect()
	{
		if(!_deployed)
		{
			transform.position += transform.up * 0.9f;
			_deployed = true;
		}
		else
		{
			transform.position -= transform.up * 0.9f;
			_deployed = false;
		}
	}
}