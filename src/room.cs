// created on 8/5/2003 at 2:53 PM

using System;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
///<summary>
///This is a little static helper class to keep track of the available exits.
///</summary>
public class Directions
{
	///<summary>This defines the maximum number of directions.</summary>
	public const int MAX_DIR = 10;
	///<summary>The definition of up.</summary>
	public const int DIR_UP = 0;
	///<summary>The definition of down.</summary>
	public const int DIR_DOWN = 1;
	///<summary>The definition of north.</summary>
	public const int DIR_NORTH = 2;
	///<summary>The definition of south.</summary>
	public const int DIR_SOUTH = 3;
	///<summary>The definition of east.</summary>
	public const int DIR_EAST = 4;
	///<summary>The definition of west.</summary>
	public const int DIR_WEST = 5;
	///<summary>The definition of northeast.</summary>
	public const int DIR_NORTHEAST = 6;
	///<summary>The definition of northwest.</summary>
	public const int DIR_NORTHWEST = 7;
	///<summary>The definition of southeast.</summary>
	public const int DIR_SOUTHEAST = 8;
	///<summary>The definition of southwest.</summary>
	public const int DIR_SOUTHWEST = 9;
	
	///<summary>
	///If you pass this your DIR_* number it will give you the string that matches that name.
	///</summary>
	public readonly static string[] DIR_NAMES = new string[]{"Up", "Down", "North", "South", "East",
	"West", "Northeast", "Northwest", "Southeast", "Southwest" };
	
	///<summary>
	///If you pass this your DIR_* number it will find the exact opposite (e.x. if you're facing
	///north it'll pick south).
	///</summary>
	public readonly static int[] DIR_REVERSE = new int[]{DIR_DOWN, DIR_UP, DIR_SOUTH, DIR_NORTH, 
	DIR_WEST, DIR_EAST, DIR_SOUTHWEST, DIR_SOUTHEAST, DIR_NORTHWEST, DIR_NORTHEAST};
	
	///<summary>
	///This takes the name of a directory and translates it back to the number
	///</summary>
	private static Dictionary<string, int> namesToDir = null;
	
	///<summary>
	///The default constructor basically just initializes the NamesToDir Hashtable
	///</summary>
	static Directions()
	{
		namesToDir = new Dictionary<string, int>();
		
		for(int i = 0; i < MAX_DIR; ++i)
		{
			namesToDir.Add(DIR_NAMES[i].ToLower(), i);
		}
		
		//Abbrevations for the two word directions
		namesToDir.Add("ne", DIR_NORTHEAST);
		namesToDir.Add("nw", DIR_NORTHWEST);
		namesToDir.Add("se", DIR_SOUTHEAST);
		namesToDir.Add("sw", DIR_SOUTHWEST);
	}
	
	///<summary>
	///This takes the name of a directory and translates it back to the number
	///</summary>
	///<value>Returns the Hashtable that maps a name to the DIR_* number.</value>
	public static Dictionary<string, int> NamesToDir
	{
		get { return namesToDir; }
	}

}

	public enum DoorState
	{
		Open,
		Closed,
		Locked
	}
	
///<summary>
///Room represents one square of land in the world that a player, mobs and objects
///can occupy.
///</summary>
public class Room
{
	///<summary>The unique ID</summary>
	private string uID;
	
	///<summary>The room title</summary>
	private string title;
	
	///<summary>The room description</summary>
	private string description;
	///<summary>The parent area's information.</summary>
	private Area parentArea;
	///<summary>Links to the adjecent rooms.</summary>
	private Room[] exits = new Room[Directions.MAX_DIR];
	///<summary>Temporary links that hold the UID's of the adjecent rooms and are later translated into References</summary>
	private string[] tempExits = new string[Directions.MAX_DIR];
	///<summary>A list of the players that are currently in this room.</summary>
	private List<Thing> containedList = new List<Thing>();

	private DoorState[] doorStates = new DoorState[Directions.MAX_DIR];
	
