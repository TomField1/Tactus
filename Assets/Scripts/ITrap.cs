using UnityEngine;
using System.Collections;

public interface ITrap
{
	// Use this for initialization
	void Start();
	
	// Update is called once per frame
	void Update();

	// What does our trap do?
	void Effect();
}