#region Using Namespace
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MobileAIFramework;
#endregion

namespace MobileAIFramework.Navigation.Pathfinding
{
	public class NavNode : NavCell, IComparable<NavNode>
	{
		//The parent node
		private	NavNode m_parent;
		public NavNode Parent
		{ 
			get { return m_parent; }
			set { m_parent = value; }
		}
		
		//Moving cost for the node
		private float m_movingCost;
		public float MovingCost
		{
			get { return m_movingCost; }
		}

		//Create an instance of a pathfinding node
		public NavNode(NavCell cell, NavNode parent, float movingCost) : 
			base(cell.Id, cell.IsWalkable, cell.Center)
		{		
			m_parent 	 = parent;
			m_movingCost = movingCost;
		}
		
		//Concatenates two NavNodes together
		public static NavNode Concatenate(NavNode first, NavNode second)
		{
			NavNode concat = first;
			int lastIndex = 0;
			
			while (concat.Parent != null)
			{
				concat = concat.Parent;
				lastIndex = concat.Id;
			}
			
			if (second.Id != lastIndex)
				concat.Parent = second;
			else
				concat.Parent = second.Parent;
			
			return first;
		}
		
		//Method to sort the node
		public int CompareTo(NavNode toCompare)
		{
			if (toCompare == null)
				return 1;
				
			return this.MovingCost.CompareTo(toCompare.MovingCost);
		}
		
		//Redefines the Equal operator, to check only over the NavNode id
		public override bool Equals (object obj)
		{
			//If it is null, return
			if (obj == null)
			{
				return false;
			}
			
			//Cast the obj as NavNode
			NavNode node = obj as NavNode;
			if (node == null)
			{
				return false;
			}
			
			// Return true if the id matches:
			return this.Id == node.Id;
		}
		
		//Get Hash Information
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}