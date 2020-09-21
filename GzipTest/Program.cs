using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using GzipTest.Utils;

namespace GzipTest
{
	class Program
	{
		static void Main(string[] args)
		{
			CompressionSettings settings;
			try
			{
				settings = new CompressionSettings(args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadKey();
				return;
			}
			
			Console.WriteLine("Try to {0} file '{1}'", settings.CompressionMode.ToString().ToLower(), settings.InputFile);
			var stopwatch = Stopwatch.StartNew();

			StreamManager streamManager = new StreamManager(settings);
			streamManager.Run();

			stopwatch.Stop();

			Console.WriteLine("{0}ion completed", settings.CompressionMode);
			Console.WriteLine("Time elapsed: {0:0.000}s", stopwatch.Elapsed.TotalSeconds);
			Console.ReadKey();
		}
	}
}
