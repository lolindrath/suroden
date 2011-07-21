using System;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace Suroden
{
	///<summary>
	///The encapsulates all the fields a Player will need.
	///</summary>
	public class Player : Thing, IXmlSerializable
	{
		///<summary>This is the path to the player directory</summary>
		public const string PLAYER_DIR = @"..\\player\\";
		///<summary>The tcpClient object for the players connection</summary>
		TcpClient tcpClient;
		///<summary>The socket for the players collection.</summary>
		public Socket socket;
		///<summary>The input buffer for a player.</summary>
		StringBuilder inBuffer;
		///<summary>The output buffer for a player.</summary>
		StringBuilder outBuffer;
		///<summary>The player's current state.</summary>
		PlayerStates playerState;
		///<summary>Whether or not the player would like to use ANSI color</summary>
		bool ansiOn;

		///<summary>Tells whether the player file needs saved or not</summary>
		bool dirty;
		
		///<summary>The player's password</summary>
		private string password;
		///<summary>The player's established (physical) gender.</summary>
		private Gender playerGender = Gender.Neutral;
		
		///<summary>
		///This constuctor sets up some of the basic defaults for the player fields
		///</summary>
		public Player()
		{
			inBuffer = new StringBuilder();
			outBuffer = new StringBuilder();
			Name = "Joe Error";
			password = "Joe2Error";
			ansiOn = true;
			dirty = false;
			Level = 1;
		}
		
		///<summary>
		///This constructor calls the default and then setups the network
		///specific fields.
		///</summary>
		///<param name="tcp">The tcpClient had from the Accept() on our listening socket.</param>
		public Player(TcpClient tcp):this()
		{
			tcpClient = tcp;
			socket = getSocket(tcpClient);
			
			playerState = PlayerStates.LoggingIn;
			Name = "Logging In";
		}
		
		///<summary>
		///This uses a very hacked way to get the socket out of the TcpClient so we can poll it.
		///</summary>
		///<param name="c">The TcpClient to get the Socket from.</param>
		///<returns>the socket corresponding to the TcpClient sent</returns>
		private Socket getSocket(TcpClient c)
		{
			//PropertyInfo pi = typeof(TcpClient).GetProperty("Client", BindingFlags.Instance|BindingFlags.NonPublic);
			//return (Socket)pi.GetValue(c, null);
            return c.Client;
		}
		
		///<summary>
		///Decides whether or not the socket and be written to.
		///</summary>
		///<returns>whether or not the socket can be written to.</returns>
		public bool canWrite()
		{
			return socket.Poll(1, SelectMode.SelectWrite);
		}
		
		///<summary>
		///Decides whether or not we can read from the socket.
		///</summary>
		///<returns>whether or not the socket can be read.</returns>
		public bool canRead()
		{
            if (socket.Connected == false)
                return false;
            else 
                return socket.Poll(1, SelectMode.SelectRead);
		}
		
		///<summary>
		///Decides whether or not the socket is in an erroneous state.
		///</summary>
		///<returns>whether or not the socket is in an errorneous state.</returns>
		public bool canError()
		{
			return socket.Poll(1, SelectMode.SelectError);
		}
		
		///<summary>
		///Serializes the player file to Xml
		///</summary>
		public void Save()
		{
			XmlSerializer x = new XmlSerializer(typeof(Player));
			TextWriter writer = File.CreateText(PLAYER_DIR + Name.ToLower() + ".xml");
			x.Serialize(writer, this);
			writer.Close();
		}
		
		///<summary>
		///Appends text to the incoming data buffer for the user
		///</summary>
		///<param name="line">text to append</param>
		public void AppendInBuffer(string line)
		{
			//handle backspaces from character by character terminals
			int i;
    		while((i = line.IndexOf("\b")) != -1)
    		{
				//at beginning of line so have to g into inBuffer and erase some
        		if(i == 0)
        		{
            		line = line.Remove(0, 1);
            		if(inBuffer.Length > 0)
					{
						inBuffer = inBuffer.Remove(inBuffer.Length-1, 1);
					}
        		}
        		else
				{
            		line = line.Remove(i-1, 2);
				}
			}
			inBuffer.Append(line);
		}
		
		///<summary>
		///Returns the network stream for the tcpClient
		///</summary>
		///<value>the network stream for the tcpClient</value>
		[XmlIgnore]
		public NetworkStream Stream
		{
			get { return tcpClient.GetStream(); }
		}
		
		///<summary>
		///The TcpClient for the player
		///</summary>
		///<value>The TcpClient for the player</value>
		[XmlIgnore]
		public TcpClient TcpClient
		{
			get { return tcpClient; }
			set
			{ 
				tcpClient = value; 
				socket = getSocket(tcpClient);
			}
		}
		
		///<summary>
		///tells whether or not the player is connected
		///</summary>
		///<value>returns whether or not the player is connected.</value>
		[XmlIgnore]
		public bool Connected
		{
			get { return socket.Connected; }
		}
		
		///<summary>
		///gets and sets the incoming data buffer
		///</summary>
		///<value>the incoming data buffer</value>
		[XmlIgnore]
		public StringBuilder InBuffer
		{
			get { return inBuffer; }
			set { inBuffer = value; }
		}
		
		///<summary>
		///gets and sets the outgoing data buffer
		///</summary>
		///<value>the outgoing data buffer</value>
		[XmlIgnore]
		public StringBuilder OutBuffer
		{
			get { return outBuffer; }
			set { outBuffer = value; }
		}
		
		
		///<summary>
		///gets or sets the players password
		///</summary>
		///<value>a string containing the players password.</value>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}
		
		///<summary>
		///gets or sets the player's current state
		///</summary>
		///<value>the players state</value>
		[XmlIgnore]
		public PlayerStates PlayerState
		{
			get { return playerState; }
			set { Log.Info(Name + " changes state from " + playerState.ToString() + " to " + value.ToString());
			playerState = value; }
		}
		
		///<summary>
		///gets or sets whether the player uses ANSI color or not
		///</summary>
		///<value>whether or not to use ANSI color</value>
		[XmlIgnore]
		public bool AnsiOn
		{
			get { return ansiOn; }
			set { ansiOn = value; }
		}

		[XmlIgnore]
		public bool Dirty
		{
			get { return dirty; }
			set { dirty = value; }
		}
		
		///<summary>
		///gets or sets the player's gender
		///</summary>
		public Gender PlayerGender
		{
			get { return playerGender; }
			set { playerGender = value; }
		}
		
		public void WriteXml (XmlWriter writer)
		{
			writer.WriteElementString("Name", Name);
			writer.WriteElementString("Password", Password);
			writer.WriteElementString("PlayerGender", PlayerGender.ToString());
			writer.WriteElementString("ShortDesc", ShortDesc);
			writer.WriteElementString("CurrentRoom", CurrentRoom.UID);
			writer.WriteElementString("Level", Level.ToString());
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
					 else if(reader.LocalName.Equals("Password"))
					 {
						 Password = reader.ReadString();
					 }
					 else if(reader.LocalName.Equals("PlayerGender"))
					 {
						 PlayerGender = (Gender) Enum.Parse(typeof(Gender), reader.ReadString(), false);
					 }
					 else if(reader.LocalName.Equals("ShortDesc"))
					 {
						 ShortDesc = reader.ReadString();
					 }
					 else if(reader.LocalName.Equals("CurrentRoom"))
					 {
						 CurrentRoom = (Room)Mud.mudSuroden.WorldSuroden.RoomList[reader.ReadString()];

						 if(CurrentRoom == null)
						 {
						 	Log.Error("Current Room is null!: " + Name);
						 }
					 }
					 else if(reader.LocalName.Equals("Level"))
					 {
						 Level = Convert.ToInt32(reader.ReadString());
					 }
				 }
			 }
		}
		
		public XmlSchema GetSchema()
		{
			return null;
		}
		
	}//end class Player
	
	///<summary>
	///This enum describes the current state a player is in.
	///</summary>
	public enum PlayerStates
	{
		///<summary>A player is sitting at the Name: prompt</summary>
		LoggingIn,
		///<summary>A new player just starting out</summary>
		NewPlayer,
		EnterSex,
		NewPassword,
		ConfirmPassword,
		///<summary>A player is sitting at the Password: prompt</summary>
		EnteringPassword,
		///<summary>A player is currently in the world in a fully cognitive state.</summary>
		Playing,
		///<summary>A player is in the process of being removed from the system.</summary>
		Quitting
	}//end enum PlayerStates
	
	///<summary>
	///This describes all the possible genders a player may have
	///</summary>
	public enum Gender
	{
		///<summary>the player's gender is male</summary>
		Male,
		///<summary>the player's gender is female</summary>
		Female,
		///<summary>the player's gender is neutral</summary>
		Neutral
	}//end enum PlayerGenders
}
