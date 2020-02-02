using System;

namespace IoT.Protocol.Upnp
{
    public class Icon
    {
        internal Icon(Uri uri, string mime, int depth, int width, int height)
        {
            Uri = uri;
            Mime = mime;
            Depth = depth;
            Width = width;
            Height = height;
        }

        public Uri Uri { get; set; }
        public string Mime { get; set; }
        public int Depth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}