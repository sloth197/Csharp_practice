using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");  //서버의 IP주소
        int port = 8888;    //포트 번호 지정
        IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        clientSocket.Connect(endPoint); //서버 연결
        Console.WriteLine("서버에 연결되었습니다"); //출력 메세지

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start(clientSocket);

        while(true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data);
        }
    }
    private static void ReceiveMessages(object clientObj)
    {
        Socket clientSocket = (Socket)clientObj;
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while((bytesRead = clientSocket.Receive(buffer)) >0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("서버는" + message);
            }
        }
        catch (SocketException)
        {

        }
        finally
        {
            clientSocket.Close();
        }
    }
}