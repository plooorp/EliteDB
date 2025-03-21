
namespace Importer;

using System.Data.SQLite;
using System.Text.Json.Nodes;
using static Utils;


public class Spansh
{
    static readonly EliteDB _db = EliteDB.Instance;

    public static void InsertGalaxyFile(string path)
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
                List<JsonObject> bodies = TryJsonArrayToList<JsonObject>(TryGetArray(system, "bodies"), (node) => node.AsObject());
                foreach (JsonObject body in bodies)
                    InsertBody(t, body, systemId64);
                List<JsonObject> factions = TryJsonArrayToList<JsonObject>(TryGetArray(system, "factions"), (node) => node.AsObject());
                foreach (JsonObject faction in factions)
                    InsertFaction(t, faction, systemId64);
                progress.systemsInserted++;
                progress.bodiesInserted += bodies.Count;
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
            INSERT INTO spansh_system(id64, name, x, y, z, allegiance, government, primaryEconomy,
                                      secondaryEconomy, security, population, bodyCount,
                                      controllingFactionName, controllingFactionGovernment,
                                      controllingFactionAllegiance, controllingPower, powers,
                                      powerState, date)
            VALUES($id64, $name, $x, $y, $z, $allegiance, $government, $primaryEconomy,
                   $secondaryEconomy, $security, $population, $bodyCount, $controllingFactionName,
                   $controllingFactionGovernment, $controllingFactionAllegiance, $controllingPower,
                   $powers, $powerState, $date);";
        cmd.Parameters.AddWithValue("$id64", TryGetValueS<long>(obj, "id64"));
        cmd.Parameters.AddWithValue("$name", TryGetValueR<string>(obj, "name"));
        cmd.Parameters.AddWithValue("$x", TryGetValueS<double>(obj, "coords", "x"));
        cmd.Parameters.AddWithValue("$y", TryGetValueS<double>(obj, "coords", "y"));
        cmd.Parameters.AddWithValue("$z", TryGetValueS<double>(obj, "coords", "z"));
        cmd.Parameters.AddWithValue("$allegiance", TryGetValueR<string>(obj, "allegiance"));
        cmd.Parameters.AddWithValue("$government", TryGetValueR<string>(obj, "government"));
        cmd.Parameters.AddWithValue("$primaryEconomy", TryGetValueR<string>(obj, "primaryEconomy"));
        cmd.Parameters.AddWithValue("$secondaryEconomy", TryGetValueR<string>(obj, "secondaryEconomy"));
        cmd.Parameters.AddWithValue("$security", TryGetValueR<string>(obj, "security"));
        cmd.Parameters.AddWithValue("$population", TryGetValueS<long>(obj, "population"));
        cmd.Parameters.AddWithValue("$bodyCount", TryGetValueS<int>(obj, "bodyCount"));
        cmd.Parameters.AddWithValue("$controllingFactionName", TryGetValueR<string>(obj, "controllingFaction", "name"));
        cmd.Parameters.AddWithValue("$controllingFactionGovernment", TryGetValueR<string>(obj, "controllingFaction", "government"));
        cmd.Parameters.AddWithValue("$controllingFactionAllegiance", TryGetValueR<string>(obj, "controllingFaction", "allegiance"));
        cmd.Parameters.AddWithValue("$controllingPower", TryGetValueR<string>(obj, "controllingPower"));
        cmd.Parameters.AddWithValue("$powers", TryGetArray(obj, "powers")?.ToJsonString());
        cmd.Parameters.AddWithValue("$powerState", TryGetValueR<string>(obj, "powerState"));
        cmd.Parameters.AddWithValue("$date", SpanshTimestampToUNIX(TryGetValueR<string>(obj, "date")));
        cmd.ExecuteNonQuery();

