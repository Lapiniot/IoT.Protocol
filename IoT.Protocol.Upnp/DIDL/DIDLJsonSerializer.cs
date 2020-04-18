using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IoT.Protocol.Upnp.DIDL
{
    public static class DIDLJsonSerializer
    {
        public static void Serialize(Utf8JsonWriter writer, int total, IEnumerable<Item> items, IEnumerable<Item> parents,
            bool emitResourceData, bool emitVendorData)
        {
            if(writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStartObject();
            writer.WriteNumber("total", total);

            if(items != null)
            {
                writer.WriteStartArray("result");

                foreach(var item in items)
                {
                    writer.WriteStartObject();

                    writer.WriteString("id", item.Id);
                    writer.WriteBoolean("container", item is Container);
                    writer.WriteString("class", item.Class);
                    writer.WriteString("title", item.Title);

                    if(item is MediaItem mi) EmitMediaItem(writer, mi);

                    if(item.AlbumArts != null) EmitAlbumArts(writer, item);

                    if(emitResourceData && item.Resource is { } r) EmitResource(writer, r);

                    if(emitVendorData && item.Vendor is { } vendor && vendor.Count > 0) EmitVendorData(writer, vendor);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            if(parents != null) EmitParents(writer, parents);

            writer.WriteEndObject();

            writer.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitParents(Utf8JsonWriter writer, IEnumerable<Item> parents)
        {
            writer.WriteStartArray("parents");
            foreach(var parent in parents)
            {
                writer.WriteStartObject();
                writer.WriteString("id", parent.Id);
                writer.WriteString("parentId", parent.ParentId);
                writer.WriteString("title", parent.Title);
                if(parent.Resource is { Url: { } url }) writer.WriteString("url", url);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitVendorData(Utf8JsonWriter w, IDictionary<string, string> vendor)
        {
            w.WriteStartObject("vendor");
            foreach(var (k, v) in vendor)
            {
                if(!string.IsNullOrEmpty(v)) w.WriteString(k, v);
            }

            w.WriteEndObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitResource(Utf8JsonWriter writer, Resource res)
        {
            writer.WriteStartObject("res");
            writer.WriteString("url", res.Url);
            writer.WriteString("proto", res.Protocol);

            if(res.Attributes is { Count: var count } && count > 0)
            {
                foreach(var (k, v) in res.Attributes) writer.WriteString(k, v);
            }

            writer.WriteEndObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitAlbumArts(Utf8JsonWriter writer, Item item)
        {
            writer.WriteStartArray("albumArts");
            foreach(var art in item.AlbumArts) writer.WriteStringValue(art);
            writer.WriteEndArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EmitMediaItem(Utf8JsonWriter writer, MediaItem item)
        {
            if(item.Creator != null) writer.WriteString("creator", item.Creator);
            if(item.Album != null) writer.WriteString("album", item.Album);
            if(item.Date != null) writer.WriteString("date", item.Date.ToString());
            if(item.Genre != null) writer.WriteString("genre", item.Genre);
            if(item.Description != null) writer.WriteString("description", item.Description);
            if(item.TrackNumber != null) writer.WriteNumber("track", item.TrackNumber.Value);

            if(item.Artists?.Count > 0)
            {
                writer.WriteStartArray("artists");
                foreach(var artist in item.Artists) writer.WriteStringValue(artist);
                writer.WriteEndArray();
            }

            if(item.Authors?.Count > 0)
            {
                writer.WriteStartArray("authors");
                foreach(var artist in item.Authors) writer.WriteStringValue(artist);
                writer.WriteEndArray();
            }

            if(item.Genres?.Count > 0)
            {
                writer.WriteStartArray("genres");
                foreach(var artist in item.Genres) writer.WriteStringValue(artist);
                writer.WriteEndArray();
            }
        }
    }
}