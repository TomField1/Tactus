using UnityEngine;
using System.Collections;

public class Stars : MonoBehaviour {

	[SerializeField] SpriteRenderer _star1;
	[SerializeField] SpriteRenderer _star2;
	[SerializeField] SpriteRenderer _star3;

	// Use this for initialization
	void Start () 
	{
		if(GameManager.Score <= GameManager.MaxScore * 0.3f)
		{
			_star1.color = Color.white;
			_star2.color = Color.clear;
			_star3.color = Color.clear;
		}
		else if(GameManager.Score <= GameManager.MaxScore * 0.6f)
		{
			_star1.color = Color.white;
			_star2.color = Color.white;
			_star3.color = Color.clear;
		}
		else
		{
			_star1.color = Color.white;
			_star2.color = Color.white;
			_star3.color = Color.white;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}