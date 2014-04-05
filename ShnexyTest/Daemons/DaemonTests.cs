using System;
using System.Linq;
using System.Threading;
using Daemons;
using Moq;
using NUnit.Framework;

namespace ShnexyTest.Daemons
{
    [TestFixture]
    public class DaemonTests
    {
        [Test]
        public void CannotStartDaemonTwice()
        {
            var mockDaemon = new Mock<Daemon>().Object;
            Assert.True(mockDaemon.Start());
            Assert.False(mockDaemon.Start());
        }

        private class TestDaemon : Daemon
        {
            private readonly Action _execute;

            public TestDaemon(Action execute)
            {
                _execute = execute;
            }

            public override int WaitTimeBetweenExecution
            {
                get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
            }

            protected override void Run()
            {
                _execute();
            }
        }

        private Daemon StartDaemonAndAwaitStartup(Action daemonAction, out Thread workingThread)
        {
            var threadLocker = new object();
            Thread workerThread = null;
            var mockDaemon = new TestDaemon(
                () =>
                    {
                        lock (threadLocker)
                        {
                            workerThread = Thread.CurrentThread;
                            Monitor.Pulse(threadLocker);
                        }
                        daemonAction();
                    });
            Assert.True(mockDaemon.Start());
            lock (threadLocker)
            {
                while (workerThread == null)
                {
                    Monitor.Wait(threadLocker);
                }
            }

            workingThread = workerThread;
            return mockDaemon;
        }

        [Test]
        public void DaemonGracefullyShutsDown()
        {
            const int threadExecuteTime = 3000;
            var hasFinished = false;
            Thread workerThread;
            var mockDaemon = StartDaemonAndAwaitStartup(() =>
                {
                    Thread.Sleep(threadExecuteTime);
                    hasFinished = true;
                }, out workerThread);

            mockDaemon.Stop();
            workerThread.Join();
            
            Assert.True(hasFinished);
        }

        [Test]
        public void DaemonHandlesExceptionsAndDoesNotCrash()
        {
            var workLock = new object();
            int workNumber = 0;
            var finished = false;
            Thread workerThread;
            const string testException = "Test exception";
            var mockDaemon = StartDaemonAndAwaitStartup(() =>
            {
                lock (workLock)
                {
                    if (workNumber == 0)
                    {
                        workNumber++;
                        throw new Exception(testException);
                    }
                    finished = true;
                    Monitor.Pulse(workLock);
                }
            }, out workerThread);

            lock (workLock)
            {
                while (!finished)
                {
                    Monitor.Wait(workLock);
                }
            }
            Assert.True(mockDaemon.IsRunning);
            Assert.AreEqual(1, mockDaemon.LoggedExceptions.Count);
            Assert.AreEqual(testException, mockDaemon.LoggedExceptions.First().Message);
        }
    }
}
