using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Daemons
{
    public abstract class BaseDaemon
    {
        public abstract int WaitTimeBetweenExecution { get; }

        protected bool IsRunning { get; private set; }
        protected bool IsStopping { get; private set; }

        protected abstract void Run();
        private readonly Queue<Action> m_EventQueue = new Queue<Action>();
        private readonly HashSet<EventInfo> m_ActiveEventHandlers = new HashSet<EventInfo>();
        protected IList<EventInfo> ActiveEventHandlers
        {
            get { return new List<EventInfo>(m_ActiveEventHandlers); }
        }

        protected void RegisterEvent<TEventArgs>(EventInfo eventInfo, Action<Object, TEventArgs> callback)
            where TEventArgs : EventArgs
        {
            var action = new Action<object, TEventArgs>((sender, args) =>
                {
                    lock (m_EventQueue)
                    {
                        m_EventQueue.Enqueue(() => callback(sender, args));
                        Monitor.Pulse(m_EventQueue);
                    }
                });

            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            m_ActiveEventHandlers.Add(eventInfo);
        }

        protected void ProcessNextEvent()
        {
            lock (m_EventQueue)
            {
                while (m_EventQueue.Count == 0)
                {
                    Monitor.Wait(m_EventQueue);
                }
                m_EventQueue.Dequeue()();
            }
        }

        protected bool ProcessNextEventNoWait()
        {
            lock (m_EventQueue)
            {
                if (m_EventQueue.Count > 0)
                {
                    m_EventQueue.Dequeue()();
                    return true;
                }
                return false;
            }
        }

        public void Start()
        {
            lock (this)
            {
                if (IsRunning)
                    return;

                IsRunning = true;
            }

            IsStopping = false;

            var workerThread = new Thread(() =>
                {
                    var firstExecution = true;
                    var lastExecutionTime = DateTime.Now;
                    while (!IsStopping)
                    {
                        try
                        {
                            var currTime = DateTime.Now;    
                            if (firstExecution ||
                                (currTime - lastExecutionTime).TotalMilliseconds > WaitTimeBetweenExecution)
                            {
                                lastExecutionTime = currTime;
                                firstExecution = false;
                                Run();
                            }
                            else
                            {
                                //Sleep until the approximate time that we're ready
                                var waitTime = (WaitTimeBetweenExecution - (currTime - lastExecutionTime).TotalMilliseconds);
                                Thread.Sleep((int)waitTime);
                            }
                            
                        }
                        catch (Exception e)
                        {
                            HandleException(e);
                        }
                    }

                    CleanUp();
                    IsRunning = false;
                });

            workerThread.Start();
        }

        public void Stop()
        {
            IsStopping = true;
        }

        protected virtual void CleanUp()
        {
            
        }

        private void HandleException(Exception e)
        {
            //To be filled out when we have a logging mechanism in place
        }
    }
}
