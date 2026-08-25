using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CreationMaster;

internal static class Fc26ActivityLog
{
    private static readonly object Gate = new object();
    private static readonly List<Entry> Entries = new List<Entry>();

    internal static void Add(string action, string description)
    {
        lock (Gate)
        {
            Entries.Add(new Entry(DateTime.Now, action ?? string.Empty, description ?? string.Empty));
            if (Entries.Count > 1000) Entries.RemoveRange(0, Entries.Count - 1000);
        }
    }

    internal static Entry[] Snapshot()
    {
        lock (Gate) return Entries.ToArray();
    }

    internal static void Export(string fileName)
    {
        var lines = Snapshot().Select(entry => entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + entry.Action + "\t" + entry.Description);
        File.WriteAllLines(fileName, new[] { "timestamp\taction\tdescription" }.Concat(lines), new UTF8Encoding(true));
    }

    internal sealed class Entry
    {
        internal Entry(DateTime timestamp, string action, string description)
        {
            Timestamp = timestamp; Action = action; Description = description;
        }
        internal DateTime Timestamp { get; }
        internal string Action { get; }
        internal string Description { get; }
        public override string ToString() => Timestamp.ToString("HH:mm:ss") + "  " + Action + " — " + Description;
    }
}
