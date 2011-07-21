namespace Suroden
{
	///<summary>
	///The chat command sends a message to all users, globally.
	///</summary>
	public class ChatCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public ChatCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This gets the users message and passes it on to everyone currently
		///playing.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			string you = "You chat: {x{o{y\'{x{o{b" + cp.Arguments + "{x{o{y\'{x\n\r";
			string everyone = "\n\r" + p.Name + " chats: {o{y\'{x{o{b" + cp.Arguments + "{x{o{y\'{x\n\r";
			
			Mud.mudSuroden.writeAll(p, everyone, you);
		}
	}
	
	///<summary>
	///This implements the quit command which prints a thankyou message and
	///kicks the user off the system.
	///</summary>
	public class QuitCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public QuitCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This prints a thankyou message to the user then marks them
		///as trying to quit so the main loop can disconnect and delete them.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			string you = "{o{yThanks for playing Suroden.{x\n\r";
			string everyone = "\n\r" + p.Name + " has just quit.\n\r";
			
			Mud.mudSuroden.writeAll(p, everyone, you);
			p.PlayerState = PlayerStates.Quitting;
		}
	}
	
	///<summary>
	///This class implements the who command allowing a player to see who is online
	///at any given time.
	///</summary>
	public class WhoCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public WhoCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This prints out a list of all the players currently signed on.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			p.OutBuffer.Append("{o{bPeople Currently on:{x\n\r");
			foreach(Player player in Mud.mudSuroden.PlayerList)
			{
				if(player.PlayerState == PlayerStates.Playing)
				{
					p.OutBuffer.Append(player.Name);
					p.OutBuffer.Append(" ");
					p.OutBuffer.Append(player.ShortDesc);
					p.OutBuffer.Append("\n\r");
				}
			}
		}
	}
	
	///<summary>
	///This class implements the commands command. It displays all the 
	///current commands available to a user at this time.
	///</summary>
	public class CommandsCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public CommandsCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This loops through the command list and prints all the commands
		///available for a player to use.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			int col = 0;
			p.OutBuffer.Append("{o{b--------------------------------[ {wCommands{b ]-------------------------------{x\n\r\n\r");
			foreach(BaseCommand c in Mud.mudSuroden.CommandList)
			{
				p.OutBuffer.AppendFormat("{0,-10}", c.CommandVerb);
				p.OutBuffer.Append("    ");
				if(++col % 4 == 0)
				{
					p.OutBuffer.Append("\n\r");
				}
			}
			
			if(col % 4 != 0)
			{
				p.OutBuffer.Append("\n\r");
			}
		}
	}
	
	///<summary>
	///This class generalizes all the moving commands. It takes care of
	///north, south, east, west, up, down, northeast, northwest, southeast, southwest.
	///</summary>
	public class MoveCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public MoveCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This generalizes player movement by seeing what direction the specific
		///MoveCommand object was invoked for.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			int dir = Directions.NamesToDir[CommandVerb.ToLower()];
			if(p.CurrentRoom.Exits[dir] == null)
			{
				p.OutBuffer.Append("You can't go that way.\n\r");
			}
			else if(p.CurrentRoom.DoorStates[dir] == DoorState.Closed)
			{
				p.OutBuffer.Append("The door is closed.\n\r");
			}
			else if(p.CurrentRoom.DoorStates[dir] == DoorState.Locked)
			{
				p.OutBuffer.Append("The door is locked.\n\r");
			}
			else
			{
				p.CurrentRoom.WriteAll(p, p.Name + " moves " + Directions.DIR_NAMES[dir] + ".\n\r", "You move  " + Directions.DIR_NAMES[dir] + ".\n\r");
				p.CurrentRoom.ContainedList.Remove(p);
				
				p.CurrentRoom = p.CurrentRoom.Exits[dir];
				p.CurrentRoom.ContainedList.Add(p);
				
				p.CurrentRoom.WriteAll(p, p.Name + " has arrived from " + Directions.DIR_NAMES[Directions.DIR_REVERSE[dir]] + ".\n\r", null);
				Mud.mudSuroden.CommandList.findCommand("look").doCommand(p, new CommandParser(""));

				p.Dirty = true;
			}
		}
	}
	
	///<summary>
	///This class implements the look command which allows a player to
	///see the rooms description and who's in it.
	///</summary>
	public class LookCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public LookCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This prints out the description and title of the room plus a list
		///of the players inside.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			p.OutBuffer.Append("{o{y");
			p.OutBuffer.Append(p.CurrentRoom.Title);
			p.OutBuffer.Append("{x\n\r{o{w");
			p.OutBuffer.Append(p.CurrentRoom.Description);
			p.OutBuffer.Append("{x\n\r\n\r");
			
			//print obvious exits
			Mud.mudSuroden.CommandList.findCommand("exits").doCommand(p, new CommandParser("exits auto"));
			
			//print people in this room
			p.OutBuffer.Append("{o{yYou see here:{x\n\r");
			
			int playerCount = 0;
			foreach(Thing t in p.CurrentRoom.ContainedList)
			{
				if(t != p)
				{
					p.OutBuffer.Append("{o{w");
					p.OutBuffer.Append(t.Name);
					p.OutBuffer.Append(" ");
					p.OutBuffer.Append(t.ShortDesc);
					p.OutBuffer.Append("{x\n\r");
					playerCount++;
				}
			}
			
			if(playerCount == 0)
			{
				p.OutBuffer.Append("{o{wJust you, what a lonely world.{x\n\r");
			}
			
			p.OutBuffer.Append("\n\r");
		}
	}
	
	///<summary>
	///This class implements the exits command which displays all the possible
	///exits to a user.
	///</summary>
	public class ExitsCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public ExitsCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This loops through all the exits for the room the player is currently
		///in and prints them.
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			bool auto = false;
			if(cp.getArgument() == "auto")
			{
				auto = true;
			}
			
			if(auto)
			{
				p.OutBuffer.Append("{o{y[Exits");
			}
			else
			{
				p.OutBuffer.Append("{o{yExits from here are:{x\n\r");
			}
			for(int i = 0; i < Directions.MAX_DIR; ++i)
			{
				if(p.CurrentRoom.Exits[i] != null)
				{
					if(auto)
					{
						p.OutBuffer.AppendFormat(" {0}", Directions.DIR_NAMES[i]);
					}
					else
					{
						p.OutBuffer.AppendFormat("{{o{{w{0}:{{x {{o{{r{1}{{x\n\r", Directions.DIR_NAMES[i], p.CurrentRoom.Exits[i].Title);
					}
				}
			}
			
			if(auto)
			{
				p.OutBuffer.Append("]{x\n\r");
			}
		}
	}
	
	///<summary>
	///This command will serialize a player to their xml file so they can
	///reload from that state next time.
	///</summary>
	public class SaveCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public SaveCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///This invokes the serializer
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			p.Save();
			p.OutBuffer.Append("{o{ySave Successful{x\n\r");
		}
	}
	
	///<summary>
	///This allows players to set their own short description
	///</summary>
	public class ShortDescCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public ShortDescCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///set the short description
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			p.ShortDesc = cp.Arguments;
			p.OutBuffer.Append("{o{bYour short description was set to: ");
			p.OutBuffer.Append(p.ShortDesc);
			p.OutBuffer.Append("{x\n\r");

			p.Dirty = true;
		}
	}
	
	///<summary>
	///This will let one player talk directly to another player
	///</summary>
	public class SayCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public SayCommand(string pCommandVerb):base(pCommandVerb)
		{
		}
		
		///<summary>
		///set the short description
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			string target = cp.getArgument();
			Player targetPlayer = null;
			foreach(Player player in Mud.mudSuroden.PlayerList)
			{
				if(target.ToLower() == player.Name.ToLower().Substring(0, target.Length))
				{
					targetPlayer = player;
				}
			}
			
			if(targetPlayer == null)
			{
				p.OutBuffer.Append(target);
				p.OutBuffer.Append(" doesn't exist.\n\r");
			}
			else if(targetPlayer == p)
			{
				p.OutBuffer.Append("{o{bTalking to ourself again?{x\n\r");
			}
			else
			{
				p.OutBuffer.Append("{o{bYou say to ");
				p.OutBuffer.Append(targetPlayer.Name);
				p.OutBuffer.Append(" '{x{o{y");
				p.OutBuffer.Append(cp.RemainingArguments);
				p.OutBuffer.Append("{x{o{b'{x\n\r");
				
				targetPlayer.OutBuffer.Append("\n\r{o{b");
				targetPlayer.OutBuffer.Append(p.Name);
				targetPlayer.OutBuffer.Append(" says '{x{o{y");
				targetPlayer.OutBuffer.Append(cp.RemainingArguments);
				targetPlayer.OutBuffer.Append("{x{o{b'{x\n\r");
			}
		}
	}
	
	///<summary>
	///This shuts the mud down
	///</summary>
	public class ShutdownCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public ShutdownCommand(string pCommandVerb):base(pCommandVerb)
		{
			useLevel = Mud.IMP_LEVEL;
		}
		
		///<summary>
		///shutdown the mud properly
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			Mud.mudSuroden.MudRunning = false;
		}
	}

	///<summary>
	///This restarts the mud without players losing connections
	///</summary>
	public class CopyoverCommand : BaseCommand
	{
		///<summary>
		///The default constructor calls the parent constructor.
		///</summary>
		///<param name="pCommandVerb">
		///The command verb that called this Command Object.
		///</param>
		public CopyoverCommand(string pCommandVerb):base(pCommandVerb)
		{
			useLevel = Mud.IMP_LEVEL;
		}
		
		///<summary>
		///shutdown the mud properly
		///</summary>
		///<param name="p">The player who invoked the command.</param>
		///<param name="cp">The options the player sent along.</param>
		override public void doCommand(Player p, CommandParser cp)
		{
			p.OutBuffer.Append("Copyover doesn't work.\n\r");
			/*StreamWriter file = new StreamWriter("c:\\test.txt");
			foreach(Player player in Mud.mudSuroden.PlayerList)
			{
				file.WriteLine("{0} {1}", player.socket.Handle.ToInt32(), player.Name);
			}
			string[] args = new string[1];
			args[0] = term;
			int retval = Mud.execl("surodensolution.exe", args);

			retval = retval;*/
		}
	}

	public class OpenCommand : BaseCommand
	{
		public OpenCommand(string pCommandVerb):base(pCommandVerb)
		{
			
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			string direction = cp.getArgument();

			if(direction == "")
			{
				p.OutBuffer.Append("Open a door in which direction?\n\r");
			}
			else
			{
				int dir = Directions.NamesToDir[Mud.mudSuroden.CommandList.findCommand(direction).CommandVerb];

				if(p.CurrentRoom.Exits[dir] != null)
				{
					if(p.CurrentRoom.DoorStates[dir] == DoorState.Closed)
					{
						p.CurrentRoom.DoorStates[dir] = DoorState.Open;
						p.CurrentRoom.WriteAll(p, p.Name + " opens the " + Directions.DIR_NAMES[dir] + " door.\n\r", "You open the door.\n\r");

						//open other side
						p.CurrentRoom.Exits[dir].DoorStates[Directions.DIR_REVERSE[dir]] = DoorState.Open;

						p.CurrentRoom.Exits[dir].WriteAll(p, "The " + Directions.DIR_NAMES[Directions.DIR_REVERSE[dir]] + " door opens\n\r", "");
						
					}
					else if(p.CurrentRoom.DoorStates[dir] == DoorState.Locked)
					{
						p.CurrentRoom.WriteAll(p, p.Name + " tries to open a locked door\n\r", "The door doesn't budge, its locked.\n\r");
					}
					else if(p.CurrentRoom.DoorStates[dir] == DoorState.Open)
					{
						p.OutBuffer.Append("The door is already open.\n\r");
					}
					else
					{
						p.OutBuffer.Append("There isn't a door in that direction.\n\r");
					}
				}
			}
		}
	} //end OpenCommand

	public class CloseCommand : BaseCommand
	{
		public CloseCommand(string pCommandVerb):base(pCommandVerb)
		{
			
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			string direction = cp.getArgument();

			if(direction == "")
			{
				p.OutBuffer.Append("Close a door in which direction?\n\r");
			}
			else
			{
				int dir = Directions.NamesToDir[Mud.mudSuroden.CommandList.findCommand(direction).CommandVerb];

				if(p.CurrentRoom.Exits[dir] != null)
				{
					if(p.CurrentRoom.DoorStates[dir] == DoorState.Open)
					{
						p.CurrentRoom.DoorStates[dir] = DoorState.Closed;
						p.CurrentRoom.WriteAll(p, p.Name + " closes the " + Directions.DIR_NAMES[dir] + " door.\n\r", "You close the door.\n\r");

						//close other side
						p.CurrentRoom.Exits[dir].DoorStates[Directions.DIR_REVERSE[dir]] = DoorState.Closed;
						p.CurrentRoom.Exits[dir].WriteAll(p, "The " + Directions.DIR_NAMES[Directions.DIR_REVERSE[dir]] + " door closes\n\r", "");
						
					}
					else if(p.CurrentRoom.DoorStates[dir] == DoorState.Closed 
						|| p.CurrentRoom.DoorStates[dir] == DoorState.Locked)
					{
						p.OutBuffer.Append("The door is already closed.\n\r");
					}
					else
					{
						p.OutBuffer.Append("There isn't a door in that direction.\n\r");
					}
				}
			}
		}
	}//end CloseCommand

	public class GetCommand : BaseCommand
	{
		public GetCommand(string pCommandVerb):base(pCommandVerb)
		{
		
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			string name = cp.getArgument();

			if(name == "")
			{
				p.OutBuffer.Append("Get what?\n\r");
			}

			Object o = p.CurrentRoom.FindObject(p, name);
			
			if(o == null)
			{
				p.OutBuffer.Append("You can't seem to find that.\n\r");
			}
			else
			{
				p.CurrentRoom.WriteAll(p, p.Name + " picks up " + o.ShortDesc + "\n\r", "You pick up " + o.ShortDesc + "\n\r");

				p.CurrentRoom.ContainedList.Remove(o);

				p.Inv.Add(o);
			}
		}

	}//end GetCommand

	public class DropCommand : BaseCommand
	{
		public DropCommand(string pCommandVerb):base(pCommandVerb)
		{
			
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			string name = cp.getArgument();

			if(name == "")
			{
				p.OutBuffer.Append("Drop what?\n\r");
			}

			foreach(Object o in p.Inv)
			{
				if(o.Name.ToLower() == name.ToLower())
				{
					p.CurrentRoom.WriteAll(p, p.Name + " drops " + o.ShortDesc + "\n\r", "You drop " + o.ShortDesc + "\n\r");
					p.Inv.Remove(o);
					p.CurrentRoom.ContainedList.Add(o);
					return;
				}
			}

			p.OutBuffer.Append("You don't seem to have that.\n\r");
		}

	}//end InventoryCommand

	public class InventoryCommand : BaseCommand
	{
		public InventoryCommand(string pCommandVerb):base(pCommandVerb)
		{
			
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			if(p.Inv.Count == 0)
			{
				p.OutBuffer.Append("There's nothing in your inventory.\n\r");
			}
			else
			{
				p.OutBuffer.Append("In your inventory: \n\r");
				foreach(Object o in p.Inv)
				{
					p.OutBuffer.Append("    " + o.ShortDesc + "\n\r");
				}
			}
		}

	}//end InventoryCommand

	public class GiveCommand : BaseCommand
	{
		public GiveCommand(string pCommandVerb):base(pCommandVerb)
		{
			
		}

		public override void doCommand(Player p, CommandParser cp)
		{
			string what = cp.getArgument();
			string whom = cp.getArgument();

			if(what == "" || whom == "")
			{
				p.OutBuffer.Append("Give what to whom? Syntax: give <inv object> <player>\n\r");
			}

			Object found = null;
			foreach(Object o in p.Inv)
			{
				if(o.Name.ToLower() == what.ToLower())
				{
					found = o;
					break;
				}
			}

			if(found == null)
			{
				p.OutBuffer.Append("You don't seem to have that object.\n\r");
				return;
			}

			foreach(Thing t in p.CurrentRoom.ContainedList)
			{
				if(t.Name.ToLower() == whom.ToLower())
				{
					p.Inv.Remove(found);
					t.Inv.Add(found);

					p.CurrentRoom.WriteAll(p, p.Name + " gives " + t.Name + " " + found.Name + "\n\r", "You give " + t.Name + " " + found.Name + "\n\r");
				}
			}
		}

	}//end GiveCommand
}
