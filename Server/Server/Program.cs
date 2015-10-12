using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Messages;


namespace Server_Hub
{   class Hasher
    {
        public Hasher()
        { }
        public Int32 HashMe(byte[] a)
        {
            Int32 ans=0;
            return ans;
        }
    } 
    class Request_Handler
    {
        Socket socks;
        TcpClient cli;
        public Request_Handler(Socket s)
        {
            this.socks = s;
        }

        public Request_Handler(TcpClient c)
        {
            cli = c;
        }

        public void Handling()
        {
            string checkr;
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];
            string flnme;
            NetworkStream stream = cli.GetStream();
            string[] dirs = Directory.GetFiles(@"E:\Music");
            for (int q = 0; q < dirs.Length; q++) {
                FileInfo inf = new FileInfo(dirs[q]);
                flnme = (Path.GetFileName(dirs[q]));
                filenames = Encoding.ASCII.GetBytes(flnme);
                //socks.Send(filenames);
                stream.Write(filenames, 0, filenames.Length);
                Console.WriteLine("Sent: {0}", flnme);
                while (true) {
                    // Прочесть ответ сервера.
                    //Int32 bytes_1 = socks.Receive(bytes);
                    Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                    
                    checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                    if (checkr != "") {
                        checkr = "";
                        break;
                    }
                }
            }
            flnme = "End of list";
            filenames = Encoding.ASCII.GetBytes(flnme);
            stream.Write(filenames, 0, filenames.Length);
            //socks.Send(filenames);
            Console.WriteLine("Sent: {0}", flnme);

        }
    }
    class File_Translator
    {
        private int port;
        public File_Translator(int p)
        {
            this.port = p;
        }
        public void Send(string s)
        {

        }
    }
    class Server
    {
        // Входящие данные от клиента.
        public static string data = null;

        public static void StartListening()
        {
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 13000);
            //Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpListener listener = new TcpListener(IPAddress.Any, 13000);
            

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            
            // Привязка сокета к конечной точке и ожидание коннектов.
            try
            {

                // Начало прослушки.
                //               listener.Bind(EndPoint);
                //            listener.Listen(100);
                listener.Start();

                // Начало прослушки.
                while (true)
                {

                    Console.Write("Waiting for a connection... ");

                    // Потверждение соединения.
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    //Socket handler = listener.Accept();
                    Request_Handler RH = new Request_Handler(client);
                    new Thread(RH.Handling).Start();
                    Console.WriteLine("Connected!");


                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartListening();
        }
    }
}
