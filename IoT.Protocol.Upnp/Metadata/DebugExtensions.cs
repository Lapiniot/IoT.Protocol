namespace IoT.Protocol.Upnp.Metadata;

public static class DebugExtensions
{
    public static void DebugDump(this ServiceDescription metadata, TextWriter textWriter)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(textWriter);

        foreach(var action in metadata.Actions)
        {
            action.DebugDump(textWriter);
        }
    }

    public static void DebugDump(this ServiceAction action, TextWriter textWriter)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(textWriter);

        var outArgs = action.Arguments.Where(a => a.Direction == ArgumentDirection.Out).ToArray();
        var inArgs = action.Arguments.Where(a => a.Direction == ArgumentDirection.In).ToArray();
        if(outArgs.Length > 0)
        {
            DumpPrototype(textWriter, outArgs, n => n);
        }
        else
        {
            textWriter.Write("void");
        }

        textWriter.Write(" ");
        textWriter.Write(action.Name);
        DumpPrototype(textWriter, inArgs, n => char.ToUpperInvariant(n[0]) + n[1..]);
        textWriter.WriteLine(";");
    }

    private static void DumpPrototype(TextWriter tw, IReadOnlyList<Argument> args, Func<string, string> nameHandler)
    {
        tw.Write("(");
        for(var i = 0; i < args.Count; i++)
        {
            if(i != 0) tw.Write(", ");
            var arg = args[i];
            tw.Write(arg.RelatedStateVar);
            tw.Write(" ");
            tw.Write(nameHandler(arg.Name));
        }

        tw.Write(")");
    }
}