using UnityEngine;
using System.Collections;
using MobileAIFramework;
using MobileAIFramework.Navigation.Pathfinding;

public class EnemyScript : MonoBehaviour {

    const float _waitTime = 0.5f;
    float _timer = 0;
	bool moving = false;
	float _speed;

	float m_searchTimer = 0;

    protected NavNode _path;

	Vector3 m_lookDir = new Vector3();
	RaycastHit rayHit;
	Transform m_target = null;
	bool m_foundPlayer = false;

	public GameObject _mazeTopLeft;
	public GameObject _mazeBottomRight;
	[SerializeField] PlayerScript _player;
	[SerializeField] GameManager _gameManager;

	// Use this for initialization
	void Start () 
	{        
		_speed = _gameManager.Speed;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Move();

		_speed = _gameManager.Speed;

		if(Physics.Raycast (transform.position, m_lookDir, out rayHit, 15) && rayHit.collider.tag == "Player" && m_foundPlayer == false)
		{
			m_target = rayHit.transform;
			m_searchTimer = 0;
			m_foundPlayer = true;
			moving = false;
		}

		if(m_searchTimer >= 5.0f && m_foundPlayer == true)
		{
			m_target = null;
			m_foundPlayer = false;
		}

		m_searchTimer += Time.deltaTime;
	}
	
    void Move()
    {
        _timer += Time.deltaTime;

		if(m_target != null && moving == false)
		{
			_path = MobileAI.Pathfind(transform.position, _player.transform.position);
			moving = true;
		}
        else if(_timer > _waitTime && moving == false)
        {
            _path = MobileAI.Pathfind(transform.position, new Vector3(Random.Range(_mazeTopLeft.transform.position.x, _mazeBottomRight.transform.position.x), 0, Random.Range(_mazeTopLeft.transform.position.z, _mazeBottomRight.transform.position.z)));
			//_path = MobileAI.Pathfind(transform.position, _player.transform.position);

			moving = true;
		}

		if (_path != null && moving == true)
		{
			Vector3 toPath = _path.Center - transform.position;
			m_lookDir = toPath;
			m_lookDir.Normalize();
			transform.position += Vector3.Normalize(toPath) * _speed / 10;
			
			if(toPath.magnitude <= 0.2f)
			{
				_path = _path.Parent;
			}

			if(_path.Parent == null)
			{
				moving = false;
				_timer = 0;
			}
		}
		else
		{
			moving = false;
		}
    }

	void OnCollisionEnter(Collision collision)
	{

	}
}