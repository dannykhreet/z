using DocumentFormat.OpenXml.Drawing.Diagrams;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.WorkerService.Provisioner.Utils
{
    public class SftpConnector
    {
        private string _SFTPLocation = "";
        private string _SFTPUser = "";
        private string _SFTPPassword = "";
        public SftpConnector(string SFTPLocation, string SFTPUser, string SFTPPassword)
        {
            _SFTPLocation = SFTPLocation;
            _SFTPUser = SFTPUser; 
            _SFTPPassword = SFTPPassword;
        }

        public bool HasConfiguration()
        {
            return (!string.IsNullOrEmpty(_SFTPLocation) && !string.IsNullOrEmpty(_SFTPUser) && !string.IsNullOrEmpty(_SFTPPassword));
        }

        /// <summary>
        /// GetFileDynamic; Get latest file in a specific location, due to dynamic naming.
        /// If possbile file name is supplied only files are checked starting with that filename (extension is ignored).
        /// </summary>
        /// <param name="fileContent">string, content of file or forced error message, if return is false this can be used for logging.</param>
        /// <returns>true/false depending on outcome.</returns>
        public bool GetFileDynamic(string possibleFileName, out string fileContent)
        {
            var output = false;
            string content = string.Empty;
            if (this.HasConfiguration())
            {
                try
                {
                    //TODO refactor, move to seperate method
                    var port = 22; //22 default for sftp
                    var filelocation = ""; //default file location
                    var host = ""; //host, will be based on sftplocation (contains host, port, file location)

                    if (_SFTPLocation.Contains("/"))
                    {
                        //containst a path for file (e.g. filelocation), therefor split in host and filelocation
                        host = _SFTPLocation.Split("/")[0];
                        filelocation = _SFTPLocation.Split("/")[1];
                    }
                    else
                    {
                        host = _SFTPLocation;
                    }

                    if (host.Contains(":"))
                    {
                        //host contains host address and port, therefor split in host and port.
                        port = Convert.ToInt32(host.Split(":")[1]);
                        host = _SFTPLocation.Split(":")[0];
                    }

                    using (var client = new SftpClient(host, port, _SFTPUser, _SFTPPassword))
                    {
                        client.Connect();

                        if (client.IsConnected)
                        {
                         
                            try
                            {
                                var possibleItems = client.ListDirectory(filelocation);
                                if(!string.IsNullOrEmpty(possibleFileName)) {
                                    //filterOut only specific file name
                                    possibleItems = possibleItems.Where(x => x.Name.StartsWith(possibleFileName));
                                }
                                var sorted = possibleItems.OrderByDescending(x => x.LastWriteTime);
                                if (possibleItems.Count() > 0)
                                {
                                    var possibleFile = sorted.FirstOrDefault();
                                    if(possibleFile != null)
                                    {
                                        var fileName = possibleFile.FullName;
                                        content = client.ReadAllText(fileName);
                                        if (!string.IsNullOrEmpty(content))
                                        {
                                            output = true;
                                        } else
                                        {
                                            output = false;
                                            content = string.Format("Error: no content @ {0}", fileName);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                content = string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation);
                            }
                        }

                        client.Disconnect();
                    }


                }
                catch (Exception ex)
                {
                    content = string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation);
                }
            }
            fileContent = content;
            return output;
        }
        

        public bool GetFileAsString(string fileName, out string fileContent)
        {
            var output = false;
            string content = string.Empty;
            if(this.HasConfiguration())
            {
                try
                {
                    //TODO refactor, move to seperate method
                    var port = 22; //22 default for sftp
                    var filelocation = ""; //default file location
                    var host = ""; //host, will be based on sftplocation (contains host, port, file location)

                    if (_SFTPLocation.Contains("/"))
                    {
                        //containst a path for file (e.g. filelocation), therefor split in host and filelocation
                        host = _SFTPLocation.Split("/")[0];
                        filelocation = _SFTPLocation.Split("/")[1];
                    }
                    else
                    {
                        host = _SFTPLocation;
                    }

                    if (host.Contains(":"))
                    {
                        //host contains host address and port, therefor split in host and port.
                        port = Convert.ToInt32(host.Split(":")[1]);
                        host = _SFTPLocation.Split(":")[0];
                    }

                    using (var client = new SftpClient(host, port, _SFTPUser, _SFTPPassword))
                    {
                        client.Connect();

                        if (client.IsConnected)
                        {
                          
                            try
                            {
                                content = client.ReadAllText(string.IsNullOrEmpty(filelocation) ? fileName : string.Concat(filelocation, "/", fileName));
                                if(!string.IsNullOrEmpty(content))
                                {
                                    output = true;
                                } 

                            }
                            catch (Exception ex)
                            {
                                content = string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation);
                            }
                        }

                        client.Disconnect();
                    }


                }
                catch (Exception ex)
                {
                    content = string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation);
                }
            }
            fileContent = content;
            return output;
        }

        public string SendFile(Stream file, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            var uploadSuccessed = false;

            if(this.HasConfiguration())
            {
                try
                {
                    var port = 22; //22 default for sftp
                    var filelocation = ""; //default file location
                    var host = ""; //host, will be based on sftplocation (contains host, port, file location)

                    if (_SFTPLocation.Contains("/"))
                    {
                        //containst a path for file (e.g. filelocation), therefor split in host and filelocation
                        host = _SFTPLocation.Split("/")[0];
                        filelocation = _SFTPLocation.Split("/")[1];
                    }
                    else
                    {
                        host = _SFTPLocation;
                    }

                    if (host.Contains(":"))
                    {
                        //host contains host address and port, therefor split in host and port.
                        port = Convert.ToInt32(host.Split(":")[1]);
                        host = _SFTPLocation.Split(":")[0];
                    }

                    sb.AppendLine(string.Format("host: {0}, port {1}, location {2}", host, port, filelocation));

                    using (var client = new SftpClient(host, port, _SFTPUser, _SFTPPassword))
                    {
                        client.Connect();

                        if (client.IsConnected)
                        {
                            sb.AppendLine(string.Concat("Provisioner connected with host: ", host));

                            try
                            {
                                client.UploadFile(file, string.IsNullOrEmpty(filelocation) ? fileName : string.Concat(filelocation, "/", fileName));

                                uploadSuccessed = true;
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine(string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation));
                            }
                        }

                        client.Disconnect();
                    }

                    if (uploadSuccessed) { sb.AppendLine("Upload succeeded."); }
                    else { sb.AppendLine("Upload FAILED."); }

                } catch(Exception ex)
                {
                    sb.AppendLine(string.Format("Error: {0} @ {1}", ex.Message, _SFTPLocation));
                }
                

            } else 
            {
                sb.AppendLine("No configuration");
            }

            return sb.ToString();
        } 
      

    }
}
