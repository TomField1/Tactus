using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework;
using MobileAIFramework.Navigation;

public class GameManager : MonoBehaviour {

	public static float Score = 0;
	public static float MaxScore = 1490;
	public static float TimeValue = 60;

	[SerializeField] List<EnemyScript> _enemyList;
	[SerializeField] PlayerScript _player;
	[SerializeField] MobileAI _mobileAIFramework;
	[SerializeField] GameObject _pickUp;

	//NavCell[,] _navGrid;

	[SerializeField] Text _score;
	[SerializeField] Text _time;

	[SerializeField] float _gameSpeed;
	[SerializeField] float _maxGameSpeed;
	[SerializeField] float _minGameSpeed;

	[SerializeField] List<WinZone> _winZoneList;

	public static float RunCount = 1;

	bool _timeSlowed = false;

	// Use this for initialization
	void Start() 
	{
		_gameSpeed *= RunCount;

		int windex = Random.Range(0, 5);

		_winZoneList[windex].Enabled = true;
	}
	
	// Update is called once per frame
	void Update() 
	{
		_score.text = "Score: " + Score;
		TimeValue -= Time.deltaTime * _gameSpeed;

		_time.text = TimeValue.ToString("F1");

		if(_player.Focus <= 0.0f)
		{
			_gameSpeed = _maxGameSpeed;
			_timeSlowed = false;
		}

		if(TimeValue <= 0.0f)
		{
			Application.LoadLevel(2);
		}
	}

	/// <summary>
	/// Sets the time scale.
	/// </summary>
	public void SetTimeScale()
	{
		if(_player.Focus >= 0.0f && !_timeSlowed)
		{
			_gameSpeed = _minGameSpeed;
			_timeSlowed = true;
		}
		else if(_timeSlowed)
		{
			_gameSpeed = _maxGameSpeed;
			_timeSlowed = false;
		}
	}

	/// <summary>
	/// Gets the game speed.
	/// </summary>
	public float Speed
	{
		get { return _gameSpeed; }
	}

	/// <summary>
	/// Gets a value indicating whether this time is slowed.
	/// </summary>
	public bool TimeSlowed
	{
		get { return _timeSlowed; }
	}
}