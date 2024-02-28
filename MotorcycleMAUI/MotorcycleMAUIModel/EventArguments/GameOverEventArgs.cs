using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotorcycleMAUIModel.EventArguments
{
	public class GameOverEventArgs
	{
		public int Time { get; set; }

		public bool IsGameOver { get; set; }

		public GameOverEventArgs(int time, bool isGameOver)
		{
			Time = time;
			IsGameOver = isGameOver;
		}
	}
}
