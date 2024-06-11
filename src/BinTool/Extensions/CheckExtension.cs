namespace System;

public static class CheckExtension
{
    public static T NotNull<T>(this T val, string tipsMessage) => NotNull(val, tipsMessage, null);
    
    public static T NotNull<T>(this T val, string tipsMessage, Dictionary<string,string>? data)
    {
        if (val == null) throw new CheckException(tipsMessage);

        var strVal = val as string;
        if (strVal != null && strVal == "")
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);  
            throw new CheckException(tipsMessage);
        }

        return val;
    }

    public static T NotNull<T>(this T val, CheckExceptionInfo tips) => NotNull(val, tips, null);

    public static T NotNull<T>(this T val, CheckExceptionInfo tips, Dictionary<string, string>? data)
    {
        if (val == null)
        {
            if (data != null) tips.Message = SetTipsData(tips.Message, data);
            throw new CheckException(tips);
        }
        return val;
    }

    public static string NotNullOrWhiteSpace(this string val, string tipsMessage) => NotNullOrWhiteSpace(val, tipsMessage, null);
    
    public static string NotNullOrWhiteSpace(this string val, string tipsMessage, Dictionary<string, string>? data)
    {
        if (string.IsNullOrWhiteSpace(val))
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static string NotNullOrWhiteSpace(this string val, CheckExceptionInfo tipsMessage) => NotNullOrWhiteSpace(val, tipsMessage, null);
    
    public static string NotNullOrWhiteSpace(this string val, CheckExceptionInfo tipsMessage, Dictionary<string, string>? data)
    {
        if (string.IsNullOrWhiteSpace(val))
        {
            if (data != null) tipsMessage.Message = SetTipsData(tipsMessage.Message, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static bool MustBeTrue(this bool val, string tipsMessage) => MustBeTrue(val, tipsMessage, null);
    
    public static bool MustBeTrue(this bool val, string tipsMessage, Dictionary<string,string>? data)
    {
        if (!val)
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static bool MustBeTrue(this bool val, CheckExceptionInfo tipsMessage) => MustBeTrue(val, tipsMessage, null);
    
    public static bool MustBeTrue(this bool val, CheckExceptionInfo tipsMessage, Dictionary<string,string>? data)
    {
        if (!val)
        {
            if (data != null) tipsMessage.Message = SetTipsData(tipsMessage.Message, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static bool MustBeFalse(this bool val, string tipsMessage) => MustBeFalse(val, tipsMessage, null);
    
    public static bool MustBeFalse(this bool val, string tipsMessage, Dictionary<string,string>? data)
    {
        if (val)
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static bool MustBeFalse(this bool val, CheckExceptionInfo tipsMessage) => MustBeFalse(val, tipsMessage, null);
    
    public static bool MustBeFalse(this bool val, CheckExceptionInfo tipsMessage, Dictionary<string,string>? data)
    {
        if (val)
        {
            if (data != null) tipsMessage.Message = SetTipsData(tipsMessage.Message, data);
            throw new CheckException(tipsMessage);
        }
        return val;
    }

    public static T[] CheckLength<T>(this T[] tlist, int min, int max, string tipsMessage) => CheckLength(tlist, min, max, tipsMessage, null);
    
    public static T[] CheckLength<T>(this T[] tlist, int min, int max, string tipsMessage, Dictionary<string,string>? data)
    {
        if (tlist.Length < min || tlist.Length > max)
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new ArgumentException(tipsMessage);
        }

        return tlist;
    }

    public static string IPv4(this string val, string tipsMessage) => IPv4(val, tipsMessage, null);
    
    public static string IPv4(this string val, string tipsMessage, Dictionary<string,string>? data)
    {
        var regex = @"(\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5])";
        if (!Regex.IsMatch(val, regex))
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new ArgumentException(tipsMessage);
        }

        return val;
    }

    public static int Port<T>(this T val, string tipsMessage) => Port(val, tipsMessage, null);
    
    public static int Port<T>(this T val, string tipsMessage, Dictionary<string,string>? data)
    {
        int? intVal;

        var strVal = val as string;
        if (strVal != null && int.TryParse(strVal, out int intVals))
        {
            intVal = intVals;
        }
        else
        {
            intVal = val as int?;
        }

        if (intVal == null || intVal <= 0 || intVal > 65535)
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new ArgumentException(tipsMessage);
        }

        return intVal.Value;
    }

    public static bool MustTrue(this bool val, string tipsMessage) => MustTrue(val, tipsMessage,null);

    public static bool MustTrue(this bool val, string tipsMessage, Dictionary<string, string>? data)
    {
        if (!val)
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);
            throw new ArgumentException(tipsMessage);
        }

        return val;
    }

    private static string SetTipsData(string tips,Dictionary<string,string> data)
    {
        foreach (var item in data)
        {
            tips = tips.Replace(item.Key, item.Value);
        }

        return tips;
    }
}

public class CheckExceptionInfo
{
    public CheckExceptionInfo(string message) : this(999, message)
    { }

    public CheckExceptionInfo(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public int Code{ get; set; }

    public string Message { get; set; } = string.Empty;
}

public class CheckException : Exception
{
    public CheckException(string message) : base(message)
    {
        Info = new CheckExceptionInfo(message);
    }

    public CheckException(CheckExceptionInfo info) : base(info.Message)
    {
        Info = info;
    }

    public CheckExceptionInfo Info{ get; set; }
}
