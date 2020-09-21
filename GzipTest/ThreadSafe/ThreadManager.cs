using System;
using System.Threading;

namespace GzipTest.ThreadSafe
{
    public class ThreadManager
    {
	    private readonly int m_maxThreadCount;
	    private readonly object m_threadLocker = new object();
	    private int m_runningThreads;
	    private int m_counter;
	    private bool m_interrupted;

	    public event ThreadExceptionEventHandler OnExceptionOccured;
		
		#region Constructors

		public ThreadManager():this(Environment.ProcessorCount)
	    {

	    }

	    public ThreadManager(int maxThreadsCount)
	    {
		    m_maxThreadCount = maxThreadsCount;
	    }

	    #endregion

	    public bool TryToStartNewThread(ThreadStart innerWork)
	    {
			lock (m_threadLocker)
		    {
			    if (m_interrupted || m_runningThreads >= m_maxThreadCount)
				    return false;

				m_runningThreads++;
			}

		    Thread newThread = new Thread(DoWork);
		    newThread.Name = String.Format("Thread #{0}", m_counter++); //for debug purposes only
		    newThread.Start(innerWork);

			return true;
		}

	    public void WaitForCompletion()
	    {
		    lock (m_threadLocker)
			    while (m_runningThreads > 0)
				    Monitor.Wait(m_threadLocker);
	    }

	    public void Interrupt()
	    {
		    m_interrupted = true;
		    lock (m_threadLocker)
			    Monitor.PulseAll(m_threadLocker);
	    }

	    private void DoWork(object obj)
	    {
		    ThreadStart innerWork = obj as ThreadStart;

		    if (innerWork != null && !m_interrupted)
		    {
			    try
			    {
					innerWork.Invoke();
			    }
			    catch (Exception exc)
			    {
					OnExceptionOccured?.Invoke(this, new ThreadExceptionEventArgs(exc));
			    }
		    }
			
		    lock (m_threadLocker)
		    {
			    m_runningThreads--;
			    Monitor.Pulse(m_threadLocker);
		    }
	    }
	}
}
