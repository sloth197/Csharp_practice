using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static List<Socket> clients = new List<Socket>();   //클라이언트 소켓 리스트
    private static readonly object lockObject = new object();   //스레드 안정성을 위한 락 오브젝트
    static void Main(String[] args) {
        IPAddress ipAddress = IPAddress.Any;    //모든 IP 주소에서 수신 대기함
        int port = 8888;    //포트번호 8888 지정
        IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(endPoint);    //소켓에 엔드포인트 바인딩
        serverSocket.Listen(15);        //최대 15개의 연결을 대기
        Console.WriteLine("서버가 연결되었습니다. 잠시만 기다려주세요");

        while(true) 
        {
            Socket clientSocket = serverSocket.Accept();    //클라이언트 연결 수락
            lock (lockObject) 
            {
                clients.Add(clientSocket);  //클라이언트 소켓 리스트에 추가
            }
            Thread clientThread = new Thread(HandleClient); //클라이언트 처리 스레드 생성
            clientThread.Start(clientSocket);   //스레드 시작
        }
    }
    private static void HandleClient(object clientObj) 
    {
        Socket clientSocket = (Socket)clientObj;
        byte[] buffer = new byte[1024]; //데이터 버퍼
        int bytesRead;

        try
        {
            while ((bytesRead = clientSocket.Receive(buffer)) > 0)  //데이터 수신
            {
                string message = Encoding.UTF8.GetString(buffer,0,bytesRead);   //수신된 데이터를 문자열로 변환
                Console.WriteLine("받았습니다" + message);
                BroadcastMessage(message, clientSocket);    //메세지 브로드캐스트
            }
        }
        catch (SocketException)
        {
            //클라이언트 연결 종료 시 예외 처리
        }
        finally
        {
            lock(lockObject)
            {
                clients.Remove(clientSocket);   //클라이언트 리스트에서 제거한다
            }
            clientSocket.Close();   //클라이언트 소켓 닫기
        }
    } 
    private static void BroadcastMessage(string message, Socket senderSocket)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        lock (lockObject)
        {
            foreach(var client in clients)
            {
                if(client != senderSocket)  //송신자를 제외한 모든 클라이언트들에게 메세지를 전송
                {
                    client.Send(data);
                }
            }
        }
        
    }
}