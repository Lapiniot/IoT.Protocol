using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IoT.Protocol.Upnp.Metadata
{
    public static class DebugExtensions
    {
        public static void DebugDump(this ServiceMetadata metadata, TextWriter textWriter)
        {
            if(metadata == null) throw new ArgumentNullException(nameof(metadata));
            if(textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            foreach(var action in metadata.Actions)
            {
                action.DebugDump(textWriter);
            }
        }

        public static void DebugDump(this ServiceAction action, TextWriter textWriter)
        {
            if(action == null) throw new ArgumentNullException(nameof(action));
            if(textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            var outArgs = action.Arguments.Where(a => a.Direction == "out").ToArray();
            var inArgs = action.Arguments.Where(a => a.Direction == "in").ToArray();
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
                tw.Write(arg.StateVariable.DataType.Name);
                tw.Write(" ");
                tw.Write(nameHandler(arg.Name));
            }

            tw.Write(")");
        }
    }
}