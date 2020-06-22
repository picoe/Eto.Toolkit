using System;
using Eto.Forms;
using System.Collections.Generic;

namespace Eto.UnitTest.UI
{
    class AsyncQueue
	{
		List<Action> actions = new List<Action>();
		Dictionary<string, Action> namedActions = new Dictionary<string, Action>();
		UITimer timer;
		double delay = 0.2;
		bool isQueued;

		public double Delay
		{
			get => delay;
			set
			{
				delay = value;
				if (timer != null)
					timer.Interval = delay;
			}
		}

		public void Add(string name, Action action)
		{
			lock (this)
			{
				namedActions[name] = action;
				Start();
			}
		}

		public void Add(Action action)
		{
			lock (this)
			{
				actions.Add(action);
				Start();
			}
		}

		void Start()
		{
			if (!isQueued)
			{
				isQueued = true;
				/**
				Application.Instance.AsyncInvoke(FlushQueue);
				/**/
				Application.Instance.AsyncInvoke(StartTimer);
				/**/
			}
		}

		void StartTimer()
		{
			if (timer == null)
			{
				timer = new UITimer { Interval = delay };
				timer.Elapsed += Timer_Elapsed;
			}
			timer.Start();
		}

		void Timer_Elapsed(object sender, EventArgs e) => FlushQueue();

		void FlushQueue()
		{
			List<Action> actionList;
			lock (this)
			{
				actionList = actions;
				actionList.AddRange(namedActions.Values);
				namedActions.Clear();
				actions = new List<Action>();
				isQueued = false;
				timer?.Stop();
			}

			foreach (var action in actionList)
			{
				action();
			}
		}
    }
}
