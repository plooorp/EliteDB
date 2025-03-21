
namespace Importer;

using System.Text.Json.Nodes;
using System.Data.SQLite;
using static Utils;


public class EDSM
{
    static readonly EliteDB _db = EliteDB.Instance;

    public static void InsertSystemsPopulated(string path)
    {
        JsonObjectStreamReader systems = new(path);
        JsonObject? system;
        CreateTables();
        Progress progress = new();
        Timer timer = new(state => Console.WriteLine(state), progress, 2_000, 30_000);
        while ((system = systems.ReadObject()) is not null)
        {
            if (system["id64"] is null || system["name"] is null)
                continue;
            string name = system["name"]!.AsValue().GetValue<string>();
            long systemId64 = system["id64"]!.AsValue().GetValue<long>();
            // Console.Write($"{system.name} {system.x} {system.y} {system.z}");
            using SQLiteTransaction t = _db.BeginTransaction();
            try
            {
                InsertSystem(t, system);
                // List<JsonObject> bodies = TryJsonArrayToList<JsonObject>(TryGetArray(system, "bodies"));
                // foreach (JsonObject body in bodies)
                //     InsertBody(t, body, systemId64);
                List<JsonObject> factions = TryJsonArrayToList<JsonObject>(TryGetArray(system, "factions"), (node) => node.AsObject());
                foreach (JsonObject faction in factions)
                    InsertFaction(t, faction, systemId64);
                progress.systemsInserted++;
                // progress.bodiesInserted += bodies.Count;
                progress.factionsInserted += factions.Count;
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
                // Console.WriteLine($"Failed to insert system '{system["name"]} {e.Message} {e.StackTrace}'");
                Console.WriteLine($"Failed to insert system '{name}' {e.Message}");
            }
        }
        timer.Dispose();
        systems.Close();
        CreateIndexes();
        Console.WriteLine($"Finished with {progress.systemsInserted} systems, {progress.bodiesInserted}, and {progress.factionsInserted} factions inserted");
    }

    static void InsertSystem(SQLiteTransaction t, JsonObject obj)
    {
        using SQLiteCommand cmd = _db.CreateCommand(t);
        cmd.CommandText = @"
            INSERT INTO edsm_system(id, id64, name)
            VALUES(@id, @id64, @name);";
        cmd.Parameters.AddWithValue("@id", TryGetValueS<long>(obj, "id"));
        cmd.Parameters.AddWithValue("@id64", TryGetValueS<long>(obj, "id64"));
        cmd.Parameters.AddWithValue("@name", TryGetValueR<string>(obj, "name"));
        cmd.ExecuteNonQuery();
    }

    static void InsertFaction(SQLiteTransaction t, JsonObject obj, long? systemId64)
    {
        using SQLiteCommand cmd = _db.CreateCommand(t);
        cmd.CommandText = @"
            INSERT INTO edsm_system_faction(id, systemId64, name, allegiance,
                government, influence, state, activeStates, recoveringStates,
                pendingStates, happiness, isPlayer, lastUpdate)
            VALUES(@id, @systemId64, @name, @allegiance, @government,
                @influence, @state, @activeStates, @recoveringStates,
                @pendingStates, @happiness, @isPlayer, @lastUpdate);";
        cmd.Parameters.AddWithValue("@id", TryGetValueS<long>(obj, "id"));
        cmd.Parameters.AddWithValue("@systemId64", systemId64);
        cmd.Parameters.AddWithValue("@name", TryGetValueR<string>(obj, "name"));
        cmd.Parameters.AddWithValue("@allegiance", TryGetValueR<string>(obj, "allegiance"));
        cmd.Parameters.AddWithValue("@government", TryGetValueR<string>(obj, "government"));
        cmd.Parameters.AddWithValue("@influence", TryGetValueS<double>(obj, "influence"));
        cmd.Parameters.AddWithValue("@state", TryGetValueR<string>(obj, "state"));
        cmd.Parameters.AddWithValue("@activeStates", TryGetArray(obj, "activeStates")?.ToJsonString());
        cmd.Parameters.AddWithValue("@recoveringStates", TryGetArray(obj, "recoveringStates")?.ToJsonString());
        cmd.Parameters.AddWithValue("@pendingStates", TryGetArray(obj, "pendingStates")?.ToJsonString());
        cmd.Parameters.AddWithValue("@happiness", TryGetValueR<string>(obj, "happiness"));
        cmd.Parameters.AddWithValue("@isPlayer", TryGetValueS<bool>(obj, "isPlayer"));
        cmd.Parameters.AddWithValue("@lastUpdate", TryGetValueS<long>(obj, "lastUpdate"));
        cmd.ExecuteNonQuery();
    }

    class Progress
    {
        public Progress()
        {
            startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            systemsInserted = 0;
            bodiesInserted = 0;
            factionsInserted = 0;
        }
        public long startTime;
        public long systemsInserted;
        public long bodiesInserted;
        public long factionsInserted;
        override public string ToString()
        {
            long systemRate, bodyRate, factionRate;
            long delta = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime;
            if (delta == 0)
            {
                systemRate = 0;
                bodyRate = 0;
                factionRate = 0;
            }
            else
            {
                systemRate = systemsInserted / delta;
                bodyRate = bodiesInserted / delta;
                factionRate = factionsInserted / delta;
            }
            return $"Insertions: system {systemsInserted} ({systemRate}/s) body {bodiesInserted} ({bodyRate}/s) faction {factionsInserted} ({factionRate}/s)";
        }
    }

    static void CreateTables()
    {
        string[] statements = {
            @"CREATE TABLE IF NOT EXISTS 'edsm_system'(
                'id' INTEGER,
                'id64' INTEGER PRIMARY KEY,
                'name' TEXT NOT NULL UNIQUE
            );",
            @"CREATE TABLE IF NOT EXISTS 'edsm_system_faction'(
                'id' INTEGER,
                'systemId64' INTEGER,
                'name' TEXT,
                'allegiance' TEXT,
                'government' TEXT,
                'influence' REAL,
                'state' TEXT,
                'activeStates' TEXT,
                'recoveringStates' TEXT,
                'pendingStates' TEXT,
                'happiness' TEXT,
                'isPlayer' INTEGER,
                'lastUpdate' INTEGER,
                PRIMARY KEY ('id', 'systemId64'),
                FOREIGN KEY ('systemId64') REFERENCES 'edsm_system'('systemId64')
            );"
        };
        using SQLiteCommand cmd = _db.CreateCommand();
        foreach (string statement in statements)
        {
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }
    }

    static void CreateIndexes()
    {
        string[] statements = {
            "CREATE INDEX IF NOT EXISTS 'edsm_system_name' ON 'edsm_system'('name');",
            "CREATE INDEX IF NOT EXISTS 'edsm_system_faction_name' ON 'edsm_system_faction'('name');",
        };
        using SQLiteCommand cmd = _db.CreateCommand();
        foreach (string statement in statements)
        {
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }
    }
}
