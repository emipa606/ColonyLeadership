using System;
using System.Collections.Generic;

namespace Nandonalt_ColonyLeadership.Config;

public sealed class ConfigManager
{
    private static List<int> electionDays = [..new[] { 0, 6, 14 }]; //Means day 1, 7 and 15 on a seasonc;
    private static bool psychicSensitivityForScientist = true;
    private static string scientistLeaderDeffName = "leader4";

    private static readonly Lazy<ConfigManager> // instance = new ConfigManager();
        lazy = new Lazy<ConfigManager>
            (() => new ConfigManager());

    private ConfigManager()
    {
    }

    public static ConfigManager instance => lazy.Value;

    public static List<int> getElectionDays()
    {
        return electionDays;
    }

    public static void setElectionsDays(List<int> newDays)
    {
        electionDays = newDays;
    }

    internal static bool isPsychicSensitivityForScientist()
    {
        return psychicSensitivityForScientist;
    }

    internal static void setPsychicSensitivityForScientist(bool isSensitive)
    {
        psychicSensitivityForScientist = isSensitive;
        scientistLeaderDeffName = isSensitive ? "leader4" : "leader5";
    }

    internal static string getScientistLeaderDeffName()
    {
        return scientistLeaderDeffName;
    }
}