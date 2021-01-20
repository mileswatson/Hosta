using System;
using System.IO;
using System.Threading.Tasks;

namespace ClientWPF
{
	/// <summary>
	/// Represents the application's communication with the external environment.
	/// Provides default implementations for testing (which can be overridden).
	/// </summary>
	public class ApplicationEnvironment
	{
		private static ApplicationEnvironment env = new ApplicationEnvironment();

		public static ApplicationEnvironment Env { get => env; set => env = value; }

		//// Defaults

		public string DefaultFolder { get; protected set; }

		public virtual void Alert(string message) => Console.WriteLine(message);

		public virtual bool Confirm(string message)
		{
			Console.WriteLine($"CONFIRM: {message} Y/N");
			return Console.ReadLine()?.Trim().ToLower() == "Y";
		}

		public virtual bool CheckDirectory(string path) => Directory.Exists(path);

		public virtual bool CheckFile(string path) => File.Exists(path);

		public virtual Task<string> ReadFile(string path) => File.ReadAllTextAsync(path);

		public virtual Task WriteFile(string path, string data) => File.WriteAllTextAsync(path, data);

		public virtual Task<byte[]> ReadFileRaw(string path) => File.ReadAllBytesAsync(path);

		public virtual Task WriteFileRaw(string path, byte[] data) => File.WriteAllBytesAsync(path, data);

		public ApplicationEnvironment()
		{
			DefaultFolder = "";
		}
	}
}