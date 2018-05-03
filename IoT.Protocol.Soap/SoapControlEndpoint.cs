using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    public class SoapControlEndpoint : IControlEndpoint<SoapEnvelop, SoapEnvelop>
    {
        private HttpClient httpClient;
        object syncRoot = new object();
        private bool connected;
        private readonly Uri baseUri;
        public SoapControlEndpoint(Uri baseAddress)
        {
            baseUri = baseAddress;
        }

        public void Close()
        {
            if(connected)
            {
                lock(syncRoot)
                {
                    if(connected)
                    {
                        httpClient.Dispose();
                        httpClient = null;
                        connected = false;
                    }
                }
            }
        }

        public void Connect()
        {
            if(!connected)
            {
                lock(syncRoot)
                {
                    if(!connected)
                    {
                        httpClient = new HttpClient() { BaseAddress = baseUri };
                        connected = true;
                    }
                }
            }
        }

        public Task<SoapEnvelop> InvokeAsync(SoapEnvelop message, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Close();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}