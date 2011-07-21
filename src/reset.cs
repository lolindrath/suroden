using System;

namespace Suroden
{
	/// <summary>
	/// Summary description for reset.
	/// </summary>
	public class Reset
	{
		private string uid;
		private ResetType type;
		private string room;
		private int localLimit;
		private int globalLimit;

		public Reset()
		{
			
		}

		public string UID
		{
			get { return uid; }
			set { uid = value; }
		}

		public ResetType Type
		{
			get { return type; }
			set { type = value; }
		}

		public string Room
		{
			get { return room; }
			set { room = value; }
		}

		public int LocalLimit
		{
			get { return localLimit; }
			set { localLimit = value; }
		}

		public int GlobalLimit
		{
			get { return globalLimit; }
			set { globalLimit = value; }
		}
	}

	public enum ResetType
	{
		MobileToRoom,
		ObjectToRoom,
		ObjectToMobileInv,
		ObjectToMobileEquip
	}
}
