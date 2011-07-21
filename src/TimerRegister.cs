using System;

namespace Suroden
{
	/// <summary>
	/// Summary description for TimerRegister.
	/// </summary>
	public class TimerRegistry
	{
		private BinaryPriorityQueue<TimerAction> queue = new BinaryPriorityQueue<TimerAction>();

		public TimerRegistry()
		{
		}

		public void Run()
		{
			double now = DateTime.Now.TimeOfDay.TotalMilliseconds;

			while(queue.Count > 0 && queue.Peek().time <= now)
			{
				TimerAction a = queue.Pop();

				a.Execute();

				if(a.interval > 0)
				{
					a.time = now + a.interval;
					queue.Push(a);
				}
			}
		}

		public void AddTimerAction(TimerAction a)
		{
			queue.Push(a);
		}
	}
}
