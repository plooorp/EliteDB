namespace Importer;

using System.Text.Json.Nodes;
using System.Text.Json;

public class JsonObjectStreamReader
{
    StreamReader file;

    public JsonObjectStreamReader(string path)
    {
        file = File.OpenText(path);
    }

    public JsonNode? ReadNode()
    {
        string? line;
        while ((line = file.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Equals("[") || line.Equals("]"))
                continue;
            if (line.Substring(line.Length - 1).Equals(","))
                line = line.Substring(0, line.Length - 1);
            try { return JsonNode.Parse(line); }
            catch (JsonException e)
            {
                Console.WriteLine($"Failed to parse entry: {e} {line}");
                continue;
            }
        }
        return null;
    }

    public JsonObject? ReadObject()
    {
        JsonNode? node = ReadNode();
        if (node is null)
            return null;
        return node.AsObject();
    }

    public JsonArray? ReadArray()
    {
        JsonNode? node = ReadNode();
        if (node is null)
            return null;
        return node.AsArray();
    }

    public void Close()
    {
        file.Close();
    }
}
