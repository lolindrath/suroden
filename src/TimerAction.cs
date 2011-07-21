using System;

namespace Suroden
{
	/// <summary>
	/// Summary description for TimerAction.
	/// </summary>
	public class TimerAction : IComparable
	{
		public double time;
		public double interval = -1;

		public TimerAction()
		{
		
		}

		public virtual void Execute()
		{

		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			if(obj is TimerAction)
			{
				TimerAction a = obj as TimerAction;

				return this.time.CompareTo(a.time);
			}

			throw new ArgumentException("Only TimerAction is supported", "obj");
		}

		#endregion
	}

	public class MobWanderAction : TimerAction
	{
		public override void Execute()
		{
			base.Execute ();

			foreach(Mob mob in Mud.mudSuroden.mobInstances)
			{
				int rand = Mud.mudSuroden.Random.Next(0, Directions.MAX_DIR);

				if(mob.CurrentRoom != null && mob.CurrentRoom.Exits.GetValue(rand) != null)
				{
					mob.CurrentRoom.WriteAll(null, mob.Name + " moves " + Directions.DIR_NAMES[rand] + ".\n\r", "You move  " + Directions.DIR_NAMES[rand] + ".\n\r");
					mob.CurrentRoom.ContainedList.Remove(mob);
					mob.CurrentRoom = mob.CurrentRoom.Exits[rand];
					mob.CurrentRoom.ContainedList.Add(mob);

					mob.CurrentRoom.WriteAll(null, mob.Name + " has arrived from " + Directions.DIR_NAMES[Directions.DIR_REVERSE[rand]] + ".\n\r", null);
				}
			}
		}

	}
}
