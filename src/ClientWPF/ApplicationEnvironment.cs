using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWPF
{
	/// <summary>
	/// Represents the application's communication with the external environment.
	/// Provides default implementations for testing (which can be overridden).
	/// </summary>
	public static class ApplicationEnvironment
	{
		private static Dictionary<string, string> store = new();

		//// Delegates

		public delegate void Alerter(string message);

		public delegate bool Confirmer(string message);

		public delegate bool DirectoryChecker(string directory);

		public delegate bool FileChecker(string file);

		public delegate Task<string> FileReader(string file);

		public delegate Task FileWriter(string file, string data);

		//// Defaults

		private static Alerter alert = (string _) => { };

		private static Confirmer confirm = (string _) => true;

		private static DirectoryChecker checkDirectory = (string _) => true;

		private static FileChecker checkFile = (string file) => store.ContainsKey(file);

		private static FileReader readFile = (string key) =>
		{
			try
			{
				return Task.FromResult(store[key]);
			}
			catch (Exception e)
			{
				return Task.FromException<string>(e);
			}
		};

		private static FileWriter writeFile = (string key, string value) =>
		{
			store[key] = value;
			return Task.CompletedTask;
		};

		//// Interfaces

		public static Alerter Alert { get => alert; }

		public static Confirmer Confirm { get => confirm; }

		public static DirectoryChecker CheckDirectory { get => checkDirectory; }

		public static FileChecker CheckFile { get => checkFile; }

		public static FileReader ReadFile { get => readFile; }

		public static FileWriter WriteFile { get => writeFile; }

		//// Setters

		public static void SetAlert(Alerter alert)
		{
			ApplicationEnvironment.alert = alert;
		}

		public static void SetConfirmation(Confirmer confirm)
		{
			ApplicationEnvironment.confirm = confirm;
		}

		public static void SetDirectoryChecker(DirectoryChecker checkDirectory)
		{
			ApplicationEnvironment.checkDirectory = checkDirectory;
		}

		public static void SetFileChecker(FileChecker checkFile)
		{
			ApplicationEnvironment.checkFile = checkFile;
		}

		public static void SetFileReader(FileReader readFile)
		{
			ApplicationEnvironment.readFile = readFile;
		}

		public static void SetFileWriter(FileWriter writeFile)
		{
			ApplicationEnvironment.writeFile = writeFile;
		}
	}
}