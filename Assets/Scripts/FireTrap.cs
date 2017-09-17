using UnityEngine;
using System.Collections;

public class FireTrap : MonoBehaviour, ITrap
{
	const float _damage = 30;
	bool _deployed = false;
	float _timer = 0;
	
	PlayerScript _player;

	ParticleSystem _emitter;
	
	// Use this for initialization
	public void Start ()
	{
		_player = GameObject.Find("Player").GetComponent<PlayerScript>();	
		_emitter = GetComponent<ParticleSystem>();
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
			_emitter.enableEmission = true;
			_deployed = true;
		}
		else
		{
			_emitter.enableEmission = false;
			_deployed = false;
		}
	}
	
	void OnTriggerEnter(Collider collision)
	{
		if(collision.tag == "Player" && _emitter.enableEmission == true)
		{
			_player.Damage(_damage);
		}
	}
}