using MessengerDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ServerApp
{
    internal class Client
    {
        protected internal NetworkStream Stream { get; private set; }
        protected internal string UserName { get; private set; }

        private readonly TcpClient _tcpClient;
        private readonly Server _server;

        public Client(TcpClient tcpClient, Server server)
        {
            _tcpClient = tcpClient;
            _server = server;
        }

        public bool AcceptLogin()
        {
            Stream = _tcpClient.GetStream();
            Package package = Stream.ReceiveMessage();
            if (package.Type == PackageType.Login)
            {
                UserName = package.UserName;
                if (_server.GetLoginedUsers().Contains(UserName)) return false;
                return true;
            }
            return false;
        }

        public void Process()
        {
            try
            {
                Stream.SendMessage(PackageType.InfoData, "Server", "Пользователи в чате: " + string.Join(", ", _server.GetLoginedUsers())); //TODO: Здесь присылается второй пакет с данными о пользователях в чате
                string message = UserName + ": вошёл в чат"; // Данная багулина появлется причём только на 2м и далее подключающихся клиентах, к тому же если клиенты втыкаются в то,
                _server.BroadcastMessage(PackageType.InfoData, "Server", UserName, message);  // что их логин уже занят то со второй попытки у них всё нормально, а если с первой то без задержки не робит
                Console.WriteLine(message);
                while (true)
                {
                    try
                    {
                        Package package = Stream.ReceiveMessage();
                        switch (package.Type)
                        {
                            case PackageType.Login:
                                break;
                            case PackageType.InfoData:
                                break;
                            case PackageType.Message:
                                List<string> addressees = GetAddressees(package.Data, out message);
                                if (addressees.Count == 0) _server.BroadcastMessage(PackageType.Message, package.UserName, package.UserName, message);
                                if (addressees.Count == 1 && addressees[0] == UserName) break;
                                else
                                {
                                    addressees.Remove(UserName);
                                    _server.BroadcastMessage(PackageType.Message, package.UserName, message, addressees);
                                }
                                
                                break;
                        }
                        message = String.Format("{0}: {1}", package.UserName, package.Data);
                        Console.WriteLine(message);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", UserName);
                        Console.WriteLine(message);
                        _server.BroadcastMessage(PackageType.InfoData, "Server", UserName, message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _server.RemoveConnection(UserName);
                Close();
            }
        }

        private List<string> GetAddressees(string message, out string newMessage)
        {
            newMessage = message;
            int indexOfSeparator = message.IndexOf(':');
            if (indexOfSeparator == -1) return new List<string>();
            string[] addressees = message.Substring(0, indexOfSeparator).Split(',');
            newMessage = message.Substring(indexOfSeparator + 1, message.Length - indexOfSeparator - 1).Trim();
            for (int i = 0; i < addressees.Length; i++)
            {
                addressees[i] = addressees[i].Trim();
            }
            List<string> result = addressees.ToList<string>();
            return result;
        }

        protected internal void Close()
        {
            Stream?.Close();
            _tcpClient?.Close();
        }
    }
}
