using System;
using System.Text;

namespace Suroden
{
	///<summary>
	///This class handles a players input and allows the programmer to parse
	///it out easily and efficiently.
	///</summary>
	public class CommandParser
	{
		///<summary>
		///The commandVerb is the first word in the input line (e.x. north, south, look, who)
		///</summary>
		private string commandVerb;
		
		///<summary>
		///The original arguments are everything after the commandVerb on the input line.
		///</summary>
		private string origArguments;
		
		///<summary>
		///The remaining arguments are all the arguments not yet parsed.
		///</summary>
		private string remainingArguments;
		
		///<summary>
		///The default constructor parses off the CommandVerb and then sets the
		/// OrigArguments and RemainingArguments to everything after the CommandVerb.
		///</summary>
		///<param name="line">The input from the user.</param>
		public CommandParser(string line)
		{
			int pos = line.IndexOf(' ');
	
			if(pos > 0)
			{
				commandVerb = line.Substring(0, pos);
				remainingArguments = origArguments = line.Substring(pos+1, line.Length-pos-1);
			}
			else
			{
				//no space means that only the command is on that line.
				commandVerb = line;
			}
		}
		
		///<summary>
		///Looks for the next space in the remainingArguments and grabs from the beginning
		///of that string to the space and returns it. It then sets the remainingArguments
		///to what is left to parse.
		///</summary>
		///<returns>The next unquoted argument</returns>
		public string getArgument()
		{
			string argument = "";
			if(remainingArguments != null && remainingArguments != "")
			{
				int pos = remainingArguments.IndexOf(" ");
				
				if(pos != -1)
				{
					//cut off one argument and return it then keep the remaining
					argument = remainingArguments.Substring(0, pos);
					remainingArguments = remainingArguments.Substring(pos+1, remainingArguments.Length - pos - 1);
				}
				else
				{
					//we only have one argument left
					argument = remainingArguments;
					remainingArguments = "";
				}
			}
			
			return argument;
		}
		
		///<summary>
		///This function will check for a " in the zero position of the RemainingArguments
		///field. If it's found it will attempt to find the next ". If this isn't found it will
		///return from right after the " to the end of line. If no quote is found it returns nothing.
		///</summary>
		///<returns>The next quoted argument or nothing.</returns>
		public string getQuotedArgument()
		{
			return "";
		}
		
		///<summary>
		///Returns the argument parsing back to it's initial state.
		///</summary>
		public void resetArguments()
		{
			remainingArguments = origArguments;
		}
		
		///<summary>
		///The CommandVerb is the first word in the input line
		///</summary>
		///<returns>The CommandVerb the user wants to execute</returns>
		public string CommandVerb
		{
			get { return commandVerb; }
		}
		
		///<summary>
		///Returns the original arguments exactly as typed from the user.
		///</summary>
		///<value>A string of options the user would like to use the command.</value>
		public string Arguments
		{
			get { return origArguments; }
		}
		
		///<summary>
		///Returns all the portion of the input line not yet parsed.
		///</summary>
		///<value>A string of the remaining arguments to parse</value>
		public string RemainingArguments
		{
			get { return remainingArguments; }
		}
	}
}
