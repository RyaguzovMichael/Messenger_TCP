using System.Collections.Generic;

namespace ClientApp.Abstractions
{
    internal interface IUserInterface
    {
        List<string> MessagesHistory { get; }
        string GetUserName(string message);
        void UpdateShowMessagesHistory(bool IsStart = false);
        string GetNewMessage(string userName);
        (string, int) GetConnectData();
    }
}