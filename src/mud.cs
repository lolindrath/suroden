using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.InteropServices;

namespace Suroden
{
	/// <summary>
	/// This is the over-arching object that contains everyone.
	/// </summary>
	public class Mud
	{
		public List<Mob> mobInstances = new List<Mob>();
		public List<Object> itemInstances = new List<Object>();

		private TimerRegistry reg = new TimerRegistry();
		///<summary>The list of all players on the Mud currently.</summary>
		private List<Player> playerList;
		///<summary>The list of all available commands on the Mud.</summary>
		private CommandList commandList;
		///<summary>The TcpListener that runs the joint.</summary>
		private TcpListener mudTcpListener;
		///<summary>This determines whether or not the Mud will keep running.</summary>
		private bool mudRunning;
		///<summary>This is the ascii converter for outgoing data.</summary>
		public readonly Encoding ascii = Encoding.ASCII;
		///<summary>This holds the port the mud is currently connected to.</summary>
		private static int port = 4001;
		///<summary>This holds the whole world, containing all the areas and rooms.</summary>
		private World worldSuroden;
		///<summary>Here's the big man who runs the joint.</summary>
		static public Mud mudSuroden;
		///<summary>Defines the recieve chunk size.</summary>
		private const int RECV_LEN = 1024;
		/// <summary>Indicates at which level a player is considered immortal</summary>
		public const int IMM_LEVEL = 101;
		///<summary>Indicates at which level a player is considered an implementor</summary>
		public const int IMP_LEVEL = 1000;
		private const int MAX_NAME_LENGTH = 20;
		private const int MAX_BUFFER_LENGTH = 500000;
		public static char IAC = Convert.ToChar(255);
		public static char WILL = Convert.ToChar(251);
		public static char WONT = Convert.ToChar(252);
		public static char ECHO = Convert.ToChar(1);
		public static char TERM = Convert.ToChar(0);
		//public static string test = ((char)IAC|WONT) + ((char)ECHO|TERM);
		public static char[] TelOptEchoOff = new char[4]{IAC, WILL, ECHO, TERM};
		public static char[] TelOptEchoOn = new char[4]{IAC, WONT, ECHO, TERM};

		public const int PULSE_MOB_WANDER = 20 * PULSE_SECOND;
		public const int PULSE_MINUTE = PULSE_SECOND * 60;
		public const int PULSE_SECOND = 1000;

		private BadNameList badNames;

		public Random Random = null;
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			mudSuroden = new Mud(port);
			mudSuroden.loadCommands();
			mudSuroden.loadWorld();
			mudSuroden.LoadBadNames();
			mudSuroden.MudRunning = true;
			mudSuroden.Run(args);
			mudSuroden.Shutdown();
		}

		///<summary>
		///This is the main constructor for the Mud class
		///</summary>
		///<param name="pPort">The port to attach the Mud to</param>
		Mud(int pPort)
		{
			port = pPort;
			playerList = new List<Player>();
			commandList = new CommandList();
			mudTcpListener = new TcpListener(IPAddress.Any, port);
		}
		
