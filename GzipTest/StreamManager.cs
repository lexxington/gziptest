using System;
using System.IO;
using System.IO.Compression;
using GzipTest.Utils;
using GzipTest.ThreadSafe;

namespace GzipTest
{
	public class StreamManager
	{
		private readonly CompressionSettings m_settings;

		public StreamManager(CompressionSettings settings)
		{
			m_settings = settings;
		}

		public void Run()
		{
			using (var inputStream = File.OpenRead(m_settings.InputFile))
			{
				using (var outputStream = File.Create(m_settings.OutputFile))
				{
					InnerRun(inputStream, outputStream);
				}
			}
		}

		private void InnerRun(Stream inputStream, Stream outputStream)
		{
			Compressor compressor = Compressor.Create(m_settings, outputStream);
			try
			{
				DataBlock data;

				do
				{
					data = compressor.ReadData(inputStream);

					if (data != null)
						compressor.Write(data);

				} while (data != null);

				compressor.Complete();
				compressor.WaitForCompletion();
			}
			catch (Exception exc)
			{
				compressor.Interrupt(exc);
			}
		}
	}
}