	///<summary>
	///The default constructor wipes the Exits and TempExits clean.
	///</summary>
	public Room()
	{
		for(int i = 0; i < Directions.MAX_DIR; ++i)
		{
			exits[i] = null;
			tempExits[i] = null;
			doorStates[i] = DoorState.Open;
		}
	}
	
	///<summary>
	///This retrieves the UID
	///</summary>
	///<value>returns a string with the room's unique ID in it.</value>
	public string UID
	{
		get { return uID; }
		set { uID = value; }
	}
	
	///<summary>
	///Retrieves the rooms title.
	///</summary>
	///<value>returns a string containing the summarized room title</value>
	public string Title
	{
		get { return title; }
		set { title = value; }
	}
	
	///<summary>
	///The rooms description
	///</summary>
	///<value>This contains a multi-line string that describes a room in more detail than the title.</value>
	public string Description
	{
		get { return description; }
		set { description = value; }
	}
	
	///<summary>
	///Represents the area the room is in.
	///</summary>
	///<value>returns the area the room is in.</value>
	public Area ParentArea
	{
		get { return parentArea; }
		set { parentArea = value; }
	}
	
	///<summary>
	///Represents the Room Links to the adjecent rooms
	///</summary>
	///<value>returns an array keyed on the Directions.DIR_* numbers</value>
	public Room[] Exits
	{
		get { return exits; }
		set { exits = value; }
	}
	
	///<summary>
	///Represents the temporary UID links to the rooms.
	///</summary>
	///<value>returns an array keyed on the Directions.DIR_* numbers</value>
	public string[] TempExits
	{
		get { return tempExits; }
		set { tempExits = value; }
	}
	
	public DoorState[] DoorStates
	{
		get { return doorStates;}
		set { doorStates = value;}
	}

	///<summary>
	///Returns a list of players currently inside this room
	///</summary>
	///<value>Returns a list of players currently inside this room.</value>
	public List<Thing> ContainedList
	{
		get { return containedList; }
		set { containedList = value; }
	}
	
	///<summary>
	///This prints out a summarized bit of text about the room
	///mostly for debugging purposes.
	///</summary>
	///<returns>A string containing all the summarized data in a room</returns>
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		
		sb.Append("RoomUID: ");
		sb.Append(uID);
		sb.Append("\nRoomTitle: ");
		sb.Append(title);
		sb.Append("\nRoomDescription: ");
		sb.Append(description);
		sb.Append("\nParentArea: ");
		sb.Append(parentArea.Name);
		
		for(int i = 0; i < Directions.MAX_DIR; ++i)
		{
			if(exits[i] != null)
			{
				sb.Append("\nExit: ");
				sb.Append(Directions.DIR_NAMES[i]);
				sb.Append("=");
				sb.Append(exits[i].UID);
			}
		}
		
		sb.Append("\n");
		return sb.ToString();
	}
	
	///<summary>
	///Send a message to everyone in the room except the target, then send
	///a customized message to the target.
	///</summary>
	///<param name="pWriter">The target of the message, the person who's writing the action.</param>
	///<param name="everyone">The message that should go to everyone in the room, except the pWriter</param>
	///<param name="you">The message for just the pWriter.</param>
	public void WriteAll(Player pWriter, string everyone, string you)
	{
		foreach(Thing t in containedList)
		{
			if(t is Player)
			{
				Player p = (Player)t;
				if(p.PlayerState == PlayerStates.Playing)
				{
					if(p == pWriter)
					{
						p.OutBuffer.Append(you);
					}
					else
					{
						p.OutBuffer.Append(everyone);
					}
				}
			}
		}//end foreach
	}

	public Object FindObject(Player p, string name)
	{
		foreach(Thing t in containedList)
		{
			if(t.Name.ToLower() == name.ToLower())
			{
				if(t is Object)
					return (Object)t;
			}
		}
		return null;
	}

}//end class
}//end namespace
