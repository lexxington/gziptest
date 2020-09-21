using System;
using System.IO;
using System.Threading;
using GzipTest.Utils;

namespace GzipTest.ThreadSafe
{
    public class OutputStream
    {
		private readonly Stream m_stream;
	    private int m_counter;
	    private bool m_interrupted;
		
		public OutputStream(Stream stream)
	    {
		    m_stream = stream;
	    }

	    public void Write(DataBlock dataBlock)
	    {
		    while (!m_interrupted)
		    {
			    lock (m_stream)
			    {
					if (m_counter == dataBlock.SequenceNumber)
				    {
					    //Console.WriteLine("Thread '{0}' in work", Thread.CurrentThread.Name);

					    m_stream.Write(dataBlock.Data, 0, dataBlock.Data.Length);
					    m_counter++;
					    Monitor.PulseAll(m_stream);
					    return;
				    }

				    //Console.WriteLine("Thread '{0}' will try again", Thread.CurrentThread.Name);
					Monitor.Wait(m_stream);
				}
		    }
	    }

	    public void Interrupt()
	    {
		    m_interrupted = true;
		    lock (m_stream)
			    Monitor.PulseAll(m_stream);
	    }
    }
}
