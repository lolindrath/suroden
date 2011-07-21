// created on 8/5/2003 at 3:03 PM
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
///<summary>
///This represents an area, a subsection of  the world.
///</summary>
public class Area
{
	///<summary>The area's name.</summary>
	private string name = "";

	///<summary>The list of rooms contained in the area.</summary>
	private Dictionary<string, Room> roomList = null;
	
	private Dictionary<string, Mob> mobList = null;

	private Dictionary<string, Object> objectList = null;

	private Dictionary<string, Reset> resetList = null;

	///<summary>
	///This reads in the area file and loads up all the room data.
	///</summary>
	///<param name="file">This is the area file to read from</param>
	public Area(string file)
	{
		roomList = new Dictionary<string, Room>();
		mobList = new Dictionary<string, Mob>();
		resetList = new Dictionary<string, Reset>();
		objectList = new Dictionary<string, Object>();

		XmlTextReader reader = new XmlTextReader(file);
		while (reader.Read())
		{
			
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if (reader.LocalName.Equals("AreaData") && reader.NodeType == XmlNodeType.Element)
				{
					name = ReadAreaData(reader);
					Log.Info("Loading area " + name + "...");
				}

				if(reader.LocalName.Equals("Rooms") && reader.NodeType == XmlNodeType.Element)
				{
					Log.Info("Loading Rooms...");
					ReadRooms(reader, this);
					Log.Info("Loaded " + roomList.Count + " Rooms.");
				}

				if(reader.LocalName.Equals("Mobs") && reader.NodeType == XmlNodeType.Element)
				{
					Log.Info("Loading Mobs...");
					ReadMobs(reader, this);
					Log.Info("Loaded " + mobList.Count + " Mobs.");
				}

				if(reader.LocalName.Equals("Objects") && reader.NodeType == XmlNodeType.Element)
				{
					Log.Info("Loading Objects...");
					ReadObjects(reader, this);
					Log.Info("Loaded " + objectList.Count + " Objects.");
				}

				if(reader.LocalName.Equals("Resets") && reader.NodeType == XmlNodeType.Element)
				{
					Log.Info("Loading Resets...");
					ReadResets(reader, this);
					Log.Info("Loaded " + resetList.Count + " Resets.");
				}
			}
		}//end while
		Log.Info("Finished loading area '" + name + "' : " + roomList.Count.ToString() + " rooms found.");
	}

