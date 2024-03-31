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

        switch (logType)
        {
            case Log.INFO:
                File.WriteAllText("C:/Logs/infoLog.txt", str + Environment.NewLine);
                break;
            case Log.DEBUG:
                File.WriteAllText("C:/Logs/debugLog.txt", str + Environment.NewLine);
                break;
            case Log.NONFATAL:
                File.WriteAllText("C:/Logs/nonFatalLog.txt", str + Environment.NewLine);
                break;
            case Log.FATAL:
                File.WriteAllText("C:/Logs/fatalLog.txt", str + Environment.NewLine);
                break;
        }
    }
}