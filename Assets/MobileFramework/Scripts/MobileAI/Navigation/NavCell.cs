#region Using Namespace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace MobileAIFramework.Navigation
{
	public class NavCell
	{

		//Id of the cell
		private int m_id;
		public int Id
		{
			get { return m_id; }
		}
		
		//Tells if the cell is walkable or not
		private bool m_isWalkable;
		/// <summary>
		/// Gets or sets a value indicating whether this cell is walkable.
		/// </summary>
		public bool IsWalkable
		{
			get { return m_isWalkable; }
			set { m_isWalkable = value; }
		}
	
		//The center position of the Cell
		private Vector3 m_center;
		/// <summary>
		/// Gets the center.
		/// </summary>
		public Vector3 Center
		{
			get { return m_center; }
		}

		///<summary>
		///Create an instance of the NavCell.
		///A nav cell contains information about a cell in the NavGrid.
		/// </summary>
		public NavCell(int id, bool isWalkable, Vector3 center)
		{
			m_id = id;

			m_isWalkable = isWalkable;
			m_center 	 = center;
		}

		/// <summary>
		/// Gets the closest walkable cell from the adjacent cells.
		/// </summary>
		/// <returns>The closest walkable cell.</returns>
		public NavCell GetClosestWalkableCell()
		{
			List<KeyValuePair<NavCell, float>> nearbyWalkables = new List<KeyValuePair<NavCell, float>>();
			if(AboveCell.IsWalkable)
			   nearbyWalkables.Add(new KeyValuePair<NavCell, float>(AboveCell, (AboveCell.Center - Center).magnitude));

			if(BelowCell.IsWalkable)
				nearbyWalkables.Add(new KeyValuePair<NavCell, float>(BelowCell, (BelowCell.Center - Center).magnitude));

			if(RightCell.IsWalkable)
	       		nearbyWalkables.Add(new KeyValuePair<NavCell, float>(RightCell, (RightCell.Center - Center).magnitude));

			if(LeftCell.IsWalkable)
           		nearbyWalkables.Add(new KeyValuePair<NavCell, float>(LeftCell, (LeftCell.Center - Center).magnitude));

			nearbyWalkables.Sort(SortClosest);

			if(nearbyWalkables.Count > 0)
				return nearbyWalkables[0].Key;
			else 
				return null;
		}

		/// <summary>
		/// Private sort method to be used in FindClosestCell
		/// </summary>
		private int SortClosest(KeyValuePair<NavCell, float> a, KeyValuePair<NavCell, float> b)
		{
			return a.Value.CompareTo(b.Value);
		}

		/// <summary>
		/// Returns the cell above me.
		/// </summary>
		public NavCell AboveCell
		{
			get
			{
				NavCell tmp = MobileAI.GetCell((int)((m_id - MobileAI.Instance.GetGridSize().x) * 2));
				return tmp;
			}
        }

		/// <summary>
		/// Returns the cell below me.
		/// </summary>
		public NavCell BelowCell
		{
			get
			{
				NavCell tmp = MobileAI.GetCell((int)((m_id + MobileAI.Instance.GetGridSize().x) * 2));
				return tmp;
			}
		}

		/// <summary>
		/// Returns the cell to the left of me.
		/// </summary>
		public NavCell LeftCell
		{
			get
			{
				NavCell tmp = MobileAI.GetCell((m_id - 1) * 2);
				return tmp;
			}
        }

		/// <summary>
		/// Returns the cell to the right of me.
		/// </summary>
		/// <value>The right cell.</value>
		public NavCell RightCell
		{
			get
			{
				NavCell tmp = MobileAI.GetCell((m_id + 1) * 2);
				return tmp;
			}
        }
	}
}