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
            // Объявление переменных.
            string checkr;
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];
            string flnme;

            // Выделение потока для отправки списка.
            NetworkStream stream = cli.GetStream();

            // Отправка списка файлов.
            string[] dirs = Directory.GetFiles(@"D:\DFS");
            for (int q = 0; q < dirs.Length; q++) {
                FileInfo inf = new FileInfo(dirs[q]);
                flnme = (Path.GetFileName(dirs[q]));
                filenames = Encoding.ASCII.GetBytes(flnme);
                stream.Write(filenames, 0, filenames.Length);
                Console.WriteLine("Sent: {0}", flnme);
                while (true) {
                    Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                    
                    checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                    if (checkr != "") {
                        checkr = "";
                        break;
                    }
                }
            }

            // Отправка уведомления о завершении передачи.
            flnme = "End of list";
            filenames = Encoding.ASCII.GetBytes(flnme);
            stream.Write(filenames, 0, filenames.Length);
            Console.WriteLine("Sent: {0}", flnme);

            // Получение информации о получаемом файле.
            flnme = "";
            // Буффер дл получения ответа.
            Byte[] data = new Byte[256];
            while ((flnme == ""))
            {
                // Прочесть ответ сервера.
                Int32 bytes_1 = stream.Read(data, 0, data.Length);
                flnme = Encoding.ASCII.GetString(data, 0, bytes_1);
                
            }
            Console.WriteLine(flnme+ "!!!");
            File_Translator FT = new File_Translator(cli, flnme);
            new Thread(FT.Sending).Start();
        }
    }
    class File_Translator
    {
        TcpClient cli;
        string flname;
        public File_Translator(TcpClient c, string n)
        {
            this.cli = c;
            this.flname = n;
        }

        public void Receiving()
        {

        }

        public void Sending()
        {
            // Объявление переменных.
            string checkr = "";
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];
                        
            string path = Path.Combine(@"D:\DFS", flname);

            // Отправка списка файлов.
            if (File.Exists(path))
            {
                const int buffersz = 16384;
                byte[] buffer = new byte[buffersz];
                int btscpd = 0;
                using (FileStream inFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (NetworkStream stream = cli.GetStream())
                {
                    do
                    {
                        btscpd = inFile.Read(buffer, 0, buffersz);
                        if (btscpd > 0)
                        {
                            // Отправка пакета.
                            stream.Write(buffer, 0, btscpd);

                            // Получение подтверждения.
                            while (true)
                            {
                                Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                                checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                                if (checkr != "")
                                {
                                    checkr = "";
                                    break;
                                }
                            }
                        }
                    } while (btscpd > 0);

                    // Отправка уведомления о конце файла.
                    checkr = "End of file";
                    filenames = Encoding.ASCII.GetBytes(checkr);
                    stream.Write(filenames, 0, filenames.Length);
                    Console.WriteLine("Sent: {0}", checkr);
                }

            }           
            
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
                //listener.Bind(EndPoint);
                //listener.Listen(100);
                listener.Start();

                // Начало прослушки.
                while (true)
                {

                    Console.WriteLine("Waiting for a connection... ");

                    // Потверждение соединения.
                    TcpClient client = listener.AcceptTcpClient();
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
