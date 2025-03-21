
namespace Importer;


static class Program
{
    static EliteDB? _db = null;

    static void Main(string[] args)
    {
        int exitCode = 1;
        try
        {
            _db = EliteDB.Instance;

            string spanshGalaxyPath = "spansh/galaxy_1day.json";
            string edsmSystemsPopulatedPath = "edsm/systemsPopulated.json";

            Spansh.InsertGalaxyFile(spanshGalaxyPath);
            EDSM.InsertSystemsPopulated(edsmSystemsPopulatedPath);
            exitCode = 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Program crashed unexpectedly: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            _db?.Close();
        }
        Console.Write("enter to continue");
        Console.ReadLine();
        Environment.Exit(exitCode);
    }
}
