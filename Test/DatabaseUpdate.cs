using System.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Test
{
    // TODO: Remove Console.WriteLine when moving to a non-Console application.
    public class DatabaseUpdate
    {
        public void UpgradeDatabase(string connectionStringName, string snapshotFolder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            //  Example:  connectionString = "Data Source=.;initial catalog=MyDatabase;Integrated Security=True;";

            string targetDatabaseName = GetDatabaseNameFromConnectionString(connectionString);
            //  Example:  targetDatabaseName = "MyDatabase";

            // Assumption:  Database updates will be done with DACPAC's put in the snapshotFolder, where the DACPAC with the "last-ordered" name is the latest to apply. 
            string dacpacFileName = Directory.GetFiles(snapshotFolder, "*.dacpac").OrderByDescending(f => f).FirstOrDefault();
            //  Example:  dacpacFileName = @"Snapshots\Database_20130805_04-37-56.dacpac";
            if (!File.Exists(dacpacFileName))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("DACPAC file does not exist: " + dacpacFileName));
                return;
            }

            // Do work.
            UpgradeDatabaseWithDacpac(connectionString, targetDatabaseName, dacpacFileName);
        }
        public void UpgradeDatabaseWithDacpac(string connectionString, string databaseName, string dacpacFileName)
        {
            // Note: In order to compare versions, the database must be registered as a data-tier application.
            // Note: To set the DACPAC version, go to your Database-Project Properites -> Project Settings -> Properties... button -> Version

            Version databaseVersion = GetCurrentDacVersionFromDatabase(connectionString);
            DacPackage dacPackage = DacPackage.Load(dacpacFileName);
            if (dacPackage.Version > databaseVersion)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Upgrading from database version " + databaseVersion.ToString() + " to " + dacPackage.Version + "."));

                DacServices dacServices = new DacServices(connectionString);
                dacServices.Message += new EventHandler<DacMessageEventArgs>(DacServices_Message);
                //dacServices.ProgressChanged += new EventHandler<DacProgressEventArgs>(DacServices_ProgressChanged);  // Note: ProgressChanged does not have useful messages, so it is commented out.

                // Change any dacDeployOptions here. 
                DacDeployOptions dacDeployOptions = new DacDeployOptions();
                dacDeployOptions.RegisterDataTierApplication = true;

                dacServices.Deploy(dacPackage, databaseName, true, dacDeployOptions);
            }
            else
            {
                if (dacPackage.Version == databaseVersion)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Current database version (" + databaseVersion.ToString() + ") matches latest DACPAC version in " + dacpacFileName + ".  DACPAC not deployed."));
                }
                else
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("WARNING!  Current database version (" + databaseVersion.ToString() + ") is greater than latest DACPAC version (" + dacPackage.Version.ToString() + ") in " + dacpacFileName + ".  DACPAC not deployed."));
                }
            }           
        }

        private void DacServices_Message(object sender, DacMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        private void DacServices_ProgressChanged(object sender, DacProgressEventArgs e)
        {
            Console.WriteLine(e.Status + " " + e.Message);
        }

        private string GetDatabaseNameFromConnectionString(string connectionString)
        {
            string returnValue = string.Empty;
            const string initialCatalog = "initial catalog=";
            int initialCatalogLocation = connectionString.ToLower().IndexOf(initialCatalog, StringComparison.Ordinal);
            if (initialCatalogLocation > 0)
            {
                int startLocation = initialCatalogLocation + initialCatalog.Length;
                int endLocation = connectionString.IndexOf(";", startLocation, StringComparison.Ordinal);
                if (endLocation == -1)
                    endLocation = connectionString.Length;
                returnValue = connectionString.Substring(startLocation, endLocation - startLocation);
            }
            return returnValue;
        }
        private Version GetCurrentDacVersionFromDatabase(string connectionString)
        {
            Version returnValue = new Version();
            const string sqlStatement = "SELECT TOP 1 type_version FROM msdb.dbo.sysdac_instances_internal WHERE instance_name = db_name()";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string stringVersion = reader[0].ToString();
                                Version.TryParse(stringVersion, out returnValue);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {     
                // Suppress any errors in case the database does not exist.
                //throw;
            }
            return returnValue;
        }
    }
}
