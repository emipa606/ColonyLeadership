using System;
using System.IO;

namespace Nandonalt_ColonyLeadership.Util;

internal static class Logger
{
    public static void log(string str, Log logType = Log.INFO)
    {
        if (!Directory.Exists("C:/Logs/") || !ColonyLeadership.useLogging)
        {
            return;
        }

        if (logType == Log.INFO)
        {
            File.WriteAllText("C:/Logs/infoLog.txt", str + Environment.NewLine);
        }

        else if (logType == Log.DEBUG)
        {
            File.WriteAllText("C:/Logs/debugLog.txt", str + Environment.NewLine);
        }
        else if (logType == Log.NONFATAL)
        {
            File.WriteAllText("C:/Logs/nonFatalLog.txt", str + Environment.NewLine);
        }
        else if (logType == Log.FATAL)
        {
            File.WriteAllText("C:/Logs/fatalLog.txt", str + Environment.NewLine);
        }
    }
}