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
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.WorkingDirectory = workingDirectory; // @"C:\Windows\System32";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
                process.StartInfo = startInfo;
                process.Start();

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
