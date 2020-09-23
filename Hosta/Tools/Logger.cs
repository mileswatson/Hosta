using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hosta.Tools
{
	public class Logger
	{
		private static string baseDir = "";

		/// <summary>
		/// Creates an empty directory in the specified file path.
		/// </summary>
		/// <param name="path">The path to the log directory.</param>
		/// <param name="program">The name of the program.</param>
		public static void SetDirectory(string path, string program)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			baseDir = Path.Join(path, program);
			if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
			Directory.CreateDirectory(baseDir);
		}

		private string logPath;

		private string type;
		private string name;

		private AccessQueue logQueue = new AccessQueue();

		public Logger(object o, string name = "default")
		{
			type = o.GetType().ToString();
			this.name = name;

			logPath = Path.Join(baseDir, type);
			if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

			logPath = Path.Join(logPath, name + ".log");
			if (File.Exists(logPath)) throw new Exception("Name not unique!");
			File.Create(logPath).Close();
			Log("Logger created!");
		}

		public async void Log(object item)
		{
			var pass = logQueue.GetPass();
			string message = item.ToString();
			DateTime dt = DateTime.Now;
			string time = dt.TimeOfDay.ToString(@"hh\:mm\:ss");
			Console.WriteLine(time + " " + type + " : " + name + " - " + message);
			await pass;
			try
			{
				await File.AppendAllLinesAsync(logPath, new string[] { time + " - " + message });
			}
			finally
			{
				logQueue.ReturnPass();
			}
		}
	}
}