// See https://aka.ms/new-console-template for more information

using BinTool.Socketing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var service = new ServiceCollection();
service.AddLogging();
var provider = service.BuildServiceProvider();
var log = provider.GetService<ILogger<TcpConnection>>();

var socket = new ClientSocket("192.168.0.76",8088,OnMessageReceive,log);
socket.Connect();
Thread.Sleep(2000);
while (true)
{
    var buffer = new byte[] {1, 1, 0, 3, 0, 255, 255};
    Console.WriteLine(string.Join(" ", buffer));
    socket.QueueMessage(buffer);
    Thread.Sleep(1000);
}

Console.WriteLine("Hello, World!");


static void OnMessageReceive(byte[] buffer, TcpConnection tcpConnection)
{
    Console.WriteLine($"{buffer[0]}, {buffer.Length}");
}