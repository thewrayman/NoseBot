using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoseBot.Util
{
    public class BGLogger : IDisposable
    {

        Queue<Action> queue = new Queue<Action>();
        ManualResetEvent hasNewItems = new ManualResetEvent(false);
        ManualResetEvent terminate = new ManualResetEvent(false);
        ManualResetEvent waiting = new ManualResetEvent(false);

        Thread loggingThread;

        public BGLogger()
        {
            
            loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            // this is performed from a bg thread, to ensure the queue is serviced from a single thread
            loggingThread.Start();
            Console.WriteLine("Initialised Logger");
        }


        void ProcessQueue()
        {
            while (true)
            {
                waiting.Set();
                int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                // terminate was signaled 
                if (i == 1) return;
                hasNewItems.Reset();
                waiting.Reset();

                Queue<Action> queueCopy;
                lock (queue)
                {
                    queueCopy = new Queue<Action>(queue);
                    queue.Clear();
                }

                foreach (var log in queueCopy)
                {
                    log();
                }
            }
        }

        public void LogMessage(string message,string id, bool process=true)
        {
            Console.WriteLine("Logging message.."+message);
            lock (queue)
            {
                if (process)
                {
                    queue.Enqueue(() => AsyncLogProcess(message, id));
                }
                
            }
            hasNewItems.Set();
        }

        protected void AsyncLogProcess(string row, string id)
        {
           //Console.WriteLine("writing to file with row: "+row);
            File.AppendAllText(Path.Combine(FileDirUtil.GetGuildDir(id), FileDirUtil.PROCESSLOG), DateTime.Now+", "+ row + Environment.NewLine);
            //Console.WriteLine("added to queue");
        }
        protected void AsyncLogEvent(string row)
        {
            File.AppendAllText(FileDirUtil.GetGuildDir(FileDirUtil.EVENTLOG), row);
        }


        public void Flush()
        {
            waiting.WaitOne();
        }


        public void Dispose()
        {
            terminate.Set();
            loggingThread.Join();
        }
    }
}
