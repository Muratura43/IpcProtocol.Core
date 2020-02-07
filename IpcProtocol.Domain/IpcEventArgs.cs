using System;

namespace IpcProtocol.Domain
{
    public class IpcEventArgs
    {
        public string JsonData { get; set; }

        public IpcEventArgs(string data)
        {
            JsonData = data;
        }

        public IpcEventArgs(byte[] data)
        {
            JsonData = Convert.ToBase64String(data);
        }
    }
}
