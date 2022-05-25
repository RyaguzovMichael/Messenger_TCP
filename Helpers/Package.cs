using System.Text;
using System.Text.Json;

namespace MessengerDll
{
    public class Package
    {
        public PackageType Type { get; private set; }
        public string UserName { get; private set; }
        public string Time { get; private set; }
        public string Data { get; private set; }

        public Package(PackageType type, string userName, string time, string data)
        {
            Type = type;
            UserName = userName;
            Time = time;
            Data = data;
        }

        public byte[] ConvertToByteArray()
        {
            string jsonData = JsonSerializer.Serialize(this);
            byte[] data = Encoding.Unicode.GetBytes(jsonData);
            return data;
        }

        public static Package ConvertFromByteArray(byte[] data)
        {
            string jsonData = Encoding.Unicode.GetString(data);
            return ConvertFromString(jsonData);
        }

        public static Package ConvertFromString(string data)
        {
            Package package = JsonSerializer.Deserialize<Package>(data);
            return package;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
