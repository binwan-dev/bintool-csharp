using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Org.BouncyCastle.Asn1.X509;

namespace System;

public static class EnumExtension
{
    private static IDictionary<string, IEnumDataList> _enumDicts = new Dictionary<string, IEnumDataList>();
    private static object _lock = new();

    public static EnumData<T>[] GetEnumList<T>(this T value) where T : Enum
    {
        if (_enumDicts.TryGetValue(typeof(T).FullName, out var enums))
            return ((EnumDataList<T>) enums).EnumData.Values.ToArray();

        lock (_lock)
        {
            var enumDataList = new EnumDataList<T>();
            var typeT = typeof(T);
            foreach (var field in typeT.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumData = new EnumData<T>();
                enumData.Type = (T) Enum.Parse(typeT, field.Name);
                enumData.Value = (int) Enum.Parse(typeT, field.Name);
                enumData.Name = field.Name;
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null)
                {
                    enumData.Description = attr.Description;
                }

                enumDataList.EnumData.Add(enumData.Name, enumData);
            }

            if (!_enumDicts.ContainsKey(typeT.FullName))
                _enumDicts.Add(typeT.FullName, enumDataList);
            return enumDataList.EnumData.Values.ToArray();
        }
    }
    
    public static EnumData<T> GetEnumData<T>(this T value) where T : Enum
    {
        if (!_enumDicts.TryGetValue(typeof(T).FullName, out var enums))
        {
            var array = GetEnumList(value);
            return array.FirstOrDefault(p => p.Name == value.ToString()) ?? new EnumData<T>();
        }
        
        var enumDataList = (EnumDataList<T>) enums;
        if (!enumDataList.EnumData.TryGetValue(value.ToString(), out var enumData)) return new EnumData<T>();
        return enumData;
    }
}

internal interface IEnumDataList
{
}

public class EnumDataList<T>:IEnumDataList where T : Enum
{
    public Dictionary<string,EnumData<T>> EnumData { get; set; } = new Dictionary<string, EnumData<T>>();
}

public class EnumData<T> where T:Enum
{
    public T Type{ get; set; }

    public string Name { get; set; } = string.Empty;

    public int Value { get; set; }

    public string Description { get; set; } = string.Empty;
}
