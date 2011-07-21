// created on 8/5/2003 at 2:52 PM

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
///<summary>
///The world contains all the area's that make up the world
///</summary>
public class World
{
	private List<Area> areaList = null;
	private Dictionary<string, Room> roomList = null;
	
	///<summary>
	///Initializes the fields.
	///</summary>
	public World()
	{
		areaList = new List<Area>();
		roomList = new Dictionary<string, Room>();
	}
	
	///<summary>
	///Reads in the area file and makes the area's load themselves.
	///</summary>
	///<param name="file">the world file to load</param>
	public void Load(string file)
	{
		Log.Info("Loading World...");
		XmlTextReader reader = new XmlTextReader(file);
		
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName.Equals("AreaFile"))
				{
					areaList.Add(new Area(reader.ReadString()));
				}
			}
		}
	}
	
	///<summary>
	///The list of all rooms in the world
	///</summary>
	///<value>the list of all rooms in the world.</value>
	public Dictionary<string, Room> RoomList
	{
		get { return roomList; }
	}

	public List<Area> AreaList
	{
		get { return areaList; }
	}
	
	///<summary>
	///This turns the TempExits into real Exits (references to rooms)
	///</summary>
	///<remarks>
	///We can't do this right away, it has to be done after the load process
	///since we can't guarentee that every room will exist.
	///</remarks>
	public void FixExits()
	{
		Log.Info("Fixing Room Exits...");
		int exits = 0;
		int badExits = 0;
		foreach(Room r in roomList.Values)
		{
			for(int i = 0; i < Directions.MAX_DIR; ++i)
			{
				if(r.TempExits[i] != null && r.TempExits[i].Length > 0)
				{
					exits++;
					Room tempRoom = (Room)roomList[r.TempExits[i]];
					
					if(tempRoom == null)
					{
						badExits++;
						Log.Warn("Fix Exits: bad exit for " + i + " in " + r.UID);
					}
					else
					{
						r.Exits[i] = tempRoom;
					}
				}//end if
			}//end for
		}//end foreach
		Log.Info("Finished Fixing Exits: " + exits + " fixed, " + badExits + " bad exits.");
	}
}
}
