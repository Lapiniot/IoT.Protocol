namespace IoT.Protocol.Upnp.Metadata
{
    public class ArgumentValueRange
    {
        public string Minimum { get; }
        public string Maximum { get; }
        public string Step { get; }
        public ArgumentValueRange(string minimum, string maximum, string step)
        {
            Maximum = maximum;
            Minimum = minimum;
            Step = step;
        }
    }
}