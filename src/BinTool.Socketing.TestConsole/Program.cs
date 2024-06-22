// See https://aka.ms/new-console-template for more information



using BinTool.Socketing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var service = new ServiceCollection();
service.AddLogging();
var provider = service.BuildServiceProvider();
var log = provider.GetService<ILogger<TcpConnection>>();

var handler = new Test();
var socket = new ClientSocket("192.168.1.156",8088, handler, log);
socket.Connect();
Thread.Sleep(2000);
while (true)
{
    var buffer = new byte[] {1, 1, 0, 3, 0, 255, 255};
    Console.WriteLine(string.Join(" ", buffer));
    socket.QueueMessage(buffer);
    Test.Time = DateTime.Now;
    Thread.Sleep(100);
}

Console.WriteLine("Hello, World!");
Thread.Sleep(2000000);

public class Test : IDataReceiveHandler
{
    public static DateTime Time=DateTime.Now;

    public void HandleData(byte[] buffer, TcpConnection connection)
    {
        Console.WriteLine($"{buffer[0]}, {buffer.Length} Time: {(DateTime.Now - Test.Time).TotalMilliseconds}");
    }
}