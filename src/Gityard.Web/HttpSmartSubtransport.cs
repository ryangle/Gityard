//using LibGit2Sharp;

//namespace Gityard.Web;

//public class HttpSmartSubtransport : RpcSmartSubtransport
//{
//    protected override SmartSubtransportStream Action(string url, GitSmartSubtransportAction action)
//    {
//        string endpointUrl, contentType = null;
//        bool isPost = false;

//        switch (action)
//        {
//            case GitSmartSubtransportAction.UploadPackList:
//                endpointUrl = string.Concat(url, "/info/refs?service=git-upload-pack");
//                break;

//            case GitSmartSubtransportAction.UploadPack:
//                endpointUrl = string.Concat(url, "/git-upload-pack");
//                contentType = "application/x-git-upload-pack-request";
//                isPost = true;
//                break;

//            case GitSmartSubtransportAction.ReceivePackList:
//                endpointUrl = string.Concat(url, "/info/refs?service=git-receive-pack");
//                break;

//            case GitSmartSubtransportAction.ReceivePack:
//                endpointUrl = string.Concat(url, "/git-receive-pack");
//                contentType = "application/x-git-receive-pack-request";
//                isPost = true;
//                break;

//            default:
//                throw new InvalidOperationException();
//        }

//        return new HttpSmartSubtransportStream(this, endpointUrl, isPost, contentType);
//    }

//    private class HttpSmartSubtransportStream : SmartSubtransportStream
//    {
//        private static int MAX_REDIRECTS = 5;

//        private MemoryStream postBuffer = new MemoryStream();
//        private Stream responseStream;

//        public HttpSmartSubtransportStream(HttpSmartSubtransport parent, string endpointUrl, bool isPost, string contentType)
//            : base(parent)
//        {
//            EndpointUrl = endpointUrl;
//            IsPost = isPost;
//            ContentType = contentType;
//        }

//        private string EndpointUrl
//        {
//            get;
//            set;
//        }

//        private bool IsPost
//        {
//            get;
//            set;
//        }

//        private string ContentType
//        {
//            get;
//            set;
//        }

//        public override int Write(Stream dataStream, long length)
//        {
//            byte[] buffer = new byte[4096];
//            long writeTotal = 0;

//            while (length > 0)
//            {
//                int readLen = dataStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

//                if (readLen == 0)
//                {
//                    break;
//                }

//                postBuffer.Write(buffer, 0, readLen);
//                length -= readLen;
//                writeTotal += readLen;
//            }

//            if (writeTotal < length)
//            {
//                throw new EndOfStreamException("Could not write buffer (short read)");
//            }

//            return 0;
//        }
        
//        public override int Read(Stream dataStream, long length, out long readTotal)
//        {
//            byte[] buffer = new byte[4096];
//            readTotal = 0;

//            if (responseStream == null)
//            {
//                HttpWebResponse response = GetResponseWithRedirects();
//                responseStream = response.GetResponseStream();
//            }

//            while (length > 0)
//            {
//                int readLen = responseStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

//                if (readLen == 0)
//                    break;

//                dataStream.Write(buffer, 0, readLen);
//                readTotal += readLen;
//                length -= readLen;
//            }

//            return 0;
//        }

//        protected override void Free()
//        {
//            if (responseStream != null)
//            {
//                responseStream.Dispose();
//                responseStream = null;
//            }

//            base.Free();
//        }
//    }
//}
