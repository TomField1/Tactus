#region Using Statements
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework.Navigation.Pathfinding;
#endregion

public class PathRenderer : MonoBehaviour
{
	public NavNode _node;
	public LineRenderer _lineRenderer;

    void Start()
    {

    }
	
	void OnDestroy()
	{
		_lineRenderer.SetVertexCount(0);
	}

	/// <summary>
	/// Clears the line.
	/// </summary>
	public void ClearLine()
	{
		_lineRenderer.SetVertexCount(0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_node != null)
		{
			NavNode node = _node;
			
			int vertCount = 0;
			
			while (node != null)
			{
				_lineRenderer.SetVertexCount(vertCount + 1);
				_lineRenderer.SetPosition(vertCount, node.Center);
				
				++vertCount;
				
				node = node.Parent;
			}
		}
	}
}