		///<summary>
		///This is the workhorse function. All the sending and recieving of data
		///is done from here.
		///</summary>
		public void Run(string[] args)
		{
			DateTime lastTime = DateTime.Now;

			Log.Info("Starting Random Number Generator");
			Random = new MersenneTwister();
			

			mudTcpListener.Start();
			Log.Info("Suroden ready to rock on port " + port + "!");

			if(args.Length > 0 && args[0] == "copyover")
			{
				SetHandle(getSocket(), Convert.ToInt32(args[1]));
				mudSuroden.RecoverFromCopyover();
			}

			MobWanderAction wander = new MobWanderAction();
			wander.interval = PULSE_MOB_WANDER;
			wander.time = lastTime.TimeOfDay.TotalMilliseconds + wander.interval;
			reg.AddTimerAction(wander);

			//Handle world update stuff
			foreach(Area area in this.WorldSuroden.AreaList)
			{
				foreach(Reset reset in area.ResetList.Values)
				{
					Room room = null;
					switch(reset.Type)
					{
						case ResetType.MobileToRoom:
							room = (Room)this.WorldSuroden.RoomList[reset.Room];
							Mob mob = (Mob)area.MobList[reset.UID];
							Mob newMob = (Mob)mob.Clone();

							room.ContainedList.Add(newMob);
							newMob.CurrentRoom = room;
							mobInstances.Add(newMob);
							break;
						case ResetType.ObjectToRoom:
							room = (Room)this.WorldSuroden.RoomList[reset.Room];
							Object o = (Object)area.ObjectList[reset.UID];
							Object newObj = (Object)o.Clone();

							room.ContainedList.Add(newObj);
							newObj.CurrentRoom = room;
							itemInstances.Add(newObj);
							break;
					}
				}
			}

			while(mudRunning)
			{	
				//Check for incoming Connections
				if(mudTcpListener.Pending())
				{
					Player newPlayer = new Player(mudTcpListener.AcceptTcpClient());
					
					Log.Info("Accepting new connection from client.");
					playerList.Add(newPlayer);
					
					newPlayer.OutBuffer.Append("Welcome to the World of Suroden\n\r\n\rLogin (type new to create a new player): ");
				}
				
				//See if we can read from a player
				IEnumerator pEnum = playerList.GetEnumerator();
				while(pEnum.MoveNext())
				{
					Player p = (Player)pEnum.Current;
					//First try to kick them out if there bad
					if(p.canError() && p.Connected)
					{
						Log.Info("Bad connection from " + p.Name + " kicking them out.");
						p.PlayerState = PlayerStates.Quitting;
					}
					
					//Now try to read from them!
					if(p.canRead())
					{
						byte[] buff = new byte[RECV_LEN];
                        int bytesRead = 0;
                        try
                        {
                            bytesRead = p.Stream.Read(buff, 0, RECV_LEN);
                        }
                        catch (IOException e)
                        {
                            Log.Info(p.Name + " forcibly disconnected, kicking them out.");
                        }
						string s = Encoding.ASCII.GetString(buff, 0, bytesRead);
						
						if(bytesRead == 0)
						{
							//User disconnected
							Log.Info(p.Name + " disconnected, kicking them out.");
							p.PlayerState = PlayerStates.Quitting;
						}
						p.AppendInBuffer(s);
					}
				}
				
				//Handle world update stuff
				/*foreach(Area area in this.WorldSuroden.AreaList)
				{
					foreach(Reset reset in area.ResetList.Values)
					{
						switch(reset.Type)
						{
							case ResetType.MobileToRoom:
								Room room = (Room)this.WorldSuroden.RoomList[reset.Room];
								room.ContainedList.Add(area.MobList[reset.UID]);
								break;
						}
					}
				}*/

				MudUpdate();

				//Handle player states
				pEnum = playerList.GetEnumerator();
				while(pEnum.MoveNext())
				{
					Player p = (Player)pEnum.Current;
					switch(p.PlayerState)
					{
						case PlayerStates.LoggingIn:
							StringBuilder line = p.InBuffer;
							string name = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref name));
							
							if(name.Length > 0)
							{
								if(name == "new")
								{
									p.PlayerState = PlayerStates.NewPlayer;
									p.OutBuffer.Append("\n\rState Thy Name: ");
									break;
								}

								//Check if they are already logged in
								bool dupPlayer = false;
								foreach(Player tempPlayer in PlayerList)
								{
									if(name.ToLower() == tempPlayer.Name.ToLower())
									{
										Log.Warn("Duplicate " + tempPlayer.Name + " trying to sign on.");
										p.OutBuffer.Append("That person is already playing\n\r");
										p.PlayerState = PlayerStates.Quitting;
										dupPlayer = true;
									}
								}
								
								//we have a duplicate player so we need to skip the rest of this processing
								if(dupPlayer == true)
								{
									continue;
								}
								
								//Let's see if they have a Player file, otherwise there a new Player
								string file = Player.PLAYER_DIR + name.ToLower() + ".xml";
								if(File.Exists(file))
								{
									//attempt to load 'em
									XmlSerializer x = new XmlSerializer(typeof(Player));
									FileStream fs = new FileStream(file.ToString(), FileMode.Open);
									Player existingPlayer = (Player) x.Deserialize(fs);
									fs.Close();
									Log.Info(existingPlayer.Name + " just logged on.");
									existingPlayer.TcpClient = p.TcpClient;
						
									//switch over their InBuffer also
									existingPlayer.InBuffer = p.InBuffer;
									existingPlayer.OutBuffer = p.OutBuffer;
									existingPlayer.OutBuffer.Append("Password: ");
									existingPlayer.OutBuffer.Append(TelOptEchoOff);
									existingPlayer.PlayerState = PlayerStates.EnteringPassword;
									playerList.Remove(p);
									p = null;
									playerList.Add(existingPlayer);
									pEnum = playerList.GetEnumerator();
								}
								else
								{
									//TODO: Handle new players
									Log.Info("A New Player has just connected.");
									p.OutBuffer.Append("Welcome New Player!\n\rState Thy Name: ");
									p.PlayerState = PlayerStates.NewPlayer;
								}
								
							}
							break;
						case PlayerStates.NewPlayer:
							line = p.InBuffer;
							string newName = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref newName));
							
