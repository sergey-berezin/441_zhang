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
                str = await  abc.bufferBlock.ReceiveAsync();
                if (str == "end")
                    break;
                Console.WriteLine(str);
            }
            
          
            
          
        }
        static async Task Main()
        {
            

            await Task.WhenAll(abc.Main(), Consumer());

         
        }
    }
}
