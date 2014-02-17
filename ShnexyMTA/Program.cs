using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shnexy.Utilities;
using Shnexy.Models;
using System.Diagnostics;
using System.Net.Http;

namespace ShnexyMTA
{
    public class Program
    {
       
        static void Main(string[] args )
        {
            int Interval = 3000;

            if ((args.Length >0) && (args[0].ToInt() > 0))
                Interval = args[0].ToInt();
            Engine curEngine = new Engine();
            //create a thread
            //while true, check the queues. If the queues are not empty, process them
            while (true)
            {
                Console.Out.WriteLine("Processing Queues...");
                Debug.WriteLine("I'm going to output!"); //doesn't show up!?

                string baseURL = "http://localhost:50613/Engine/ProcessQueues";
                HttpClient httpShnexyClient = new HttpClient();
                HttpContent httpContent = new StringContent("");
                HttpResponseMessage response =
                    httpShnexyClient.PostAsync(baseURL, httpContent).Result; ;




                Console.Out.WriteLine("Sleeping Now...");
                Thread.Sleep(Interval);
            }
            
        }
    }

}

