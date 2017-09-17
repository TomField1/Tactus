#region Using Statements
using UnityEngine;
using System.Collections;
#endregion

namespace MobileAIFramework.Navigation.Pathfinding
{
	public abstract class StateSpaceSearch
	{
		//The name of the state space search algorithm
		private string m_type;
		public string Type
		{
			get { return m_type; }
		}
		
		//Reference to the navgrid where calculate the path
		protected NavGrid m_navGrid;
		
		//Create an instance of the State Space Search algoritm
		public StateSpaceSearch(string type)
		{
			m_type = type;
		}
		
		//Set the NavGrid to calculate the path
		public void SetNavGrid(NavGrid navGrid)
		{
			m_navGrid = navGrid;
		}
		
		//Calculates the path betweens two NavCells
		public virtual NavNode FindPath(int begin, int end)
		{
			return null;
		}
	}
}