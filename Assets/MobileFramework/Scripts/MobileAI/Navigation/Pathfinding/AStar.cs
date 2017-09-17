#region Using Statements
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#endregion

namespace MobileAIFramework.Navigation.Pathfinding
{
	public class AStar : StateSpaceSearch
	{
		//Create an instance of the A* Algorithm.
		public AStar() : base("AStar")
		{
		}
		
		//Create an instance of the A* Algorithm, with the reference to the grid.
		public AStar(NavGrid navGrid) : base("AStar")
		{
			SetNavGrid(navGrid);
		}
		
		//Calculates the path betweens two NavCells.
		public override NavNode FindPath(int begin, int end)
		{
			//Transform from triangle index to square index
			begin = begin >> 1;
			end = end >> 1;
			
			//Retrieve the associated square element for the starting point..
			int a_x = begin / m_navGrid.columns;
			int a_y = begin % m_navGrid.rows;
			
			//.. and the endpoint
			int b_x = end / m_navGrid.columns;
			int b_y = end % m_navGrid.rows;
			
			//Initialized the list for the nodes
			List<NavNode> closedList = new List<NavNode>();
			List<NavNode> openList 	 = new List<NavNode>();
			
			//Calculates the starting distance between the starting and ending node, and find the route backwards
			float f = (Mathf.Abs(b_x - a_x) + Mathf.Abs(b_y - a_y));
			//NavNode currentNode = new NavNode(m_navGrid.InputGrid[a_x, a_y], null, f);
			//NavNode endNode 	= new NavNode(m_navGrid.InputGrid[b_x, b_y], null, 0);
			
			NavNode currentNode = new NavNode(m_navGrid.InputGrid[b_x, b_y], null, 0);
			NavNode endNode 	= new NavNode(m_navGrid.InputGrid[a_x, a_y], null, f);
			
			//Checks if both nodes are walkable
			if (!currentNode.IsWalkable || !endNode.IsWalkable)
				return null;
			
			openList.Add(currentNode);
			
			//Until a node remains in the openList, there may be a way to reach the point
			while(openList.Count > 0)
			{
				//Get the node with the lowest moving cost of the list
				currentNode = openList[0];
				
				//Pathfinding reached the end node
				if (currentNode.Id == endNode.Id)
				{
					break;
				}
				else
				{
					//Move the current node to the closedList to prevent the algorithm to reiterate through the node
					openList.RemoveAt(0);
					closedList.Add(currentNode);
					
					//Get the list of nodes close to the current one
					List<NavNode> nextNodes = FindCloseNodes(currentNode, endNode);
					
					//Check if the nodes already exists in the closed list
					foreach (NavNode closeNode in nextNodes)
					{
						if (!openList.Contains(closeNode))
						{
							if (!closedList.Contains(closeNode))
							{
								//Add it to the open list to check it
								openList.Add(closeNode);
							}
						}
					}
					
					//Order the list in a reverse order
					openList.Sort();
					
					//Set the current note to null in case there is not a single path
					currentNode = null;
				}
			}
			
			return currentNode;
		}
		
		//Get the list of close node next to the fived quad index in the grid
		public List<NavNode> FindCloseNodes(NavNode parent, NavNode end)
		{
			List<NavNode> nodeList = new List<NavNode>();
			
			int a_x = parent.Id / m_navGrid.columns;
			int a_y = parent.Id % m_navGrid.rows;
			
			int b_x = end.Id / m_navGrid.columns;
			int b_y = end.Id % m_navGrid.rows;
			
			//Iterates through the closest cells
			for (int i = -1; i <= 1; ++i)
			{
				int tmpX = a_x + i;
				
				if ( tmpX >= m_navGrid.columns || tmpX < 0)
					continue;
				
				for (int j = -1; j <= 1; ++j)
				{
					if (i == 0 && j == 0)
						continue;
					
					int tmpY = a_y + j;
					
					if (tmpY >= m_navGrid.rows || tmpY < 0)
						continue;
					
					//If the node exists check if it is a valid walk location and calculate the moving cost
					if (m_navGrid.InputGrid[tmpX, tmpY].IsWalkable)
					{
						//Set the distance between the point and the end point in number of cells
						int f = Mathf.Abs(b_x - tmpX) + Mathf.Abs(b_y - tmpY);
						
						//Weight the movement cost
						f += (Mathf.Abs(i) + Mathf.Abs(j) <= 1) ? 10 : 14;

						//Create the NavNode and add it to the neighbour list
						NavNode neighbourNode = new NavNode(m_navGrid.InputGrid[tmpX, tmpY], parent, f);
						nodeList.Add(neighbourNode);
					}
				}
			}
			
			return nodeList;
		}
	}
}