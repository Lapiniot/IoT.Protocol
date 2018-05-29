using System;
using System.Runtime.CompilerServices;

namespace IoT.Protocol
{
    public static class MemoryExtensions
    {
        private const byte CR = 0x0d;
        private const byte LF = 0x0a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfEOL(this Span<byte> span)
        {
            var index = 0;
            return (index = span.IndexOf(CR)) > 0 && index < span.Length - 1 && span[index + 1] == LF ? index : -1;
        }
    }
}