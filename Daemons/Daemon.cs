using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Daemons.EventExposers;
using Shnexy;

namespace Daemons
{
    //For more information, see https://maginot.atlassian.net/wiki/display/SH/Design+Document%3A+SH-21
    public abstract class Daemon : IDaemon
    {
        public abstract int WaitTimeBetweenExecution { get; }

        protected bool IsRunning { get; private set; }
        protected bool IsStopping { get; private set; }

        protected abstract void Run();
        private readonly Queue<Action> _eventQueue = new Queue<Action>();
        private readonly HashSet<EventInfo> _activeEventHandlers = new HashSet<EventInfo>();
        
        /// <summary>
        /// Currently unused, but will be a useful debugging tool when investigating event callbacks.
        /// </summary>
        protected IList<EventInfo> ActiveEventHandlers
        {
            get { return new List<EventInfo>(_activeEventHandlers); }
        }

        /// <summary>
        /// Registers an event. Event callbacks will be marshalled into a thread-safe queue. The event will _not_ be dispatched automatically.
        /// To process an event, call <see cref="ProcessNextEvent">ProcessNextEvent</see> or <see cref="ProcessNextEventNoWait">ProcessNextEventNoWait</see>
        /// </summary>
        /// <typeparam name="TEventArgs">The type of EventArgs the event you're registering provides</typeparam>
        /// <param name="eventInfo">The EventInfo of your desired event. You can pass this manually, or use <see cref="ExposedEvent">ExposedEvent</see></param>
        /// <param name="callback">The delegate to be invoked when the queue is processed</param>
        protected void RegisterEvent<TEventArgs>(EventInfo eventInfo, Action<Object, TEventArgs> callback)
            where TEventArgs : EventArgs
        {
            var action = new Action<object, TEventArgs>((sender, args) =>
                {
                    lock (_eventQueue)
                    {
                        _eventQueue.Enqueue(() => callback(sender, args));
                        Monitor.Pulse(_eventQueue);
                    }
                });

            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, action.Target, action.Method);
            eventInfo.AddEventHandler(this, handler);

            _activeEventHandlers.Add(eventInfo);
        }

        /// <summary>
        /// Fires off the next event to the listening delegate, and will block until an event is received if no events are pending processing
        /// </summary>
        protected void ProcessNextEvent()
        {
            lock (_eventQueue)
            {
                while (_eventQueue.Count == 0)
                {
                    Monitor.Wait(_eventQueue);
                }
                _eventQueue.Dequeue()();
            }
        }

        /// <summary>
        /// Fires off the next event to the listening delegate. If no events are pending, it will not block. Returns true if an event was processed, false if not.
        /// </summary>
        /// <returns></returns>
        protected bool ProcessNextEventNoWait()
        {
            lock (_eventQueue)
            {
                if (_eventQueue.Count > 0)
                {
                    _eventQueue.Dequeue()();
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

        /// <summary>
        /// Allows the daemon to cleanup any resources. Called when the daemon is shutting down
        /// </summary>
        protected virtual void CleanUp()
        {
            
        }

        private void HandleException(Exception e)
        {
            //To be filled out when we have a logging mechanism in place
        }
    }
}
