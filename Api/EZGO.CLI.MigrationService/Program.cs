using EZGO.CLI.MigrationService.Data;
using System;
using System.Threading.Tasks;

namespace EZGO.CLI.MigrationService
{
    //TODO: Add check for version of migration to run
    //TODO: Add check in DB scripts for checking if data is not already done (e.g. there are already tables)
    //TODO: Add check to all migration script (in script) with ' IF NOT EXISTS ' structure when creating objects (except for functions/SPS)
    //NOTE: STRUCTURE AND WORKINGS STILL BEING DETERMINED CAN CHANGE!

    /// <summary>
    /// Main migration tool, CLI tool wrapped in a docker instance. This tool is a one time run tool when a roll-out is done for the API or a new environment is being installed.
    /// 
    /// Main flow will be as followed:
    /// - A Init is done filling the correct variables based on the logic and/or incoming environmental variables.
    /// - A check is done if it is a new environment, if so run the full db script.
    /// - A check is done if migrations must be run (default true), if so the updates are done based on the migration scripts.
    /// - Clean up is done, CLI is existed.
    /// 
    /// The migration scripts are based on standard psql scripting but have a few extra rules. 
    /// - Every script must be located in a seperate directory
    /// - Every create table, sequence or other object that are not functions or stored procedures must have a IF NOT EXISTS, to make sure that no double items are created or data loss can occur if something goes wrong.
    /// - Every migration script must not use drop tables for the same reason the IF NOT EXISTS structure must be used unless specifically needed.
    /// 
    /// -----------------------------------------------------------------------------------------------------------------------
    /// Example migration scripts to be used:
    /// -----------------------------------------------------------------------------------------------------------------------
    ///
    ///    
    ///
    ///     -- ----------------------------
    ///     -- Sequence structure for something something
    ///     -- ----------------------------
    ///     CREATE SEQUENCE IF NOT EXISTS "public"."something_id_seq" 
    ///     INCREMENT 1
    ///     MINVALUE  1
    ///     MAXVALUE 9223372036854775807
    ///     START 1
    ///     CACHE 1;
    /// 
    ///     -- ----------------------------
    ///     -- Table structure for something something
    ///     -- ----------------------------
    ///     CREATE TABLE IF EXISTS NOT "public"."something" (
    ///         "id" int4 NOT NULL DEFAULT nextval('something_id_seq'::regclass),
    ///         "version" varchar(40) COLLATE "pg_catalog"."default",
    ///         "filename" varchar(1024) COLLATE "pg_catalog"."default",
    ///         "started_on" timestamptz(6) NOT NULL,
    ///         "ended_on" timestamptz(6) NOT NULL
    ///     )
    ///     ;
    /// 
    ///     
    ///
    /// </summary>
    class Program
    {
        private static ScriptReader _scriptReader; //script reader reads psql scripts from certain dir and returns executable strings for the database access helper.
        private static ScriptExecuter _scriptExecuter; //creates a executable script based on a update script.
        private static bool _databaseAlreadyPopulated; //full install true/false; if true a full creation script of db will be executed.
        private static bool _migrationTablesAlreadyPopulated; //check if migration tables are already available if not they will be created
       // private static string _connectionString = ""; //connection string used; Must be a full connection string, will be filled by environmental variable DEFAULTCONNECTION (see init)
        public const string FULL_SCRIPT_LOCATION = "migration_scripts/full/full_db.psql"; //fill script installation location + file
        public const string MIGRATION_TABLES_LOCATION = "migration_scripts/logging/migrations_logging.psql";
        public const string MIGRATION_SCRIPT_LOCATION = "migration_scripts/updates"; //update scripts directory location.

        /// <summary>
        /// Main; Main execution of cli.
        /// </summary>
        /// <param name="args">Argumentents; Currently not used.</param>
        /// <returns>Normal cli output</returns>
        static async Task Main(string[] args)
        {
            Program.Init();

            
            Program._databaseAlreadyPopulated = (bool)await _scriptExecuter.ExecuteScalarScript(script: _scriptExecuter.ScriptCheckFullRollout);
            //check if migration tables are available for logging:
            Program._migrationTablesAlreadyPopulated = (bool)await _scriptExecuter.ExecuteScalarScript(script: _scriptExecuter.ScriptCheckMigrationTablesAvailable);
            

            if (!_migrationTablesAlreadyPopulated)
            {
                string migrationTableScript = _scriptReader.GetSingleScript(MIGRATION_TABLES_LOCATION);
                if(!string.IsNullOrEmpty(migrationTableScript))
                {
                    await _scriptExecuter.ExecuteNonQueryScript(script:migrationTableScript);
                }
            }

            if (!Program._databaseAlreadyPopulated)
            {
               
                string fullScript = _scriptReader.GetSingleScript(FULL_SCRIPT_LOCATION);
                if (!string.IsNullOrEmpty(fullScript))
                {
                    await _scriptExecuter.ExecuteNonQueryScript(script: fullScript);
                }
            }

            foreach(var updateScript in _scriptReader.GetScripts(MIGRATION_SCRIPT_LOCATION))
            {
                if(!string.IsNullOrEmpty(updateScript.Filename) && !string.IsNullOrEmpty(updateScript.Version))
                {
                    try
                    {
                        await _scriptExecuter.WrapScriptAsync(script: updateScript.Script, version: updateScript.Version, fileName: updateScript.Filename);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                        Environment.ExitCode = 0xB;
                        return; //exit for
                    }

                }
            }

            await Program.Closing();

            Environment.Exit(Environment.ExitCode); //exit code not working for some reason. 

        }

        /// <summary>
        /// Init; Initialize variables and initiate helpers for use within the migration service.
        /// </summary>
        /// <returns>Nothing</returns>
        public static void Init()
        {
            Program._scriptReader = new ScriptReader();
            Program._scriptExecuter = new ScriptExecuter();
        }

        /// <summary>
        /// Closing(); Closing the service, clean up all variables and post logging if needed.
        /// </summary>
        /// <returns>Nothing</returns>
        public static async Task Closing()
        {
            await _scriptExecuter.DisposeAsync();
        }
    }
}
