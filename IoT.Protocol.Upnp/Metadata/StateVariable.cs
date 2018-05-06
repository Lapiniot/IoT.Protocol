using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp.Metadata
{
    public class StateVariable
    {
        private static readonly IDictionary<string, Type> Map = new Dictionary<string, Type>
        {
            {"ui1", typeof(byte)},
            {"ui2", typeof(ushort)},
            {"ui4", typeof(uint)},
            {"ui8", typeof(ulong)},
            {"i1", typeof(sbyte)},
            {"i2", typeof(short)},
            {"i4", typeof(int)},
            {"i8", typeof(long)},
            {"int", typeof(int)},
            {"r4", typeof(float)},
            {"r8", typeof(double)},
            {"number", typeof(double)},
            {"fixed.14.4", typeof(double)},
            {"float", typeof(float)},
            {"char", typeof(char)},
            {"string", typeof(string)},
            {"date", typeof(DateTime)},
            {"dateTime", typeof(DateTime)},
            {"dateTime.tz", typeof(DateTime)},
            {"time", typeof(DateTime)},
            {"time.tz", typeof(DateTime)},
            {"boolean", typeof(bool)},
            {"bin.base64", typeof(string)},
            {"bin.hex", typeof(string)},
            {"uri", typeof(Uri)},
            {"uuid", typeof(string)}
        };

        internal StateVariable(string name, string dataTypeName, string defaultValue, bool sendsEvent,
            string[] allowedValues, ArgumentValueRange valueRange)
        {
            Name = name;
            DataTypeName = dataTypeName;
            DefaultValue = defaultValue;
            SendsEvent = sendsEvent;
            AllowedValues = allowedValues;
            ValueRange = valueRange;
            DataType = Map[dataTypeName];
        }

        public string Name { get; }
        public string DataTypeName { get; }
        public Type DataType { get; }
        public string DefaultValue { get; }
        public bool SendsEvent { get; }
        public string[] AllowedValues { get; }
        public ArgumentValueRange ValueRange { get; }
    }
}