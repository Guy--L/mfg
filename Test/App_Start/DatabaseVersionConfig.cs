using System.Web;
using System.Web.Mvc;

namespace Test
{
    public class DatabaseVersionConfig
    {
        public static void UpdateDBVersion()
        {
            DatabaseUpdate databaseUpdate = new DatabaseUpdate();
            const string connectionStringName = "Test";
            const string snapshotFolder = "Snapshots";
            databaseUpdate.UpgradeDatabase(connectionStringName, snapshotFolder);
        }
    }
}
