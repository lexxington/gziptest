namespace GzipTest.Utils
{
    public class DataBlock
    {
	    public DataBlock(int sequenceNumber, byte[] data)
	    {
		    SequenceNumber = sequenceNumber;
		    Data = data;
	    }

	    public int SequenceNumber { get; private set; }
	    public byte[] Data { get; private set; }
    }
}
