using System;
using System.Collections;
using System.Resources;
using System.Windows.Forms;

internal static class LegacyResourceCompiler
{
    private static int Main(string[] args)
    {
        if (args.Length != 2) return 2;
        using (var reader = new ResXResourceReader(args[0]))
        using (var writer = new ResourceWriter(args[1]))
        {
            foreach (DictionaryEntry entry in reader)
                writer.AddResource((string)entry.Key, entry.Value);
            writer.Generate();
        }
        return 0;
    }
}
