namespace Importer;

using System.Data.SQLite;
// using Npgsql;


public class EliteDB
{
    // enum ConnectionType { SQLITE, POSTGRES }
    // ConnectionType _connectionType = ConnectionType.POSTGRES;
    SQLiteConnection _con;
    static EliteDB? _instance = null;
    public static EliteDB Instance
    {
        get
        {
            if (_instance is null)
                _instance = new EliteDB();
            return _instance;
        }
    }

    private EliteDB()
    {
        // _con = new NpgsqlConnection("Host=host;Username=mylogin;Password=mypass;Database=EliteDB");
        // _con.Open();
        _con = new SQLiteConnection("Data Source=edb.sqlite;Version=3");
        _con.Open();
        SQLiteCommand cmd = CreateCommand();
        cmd.CommandText = "PRAGMA busy_timeout = 10000;";
        cmd.ExecuteNonQuery();
    }

    public SQLiteTransaction BeginTransaction()
    {
        return _con.BeginTransaction();
    }

    public SQLiteCommand CreateCommand(SQLiteTransaction t)
    {
        SQLiteCommand cmd = new();
        cmd.Transaction = t;
        return cmd;
    }

    public SQLiteCommand CreateCommand()
    {
        return _con.CreateCommand();
    }

    public void Close()
    {
        _con.Close();
        _con.Dispose();
    }
}
