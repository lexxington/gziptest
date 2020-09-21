using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using GzipTest.ThreadSafe;
using GzipTest.Utils;

namespace GzipTest
{
	public class Compressor
	{
		protected int ChunkSize { get; set; }
		protected int m_counter;

		private readonly OutputStream m_outputStream;
		private readonly BoundedQueue m_queue;
		private readonly ThreadManager m_threads;
		protected bool m_interrupted;

		public static Compressor Create(CompressionSettings settings, Stream stream)
		{
			return settings.CompressionMode == CompressionMode.Decompress ? new Decompressor(settings, stream) : new Compressor(settings, stream);
		}

		internal Compressor(CompressionSettings settings, Stream stream)
		{
			ChunkSize = settings.ChunkSize;
			m_threads = new ThreadManager(settings.ThreadsCount);
			m_queue = new BoundedQueue(settings.BufferCapacity);
			m_outputStream = new OutputStream(stream);

			m_threads.OnExceptionOccured += threads_OnExceptionOccured;
		}

		private void threads_OnExceptionOccured(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			Interrupt(e.Exception);
		}

		#region Public Methods

		public virtual DataBlock ReadData(Stream inputStream)
		{
			if (m_interrupted)
				return null;

			var readBuffer = new byte[ChunkSize];
			int readCount = inputStream.Read(readBuffer, 0, ChunkSize);

			if (readCount == 0)
				return null;

			if (readCount == ChunkSize)
				return new DataBlock(m_counter++, readBuffer);

			var realData = new byte[readCount];
			Array.Copy(readBuffer, realData, readCount);
			return new DataBlock(m_counter++, realData);
		}

		public void Write(DataBlock dataBlock)
		{
			if (m_interrupted)
				return;

			while (!m_queue.TryToEnqueue(dataBlock)) {}

			m_threads.TryToStartNewThread(DoWorkOrWait);
		}

		public void Complete()
		{
			m_queue.Complete();
		}

		public void WaitForCompletion()
		{
			m_threads.WaitForCompletion();
		}

		public void Interrupt(Exception exc)
		{
			m_interrupted = true;
			Console.WriteLine("Error is occured: {0}", exc.Message);

			m_outputStream.Interrupt();
			m_queue.Interrupt();
			m_threads.Interrupt();
		}

		#endregion

		private void DoWorkOrWait()
		{
			DataBlock current;

			do
			{
				m_queue.DequeueOrWait(out current);

				if (current != null)
				{
					var transformed = Transform(current);
					m_outputStream.Write(transformed);
				}

			} while (current != null && !m_interrupted);
		}

		//Compression
		protected virtual DataBlock Transform(DataBlock dataBlock)
		{
			using (var resultStream = new MemoryStream())
			{
				using (var zipStream = new GZipStream(resultStream, CompressionMode.Compress))
				{
					using (var writer = new BinaryWriter(zipStream))
					{
						writer.Write(dataBlock.Data);
					}
				}
				return new DataBlock(dataBlock.SequenceNumber, resultStream.ToArray());
			}
		}
	}
}
