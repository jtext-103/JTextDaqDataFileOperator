using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JTextDAQDataFileOperator.ConsoleTest
{
    public class MultithreadWriter
    {
        public int FileCount { get; set; }

        public string RootPath { get; set; }

        List<FileWriter> myFileWriters;

        public MultithreadWriter(int fileCount, string rootPath)
        {
            FileCount = fileCount;
            RootPath = rootPath;

            myFileWriters = new List<FileWriter>();
            for (int i = 0; i < fileCount; i++)
            {
                myFileWriters.Add(new FileWriter(i, rootPath));
            }
        }

        public void Begin()
        {
            long ticks = DateTime.Now.Ticks;
            Console.WriteLine("Start Writing...");
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < FileCount; i++)
            {
                var j = i;
                var t = Task.Run(() => myFileWriters[j].Begin());
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("Write Finished, Time: {0}s", ticks / 10000000);
        }
    }
}
