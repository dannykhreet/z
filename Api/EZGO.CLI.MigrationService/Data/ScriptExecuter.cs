using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.CLI.MigrationService.Data
{
    /// <summary>
    /// ScriptExecuter; Functionality to executed scripts including functionality to check if already executed
    /// On init a basic scripts are created and with the WrapScript functionality a incoming script can be executed based on the executable script. 
    /// 
    /// NOTE! on executing the migration return status codes are returned. These can be used for handling certain messages. 
    /// 
    /// - 0 : Not executing due to already exists
    /// - 1 : Success, migrations are executed
    /// 
    /// </summary>
    public class ScriptExecuter : IAsyncDisposable
    {
        private static DatabaseAccessHelper _databaseAccessHelper; //database access helper, connection to database, execution of queries.

        /// <summary>
        /// 
        /// </summary>
       // public string ScriptRollout { get; set; }
        /// <summary>
        /// 
        /// </summary>
       // public string ScriptRolloutExecute { get; set; }
        /// <summary>
        /// 
        /// </summary>
       // public string ScriptRolloutCleanup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScriptCheckFullRollout { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScriptCheckMigrationTablesAvailable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScriptCheckMigration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScriptLoggingMigrationStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ScriptLoggingMigrationEnd { get; set; }
        /// <summary>
        /// N/A
        /// </summary>
        public string LoggingStart { get; set; }
        /// <summary>
        /// N/A
        /// </summary>
        public string LoggingEnd { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ScriptExecuter() {
            Init();
        }

        /// <summary>
        /// Init; Init script variables.
        /// </summary>
        public void Init()
        {
            this.SetScriptCheckFullRollout();
            this.SetScriptCheckMigrationTablesAvailable();
            this.SetScriptCheckMigration();
            this.SetScriptLoggingMigrationStart();
            this.SetScriptLoggingMigrationEnd();

            _databaseAccessHelper = new DatabaseAccessHelper(Environment.GetEnvironmentVariable("DEFAULTCONNECTION"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<int> WrapScriptAsync(string script, string version, string fileName)
        {
            if(string.IsNullOrEmpty(version))
            {
                return -1; //no version, can not wrap script in checks cuz of no version. 
            }

            bool migrationAlreadyDone = (bool)await this.ExecuteScalarScript(script: this.ScriptCheckMigration, version: version);

            if(!migrationAlreadyDone)
            {

                int loggingId = (int)await _databaseAccessHelper.ExecuteScalarAsync(procedureNameOrQuery: this.ScriptLoggingMigrationStart, GetParameters(version:version, fileName:fileName));

                await _databaseAccessHelper.ExecuteScalarAsync(procedureNameOrQuery: script);

                await _databaseAccessHelper.ExecuteNonQueryAsync(procedureNameOrQuery: this.ScriptLoggingMigrationEnd, GetParameters(version: version, fileName: fileName, loggingId: loggingId));
            } else
            {
                return 10;
            }

            return -1;
            //return string.Format(this.ScriptRollout, incomingScript, version);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryScript(string script, string version = null, string fileName = null)
        {
            List<NpgsqlParameter> parameters = GetParameters(version: version, fileName: fileName);
            var output = await _databaseAccessHelper.ExecuteNonQueryAsync(procedureNameOrQuery:script, parameters: parameters);
            return output;
            //return string.Format(this.ScriptRolloutExecute, version, filename);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Object> ExecuteScalarScript(string script, string version = null, string fileName = null)
        {
            List<NpgsqlParameter> parameters = GetParameters(version: version, fileName: fileName);
            var output = await _databaseAccessHelper.ExecuteScalarAsync(procedureNameOrQuery: script, parameters: parameters);
            return output;
            //return string.Format(this.ScriptRolloutExecute, version, filename);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        /// <param name="loggingId"></param>
        /// <returns></returns>
        private List<NpgsqlParameter> GetParameters(string version = null, string fileName = null, int loggingId = 0)
        {
            List<NpgsqlParameter> parameters = null;
            if (!string.IsNullOrEmpty(version) || !string.IsNullOrEmpty(fileName) || loggingId != 0)
            {
                parameters = new List<NpgsqlParameter>();
                if (!string.IsNullOrEmpty(version)) parameters.Add(new NpgsqlParameter("@_version", version));
                if (!string.IsNullOrEmpty(fileName)) parameters.Add(new NpgsqlParameter("@_filename", fileName));
                if (loggingId > 0) parameters.Add(new NpgsqlParameter("@_loggingid", loggingId));
            }
            return parameters;
        }

        /// <summary>
        /// SetScriptRollout; Set ScriptRollout field with script to be used.
        /// </summary>
        //private void SetScriptRollout()
        //{
        //    var sbString = new StringBuilder();

        //    sbString.AppendLine("");

        //    sbString.AppendLine("CREATE OR REPLACE FUNCTION \"public\".\"rollout_migrations\"(\"_version\" varchar, \"_filename\" varchar)");
        //    sbString.AppendLine("  RETURNS \"pg_catalog\".\"int4\" AS $BODY$");
        //    sbString.AppendLine("DECLARE ");
        //    sbString.AppendLine("  _loggingid int4; ");
        //    sbString.AppendLine("  _statusid int4; ");
        //    sbString.AppendLine("BEGIN");

        //    sbString.AppendLine("_statusid = 0;");

        //    sbString.AppendLine("IF (SELECT (COUNT(*) = 0)::bool FROM logging_migration WHERE version = _version) THEN");


        //    sbString.AppendLine("   INSERT INTO logging_migration(\"id\",\"version\", \"filename\", \"started_on\", \"ended_on\", \"completed\") values (DEFAULT, _version, _filename, NOW(), NULL, false)");
        //    sbString.AppendLine("   RETURNING id INTO _loggingid;");

        //    sbString.AppendLine("   {0}");

        //    sbString.AppendLine("   UPDATE logging_migration SET ended_on = NOW(), completed = true WHERE version = _version AND filename = _filename AND completed = false AND id = _loggingid;");

        //    sbString.AppendLine("   _statusid = 1;");

        //    sbString.AppendLine("END IF;");

        //    sbString.AppendLine("   RETURN _statusid;");

        //    sbString.AppendLine("END;");
        //    sbString.AppendLine("$BODY$");
        //    sbString.AppendLine("  LANGUAGE plpgsql VOLATILE");
        //    sbString.AppendLine("  COST 100;");

        //    this.ScriptRollout = sbString.ToString();

        //    sbString.Clear();
        //    sbString.Length = 0;
        //    sbString = null;
        //}

        /// <summary>
        /// SetScriptExecute; Set ScriptRolloutExecute field with script to be used.
        /// </summary>
        //private void SetScriptExecute()
        //{
        //    this.ScriptRolloutExecute = "SELECT rollout_migrations('{0}','{1}');";
        //}

        ///// <summary>
        ///// SetScriptRolloutCleanup; Set ScriptRolloutCleanup field with script to be used.
        ///// </summary>
        //private void SetScriptRolloutCleanup()
        //{
        //    this.ScriptRolloutCleanup = "DROP FUNCTION IF EXISTS \"public\".\"rollout_migrations\"(\"_version\" varchar, \"_filename\" varchar);";
        //}

        /// <summary>
        /// SetScriptCheckFullRollout; Set ScriptCheckFullRollout field with script to execute
        /// </summary>
        private void SetScriptCheckFullRollout()
        {
            this.ScriptCheckFullRollout = "SELECT EXISTS (SELECT * FROM pg_tables WHERE schemaname = 'public');";
        }

        /// <summary>
        /// ScriptCheckMigrationTablesAvailable; Set ScriptCheckMigrationTablesAvailable field with script to be used
        /// </summary>
        private void SetScriptCheckMigrationTablesAvailable()
        {
            this.ScriptCheckMigrationTablesAvailable = "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'logging_migration');";
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetScriptCheckMigration()
        {
            this.ScriptCheckMigration = "SELECT (COUNT(*) <> 0)::bool FROM logging_migration WHERE version = @_version";
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetScriptLoggingMigrationStart()
        {
            this.ScriptLoggingMigrationStart = "INSERT INTO logging_migration(\"id\",\"version\", \"filename\", \"started_on\", \"ended_on\", \"completed\") values (DEFAULT, @_version, @_filename, NOW(), NULL, false) RETURNING id;";
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetScriptLoggingMigrationEnd()
        {
            this.ScriptLoggingMigrationEnd = "UPDATE logging_migration SET ended_on = NOW(), completed = true WHERE version = @_version AND filename = @_filename AND completed = false AND id = @_loggingid;";
        }

        #region - IDisposable implementation -
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {

            await DisposeAsyncCore();

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (_databaseAccessHelper != null) _databaseAccessHelper.Dispose();
            }

            _databaseAccessHelper = null;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_databaseAccessHelper != null) { await _databaseAccessHelper.DisposeAsync(); }
        }
        #endregion
    }
}
