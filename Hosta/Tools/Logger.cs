using System;
using System.IO;

namespace Hosta.Tools
{
	public enum VerbosityLevel
	{
		None,
		Critical,
		Important,
		Standard,
		Detailed
	};

	/// <summary>
	/// Used to log events that occur during runtime.
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// The path to the directory to write logs to.
		/// </summary>
		private static string baseDir = "";

		/// <summary>
		/// The maximum verbosity for console logging.
		/// </summary>
		public static VerbosityLevel consoleVerbosity = VerbosityLevel.Important;

		/// <summary>
		/// The maximum verbosity for file logging.
		/// </summary>
		public static VerbosityLevel fileVerbosity = VerbosityLevel.Critical;

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

		/// <summary>
		/// The path to the .log file to write to.
		/// </summary>
		private readonly string logPath;

		/// <summary>
		/// The type of the object that is using the logger.
		/// </summary>
		private readonly string type;

		/// <summary>
		/// The unique name of the object being logged.
		/// </summary>
		private readonly string name;

		/// <summary>
		/// Controls access to log writing.
		/// </summary>
		private readonly AccessQueue logQueue = new AccessQueue();

		/// <summary>
		/// Creates a new instance of a Logger.
		/// </summary>
		/// <param name="o">The object making the logs.</param>
		/// <param name="name">The unique name of the object making the logs.</param>
		public Logger(object o, string name = "")
		{
			type = o.GetType().ToString();

			if (name == "") name = Guid.NewGuid().ToString();
			this.name = name;

			logPath = Path.Join(baseDir, type);
			if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

			logPath = Path.Join(logPath, name + ".log");
			if (File.Exists(logPath)) throw new Exception("Name not unique!");
			File.Create(logPath).Close();
		}

		/// <summary>
		/// Logs a message.
		/// </summary>
		/// <param name="item">The message to log.</param>
		/// <param name="verbosity">A number that indicates the verbosity of the log.</param>
		public async void Log(string message, VerbosityLevel verbosity)
		{
			var pass = logQueue.GetPass();
			DateTime dt = DateTime.Now;
			string time = dt.TimeOfDay.ToString(@"hh\:mm\:ss");

			if (verbosity <= consoleVerbosity)
			{
				Console.WriteLine(time + " " + type + " " + name + " - " + message);
			}
			if (verbosity <= fileVerbosity)
			{
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

		public void LogAndThrow(Exception e, VerbosityLevel verbosity)
		{
			Log($"{e.GetType()}! {e.Message}", verbosity);
			throw e;
		}
	}
}