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
        public Request_Handler()
        {
        }

        public Request_Handler(Socket c)
        {
            this.socks = c;
        }

        public void Handling()
        {
            // Объявление переменных.
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];
            string flnme;
            while (true)
            {
                // Выделение потока для чтения запроса.
                Stream stream = new NetworkStream(socks);

                // Буффер дл получения ответа.
                flnme = "";
                Byte[] data = new Byte[256];

                // Цикл ожидания ответа.
                while (flnme == "")
                {
                    // Прочесть запрос клиента.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    flnme = Encoding.ASCII.GetString(data, 0, bytes_1);
                }

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start sending protocol for " + flnme);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(cli, flnme);
                new Thread(FT.Sending).Start();
            }
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
                        
            // Выделение пути к запрашиваемому файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            // Отправка файла
            if (File.Exists(path))
            {
                // Выделение констант.
                const int buffersz = 16384;
                byte[] buffer = new byte[buffersz];
                int btscpd = 0;

                // Начало передачи.
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
                    filenames = Encoding.ASCII.GetBytes("End of file");
                    stream.Write(filenames, 0, filenames.Length);
                    Console.WriteLine("Translation thread: File has been sent.");
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
            // Выделение сокета.
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 13000);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            
            try
            {
                // Начало прослушки.
                listener.Bind(EndPoint);
                listener.Listen(100);
                Console.WriteLine("Main thread: Waiting for a connection... ");
                
                // Ожидание соединения.
                while (true)
                {
                    // Потверждение соединения.
                    Socket client = listener.Accept();
                    Console.WriteLine("Main thread: Connected!");

                    // Объявление переменных.
                    string checkr;
                    byte[] filenames = new byte[1024];
                    string flnme;

                    // Выделение потока для отправки списка.
                    NetworkStream stream = new NetworkStream(client);

                    // Отправка списка файлов.
                    string[] dirs = Directory.GetFiles(@"D:\DFS");
                    for (int q = 0; q < dirs.Length; q++)
                    {
                        FileInfo inf = new FileInfo(dirs[q]);
                        flnme = (Path.GetFileName(dirs[q]));
                        filenames = Encoding.ASCII.GetBytes(flnme);
                        stream.Write(filenames, 0, filenames.Length);
                        Console.WriteLine("Main thread: Sent: {0}", flnme);
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

                    // Отправка отчета о передаче всего списка.
                    filenames = Encoding.ASCII.GetBytes("End of list");
                    stream.Write(filenames, 0, filenames.Length);

                    // Выделение потока для обработки запросов.
                    Request_Handler RH = new Request_Handler(client);
                    new Thread(RH.Handling).Start();

                    //Завершение цикла.
                    Console.WriteLine("Main thread: Waiting for a connection... ");
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
