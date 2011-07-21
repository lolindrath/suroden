using System;
using System.Xml;

namespace Suroden
{
	/// <summary>
	/// Summary description for mob.
	/// </summary>
	public class Mob : Thing
	{
		private Gender gender = Gender.Neutral;

		public Mob()
		{
			Name = "Blank Mob";
		}

		public Gender Gender
		{
			get {return gender;}
			set {gender = value;}
		}

		public void ReadXml (XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if(reader.LocalName.Equals("Name"))
					{
						Name = reader.ReadString();
					}
					else if(reader.LocalName.Equals("PlayerGender"))
					{
						Gender = (Gender) Enum.Parse(typeof(Gender), reader.ReadString(), false);
					}
					else if(reader.LocalName.Equals("ShortDesc"))
					{
						ShortDesc = reader.ReadString();
					}
					else if(reader.LocalName.Equals("Level"))
					{
						Level = Convert.ToInt32(reader.ReadString());
					}
				}
			}
		}
		#region ICloneable Members

		public new object Clone()
		{
			Mob tempMob = new Mob();

			tempMob.UID = this.UID;
			tempMob.Name = this.Name;
			tempMob.ShortDesc = this.ShortDesc;
			tempMob.CurrentRoom = this.CurrentRoom;
			tempMob.Level = this.Level;
			tempMob.Gender = this.Gender;
			 
			return tempMob;
		}

		#endregion
	}
}
