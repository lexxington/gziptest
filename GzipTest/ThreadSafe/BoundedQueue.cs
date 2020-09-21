using System;
using System.Collections;
using System.Threading;
using GzipTest.Utils;

namespace GzipTest.ThreadSafe
{
    public class BoundedQueue
    {
	    private readonly Queue m_queue = new Queue();
	    private bool m_completed;

		#region Constructors

		public BoundedQueue():this(Int32.MaxValue)
	    {
	    }

	    public BoundedQueue(ulong capacity)
	    {
		    BoundedCapacity = capacity;
	    }

		#endregion

		public ulong BoundedCapacity { get; set; }

	    public bool TryToEnqueue(DataBlock dataBlock)
	    {
		    lock (m_queue)
		    {
			    if ((ulong)m_queue.Count >= BoundedCapacity)
				    return false;

				m_queue.Enqueue(dataBlock);
				Monitor.PulseAll(m_queue);
			}

		    return true;
	    }

	    public void DequeueOrWait(out DataBlock dataBlock)
	    {
		    dataBlock = null;
		    lock (m_queue)
		    {
			    while (m_queue.Count == 0 && !m_completed)
				    Monitor.Wait(m_queue);

				if (m_queue.Count != 0)
					dataBlock = m_queue.Dequeue() as DataBlock;
		    }
	    }

	    public void Complete()
	    {
			lock (m_queue)
		    {
			    m_completed = true;
				Monitor.PulseAll(m_queue);
			}
	    }

	    public void Interrupt()
	    {
			Complete();
	    }

    }
}
