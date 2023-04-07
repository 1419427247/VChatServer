using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VChatService.Model;
using VChatService.Net;
using VChatService.Orm;
using VChatService.Table;

namespace VChatService;
internal static class VChat
{
    public static VConfig config = VConfig.LoadConfig();
    public static VLogger logger = new VLogger(config.Logger);
    public static VSqlite sqlite = new VSqlite(config.Sqlite);
    public static VHttpServer server = new VHttpServer(config.HttpServer);
    public static VChatBot bot = new VChatBot(config.ChatBot);

    private static void Main(string[] args)
    {
        server.Start();
        while (true)
        {
        }
    }

    public static long GetNowSeconds()
    {
        return DateTime.Now.Ticks / 10000000;
    }

    internal static string GetRandomString(int length)
    {
        string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder();
        Random random = new Random();
        for (int i = 0; i < length; i++)
        {
            sb.Append(str[random.Next(0, str.Length)]);
        }
        return sb.ToString();
    }
}

//修改1000条数据
// for(int i = 0; i < 1000; i++)
// {
//     User user = new User(){
//         Email = i + "@qq.com",
//     };
//     user = sqlite.Select(user);
//     System.Console.WriteLine(user.Name);
// }
// string connectionString = "Data Source=myDatabase.db;Version=3;";
// var connection = new SQLiteConnection(connectionString);
// connection.Open();
// string sql = "CREATE TABLE myTable (id INTEGER PRIMARY KEY, name TEXT NOT NULL)";

// using (var command = new SQLiteCommand(sql, connection))
// {
//     using (var reader = command.ExecuteReader())
//     {
//         while (reader.Read())
//         {
//             Console.WriteLine(reader);
//         }
//     }
// }

// connection.Dispose();





