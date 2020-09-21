using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using GzipTest.Utils;
using GzipTest.ThreadSafe;

namespace GzipTest
{
	public class Decompressor:Compressor
	{
		private const int Id1 = 0x1F;
		private const int Id2 = 0x8B;
		private const int DeflateCompression = 0x8;
		private const int MaxGzipFlag = 32;

		internal Decompressor(CompressionSettings settings, Stream stream):base(settings, stream)
		{
		}

		public override DataBlock ReadData(Stream inputStream)
		{
			if (m_interrupted)
				return null;

			long initialPosition = inputStream.Position;

			int current = inputStream.ReadByte(); //move to second byte first
			while (current != -1)
			{
				current = inputStream.ReadByte();

				if (current != Id1)
					continue;

				if (IsHeaderCandidate(inputStream))
				{
					inputStream.Seek(-1, SeekOrigin.Current);
					break;
				}
			}
			
			long lenght = inputStream.Position - initialPosition;

			if (lenght == 0)
				return null;

			inputStream.Position = initialPosition;

			var readBuffer = new byte[lenght];
			inputStream.Read(readBuffer, 0, (int)lenght);
			return new DataBlock(m_counter++, readBuffer);
		}

		public static bool IsHeaderCandidate(Stream stream)
		{
			byte[] header = new byte[8];

			int bytesRead = stream.Read(header, 0, header.Length);
			stream.Seek(-bytesRead, SeekOrigin.Current);

			if (bytesRead < header.Length)
				return false;

			// Check the id tokens and compression algorithm
			if (header[0] != Id2 || header[1] != DeflateCompression)
				return false;

			// Extract the GZIP flags, of which only 5 are allowed (2 pow. 5 = 32)
			if (header[2] > MaxGzipFlag)
				return false;

			// Check the extra compression flags, which is either 2 or 4 with the Deflate algorithm
			if (header[7] != 0x0 && header[7] != 0x2 && header[7] != 0x4)
				return false;

			return true;
		}

		protected override DataBlock Transform(DataBlock dataBlock)
		{
			// the trick is to read the last 4 bytes to get the length
			// gzip appends this to the array when compressing
			var lengthBuffer = new byte[4];
			Array.Copy(dataBlock.Data, dataBlock.Data.Length - 4, lengthBuffer, 0, 4);

			int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);
			var buffer = new byte[uncompressedSize];

			using (var ms = new MemoryStream(dataBlock.Data))
			{
				using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
				{
					gzip.Read(buffer, 0, uncompressedSize);
				}
			}
			return new DataBlock(dataBlock.SequenceNumber, buffer);
		}
	}
}
