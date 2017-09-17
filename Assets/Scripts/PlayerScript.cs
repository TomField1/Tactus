using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework;
using MobileAIFramework.Gesture;
using MobileAIFramework.Navigation;
using MobileAIFramework.Navigation.Pathfinding;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour 
{
    protected Transform _transform;

	NavNode _path;

	MeshRenderer _meshRenderer;

	[SerializeField] GameManager _gameManager;

	Vector2 _direction;

	float _score = 0;
	float _health = 100;

    protected Mesh _debug;

    public PathRenderer _pathRenderer;

    private const float _moveDelay = 0.5f;
    protected double _currentTime;
	
	float _speed;

	float _focus = 100;

	[SerializeField] Image _focusBar;

	public enum ControlScheme
	{
		swipe,
		tap,
		drag,
		all
	}

	GestureType _lastGestureType = GestureType.Released;

	List<GestureType> _gestures = new List<GestureType>();

	public ControlScheme _controlScheme;

	// Use this for initialization
	void Start () 
    {
        _transform = transform;
        _currentTime = 0.0;
		_speed = _gameManager.Speed;

        //MobileAI.OnDebug -= DebugPathFinding;
        //MobileAI.OnDebug += DebugPathFinding;

		_meshRenderer = gameObject.GetComponent<MeshRenderer>();

		ResetDelegateMethods();
	}

	void OnDestroy()
	{
		GestureManager.OnSwipe -= OnSwipe;
		GestureManager.OnTap -= OnTap;
		GestureManager.OnRelease -= OnTap;
		GestureManager.OnHold -= OnHold;
	}

	// Update is called once per frame  
	void Update() 
    {
		switch(_lastGestureType)
		{
		case GestureType.Drag:
			MoveTap(_moveDelay);
			break;
		case GestureType.Tap:
			MoveTap(_moveDelay);
			break;
		case GestureType.Swipe:
			MoveSwipe(_moveDelay);
			break;
		default:
			break;
		}

		_focusBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _focus);

		_speed = _gameManager.Speed * 1.2f;

		if(_gameManager.TimeSlowed)
		{
			_focus -= 0.5f;
		}

		if(_health <= 0)
		{
			Application.LoadLevel(2);
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// To be hooked into the MobileAI OnTap event.
	/// </summary>
    private void OnTap(Gesture info)
    {
		int iD = MobileAI.GetTriangleID(info.position);

		if( iD != -1 && MobileAI.GetCell(iD).IsWalkable)
		{
        	_path = MobileAI.CalculatePath(transform.position, info);
		}
		else
		{
			NavCell tmp = MobileAI.GetCell(iD).GetClosestWalkableCell();
			//_path = MobileAI.CalculatePath(transform.position, newInfo);
			if(tmp != null)
				_path = MobileAI.Pathfind(transform.position, tmp.Center);

			if(_path == null)
				return;
		}

		_pathRenderer._node = _path;
		_lastGestureType = GestureType.Tap;
    }

	/// <summary>
	/// To be hooked into the MobileAI OnSwipe Event
	/// </summary>
	private void OnSwipe(Gesture info)
	{
		_pathRenderer.ClearLine();

		if (Mathf.Abs(info.deltaPosition.x) > Mathf.Abs(info.deltaPosition.y))
		{
			if (info.deltaPosition.x > 0)
				_direction = new Vector2(0.6f, 0);
			else
				_direction = new Vector2(-0.6f, 0);
		}
		else if (Mathf.Abs(info.deltaPosition.x) < Mathf.Abs(info.deltaPosition.y))
		{
			if (info.deltaPosition.y > 0)
				_direction = new Vector2(0, 0.6f);
			else
				_direction = new Vector2(0, -0.6f);
		}

		_lastGestureType = GestureType.Swipe;
	}

	/// <summary>
	/// To be hooked into the MobileAI OnHold event.
	/// </summary>
	private void OnHold(Gesture info)
	{
		_gameManager.SetTimeScale();
	}

	/// <summary>
	/// The movement method to be used when the last gesture was a swipe.
	/// </summary>
	protected void MoveSwipe(float time)
	{
		_pathRenderer.ClearLine();
		if (_direction != Vector2.zero)
		{
			Vector3 toPath;
			Vector3 tmpPos = _transform.position + new Vector3(_direction.x, 0, _direction.y);
			_path = MobileAI.Pathfind(_transform.position, tmpPos);

			if(_path != null)
			{
				toPath = _path.Parent.Center - _transform.position;
				_transform.position += Vector3.Normalize(toPath) * _speed / 10;

				if(toPath.magnitude <= 0.2f)
				{
					_path = _path.Parent;
				}
			}
		}
	}

	/// <summary>
	/// The movement method to be used when the last gesture was a tap.
	/// </summary>
    protected void MoveTap(float time)
    {
		if(_path != null)
		{
		    Vector3 toPath = _path.Center - _transform.position;
		    _transform.position += Vector3.Normalize(toPath) * _speed / 10;

		    if(toPath.magnitude <= 0.2f)
		    {
		        _path = _path.Parent;
		        _pathRenderer._node = _path;
		        _debug = MobileAI.GetPathMesh(_path);
		    }
		}
    }

	/// <summary>
	/// Sets the control scheme. Dependent on the list of recorded gestures. (Deprecated)
	/// </summary>
	protected ControlScheme SetControlScheme()
	{
		int tapCount = 0, swipeCount = 0, dragCount = 0;

		foreach(GestureType gesture in _gestures)
		{
			if(gesture == GestureType.Drag)
				dragCount++;
			else if(gesture == GestureType.Swipe)
				swipeCount++;
			else if(gesture == GestureType.Tap)
				tapCount++;
		}

		if(swipeCount > tapCount && swipeCount > tapCount)
			return ControlScheme.swipe;
		else if(tapCount > swipeCount && tapCount > dragCount)
			return ControlScheme.tap;
		else
			return ControlScheme.drag;
	}

	/// <summary>
	/// Resets the delegate methods for the control scheme.
	/// </summary>
	private void ResetDelegateMethods()
	{
		GestureManager.OnSwipe -= OnSwipe;
		GestureManager.OnTap -= OnTap;
		GestureManager.OnRelease -= OnTap;
		GestureManager.OnHold -= OnHold;

		GestureManager.OnHold += OnHold;

		switch(_controlScheme)
		{
		case ControlScheme.swipe:
			GestureManager.OnSwipe -= OnSwipe;
			GestureManager.OnSwipe += OnSwipe;
			break;
		case ControlScheme.tap:
			GestureManager.OnTap -= OnTap;
			GestureManager.OnTap += OnTap;
			break;
		case ControlScheme.drag:
			GestureManager.OnRelease -= OnTap;
			GestureManager.OnRelease += OnTap;
			break;
		case ControlScheme.all:
			GestureManager.OnTap -= OnTap;
			GestureManager.OnTap += OnTap;
			
			GestureManager.OnRelease -= OnTap;
			GestureManager.OnRelease += OnTap;
			
			GestureManager.OnSwipe -= OnSwipe;
			GestureManager.OnSwipe += OnSwipe;
			break;
		default:
			break;
		}
	}

	/// <summary>
	/// Method for drawing the path we found. Does not draw in game.
	/// </summary>
    private void DebugPathFinding()
    {
        if(_path != null)
        {
            MobileAI.DrawPath(_path);
        }
    }
	
	public void OnTriggerEnter(Collider collider)
	{
		if(collider.tag == "Pickup")
		{
			GameManager.Score += 5;
			Destroy(collider.gameObject);
		}
		else if(collider.tag == "FocusPickup")
		{
			_focus = 100;
			GameManager.Score += 25;
			Destroy(collider.gameObject);
		}

		if(collider.tag == "Enemy")
		{
			Damage(50);
		}

		if(collider.tag == "WinZone")
		{
			GameManager.Score += GameManager.TimeValue * 2;

			if(GameManager.RunCount <= 1.5f)
				GameManager.RunCount += 0.1f;

			Application.LoadLevel(3);
		}
	}

	/// <summary>
	/// Deal the specified damage to this class.
	/// </summary>
	public void Damage(float damage)
	{
		_health -= damage;
		_meshRenderer.material.color = new Color(1, _health/100, _health/100);
	}

	/// <summary>
	/// Gets the health.
	/// </summary>
	/// <value>The health.</value>
	public float Health
	{
		get {return _health;}
	}

	/// <summary>
	/// Gets or sets the focus.
	/// </summary>
	/// <value>The focus.</value>
	public float Focus
	{
		get {return _focus;}
		set {_focus = value;}
	}

	/// <summary>
	/// Gets the score.
	/// </summary>
	/// <value>The score.</value>
	public float Score
	{
		get {return _score;}
	}
}