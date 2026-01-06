using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EZGO.CLI.MigrationService.Data
{
    ///TODO ADD CHECK ON WHICH SCRIPT NEEDS TO BE UPDATED
   
    /// <summary>
    /// ScriptReader; script reader; reading script files for execution on database.
    /// </summary>
    public class ScriptReader
    {
        /// <summary>
        /// ScriptReader; Constructor;
        /// </summary>
        public ScriptReader()
        {

        }

        /// <summary>
        /// GetFullScript; Get full scripts based on location for execution.
        /// </summary>
        /// <param name="scriptlocation">Script location for full script.</param>
        /// <returns>Creation script for execution against a new empty database.</returns>
        public string GetSingleScript(string scriptlocation)
        {
            if (File.Exists(scriptlocation)) {
                return File.ReadAllText(scriptlocation);
            } else {
                return string.Empty;  // no script so, return nothing empty.
            }
        }

        /// <summary>
        /// GetUpdateScripts; Get update scripts based on a location where the update scripts are located.
        /// </summary>
        /// <param name="directoryscriptlocation">Location to get the scripts.</param>
        /// <returns>A list of scripts (string) that need to be executed.</returns>
        public List<ScriptFile> GetScripts(string directoryscriptlocation)
        {
            var scripts = new List<ScriptFile>();
            if (Directory.Exists(directoryscriptlocation))
            {
                foreach(string directory in Directory.GetDirectories(directoryscriptlocation))
                {
                    foreach(string filelocation in Directory.GetFiles(directory))
                    {
                        var scriptFile = new ScriptFile { Script = File.ReadAllText(filelocation), 
                                                          Filename = filelocation, 
                                                          Version = (new DirectoryInfo(directory)).Name };
                        scripts.Add(scriptFile);
                    }
                }
           
            }

            return scripts;
        }

        /// <summary>
        /// Helper structure for containing file information, needed when executing scripts for logging and validation purposes. 
        /// </summary>
        public struct ScriptFile
        {
            public string Version;
            public string Script;
            public string Filename;
        }
    }
}
