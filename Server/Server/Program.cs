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
        Socket listener;
        TcpClient cli;
        bool receiving;
        public Request_Handler()
        {
        }

        public Request_Handler(Socket c, bool b)
        {
            this.listener = c;
            receiving = b;
        }

        public void Handling()
        {
            // Объявление переменных.
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];
            string flnme;
            Console.WriteLine("Main thread: Waiting for a connection... ");
            
            if (receiving)
            { 
            // Буффер для получения ответа.
            flnme = "";
            Byte[] data = new Byte[256];
                while (true)
                {
                    Socket client = listener.Accept();
                    Stream stream = new NetworkStream(client);
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
                    File_Translator FT = new File_Translator(client, flnme);
                    Thread h = new Thread(FT.Sending);
                    h.Start();
                    h.Join();
                    h.Abort();
                    flnme = "";
                }
            }
            else
            {
                // Буффер для получения ответа.
                flnme = "";
                Byte[] data = new Byte[256];
                while (true)
                {
                    Socket client = listener.Accept();
                    Stream stream = new NetworkStream(client);
                    // Цикл ожидания ответа.
                    while (flnme == "")
                    {
                        // Прочесть запрос клиента.
                        Int32 bytes_1 = stream.Read(data, 0, data.Length);
                        string assis = Encoding.ASCII.GetString(data, 0, bytes_1);
                        if (assis[0] == '3')
                        {
                            flnme = assis.Remove(0, 1);
                            stream.Write(data, 0, data.Length);
                        }
                    }

                    // Отчет о начале передачи.
                    Console.WriteLine("Handling thread: Start receiving protocol for " + flnme);

                    // Запуск передачи файла.
                    File_Translator FT = new File_Translator(client, flnme);
                    Thread h = new Thread(FT.Receiving);
                    h.Start();
                    h.Join();
                    h.Abort();
                    flnme = "";
                }
            }
        }
    }
    class File_Translator
    {
        Socket cli;
        string flname;
        public File_Translator(Socket c, string n)
        {
            this.cli = c;
            this.flname = n;
        }

        public void Receiving()
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];
            
            // Объявление переменных.
            string checker = "";
            const int buffersz = 16384;
            byte[] buffer = new byte[buffersz];
            int btscpd = 0;
            byte[] pack = new byte[1024];
            byte[] filename = new byte[1024];


            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = new NetworkStream(cli))
            {
                do
                {
                    // Буффер для получения ответа.
                    pack = new Byte[256];

                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(pack, 0, pack.Length);
                    checker = Encoding.ASCII.GetString(pack, 0, bytes_1);
                    if (checker != "End of file")
                    {
                        outFile.Write(pack, 0, bytes_1);
                        stream.Write(pack, 0, pack.Length);
                    }
                } while (checker != "End of file");
                Console.WriteLine("Downloading is complete!");
            }
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
                using (NetworkStream stream = new NetworkStream(cli))
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
        static Int32 port_hand = 15000;
        static Int32 port_rec = 13000;
        static Int32 port_down = 14000;
        // Входящие данные от клиента.
        public static string data = null;

        public static void StartListening()
        {


            // Выделение сокета для обработки запросов.
            IPEndPoint EndPointa = new IPEndPoint(IPAddress.Any, port_hand);
            Socket socks = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Начало прослушки.
            socks.Bind(EndPointa);
            socks.Listen(100);

            // Выделение сокета для отправки файла.
            IPEndPoint EndPoint1 = new IPEndPoint(IPAddress.Any, port_rec);
            Socket listener1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            
            try
            {
                // Начало прослушки.
                listener1.Bind(EndPoint1);
                listener1.Listen(100);
                Console.WriteLine("Main thread: Waiting for a connection... ");

                // Объявление переменных.
                string checkr;
                byte[] filenames = new byte[1024];
                string flnme;

                // Ожидание соединения.
                while (true)
                {
                    // Потверждение соединения.
                    Socket client = listener1.Accept();
                    Console.WriteLine("Main thread: Connected!");

                    // Выделение потока для отправки списка.
                    NetworkStream stream = new NetworkStream(client);

                    // Узнать что нужно клиенту
                    while (true)
                    {
                        Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                        checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                        if (checkr != string.Empty)
                        {
                            if (checkr[0] == '1')
                            {
                                #region List_of_files

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
                                        bytes_1 = stream.Read(bytes, 0, bytes.Length);
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
                                #endregion

                                // Выделение потока для обработки запросов.
                                Request_Handler RH = new Request_Handler(socks, true);
                                new Thread(RH.Handling).Start();
                                client.Close();
                                break;
                            }

                            if (checkr[0] == '0')
                            {
                                // Выделение потока для обработки запросов.
                                Request_Handler RH = new Request_Handler(socks, false);
                                new Thread(RH.Handling).Start();
                                client.Close();
                                Console.WriteLine("Waiting for downloading the file...");
                                break;
                            }
                        }
                    }
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
