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
        static Int16 port = 13000;
        static string server_destination = @"D:\DFS";

        public static void Receive_Message(object income)
        {
            // Приведение экземпляра класса.
            Socket client = (Socket)income;

            // Строка для хранения содержимого пакета.
            string recieved_string;

            // Выделение экземпляра класса Message для распознавания ответа.
            Message mess;

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];

            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = new NetworkStream(client);

            // Ожидание получения сообщения.
            while (true)
            {
                // Считывание массива байт из потока.
                Int32 bytes_count = stream.Read(bytes, 0, bytes.Length);
                // Перевод массива байт в строку.
                recieved_string = Encoding.ASCII.GetString(bytes, 0, bytes_count);

                // !!!Вставить сюда приведение с помощью Message_Handler!!!

                // Проверка типа сообщния.
                if ()
                {
                    new Thread(Send_Message).Start(client);
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл из хранилища.
                    Request_Handler RH = new Request_Handler(client, true);
                    new Thread(RH.Handling).Start();
                    Console.WriteLine("Waiting for choosing the file...");
                    break;
                }
                else
                {
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл в хранилище.
                    Request_Handler RH = new Request_Handler(client, false);
                    new Thread(RH.Handling).Start();
                    Console.WriteLine("Waiting for downloading the file...");
                    break;
                }
            }
        }

        public static void Send_Message(object income)
        {
            // Приведение экземпляра класса.
            Socket client = (Socket)income;

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];

            // Строка содержащая имя файла.
            string file_name;

            // Строка содержащая ответ клиента.
            string response;

            // Длина ответа клиента.
            int response_lenght;

            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = new NetworkStream(client);
            
            // Определение файлов в директории.
            string[] dirs = Directory.GetFiles(server_destination);

            // Отправка информации о каждом файле.
            for (int q = 0; q < dirs.Length; q++)
            {
                FileInfo inf = new FileInfo(dirs[q]);
                file_name = (Path.GetFileName(dirs[q]));
                bytes = Encoding.ASCII.GetBytes(file_name);
                stream.Write(bytes, 0, bytes.Length);
                Console.WriteLine("Main thread: Sent info of: {0}", file_name);
                
                // Ожидание получения подтверждения о приеме информации.
                while (true)
                {
                    response_lenght = stream.Read(bytes, 0, bytes.Length);
                    response = Encoding.ASCII.GetString(bytes, 0, response_lenght);
                    if (response != "")
                    {
                        response = "";
                        break;
                    }
                }
            }

            // Отправка отчета о передаче всего списка.
            bytes = Encoding.ASCII.GetBytes("End of list");
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void StartListening()
        {
            // Выделение сокета для обработки запросов.
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket socks = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                // Начало прослушки.
                socks.Bind(EndPoint);
                socks.Listen(100);
                Console.WriteLine("Main thread: Waiting for a connection... ");
                
                // Ожидание соединения.
                while (true)
                {
                    // Потверждение соединения.
                    Socket client = socks.Accept();
                    Console.WriteLine("Main thread: Connected!");
                    
                    // Выделение потока для работы с клиентом.
                    new Thread(Receive_Message).Start(client);

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
