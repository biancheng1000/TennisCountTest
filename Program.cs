using DataProviderService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server;

namespace EventAnaliyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            EventAnaliyServiceEmp handler = new DataProviderService.EventAnaliyServiceEmp();
            RigourTech.TennisDataAnaliy.Processor pro = new RigourTech.TennisDataAnaliy.Processor(handler);
           
           //Task t1= Task.Run(()=> 
           // {
           //     Console.WriteLine("接收客户端服务启动");
           //     Thrift.Transport.TServerSocket ts = new Thrift.Transport.TServerSocket(1889);
           //     TServer server = new TSimpleServer(pro, ts);
           //     server.Serve();
                
           // });

           //Task t2= Task.Run(()=> 
           // {
                Console.WriteLine("接收底层数据服务启动");
                Thrift.Transport.TServerSocket ts = new Thrift.Transport.TServerSocket(8807);
                TServer server = new Thrift.Server.TThreadedServer(pro, ts);
                server.Serve();
            //});

            //Task.WaitAll(new Task[] {t2 });
        }
    }
}
