using System;

using IronPython.Hosting;
using IronPython.Modules;

namespace Suroden
{
	///<summary>
	///All commands in Suroden will inherit from this so they fit in the CommandList well
	///and so you can call doCommand or get the CommandVerb from any.
	///</summary>
	abstract public class BaseCommand
	{
		///<summary>
		///This the the command verb (e.x. north, south, look, who)
		///</summary>
		protected string commandVerb;
		/// <summary>
		/// Indicates at what level a player must be to use the command
		/// </summary>
		protected int useLevel;
		
		///<summary>
		///This is the default(and only) constructor.
		///</summary>
		public BaseCommand(string pCommandVerb)
		{
			commandVerb = pCommandVerb;
			useLevel = 0;
		}
		
		///<summary>
		///The CommandVerb Property
		///</summary>
		///<value>Returns the CommandVerb of the specific instance of the command.</value>
		public string CommandVerb
		{
			get { return commandVerb; }
		}

		public int UseLevel
		{
			get { return useLevel; }
		}
		
		///<summary>
		///This is how the command is actually executed.
		///</summary>
		///<param name="p">The player who is executing the command</param>
		///<param name="cp">The CommandParser object for the command arguments sent.</param>
		abstract public void doCommand(Player p, CommandParser cp);
	}
}