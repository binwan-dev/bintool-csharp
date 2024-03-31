// See https://aka.ms/new-console-template for more information

using BinTool.DataStructs;

var arr = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

var segment = new BinArraySegment<byte>(arr);
Console.WriteLine("Origin:");
Console.WriteLine(segment.ToArray().Print());
Console.WriteLine();

segment = segment.Slice(2);
Console.WriteLine("Slice:2");
Console.WriteLine(segment.ToArray().Print());
Console.WriteLine();

segment = segment.Slice(5);
segment[0] = 11;
Console.WriteLine("Slice:5");
Console.WriteLine(segment.ToArray().Print());
Console.WriteLine();

Console.WriteLine("Origin inner array");
Console.WriteLine(segment.Array.Print());
Console.WriteLine();
Console.WriteLine("Hello, World!");