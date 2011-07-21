namespace Suroden.Test
{
	using Suroden;
	using System;
	using NUnit.Framework;
	
	///<summary>
	///NUnit2 tests for the CommandParser
	///</summary>
	[TestFixture]
	public class CommandParserTest
	{
		///<summary>the command parser variable to test against</summary>
		private CommandParser cp = null;
		
		///<summary>test that the command verb gets set properly</summary>
		[Test]
		public void TestCommandVerb()
		{
			cp = new CommandParser("look");
			Assert.AreEqual(cp.CommandVerb, "look");
		}
		
		///<summary>test that single word arguments work well</summary>
		[Test]
		public void TestArguments()
		{
			cp = new CommandParser("give lolindrath shoe");
			Assert.AreEqual(cp.RemainingArguments, cp.Arguments);
			string arg = cp.getArgument();
			Assert.AreEqual("lolindrath", arg);
			Assert.AreEqual("shoe", cp.RemainingArguments);
			arg = cp.getArgument();
			Assert.AreEqual("shoe", arg);
			Assert.AreEqual("", cp.RemainingArguments);
			cp.resetArguments();
			Assert.AreEqual(cp.Arguments, cp.RemainingArguments);
		}
		
		/*[Test]
		public void TestQuotedArguments()
		{
			cp = new CommandParser("put \"black shoe\" \"green bag\"");
			Assertion.AssertEquals(cp.RemainingArguments, cp.Arguments);
			string arg = cp.getQuotedArgument();
			Assertion.AssertEquals("black shoe", arg);
			Assertion.AssertEquals("\"green bag\"", cp.RemainingArguments);
			arg = cp.getArgument();
			Assertion.AssertEquals("shoe", arg);
			Assertion.AssertEquals("", cp.RemainingArguments);
			cp.resetArguments();
			Assertion.AssertEquals(cp.Arguments, cp.RemainingArguments);
		}*/
	}
}
