using System;
using System.Linq;
using UnityEngine.DedicatedServer;

internal class CommandLineArgumentsParser
{
    public int Port { get; }
    const int k_DefaultPort = 7777;

    public int TargetFramerate { get; }
    const int k_DefaultTargetFramerate = 30;

    public string ListenIp { get; }
    const string k_DefaultListenIp = "0.0.0.0";

    readonly string[] m_Args;

    /// <summary>
    /// Initializes the CommandLineArgumentsParser
    /// </summary>
    public CommandLineArgumentsParser() : this(Environment.GetCommandLineArgs()) { }
    
    /// <summary>
    /// Initializes the CommandLineArgumentsParser
    /// </summary>
    /// <param name="arguments">Arguments to process</param>
    public CommandLineArgumentsParser(string[] arguments)
    {
        m_Args = arguments ?? Array.Empty<string>();

        Port = ExtractValueInt("--port", k_DefaultPort);
        TargetFramerate = ExtractValueInt("--targetframerate", k_DefaultTargetFramerate);
        ListenIp = ExtractValue("--ip", k_DefaultListenIp);
    }

    /// <summary>
    /// Extracts a value for command line arguments provided
    /// </summary>
    /// <param name="argName"></param>
    /// <param name="defaultValue"></param>
    /// <param name="argumentAndValueAreSeparated"></param>
    /// <returns></returns>
    string ExtractValue(string argName, string defaultValue = null, bool argumentAndValueAreSeparated = true)
    {
        if (argumentAndValueAreSeparated)
        {
            if (!m_Args.Contains(argName))
            {
                return defaultValue;
            }

            var index = m_Args.ToList().FindIndex(0, a => a.Equals(argName));
            if (index >= 0 && index < m_Args.Length - 1)
                return m_Args[index + 1];
            return defaultValue;
        }

        foreach (var argument in m_Args)
        {
            if (argument.StartsWith(argName)) // I.E: "-epiclocale=it"
            {
                return argument.Substring(argName.Length + 1, argument.Length - argName.Length - 1);
            }
        }
        return defaultValue;
    }

    int ExtractValueInt(string argName, int defaultValue = -1)
    {
        var number = ExtractValue(argName, defaultValue.ToString());
        return Convert.ToInt32(number);
    }
}
