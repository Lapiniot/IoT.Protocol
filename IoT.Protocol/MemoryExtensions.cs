using System;
using System.Runtime.CompilerServices;

namespace IoT.Protocol
{
    public static class MemoryExtensions
    {
        private const byte CR = 0x0d;
        private const byte LF = 0x0a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindEolMarker(this Memory<byte> memory, out int index)
        {
            return TryFindEolMarker(memory.Span, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindEolMarker(this Span<byte> span, out int index)
        {
            return (index = span.IndexOf(CR)) > 0 && index < span.Length - 1 && span[index + 1] == LF;
        }
    }
}