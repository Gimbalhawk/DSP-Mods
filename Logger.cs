using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace DSP_Mods
{
	public class Logger
	{
		public static Logger Instance { get; private set; }

		private ManualLogSource LogSource { get; set; }
		private string MsgSource { get; set; }

		public static void Init(ManualLogSource logger, string source = null)
		{
			Instance = new Logger()
			{
				LogSource = logger,
				MsgSource = source
			};
		}

		public void LogInfo(string msg)
		{
			string message = (MsgSource != null ? $"{MsgSource} : " : "") + msg;

			LogSource.LogInfo(message);
		}
	}
}
