using System;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
	/// <summary>
	/// Summary description for thing.
	/// </summary>
	public class Thing : ICloneable
	{
		private string uid;
		private string name;
		private string shortDesc;
		private Room currentRoom;
		private int level;
		private List<Thing> inventory = new List<Thing>();

		public Thing()
		{
			
		}

		public string UID
		{
			get { return uid; }
			set { uid = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string ShortDesc
		{
			get { return shortDesc; }
			set { shortDesc = value; }
		}

		public Room CurrentRoom
		{
			get { return currentRoom; }
			set { currentRoom = value; }
		}

		public int Level
		{
			get { return level; }
			set { level = value; }
		}

		public List<Thing> Inv
		{
			get { return inventory; }
		}

		#region ICloneable Members

		public object Clone()
		{
			Thing temp = new Thing();

			temp.UID = this.UID;
			temp.Name = this.Name;
			temp.ShortDesc = this.ShortDesc;
			temp.CurrentRoom = this.CurrentRoom;
			temp.Level = this.Level;

			return temp;
		}

		#endregion
	}
}
