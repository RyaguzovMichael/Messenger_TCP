using System;
using System.Threading;

namespace ServerApp
{
    internal class Program
    {
        private static Server server;
        private static Thread listenThread;
        static void Main(string[] args)
        {
            try
            {
                server = new Server();
                listenThread = new Thread(server.Listen);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                server.Disconnect();
            }
        }
    }
}
