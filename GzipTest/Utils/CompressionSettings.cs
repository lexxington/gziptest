using System;
using System.IO;
using System.IO.Compression;

namespace GzipTest.Utils
{
    public class CompressionSettings
    {
	    public CompressionSettings(string[] args)
	    {
			if (args == null)
				throw new Exception("Please specify parameters");

		    if (args.Length != 3)
			    throw new Exception("Parameters count is invalid");

		    CompressionMode mode;
		    switch (args[0].ToLower())
		    {
			    case "compress":
				    mode = CompressionMode.Compress;
				    break;
			    case "decompress":
				    mode = CompressionMode.Decompress;
				    break;

			    default:
				    throw new Exception(String.Format("Invalid compression mode '{0}'", args[0]));
		    }

			if (!File.Exists(args[1]))
				throw new Exception(String.Format("File '{0}' doesn't exist", args[1]));

			Init(mode, args[1], args[2]);
	    }

	    public CompressionSettings(CompressionMode mode, string inputFile, string outputFile)
	    {
			Init(mode, inputFile, outputFile);
	    }

	    private void Init(CompressionMode mode, string inputFile, string outputFile)
	    {
		    CompressionMode = mode;
		    InputFile = inputFile;
		    OutputFile = outputFile;

		    ChunkSize = 1024 * 1024;
		    ThreadsCount = Environment.ProcessorCount;
		    //BufferCapacity = Int32.MaxValue;
		}

	    public string InputFile { get; private set; }
	    public string OutputFile { get; private set; }
	    public CompressionMode CompressionMode { get; private set; }


	    public int ThreadsCount { get; set; }
	    public int ChunkSize { get; set; }


	    public ulong BufferCapacity
	    {
		    get
		    {
			    ulong memoryInstalled = NativeMethods.GetMemoryInstalled();
			    ulong availableMemory = (ulong)Math.Round(memoryInstalled * 0.95); //get 95% of memory installed

			    return availableMemory / (ulong)ChunkSize;
		    }
	    }

    }

}
