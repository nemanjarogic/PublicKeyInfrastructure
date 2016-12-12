using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Audit : IDisposable
    {
        private static EventLog customLog = null;
        const string SourceName = "CertificationAuthority.CertificationAuthorityService";
        const string LogName = "CALogger";

        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName, Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }

        public static void WriteEvent(string eventMessage, EventLogEntryType eventType)
        {
            if (customLog != null)
            {
                EventLog.WriteEntry(SourceName, eventMessage, eventType);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event to event log."));
            }
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
