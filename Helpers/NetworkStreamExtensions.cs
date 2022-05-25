using System;
using System.Net.Sockets;
using System.Text;

namespace MessengerDll
{
    public static class NetworkStreamExtensions
    {
        public static void SendMessage(this NetworkStream stream, PackageType type, string userName, string message)
        {
            string time = $"{DateTime.Now.Hour:d2}:{DateTime.Now.Minute:d2}";
            Package package = new(type, userName, time, message);
            string jsonData = package.ToString();
            byte[] data = Encoding.Unicode.GetBytes(jsonData);
            stream.Write(data, 0, data.Length);
        }

        public static Package ReceiveMessage(this NetworkStream stream)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new();
            int bytesCount = 0;
            do
            {
                bytesCount = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytesCount));
            } while (stream.DataAvailable);
            return Package.ConvertFromString(builder.ToString(0, builder.Length));
        }
    }
}