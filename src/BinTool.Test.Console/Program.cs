// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Diagnostics;
using BinTool.DataStructs;

TestEnum();

static void TestEnum()
{
    var personTypeList = PersonType.Unknow.GetEnumList();
    foreach (var data in personTypeList)
    {
        Console.WriteLine($"Type: {data.Type.ToString()}, Value: {data.Value}, Description: {data.Description}");
    }

    var personTypeData = PersonType.Young.GetEnumData();
    Console.WriteLine("Print PersonType.Young Infomation");
    Console.WriteLine($"Type: {personTypeData.Type.ToString()}, Value: {personTypeData.Value}, Description: {personTypeData.Description}");

    var stopwatch = new Stopwatch();
    stopwatch.Start();
    for (var i = 0; i < 1000000; i++)
    {
        var s = (int) PersonType.Young;
    }
    stopwatch.Stop();
    Console.WriteLine($"Normal get value millseconds: {stopwatch.ElapsedMilliseconds}");
    
    stopwatch.Restart();
    for (var i = 0; i < 1000000; i++)
    {
        var s =  PersonType.Young.GetEnumData().Value;
    }
    stopwatch.Stop();
    Console.WriteLine($"BinTool Enum get value millseconds: {stopwatch.ElapsedMilliseconds}");
}

static void TestSegment()
{
    var arr = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    var segment = new BinArraySegment<byte>(arr);
    Console.WriteLine("Origin:");
    Console.WriteLine(segment.ToArray().Print());
    Console.WriteLine();

    segment = segment.Slice(2, 30);
    Console.WriteLine("Slice:2,30");
    Console.WriteLine(segment.ToArray().Print());
    Console.WriteLine();
    //
    // segment = segment.Slice(5);
    // segment[0] = 11;
    // Console.WriteLine("Slice:5");
    // Console.WriteLine(segment.ToArray().Print());
    // Console.WriteLine();
    //
    // Console.WriteLine("Origin inner array");
    // Console.WriteLine(segment.Array.Print());
    // Console.WriteLine();
    // Console.WriteLine("Hello, World!");
}

enum PersonType
{
    [Description("未知")]
    Unknow = 0,
    [Description("儿童")]
    Child = 1,
    [Description("青少年")]
    Young = 2,
    [Description("成年人")]
    Adult = 3,
    [Description("老人")]
    Older = 4
}
