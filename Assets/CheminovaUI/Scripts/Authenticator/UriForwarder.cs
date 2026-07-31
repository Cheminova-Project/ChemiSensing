using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;

public static class UriForwarder
{
    public static void SendToExistingInstance(string[] args)
    {
        List<string> argsList = args.ToList();
        argsList.RemoveAt(0); // Remove application name from args list
        using (var client = new NamedPipeClientStream("ChemiAnalysisPipe"))
        {
            client.Connect(200);
            using (var writer = new StreamWriter(client))
            {
                writer.WriteLine(string.Join(" ", argsList.ToArray()));
            }
        }
    }
}