        // factions = TryJsonArrayToList<Faction>(TryGetArray(obj, "factions"), (node) => new Faction((JsonObject)node, nonmemberID64));
        // bodies = TryJsonArrayToList<Body>(TryGetArray(obj, "bodies"), (node) => new Body((JsonObject)node, nonmemberID64));
        // foreach (Spansh.Faction faction in system.factions)
        //     InsertSpanshFaction(t, faction);
        // foreach (Spansh.Body body in system.bodies)
        //     InsertSpanshBody(t, body);
    }

    static void InsertFaction(SQLiteTransaction t, JsonObject obj, long? systemId64)
    {
        using SQLiteCommand cmd = _db.CreateCommand(t);
        cmd.CommandText = @"
            INSERT INTO spansh_system_faction(name, systemId64, allegiance, government, influence, state)
            VALUES($name, $systemId64, $allegiance, $government, $influence, $state);";
        cmd.Parameters.AddWithValue("$name", TryGetValueR<string>(obj, "name"));
        cmd.Parameters.AddWithValue("$systemId64", systemId64);
        cmd.Parameters.AddWithValue("$allegiance", TryGetValueR<string>(obj, "allegiance"));
        cmd.Parameters.AddWithValue("$government", TryGetValueR<string>(obj, "governmentstate"));
        cmd.Parameters.AddWithValue("$influence", TryGetValueS<double>(obj, "influencestate"));
        cmd.Parameters.AddWithValue("$state", TryGetValueR<string>(obj, "state"));
        cmd.ExecuteNonQuery();
    }

    static void InsertBody(SQLiteTransaction t, JsonObject obj, long? systemId64)
    {
        using SQLiteCommand cmd = _db.CreateCommand(t);
        cmd.CommandText = @"
            INSERT INTO spansh_body(id64, systemId64, bodyId, name, type, subType, distanceToArrival)
            VALUES($id64, $systemId64, $bodyId, $name, $type, $subType, $distanceToArrival);";
        cmd.Parameters.AddWithValue("$id64", TryGetValueS<long>(obj, "id64"));
        cmd.Parameters.AddWithValue("$systemId64", systemId64);
        cmd.Parameters.AddWithValue("$bodyId", TryGetValueS<int>(obj, "bodyId"));
        cmd.Parameters.AddWithValue("$name", TryGetValueR<string>(obj, "name"));
        cmd.Parameters.AddWithValue("$type", TryGetValueR<string>(obj, "type"));
        cmd.Parameters.AddWithValue("$subType", TryGetValueR<string>(obj, "subType"));
        cmd.Parameters.AddWithValue("$distanceToArrival", TryGetValueS<double>(obj, "distanceToArrival"));
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
            @"CREATE TABLE IF NOT EXISTS 'spansh_system'(
                'id64' INTEGER PRIMARY KEY,
                'name' TEXT NOT NULL UNIQUE,
                'x' REAL,
                'y' REAL,
                'z' REAL,
                'allegiance' TEXT,
                'government' TEXT,
                'primaryEconomy' TEXT,
                'secondaryEconomy' TEXT,
                'security' TEXT,
                'population' INTEGER,
                'bodyCount' INTEGER,
                'controllingFactionName' TEXT,
                'controllingFactionGovernment' TEXT,
                'controllingFactionAllegiance' TEXT,
                'controllingPower' TEXT,
                'powers' TEXT,
                'powerState' TEXT,
                'date' INTEGER,
                FOREIGN KEY ('controllingFactionName') REFERENCES 'spansh_system_faction'('name')
            );",
            @"CREATE TABLE IF NOT EXISTS 'spansh_body'(
                'id64' INTEGER PRIMARY KEY,
                'systemId64' INTEGER NOT NULL,
                'bodyId' INTEGER,
                'name' TEXT NOT NULL,
                'type' TEXT,
                'subType' TEXT,
                'distanceToArrival' REAL,
                FOREIGN KEY ('systemId64') REFERENCES 'spansh_system'('id64')
            );",
            @"CREATE TABLE IF NOT EXISTS 'spansh_system_faction'(
                'name' TEXT,
                'systemId64' INTEGER,
                'allegiance' TEXT,
                'government' TEXT,
                'influence' REAL,
                'state' TEXT,
                PRIMARY KEY ('name', 'systemId64'),
                FOREIGN KEY ('systemId64') REFERENCES 'spansh_system'('id64')
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
            "CREATE INDEX IF NOT EXISTS 'spansh_system_name' ON 'spansh_system'('name');",
            "CREATE INDEX IF NOT EXISTS 'spansh_system_coords' ON 'spansh_system'('z', 'y', 'z');",
            "CREATE INDEX IF NOT EXISTS 'spansh_body_systemId64' ON 'spansh_body'('systemId64');",
            "CREATE INDEX IF NOT EXISTS 'spansh_body_name' ON 'spansh_body'('name');"
        };
        using SQLiteCommand cmd = _db.CreateCommand();
        foreach (string statement in statements)
        {
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }
    }
}
