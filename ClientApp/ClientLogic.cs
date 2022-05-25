using ClientApp.Abstractions;
using MessengerDll;
using System;
using System.Net.Sockets;
using System.Threading;

namespace ClientApp
{
    internal class ClientLogic
    {
        private string _userName;
        private string _host;
        private int _port;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly IUserInterface _ui;

        public ClientLogic(IUserInterface ui)
        {
            _ui = ui;
        }

        public bool TryConnect((string, int) value)
        {
            try
            {
                _host = value.Item1;
                _port = value.Item2;
                _tcpClient = new TcpClient(_host, _port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StartClient()
        {
            try
            {
                LoginToServer();
                Thread receiveThread = new(Listen);
                receiveThread.Start();
                while (true)
                {
                    _stream.SendMessage(PackageType.Message, _userName, _ui.GetNewMessage(_userName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        private void LoginToServer()
        {
            bool firstTry = true;
            do
            {
                if (!firstTry) _tcpClient = new TcpClient(_host, _port);
                _userName = _ui.GetUserName("");
                _stream = _tcpClient.GetStream();
                _stream.SendMessage(PackageType.Login, _userName, "");
                Package package = _stream.ReceiveMessage();
                if (package.Type == PackageType.Login && package.Data == "Ok") break;
                _ui.MessagesHistory.Add(package.Data);
                firstTry = false;
            } while (true);
            _ui.MessagesHistory.Clear();
            _ui.MessagesHistory.Add($"Добро пожаловать, {_userName}");
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    Package package = _stream.ReceiveMessage();
                    switch (package.Type)
                    {
                        case PackageType.Login:
                            break;
                        case PackageType.InfoData:
                            _ui.MessagesHistory.Add(package.Data);
                            break;
                        case PackageType.Message:
                            _ui.MessagesHistory.Add(package.UserName + " " + package.Time + ": " + package.Data);
                            break;
                    }
                    _ui.UpdateShowMessagesHistory();
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            _stream?.Close();
            _tcpClient?.Close();
            Environment.Exit(0);
        }
    }
}
