// created on 6/24/2003 at 10:00 AM

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
	///<summary>
	///The Color class is a singleton that holds the ansi color sequences and the
	///translation table to convert player color codes into displayable ANSI
	///color.
	///</summary>
	public class Color
	{
		///<summary>This is where the actual instance is kept</summary>
		private static readonly Color instance = new Color();

		///<summary>This turns the colors back to the default</summary>
		private const string NORMAL = "\x1B[0m"; //* 0 for normal display
		///<summary>This makes the colors appear brighter</summary>
		private const string BOLD = "\x1B[1m"; //* 1 for bold on
		///<summary>This underlines the text</summary>
		private const string UNDERLINE = "\x1B[4m"; //* 4 underline (mono only)
		///<summary>This makes the text blink. A horrible Death and life-long misfortune for those who use it!</summary>
		private const string BLINK = "\x1B[5m"; //* 5 blink on
		///<summary>This flips the background and foreground colors.</summary>
		private const string REVERSE = "\x1B[7m"; //* 7 reverse video on
		///<summary>You can't the text when you use this</summary>
		private const string INVIS = "\x1B[8m"; //* 8 nondisplayed (invisible)
		///<summary>Turns the foreground text color black</summary>
		private const string BLACK = "\x1B[30m"; //* 30 black foreground
		///<summary>Turns the foreground text color red</summary>
		private const string RED = "\x1B[31m"; //* 31 red foreground
		///<summary>Turns the foreground text color green.</summary>
		private const string GREEN = "\x1B[32m"; //* 32 green foreground
		///<summary>Turns the foreground text color yellow.</summary>
		private const string YELLOW = "\x1B[33m"; //* 33 yellow foreground
		///<summary>Turns the foreground text color blue.</summary>
		private const string BLUE = "\x1B[34m"; //* 34 blue foreground
		///<summary>Turns the foreground text color magenta.</summary>
		private const string MAGENTA = "\x1B[35m"; //* 35 magenta foreground
		///<summary>Turns the foreground text color cyan.</summary>
		private const string CYAN = "\x1B[36m"; //* 36 cyan foreground
		///<summary>turns the foreground text color white.</summary>
		private const string WHITE = "\x1B[37m"; //* 37 white foreground
		///<summary>Turns the background text color black.</summary>
		private const string B_BLACK = "\x1B[40m"; //* 40 black background
		///<summary>Turns the background text color red.</summary>
		private const string B_RED = "\x1B[41m"; //* 41 red background
		///<summary>Turns the background text color green.</summary>
		private const string B_GREEN = "\x1B[42m"; //* 42 green background
		///<summary>Turns the background text color yellow.</summary>
		private const string B_YELLOW = "\x1B[43m"; //* 43 yellow background
		///<summary>Turns the background text color blue.</summary>
		private const string B_BLUE = "\x1B[44m"; //* 44 blue background
		///<summary>Turns the background text color magenta.</summary>
		private const string B_MAGENTA = "\x1B[45m"; //* 45 magenta background
		///<summary>Turns the background text color cyan.</summary>
		private const string B_CYAN = "\x1B[46m"; //* 46 cyan background
		///<summary>Turns the background text color white.</summary>
		private const string B_WHITE = "\x1B[47m"; //* 47 white background

		///<summary>
		///This Dictionary matches the player color sequence to it's ANSI color sequence.
		///</summary>
		private Dictionary<string, string> colorTrans;
		
		///<summary>
		///The default constructor for Color sets up the colorTrans Hashtable.
		///</summary>
		private Color()
		{
			colorTrans = new Dictionary<string, string>();
			colorTrans.Add("{{", "{");
			colorTrans.Add("{x", NORMAL);
			colorTrans.Add("{o", BOLD);
			colorTrans.Add("{l", BLINK);
			colorTrans.Add("{n", UNDERLINE);
			colorTrans.Add("{e", REVERSE);
			colorTrans.Add("{b", BLUE);
			colorTrans.Add("{c", CYAN);
			colorTrans.Add("{d", BLACK);
			colorTrans.Add("{g", GREEN);
			colorTrans.Add("{m", MAGENTA);
			colorTrans.Add("{r", RED);
			colorTrans.Add("{w", WHITE);
			colorTrans.Add("{y", YELLOW);
			colorTrans.Add("{B", B_BLUE);
			colorTrans.Add("{C", B_CYAN);
			colorTrans.Add("{D", B_BLACK);
			colorTrans.Add("{G", B_GREEN);
			colorTrans.Add("{M", B_MAGENTA);
			colorTrans.Add("{R", B_RED);
			colorTrans.Add("{W", B_WHITE);
			colorTrans.Add("{Y", B_YELLOW);
		}

		///<summary>
		///Instance retrieves the Singleton instance of the Color class
		///</summary>
		///<value>Returns the instance of the Color class to use.</value>
		public static Color Instance
		{
			get { return instance; }
		}

		///<summary>
		///This takes the player colors like {x and {x and {y and turns them into
		///ANSI colors that are displayable to telnet clients.
		///</summary>
		///<param name="text">
		///This is the text to convert. If it contains player color sequences
		///They will be converted to ANSI codes.
		///</param>
		///<param name="AnsiOn">
		///If AnsiOn == false it well strip all the player sequences with nothing,
		///otherwise it will translate the color to ANSI color.
		///</param>
		///<returns>The translated text</returns>
		public StringBuilder Convert(StringBuilder text, bool AnsiOn)
		{
			//StringBuilder text = new StringBuilder(line);

			if(AnsiOn)
			{
				foreach(string key in colorTrans.Keys)
				{
					text.Replace(key, colorTrans[key]);
				}
			}
			else
			{
				int i = -1;
				text.Replace("{{", "{");
				while((i = text.ToString().IndexOf("{")) != -1)
				{
					text.Remove(i, 2);
				}
			}
			return text;
		}
	}
}
