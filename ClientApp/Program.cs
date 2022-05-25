using ClientApp.Abstractions;

namespace ClientApp
{
    internal class Program
    {
        static void Main()
        {
            IUserInterface UI = new ClientConsoleInterface();
            ClientLogic clientLogic = new(UI);
            while (!clientLogic.TryConnect(UI.GetConnectData()))
            {
                System.Console.WriteLine("Данный хост недоступен");
            }
            clientLogic.StartClient();
        }
    }
}