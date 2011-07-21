using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Suroden
{
	public class BadNameList
	{
		private Dictionary<string, string> badNames = new Dictionary<string, string>();

		public BadNameList(string file)
		{
			StreamReader sr = File.OpenText(file);
			string input = null;
			while ((input = sr.ReadLine()) != null)
			{
				badNames.Add(input, "banned");
			}
			sr.Close();
		}

		public Dictionary<string, string> BadNames
		{
			get { return badNames; }
		}
	}
}
