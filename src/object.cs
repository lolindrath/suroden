using System;

namespace Suroden
{
	/// <summary>
	/// Summary description for Object.
	/// </summary>
	public class Object : Thing, ICloneable
	{
		public Object()
		{

		}
		#region ICloneable Members

		public new object Clone()
		{
			Object temp = new Object();

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
