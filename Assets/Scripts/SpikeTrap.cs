using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour, ITrap
{
	const float _damage = 30;
	bool _deployed = false;
	float _timer = 0;

	PlayerScript _player;
	
	// Use this for initialization
	public void Start ()
	{
		_player = GameObject.Find("Player").GetComponent<PlayerScript>();	
	}
	
	// Update is called once per frame
	public void Update ()
	{
		_timer += Time.deltaTime;

		if(_timer >= 1)
		{
			Effect();
			_timer = 0;
		}
	}

	// Called when the trap is triggered
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

	void OnTriggerEnter(Collider collision)
	{
		if(collision.tag == "Player")
		{
			_player.Damage(_damage);
		}
	}
}