							if(newName.Length > 0)
							{
								p.Name = newName;
								
								string file = Player.PLAYER_DIR + newName.ToLower() + ".xml";
								if(File.Exists(file) || badNames.BadNames.ContainsKey(newName.ToLower()))
								{
									p.OutBuffer.Append("\n\rSorry, that's taken\n\rState Thy Name: ");
								}
								else if(newName.Length > MAX_NAME_LENGTH)
								{
									p.OutBuffer.Append("\n\rSorry, your name is too long.\n\r");
									p.OutBuffer.Append("Names should be shorter than ");
									p.OutBuffer.Append(MAX_NAME_LENGTH.ToString());
									p.OutBuffer.Append("\n\rState Thy Name: ");
								}
								else
								{
									p.OutBuffer.Append("Enter your sex (M/F/N): ");
									p.PlayerState = PlayerStates.EnterSex;
								}
							}

							break;
						case PlayerStates.EnterSex:
							line = p.InBuffer;
							string sex = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref sex));

							if(sex.Length > 0)
							{
								if(sex.ToUpper()[0] == 'M')
								{
									p.PlayerGender = Gender.Male;
									p.PlayerState = PlayerStates.NewPassword;
									p.OutBuffer.Append("\n\rEnter New Password: ");
								}
								else if(sex.ToUpper()[0] == 'F')
								{
									p.PlayerGender = Gender.Female;
									p.PlayerState = PlayerStates.NewPassword;
									p.OutBuffer.Append("\n\rEnter New Password: ");
								}
								else if(sex.ToUpper()[0] == 'N')
								{
									p.PlayerGender = Gender.Neutral;
									p.PlayerState = PlayerStates.NewPassword;
									p.OutBuffer.Append("\n\rEnter New Password: ");
								}
								else
								{
									p.OutBuffer.Append("Enter your sex (M/F/N): ");
								}
							}

							break;
						case PlayerStates.NewPassword:
							line = p.InBuffer;
							string newPassword = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref newPassword));
							
							if(newPassword.Length > 0)
							{
								p.Password = newPassword;
							
								p.PlayerState = PlayerStates.ConfirmPassword;
								p.OutBuffer.Append("\n\rConfirm Password: ");
								p.OutBuffer.Append(TelOptEchoOff);
							}
							break;
						case PlayerStates.ConfirmPassword:
							line = p.InBuffer;
							string confirmPassword = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref confirmPassword));

							if(confirmPassword.Length > 0)
							{
								if(p.Password == confirmPassword)
								{
									p.PlayerState = PlayerStates.Playing;
									p.OutBuffer.Append(TelOptEchoOn);
									p.OutBuffer.Append("\n\rWelcome to Suroden!\n\r");
									p.CurrentRoom = (Room)Mud.mudSuroden.WorldSuroden.RoomList["Test1"];
									p.CurrentRoom.ContainedList.Add(p);
									interpret(p, "look");
								}
								else
								{
									p.OutBuffer.Append("\n\rPasswords to not match\n\rEnter New Password: ");
									p.PlayerState = PlayerStates.NewPassword;
								}
							}
							break;
						case PlayerStates.EnteringPassword:
							line = p.InBuffer;
							string password = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref password));
							
							if(password.Length > 0)
							{
								if(password == p.Password)
								{
									p.OutBuffer.Append("Congratulations, You're in\n\r");
									writeAll(p, "\n\r" + p.Name + " has just logged in.\n\r", "");
									p.PlayerState = PlayerStates.Playing;
									if(p.CurrentRoom == null)
									{
										p.CurrentRoom = (Room)Mud.mudSuroden.WorldSuroden.RoomList["Test1"];
									}
									p.CurrentRoom.ContainedList.Add(p);
									interpret(p, "look");
								}
								else
								{
									p.OutBuffer.Append("You've been denied!\n\r");
									Log.Warn("Unsuccessful Login by " + p.Name + ".");
									p.PlayerState = PlayerStates.Quitting;
								}
							}
							
							break;
						case PlayerStates.Playing:
							line = p.InBuffer;
							string command = "";
							p.InBuffer = new StringBuilder(getLine(line.ToString(), ref command));
							
							if(command.Length > 0)
							{
								interpret(p, command);
							}
							
							break;
						case PlayerStates.Quitting:
							Log.Info(p.Name + " has quit.");
							
							if(p.Dirty)
							{
								p.Save();
							}

                            if(p.TcpClient.Connected == true)
                                p.Stream.Close();

							p.TcpClient.Close();
							playerList.Remove(p);
							pEnum = playerList.GetEnumerator();
							break;
					}
				}
				
				//See if we can write data to any of the players
				foreach(Player p in playerList)
				{
					if(p.canWrite() && p.OutBuffer.ToString().Length > 0)
					{
						if(p.PlayerState == PlayerStates.Playing)
						{
							//TODO: Player configurable prompt
							p.OutBuffer.Append("{o{g>>{x ");
						}
						
						p.OutBuffer = Color.Instance.Convert(p.OutBuffer, p.AnsiOn);
						p.Stream.Write(ascii.GetBytes(p.OutBuffer.ToString()), 0, p.OutBuffer.Length);
						p.OutBuffer = new StringBuilder("");
					}
				}

				/*TimeSpan blah = TimeSpan.FromMilliseconds(((double)1/PULSE_PER_SECOND)*1000);

				long diff = DateTime.Now.Ticks - lastTime.Ticks;

				if(blah.Ticks - diff > 0)
				{
					Thread.Sleep(TimeSpan.FromTicks(blah.Ticks - diff));
				}

				lastTime = DateTime.Now;*/

				Thread.Sleep(1);
			}
		}

		private void MudUpdate()
		{
			reg.Run();
		}

		///<summary>
		///This uses a very hacked way to get the socket out of the TcpClient so we can poll it.
		///</summary>
		///<returns>the socket corresponding to the TcpClient sent</returns>
		public Socket getSocket()
		{
			PropertyInfo pi = typeof(TcpListener).GetProperty("Server", BindingFlags.Instance|BindingFlags.NonPublic);
			return (Socket)pi.GetValue(this.mudTcpListener, null);
		}
		
		///<summary>
		///This tracks whether the mud is actually running or not.
		///</summary>
		///<value>a boolean value telling whether the mud is running or not.</value>
		public bool MudRunning
		{
			get { return mudRunning; }
			set { mudRunning = value; }
		}
		
		///<summary>
		///Contains all the commands for the mud.
		///</summary>
		///<value>a list of all the commands in the mud.</value>
		public CommandList CommandList
		{
			get { return commandList; }
		}
		
		///<summary>
		///Contains all the players currently connected to the mud.
		///</summary>
		///<value>a list of all the players connected to the mud.</value>
		public List<Player> PlayerList
		{
			get { return playerList; }
		}
		
		///<summary>
		///The object that holds the representation of the world of Suroden
		///</summary>
		///<value>the world of Suroden</value>
		public World WorldSuroden
		{
			get { return worldSuroden;}
		}

		///<summary>
		///Retrieves a single line of input from the buffer.
		///</summary>
		///<param name="line">The incoming stream</param>
		///<param name="command">The single line found (returns through this)</param>
		///<returns>The remaining buffer, possbily less one line.</returns>
		private string getLine(string line, ref string command)
		{
			int i = line.IndexOf('\n');
			
			if(i > 0)
			{
				command = stripNR(line.Substring(0, i));
				
				if(i+1 <= line.Length)
				{
					string temp = line.Substring(i+1);
					
					return temp;
				}
				else
				{
					return "";
				}
			}
			else
			{
				command = "";
				return line;
			}
		}
		
		///<summary>
		///Removes any new lines or returns from the beginning of a line
		///</summary>
		///<param name="line"></param>
		///<returns></returns>
		private string stripNR(string line)
		{
			while(line.Length > 0 && (line[0] == '\n' || line[0] == '\r') )
			{
				line = line.Remove(0, 1);
			}

			while(line.Length > 0 && (line[line.Length-1] == '\n' || line[line.Length-1] == '\r'))
			{
				line = line.Remove (line.Length -1, 1);
			}
			
			return line;
		}
		
		///<summary>
		///Takes one line of input and passes it through the command parser, then
		///if it is a valid command it runs it for the user.
		///</summary>
		///<param name="p">the player who input the line</param>
		///<param name="line">the command line itself</param>
		private void interpret(Player p, string line)
		{
			CommandParser cp = new CommandParser(line);
			
			BaseCommand c = CommandList.findCommand(cp.CommandVerb);
			
			if(c != null && p.Level >= c.UseLevel)
			{
				c.doCommand(p, cp);
			}
			else
			{
				p.OutBuffer.Append("Type 'commands' to see what commands are available.\n\r");
			}
		}
		
		///<summary>
		///Sends a message to everyone currently playing
		///</summary>
		///<param name="pWriter">Who wants to send the message</param>
		///<param name="everyone">The message to everyone but the writer</param>
		///<param name="you">The message to the writer.</param>
		public void writeAll(Player pWriter, string everyone, string you)
		{
			foreach(Player p in playerList)
			{
				if(p.PlayerState == PlayerStates.Playing)
				{
					if(p == pWriter && you != null)
					{
						p.OutBuffer.Append(you);
					}
					else
					{
						p.OutBuffer.Append(everyone);
					}
				}
			}
		}
		
		///<summary>
		///This populates the CommandList with all the available commands 
		///and their aliases on the Mud.
		///</summary>
		public void loadCommands()
		{
			Log.Info("Loading Commands");
			commandList.Add(new ChatCommand("chat"));
			commandList.Add(new QuitCommand("quit"));
			commandList.Add(new MoveCommand("north"));
			commandList.Add(new MoveCommand("south"));
			commandList.Add(new MoveCommand("east"));
			commandList.Add(new MoveCommand("west"));
			commandList.Add(new MoveCommand("northeast"));
			commandList.Add(new MoveCommand("ne"));
			commandList.Add(new MoveCommand("northwest"));
			commandList.Add(new MoveCommand("nw"));
			commandList.Add(new MoveCommand("southeast"));
			commandList.Add(new MoveCommand("se"));
			commandList.Add(new MoveCommand("southwest"));
			commandList.Add(new MoveCommand("sw"));
			commandList.Add(new MoveCommand("up"));
			commandList.Add(new MoveCommand("down"));
			commandList.Add(new LookCommand("look"));
			commandList.Add(new WhoCommand("who"));
			commandList.Add(new ExitsCommand("exits"));
			commandList.Add(new CommandsCommand("commands"));
			commandList.Add(new SaveCommand("save"));
			commandList.Add(new ShortDescCommand("shortdesc"));
			commandList.Add(new SayCommand("say"));
			commandList.Add(new ShutdownCommand("shutdown"));
			commandList.Add(new CopyoverCommand("copyover"));
			commandList.Add(new OpenCommand("open"));
			commandList.Add(new CloseCommand("close"));
			commandList.Add(new GetCommand("get"));
			commandList.Add(new DropCommand("drop"));
			commandList.Add(new InventoryCommand("inventory"));
			commandList.Add(new GiveCommand("give"));
			Log.Info("Finished Loading Commands: " + CommandList.Count.ToString() + " Commands Loaded.");
		}
		
		///<summary>
		///This kicks off the world loading process.
		///</summary>
		public void loadWorld()
		{
			worldSuroden = new World();
			worldSuroden.Load("../world/world.xml");
			worldSuroden.FixExits();
		}
		
		///<summary>
		///This runs through the shutdown process, gracefully kicking people
		///out and closing file connections, etc.
		///</summary>
		public void Shutdown()
		{
			Log.Info("Suroden is shutting down...");
			mudTcpListener.Stop();
			Log.Info("Suroden was successfully shutdown.");
		}

		public void RecoverFromCopyover()
		{
			StreamReader re = File.OpenText("c:\\test.txt");
			string input = null;
			while ((input = re.ReadLine()) != null)
			{
				string[] args = input.Split(new char[]{' ', ':'});

				string file = Player.PLAYER_DIR + args[1].ToLower() + ".xml";
				if(File.Exists(file))
				{
					//attempt to load 'em
					XmlSerializer x = new XmlSerializer(typeof(Player));
					FileStream fs = new FileStream(file.ToString(), FileMode.Open);
					Player existingPlayer = (Player) x.Deserialize(fs);
					fs.Close();
					Log.Info(existingPlayer.Name + " coming back from copyover.");
					existingPlayer.TcpClient = new TcpClient();
					SetHandle(existingPlayer.socket, Convert.ToInt32(args[0]));
					existingPlayer.PlayerState = PlayerStates.Playing;
					playerList.Add(existingPlayer);
				}
				
			}
			re.Close();
		}

		public void SetHandle(Socket s, int handle)
		{
			FieldInfo fi = typeof(Socket).GetField("m_Handle", BindingFlags.Instance|BindingFlags.NonPublic);
			fi.SetValue(s, new IntPtr(handle));
		}

		public int GetHandle(Socket s)
		{
			FieldInfo fi = typeof(Socket).GetField("m_Handle", BindingFlags.Instance|BindingFlags.NonPublic);

			if(fi != null)
			{
				return ((IntPtr)fi.GetValue(s)).ToInt32();
			}
			else
			{
				return -1;
			}
		}

		public void LoadBadNames()
		{
			Log.Info("Loading list of bad names...\n");
			badNames = new BadNameList(@"..\sys\badnames.txt");
		}
	}
}//end namespace Suroden