//end Area(string)

	///<summary>
	///The area's name
	///</summary>
	///<value>a string containing the area's name.</value>
	public string Name
	{
		get{return name;}
		set{name = value;}
	}

	public Dictionary<string, Room> RoomList
	{
		get { return roomList; }
	}

	public Dictionary<string, Mob> MobList
	{
		get { return mobList; }
	}

	public Dictionary<string, Object> ObjectList
	{
		get { return objectList; }
	}

	public Dictionary<string, Reset> ResetList
	{
		get { return resetList; }
	}

	private static string ReadAreaData(XmlReader reader)
	{
		string name = "";
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if (reader.LocalName.Equals("AreaName") && reader.NodeType == XmlNodeType.Element)
				{
					name = reader.ReadString();
				}
				else if(reader.LocalName.Equals("AreaData") && reader.NodeType == XmlNodeType.EndElement)
				{
					return name;
				}
			}
		}

		return name;
	}

	private static void ReadRooms(XmlReader reader, Area area)
	{
		Room tempRoom = null;

		int lastDir = -1;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName.Equals("Room") && reader.NodeType == XmlNodeType.Element)
				{
					tempRoom = new Room();
					tempRoom.ParentArea = area;
				}

				if(reader.LocalName.Equals("UID") && reader.NodeType == XmlNodeType.Element)
				{
					tempRoom.UID = reader.ReadString();
				}
				
				if(reader.LocalName.Equals("Title") && reader.NodeType == XmlNodeType.Element)
				{
					tempRoom.Title = reader.ReadString();
				}
				
				if(reader.LocalName.Equals("Description") && reader.NodeType == XmlNodeType.Element)
				{
					tempRoom.Description = reader.ReadString();
					tempRoom.Description = tempRoom.Description.Replace("\n", " ");
					tempRoom.Description = tempRoom.Description.Replace("\t", "");
					tempRoom.Description = tempRoom.Description.Replace("  ", "");
				}
				
				if(reader.LocalName.Equals("North") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_NORTH;
					tempRoom.TempExits[Directions.DIR_NORTH] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("South") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_SOUTH;
					tempRoom.TempExits[Directions.DIR_SOUTH] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("East") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_EAST;
					tempRoom.TempExits[Directions.DIR_EAST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("West") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_WEST;
					tempRoom.TempExits[Directions.DIR_WEST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("NorthEast") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_NORTHEAST;
					tempRoom.TempExits[Directions.DIR_NORTHEAST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("NorthWest") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_NORTHWEST;
					tempRoom.TempExits[Directions.DIR_NORTHWEST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("SouthEast") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_SOUTHEAST;
					tempRoom.TempExits[Directions.DIR_SOUTHEAST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("SouthWest") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_SOUTHWEST;
					tempRoom.TempExits[Directions.DIR_SOUTHWEST] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("Up") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_UP;
					tempRoom.TempExits[Directions.DIR_UP] = reader.GetAttribute("uid");
				}
				
				if(reader.LocalName.Equals("Down") && reader.NodeType == XmlNodeType.Element)
				{
					lastDir = Directions.DIR_DOWN;
					tempRoom.TempExits[Directions.DIR_DOWN] = reader.GetAttribute("uid");
				}

				if(reader.LocalName.Equals("DoorState") && reader.NodeType == XmlNodeType.Element)
				{
					tempRoom.DoorStates[lastDir] = (DoorState) Enum.Parse(typeof(DoorState), reader.ReadString());
				}

				if(reader.LocalName.Equals("KeyUID") && reader.NodeType == XmlNodeType.Element)
				{
					//TODO: implement keys
				}

				if(reader.LocalName.Equals("Room") && reader.NodeType == XmlNodeType.EndElement)
				{
					area.RoomList.Add(tempRoom.UID, tempRoom);
					Mud.mudSuroden.WorldSuroden.RoomList.Add(tempRoom.UID, tempRoom);
				}

				if(reader.LocalName.Equals("Rooms") && reader.NodeType == XmlNodeType.EndElement)
				{
					return;
				}
			}
		}

		return;
	}

	private static void ReadMobs(XmlReader reader, Area area)
	{
		Mob tempMob = null;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName.Equals("Mob") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob = new Mob();
				}

				if(reader.LocalName.Equals("Mob") && reader.NodeType == XmlNodeType.EndElement)
				{
					area.MobList.Add(tempMob.UID,  tempMob);
				}

				if(reader.LocalName.Equals("UID") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob.UID = reader.ReadString();
				}

				if(reader.LocalName.Equals("Name") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob.Name = reader.ReadString();
				}

				if(reader.LocalName.Equals("ShortDesc") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob.ShortDesc = reader.ReadString();
				}

				if(reader.LocalName.Equals("Level") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob.Level = Convert.ToInt32(reader.ReadString());
				}

				if(reader.LocalName.Equals("Gender") && reader.NodeType == XmlNodeType.Element)
				{
					tempMob.Gender = (Gender) Enum.Parse(typeof(Gender), reader.ReadString(), false);
				}

				if(reader.LocalName.Equals("Mobs") && reader.NodeType == XmlNodeType.EndElement)
				{
					return;
				}
			}
		}

		return;
	}

	private void ReadObjects(XmlTextReader reader, Area area)
	{
		Object tempObj = null;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName.Equals("Object") && reader.NodeType == XmlNodeType.Element)
				{
					tempObj = new Object();
				}

				if(reader.LocalName.Equals("UID") && reader.NodeType == XmlNodeType.Element)
				{
					tempObj.UID = reader.ReadString();
				}

				if(reader.LocalName.Equals("Name") && reader.NodeType == XmlNodeType.Element)
				{
					tempObj.Name = reader.ReadString();
				}

				if(reader.LocalName.Equals("ShortDesc") && reader.NodeType == XmlNodeType.Element)
				{
					tempObj.ShortDesc = reader.ReadString();
				}

				if(reader.LocalName.Equals("Level") && reader.NodeType == XmlNodeType.Element)
				{
					tempObj.Level = Convert.ToInt32(reader.ReadString());
				}

				if(reader.LocalName.Equals("Object") && reader.NodeType == XmlNodeType.EndElement)
				{
					area.ObjectList.Add(tempObj.UID,  tempObj);
				}

				if(reader.LocalName.Equals("Objects") && reader.NodeType == XmlNodeType.EndElement)
				{
					return;
				}
			}
		}
	}

	public static void ReadResets(XmlReader reader, Area area)
	{
		while(reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName.Equals("Mob") && reader.NodeType == XmlNodeType.Element)
				{
					ReadMobResets(reader, area);
				}
				else if(reader.LocalName == "Object" && reader.NodeType == XmlNodeType.Element)
				{
					ReadObjectResets(reader, area);
				}																		  
				else if(reader.LocalName.Equals("Object") && reader.NodeType == XmlNodeType.Element)
				{
					
				}
				else if(reader.LocalName.Equals("Resets") && reader.NodeType == XmlNodeType.EndElement)
				{
					return;
				}
			}
		}
	}

	public static void ReadMobResets(XmlReader reader, Area area)
	{
		Reset tempReset = new Reset();
		tempReset.Type = ResetType.MobileToRoom;

		while(reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName.Equals("Mob") && reader.NodeType == XmlNodeType.EndElement)
				{
					area.ResetList.Add(tempReset.UID, tempReset);
					return;
				}
				else if(reader.LocalName.Equals("UID") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.UID = reader.ReadString();
				}
				else if(reader.LocalName.Equals("Room") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.Room = reader.ReadString();
				}
				else if(reader.LocalName.Equals("LocalLimit") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.LocalLimit = Convert.ToInt32(reader.ReadString());
				}
				else if(reader.LocalName.Equals("GlobalLimit") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.GlobalLimit = Convert.ToInt32(reader.ReadString());
				}
			}
		}
	}

	public static void ReadObjectResets(XmlReader reader, Area area)
	{
		Reset tempReset = new Reset();
		tempReset.Type = ResetType.ObjectToRoom;

		while(reader.Read())
		{
			if(reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement)
			{
				if(reader.LocalName == "Object" && reader.NodeType == XmlNodeType.EndElement)
				{
					area.ResetList.Add(tempReset.UID, tempReset);
					return;
				}
				else if(reader.LocalName.Equals("UID") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.UID = reader.ReadString();
				}
				else if(reader.LocalName.Equals("Room") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.Room = reader.ReadString();
				}
				else if(reader.LocalName.Equals("LocalLimit") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.LocalLimit = Convert.ToInt32(reader.ReadString());
				}
				else if(reader.LocalName.Equals("GlobalLimit") && reader.NodeType == XmlNodeType.Element)
				{
					tempReset.GlobalLimit = Convert.ToInt32(reader.ReadString());
				}
			}
		}
	}
}
}
 