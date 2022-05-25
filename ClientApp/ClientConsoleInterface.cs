using ClientApp.Abstractions;
using System;
using System.Collections.Generic;

namespace ClientApp
{
    internal class ClientConsoleInterface : IUserInterface
    {
        private List<string> _messagesHistory = new();

        public List<string> MessagesHistory { get; private set; }

        public string GetNewMessage(string userName)
        {
            UpdateShowMessagesHistory();
            string message = Console.ReadLine();
            MessagesHistory.Add(userName + $" {DateTime.Now.Hour:d2}:{DateTime.Now.Minute:d2}: " + message);
            return message;
        }

        public string GetUserName(string message)
        {
            UpdateShowMessagesHistory(true);
            if (message != "") Console.WriteLine(message);
            Console.Write("Введите своё имя: ");
            return Console.ReadLine();
        }

        public void UpdateShowMessagesHistory(bool IsStart = false)
        {
            Console.Clear();
            foreach (string historyMessage in MessagesHistory)
            {
                Console.WriteLine(historyMessage);
            }
            if (!IsStart)
            {
                Console.WriteLine("Если просто написать сообщение, то оно отправится всем в чате,\n" +
                                  "Если хотите отправить определённым пользователям, то сначала перечислите их через запятую,\n" +
                                  "и после двоеточия напишите сообщение.");
                Console.Write("Введите сообщение: ");
            }
        }

        public (string, int) GetConnectData()
        {
            Console.Write("Введите адрес сервера: ");
            string IP = Console.ReadLine();
            int port;
            do
            {
                Console.Write("Введите порт сервера: ");
            } while (!int.TryParse(Console.ReadLine(), out port));
            return (IP, port);
        }
    }
}