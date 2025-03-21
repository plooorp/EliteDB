namespace Importer;

using System.Text.Json.Nodes;

public static class Utils
{
    public static T? TryGetValueS<T>(JsonObject obj, params string[] keys) where T : struct
    {
        JsonNode? o = obj;
        foreach (string key in keys)
        {
            o = o[key];
            if (o is null)
                return null;
        }
        return o.AsValue().GetValue<T>();
    }

    public static T? TryGetValueR<T>(JsonObject obj, params string[] keys) where T : class
    {
        JsonNode? o = obj;
        foreach (string key in keys)
        {
            o = o[key];
            if (o is null)
                return null;
        }
        return o.AsValue().GetValue<T>();
    }

    public static JsonObject? TryGetObject(JsonObject obj, params string[] keys)
    {
        JsonNode? o = obj;
        foreach (string key in keys)
        {
            o = o[key];
            if (o is null)
                return null;
        }
        try { return o.AsObject(); }
        catch (InvalidOperationException) { return null; }
    }

    public static JsonArray? TryGetArray(JsonObject obj, params string[] keys)
    {
        JsonNode? o = obj;
        foreach (string key in keys)
        {
            o = o[key];
            if (o is null)
                return null;
        }
        try { return o.AsArray(); }
        catch (InvalidOperationException) { return null; }
    }

    // public static IEnumerable<T> JsonArrayEnumerator<T>(JsonArray arr) where T : class
    // {
    //     IEnumerator<JsonNode?> iter = arr.GetEnumerator();
    //     return (T)iter.Current;
    // }

    public static DateTime? ParseEDSMTimestamp(string? timestamp)
    {
        if (timestamp is null || timestamp.Length == 0)
            return null;
        try { return DateTime.ParseExact(timestamp + "Z", "yyyy-MM-dd HH:mm:ssK", null); }
        catch (Exception) { return null; }
    }

    public static long? EDSMTimestampToUNIX(string? timestamp)
    {
        if (timestamp is null || timestamp.Length == 0)
            return null;
        try
        {
            DateTime t = DateTime.ParseExact(timestamp + "Z", "yyyy-MM-dd HH:mm:ssK", null);
            return ((DateTimeOffset)t).ToUnixTimeSeconds();
        }
        catch (Exception) { return null; }
    }

    public static DateTime? ParseSpanshTimestamp(string? timestamp)
    {
        if (timestamp is null)
            return null;
        try { return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:sszz", null); }
        catch (Exception) { return null; }
    }

    public static long? SpanshTimestampToUNIX(string? timestamp)
    {
        if (timestamp is null)
            return null;
        try
        {
            DateTime t = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:sszz", null);
            return ((DateTimeOffset)t).ToUnixTimeSeconds();
        }
        catch (Exception) { return null; }
    }

    public static List<T> TryJsonArrayToList<T>(JsonArray? arr)
    {
        if (arr is null)
            return new List<T>(0);
        List<T> list = new(arr.Count);
        foreach (JsonNode? node in arr)
        {
            try
            {
                if (node is null)
                    continue;
                list.Add(node.AsValue().GetValue<T>());
            }
            catch (Exception) { continue; }
        }
        return list;
    }

    public static List<T> TryJsonArrayToList<T>(JsonArray? arr, Func<JsonNode, T> intermediary)
    {
        if (arr is null)
            return new List<T>(0);
        List<T> list = new(arr.Count);
        foreach (JsonNode? node in arr)
        {
            try
            {
                if (node is null)
                    continue;
                list.Add(intermediary(node));
            }
            catch (Exception) { continue; }
        }
        return list;
    }
}
