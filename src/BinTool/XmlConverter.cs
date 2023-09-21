using System.Xml.Serialization;

namespace BinTool;

public class XmlConverter
{
    public static T? Deserialize<T>(string? payload)
    {
        T? obj = default(T);

        if (string.IsNullOrWhiteSpace(payload))
        {
            return obj;
        }

        var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        var serializer = new XmlSerializer(typeof(T));
	return (T?)serializer.Deserialize(xmlStream);
    }
}
