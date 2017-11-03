using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Trans_Client
{
    class Program
    {
        static int port;
        static int recvByte;
        static int sendByte;
        static int fiRecvByte;
        static int fiSendByte;
        static string recvStr;
        static string sendStr;
        static string fiRecvStr;
        static string fiSendStr;
        static byte[] recvBytes;
        static byte[] sendBytes;
        static byte[] fiRecvBytes;
        static byte[] fiSendBytes;

        static void Main(string[] args)
        {
            Console.WriteLine("#####TCP通信客户端#####");
            int choose;
            ParameterizedThreadStart CStrat = new ParameterizedThreadStart(Recv);
            Thread CThread = new Thread(CStrat);

            InitData();
            port = 8900;
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipe = new IPEndPoint(ip, port);

            Socket cs = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            Console.WriteLine("Connecting......");
            Console.WriteLine("Please wait......");
            Console.WriteLine();
            Console.WriteLine();

            cs.Connect(ipe);
            CThread.Start(cs);

            do
            {
                Console.WriteLine("Choose send mode:");
                Console.WriteLine("1.Send Message");
                Console.WriteLine("2.Send File");
                Console.WriteLine("3.Quit");
                Console.WriteLine("Your Choose:");
                choose = int.Parse(Console.ReadLine());
                switch (choose)
                {
                    case 1:
                        SendMessage(cs);
                        break;
                    case 2:
                        SendFile(cs);
                        break;
                    case 3:
                        CThread.Abort(cs);
                        cs.Close();
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
                
            } while (false);

            Console.ReadLine();
        }

        private static void InitData()
        {
            recvByte = 0;
            sendByte = 0;
            recvStr = "";
            sendStr = "";
            recvBytes = new byte[1024];
            sendBytes = new byte[1024];
        }

        internal static void SendMessage(Socket cs)
        {
            string style = "1";//MODE 1
            byte[] styleByte = new byte[1024];
            styleByte = Encoding.ASCII.GetBytes(style);
            Console.WriteLine("Please type a message:");
            sendStr = Console.ReadLine();
            sendBytes = Encoding.ASCII.GetBytes(sendStr);
            Console.WriteLine("Sending......");
            cs.Send(styleByte);
            cs.Send(sendBytes, sendBytes.Length, 0);
        }

        public static void SendFile(Socket cs)
        {
            string filePath;
            Console.WriteLine();
            Console.WriteLine("Please type file path:");
            filePath = Console.ReadLine();
            FileInfo fi = new FileInfo(filePath);
            FileStream fiStream = fi.OpenRead();
            int packetSize = 1000;
            int packetCount = (int)(fi.Length / (packetSize));
            int lastPacketData = (int)(fi.Length - ((long)packetSize * packetCount));
            byte[] data = new byte[packetSize];

            string style = "2";//MODE 2
            byte[] styleByte = new byte[1024];
            styleByte = Encoding.ASCII.GetBytes(style);
            cs.Send(styleByte);

            cs.Send(Encoding.ASCII.GetBytes(packetSize.ToString()));//发送包的大小
            cs.Send(Encoding.ASCII.GetBytes(packetCount.ToString()));//发送包的总数量

            for (int i = 0; i < packetCount; i++)
            {
                fiStream.Read(data, 0, data.Length);
                SendVarData(cs, data);
            }
            if (lastPacketData != 0)
            {
                data = new byte[lastPacketData];
                fiStream.Read(data, 0, data.Length);
                SendVarData(cs, data);
            }
        }

        public static void Recv(object c)
        {
            Socket cs = (Socket)c;
            recvByte = cs.Receive(recvBytes, recvBytes.Length, 0);
            recvStr += Encoding.ASCII.GetString(recvBytes, 0, recvByte);
            Console.WriteLine("Client get messages: ");
        }

        public static int SendVarData(Socket cs, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;
            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            sent = cs.Send(datasize);

            while(total < size)
            {
                sent = cs.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }
            return total;
        }
    }
}
