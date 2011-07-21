using System;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{

///<summary>
///A specialized list to hold the Commands for Suroden.
///</summary>
public class CommandList: List<BaseCommand>
{
	///<summary>
	///Does a case insensitive, partial match (e.x. no would match north)
	///of a player's input
	///</summary>
	///<param name="verb">the command verb to search for.</param>
	///<returns>The command found or null if no command was found.</returns>
	public BaseCommand findCommand(string verb)
	{
		foreach(BaseCommand c in this)
		{
			if(!(verb.Length > c.CommandVerb.Length) && verb.ToLower() == c.CommandVerb.ToLower().Substring(0, verb.Length))
			{
				return c;
			}
		}
		return null;
	}
}//end class
}//end namespace
