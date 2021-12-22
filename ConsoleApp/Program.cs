using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ModelLibrary;
using YOLOv4MLNet;

namespace ConsoleApp
{
    class Program
    {
        
        
        private static async Task Consumer()
        {
            string str;
            while (true)
            {
                str = await  abc.bB.ReceiveAsync();
                if (str == "end")
                    break;
                Console.WriteLine(str);
            }
            
          
            
          
        }
        static async Task Main()
        {

            Console.WriteLine("Write input path: ");
            string imageFolder = @"D:\yolo\YOLOv4MLNet\YOLOv4MLNet\Assets\Images";
            await Task.WhenAll(abc.Main(imageFolder), Consumer());

         
        }
    }
}
