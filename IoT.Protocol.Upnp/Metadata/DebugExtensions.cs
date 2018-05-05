using System;
using System.IO;
using System.Linq;

namespace IoT.Protocol.Upnp.Metadata
{
    public static class DebugExtensions
    {
        public static void DebugDump(this ServiceMetadata metadata, TextWriter textWriter)
        {
            foreach(var action in metadata.Actions)
            {
                action.DebugDump(textWriter);
            }
        }

        public static void DebugDump(this ServiceAction action, TextWriter textWriter)
        {
            var outArgs = action.Arguments.Where(a => a.Direction == "out").ToArray();
            var inArgs = action.Arguments.Where(a => a.Direction == "in").ToArray();
            if(outArgs.Length > 0)
                DumpPrototype(textWriter, outArgs, n => n);
            else
                textWriter.Write("void");
            textWriter.Write(" ");
            textWriter.Write(action.Name);
            DumpPrototype(textWriter, inArgs, n => n.Substring(0, 1).ToLower() + n.Substring(1));
            textWriter.WriteLine(";");
        }

        private static void DumpPrototype(TextWriter tw, Argument[] args, Func<string, string> nameHandler)
        {
            tw.Write("(");
            for(int i = 0; i < args.Length; i++)
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