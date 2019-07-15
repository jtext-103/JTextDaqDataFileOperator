using System;

namespace JTextDAQDataFileOperator.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //模仿多线程从 N 张采集卡分别接收数据并最终结束，采样率等其他参数在方法内设置
            //MultithreadWriter myMultithreadWriter = new MultithreadWriter(2, @"C:\Users\wyx\Desktop\");
            //myMultithreadWriter.Begin();

            //测试读方法，看数据可在类方法中设置断点
            Reader myReader = new Reader(@"C:\Users\wyx\Desktop\", "0", 0);      
            myReader.Begin();

            Console.WriteLine("Mission complete!");
            Console.ReadKey(false);
        }
    }
}
