using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    public class TerminalManager
    {
        public static bool ExecuteTerminalCommand(string workingDirectory, string command)
        {
            bool isCommandExecuted = false;

            try
            {
                using(Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.WorkingDirectory = workingDirectory;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C " + command;
                    process.StartInfo = startInfo;
                    process.Start();
                }

                isCommandExecuted = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on executing terminal command: " + ex.Message);
            }

            return isCommandExecuted;
        }
    }
}
