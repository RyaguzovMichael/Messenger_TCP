using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MessengerDll;

namespace ServerApp
{
    internal class Server
    {
        private static TcpListener _tcpListener;
        private List<Client> _clients = new();

        internal void RemoveConnection(string userName)
        {
            Client client = _clients.FirstOrDefault(c => c.UserName == userName);
            if (client != null) _clients.Remove(client);
        }

        internal void Listen()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, 8888);
                _tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидаем подключений...");

                while (true)
                {
                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    Client client = new(tcpClient, this);
                    if(!client.AcceptLogin())
                    {
                        client.Stream.SendMessage(PackageType.Login, client.UserName, "Это имя уже занято"); 
                        continue;
                    }
                    client.Stream.SendMessage(PackageType.Login, client.UserName, "Ok");  //TODO: Здесь отправляется первое сообщение о том что данный логин доступен
                    //Thread.Sleep(150); //Если убрать эту задержку то клиент воспринимает первый пакет и следующий как один
                    Thread clientThread = new(client.Process);
                    _clients.Add(client);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        internal void BroadcastMessage(PackageType type, string userName,string messageOwner, string message)
        {
            foreach (Client client in _clients)
            {
                if (client.UserName != messageOwner) client.Stream.SendMessage(type, userName, message);
            }
        }

        internal void BroadcastMessage(PackageType type, string userName, string message, List<string> addressees)
        {
            for (int i = 0, removed  = 0; i < _clients.Count - removed; i++)
            {
                if (addressees.Contains(_clients[i].UserName))
                {
                    _clients[i].Stream.SendMessage(type, userName, message);
                    addressees.Remove(_clients[i].UserName);
                    i--;
                    removed++;
                }
            }
            if (addressees.Count != 0)
            {
                Client client = _clients.FirstOrDefault(c => c.UserName == userName);
                if (client != null) client.Stream.SendMessage(PackageType.InfoData, "Server", "Не найденные пользователи: " + string.Join(", ", addressees));
            }
        }

        internal string[] GetLoginedUsers()
        {
            string[] names = new string[_clients.Count];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = _clients[i].UserName;
            }
            return names;
        }

        internal void Disconnect()
        {
            _tcpListener.Stop();
            foreach (Client client in _clients) client.Close();
            Environment.Exit(0);
        }
    }
}