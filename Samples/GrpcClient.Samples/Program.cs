using Grpc.Net.Client;
using GrpcService.Samples;
using System;
using System.Threading.Tasks;

namespace GrpcClient.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "lostsea" });
            Console.WriteLine("Greeter 服务返回数据: " + reply.Message);

            var client1 = new Cat.CatClient(channel);
            var reply1 = client1.GetCatName(
                new CatRequest { Name = "妮妮" });
            Console.WriteLine("Cat 服务返回数据: " + reply1.Message);



            Console.ReadKey();

        }
    }
}
