using System;
using System.IO;
using System.Text;

namespace Suroden
{
///<summary>
///This class writes the log files out while also printing the messages to the Console
///</summary>
public class Log
{
	///<summary>where the logs reside</summary>
	private static string LOG_DIR = @"..\log\";
	///<summary>the file handle we write to all the time</summary>
	private static TextWriter file = null;
	
	///<summary>
	///creates the log file name from the current date and opens up that new file.
	///</summary>
	static Log()
	{
		string date = DateTime.Now.ToString().Replace(":", ".").Replace("/", "-");;
		file = new StreamWriter(LOG_DIR + date + ".log");
	}
	
	///<summary>
	///Append the log message to our open file handle in our nice format.
	///</summary>
	///<param name="level">the logging level</param>
	///<param name="message">the message to log</param>
	private static void LogMessage(string level, object message)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("[{0}] [{1:-5}] - {2}", DateTime.Now.ToString(), level, message);
		file.WriteLine(sb.ToString());
		file.Flush();
		Console.WriteLine(sb.ToString());
	}
	
	///<summary>
	///Write a debug message to the log
	///Debug messages are mainly just for new development debugging.
	///</summary>
	///<param name="message">The message to log</param>
	public static void Debug(object message)
	{
		LogMessage("DEBUG", message);
	}
	
	///<summary>
	///Write an info message to the log
	///An info message updates the log on the current status of the mud.
	///(i.e. someone just signed on or the MUD just loaded an area from a file)
	///</summary>
	///<param name="message">The message to log</param>
	public static void Info(object message)
	{
		LogMessage("INFO", message);
	}
	
	///<summary>
	///Write a warning to the log
	///A warning should be something that is out of the ordinary but can
	///happen on a daily basis (i.e. a user attempts to sign on when he's already signed on)
	///</summary>
	///<param name="message">The message to log</param>
	public static void Warn(object message)
	{
		LogMessage("WARN", message);
	}
	
	///<summary>
	///Write an error to the log
	///An error is something that isn't going to cause a MUD crash in the long run
	///but still isn't something that the MUD should do normally.
	///</summary>
	///<param name="message">The message to log</param>
	public static void Error(object message)
	{
		LogMessage("ERROR", message);
	}
	
	///<summary>
	///This writes a fatal message to the log.
	///A fatal message should only be something that is going to cause
	///the MUDs untimely death almost instantly
	///</summary>
	///<param name="message">The message to log</param>
	public static void Fatal(object message)
	{
		LogMessage("FATAL", message);
	}
	
	/*void Debug(object message, Exception t)
	{
		
	}
	
	void Info(object message, Exception t)
	{
		
	}
	
	void Warn(object message, Exception t)
	{
		
	}
	
	void Error(object message, Exception t)
	{
		
	}
	
	void Fatal(object message, Exception t)
	{
		
	}*/
}//end class Log
}
