using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Upnp.Services
{
    public sealed class AVTransportService : SoapActionInvoker
    {
        public AVTransportService(SoapControlEndpoint endpoint, string deviceId) : 
            base(endpoint, new Uri($"{deviceId}-MR/upnp.org-AVTransport-1/control", UriKind.Relative), UpnpServices.AVTransport)
        {
        }

        public AVTransportService(SoapControlEndpoint endpoint, Uri controlUri) :
            base(endpoint, controlUri, UpnpServices.AVTransport)
        {
        }

        public Task<IDictionary<string, string>> GetMediaInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            //UInt32 NrTracks, 
            //String MediaDuration, 
            //String CurrentURI, 
            //String CurrentURIMetaData, 
            //String NextURI, 
            //String NextURIMetaData, 
            //String PlayMedium, 
            //String RecordMedium, 
            //String WriteStatus
            return InvokeAsync("GetMediaInfo", cancellationToken, ("InstanceID", instanceId));
        }

        //void SetRecordQualityMode(UInt32 instanceID, String newRecordQualityMode);

        public Task<IDictionary<string, string>> GetPositionInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            //UInt32 Track, 
            //String TrackDuration, 
            //String TrackMetaData, 
            //String TrackURI, 
            //String RelTime, 
            //String AbsTime, 
            //Int32 RelCount, 
            //Int32 AbsCount
            return InvokeAsync("GetPositionInfo", cancellationToken, ("InstanceID", instanceId));
        }

        public Task<IDictionary<string, string>> GetTransportInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            //String CurrentTransportState, 
            //String CurrentTransportStatus, 
            //String CurrentSpeed
            return InvokeAsync("GetTransportInfo", cancellationToken, ("InstanceID", instanceId));
        }

        public Task SetAVTransportUriAsync(uint instanceId = 0, string currentUri = null, string currentUriMetaData = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("SetAVTransportURI", cancellationToken,
                ("InstanceID", instanceId),
                ("CurrentURI", currentUri),
                ("CurrentURIMetaData", currentUriMetaData));
        }

        public Task SetNextAVTransportUriAsync(uint instanceId = 0, string nextUri = null, string nextUriMetaData = null, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("SetNextAVTransportURI", cancellationToken,
                ("InstanceID", instanceId),
                ("NextURI", nextUri),
                ("NextURIMetaData", nextUriMetaData));
        }

        public Task StopAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Stop", cancellationToken, ("InstanceID", instanceId));
        }

        public Task PlayAsync(uint instanceId = 0, string speed = "1", CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Play", cancellationToken, ("InstanceID", instanceId), ("Speed", speed));
        }

        public Task PauseAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Pause", cancellationToken, ("InstanceID", instanceId));
        }

        public Task NextAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Next", cancellationToken, ("InstanceID", instanceId));
        }

        public Task PreviousAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Previous", cancellationToken, ("InstanceID", instanceId));
        }

        public Task SeekAsync(uint instanceId = 0, string seekMode = null, string target = null, CancellationToken cancellationToken = default)
        {
            // seekMode:
            //TRACK_NR
            //ABS_TIME
            //REL_TIME
            //ABS_COUNT
            //REL_COUNT
            //CHANNEL_FREQ
            //TAPE-INDEX
            //FRAME
            return InvokeAsync("Seek", cancellationToken, ("InstanceID", instanceId), ("Unit", seekMode), ("Target", target));
        }

        public Task SetPlayModeAsync(uint instanceId = 0, string newPlayMode = "NORMAL", CancellationToken cancellationToken = default)
        {
            //newPlayMode:
            //NORMAL
            //SHUFFLE
            //REPEAT_SHUFFLE
            //REPEAT_TRACK
            //REPEAT_ONE
            //REPEAT_ALL
            //RANDOM
            //DIRECT_1
            //INTRO
            return InvokeAsync("SetPlayMode", cancellationToken, ("InstanceID", instanceId), ("NewPlayMode", newPlayMode));
        }

        public Task<IDictionary<string, string>> GetCurrentTransportActionsAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetCurrentTransportActions", cancellationToken, ("InstanceID", instanceId));
        }

        public Task<IDictionary<string, string>> GetTransportSettingsAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetTransportSettings", cancellationToken, ("InstanceID", instanceId));
        }

        public Task<IDictionary<string, string>> GetDeviceCapabilitiesAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetDeviceCapabilities", cancellationToken, ("InstanceID", instanceId));
        }

        //void Record(UInt32 instanceID);
    }
}