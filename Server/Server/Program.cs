using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Messages;


namespace Server_Hub
{
    // Часть сервера, отвечающая за обработку запросов.
    class Request_Handler
    {
        // Сокет для обрабатываемого пользователя.
        Socket client;
        // Поток для общения с клиентом.
        Stream stream;
        
        public Request_Handler(Socket c)
        {
            // Сокет для коммуникации с клиентом.
            client = c;
            // Поток общения с клиентом.
            stream = new NetworkStream(client);
        }

        // Протокол обработки запроса на отправку файла.
        public void Handle_Send()
        {
            // Строка для ранения имени файла.
            string flnme = "";
            // Массив для считывания ответа из потока.
            Byte[] data = new Byte[256];

            // Цикл ожидания ответа.
            while (true)
            {
                while (flnme == "")
                {
                    // Прочесть запрос клиента.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    string assis = Encoding.ASCII.GetString(data, 0, bytes_1);
                    // !!!Переделать обработку полученного сообщения используя Message_Handler!!!
                }

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start sending protocol for " + flnme);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, flnme);
                Thread h = new Thread(FT.Sending);
                h.Start();

                // Ожидание конца передачи файла.
                h.Join();
                h.Abort();
                flnme = "";
            }
        }

        // Протокол обработки запроса на получение файла.
        public void Handle_Recieve()
        {
            // Строка для хранения имени запрашиваемого файла.
            string flnme = "";
            
            while (true)
            {
                // Получение имени файла.
                flnme = Receive_Message(stream);

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start receiving protocol for " + flnme);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, flnme);
                Thread h = new Thread(FT.Receiving);
                h.Start();

                // Ожидание окончания передачи.
                h.Join();
                h.Abort();
                flnme = "";
            }
        }

        // Протокол получения сообщения.
        public string Receive_Message(Stream stream)
        {
            // Буффер для получения ответа.
            Byte[] data = new Byte[256];
            // Строка для хранения имени запрашиваемого файла.
            string flnme = "";

            while (flnme == "")
            {
                // Прочесть запрос клиента.
                Int32 bytes_1 = stream.Read(data, 0, data.Length);
                string assis = Encoding.ASCII.GetString(data, 0, bytes_1);
                // !!!Обработка с помощью Message_Handler!!!
            }
            return flnme;
        }


        /* public void Handling()
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
         }*/
    }

    // Часть сервера, отвечающая за передачу файлов.
    class File_Translator
    {
        // Сокет для коммуникации с клиентом.
        Socket client;
        // Имя обрабатываемого файла.
        string flname;

        public File_Translator(Socket c, string n)
        {
            // Сокет для коммуникации с клиентом.
            client = c;
            // Имя обрабатываемого файла.
            flname = n;
        }

        // Протокол получения файла.
        public void Receiving()
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];

            // Страка для хранения обработанного ответа.
            string decrypted = string.Empty;
            // Страка для хранения необработанного ответа.
            string assist = string.Empty;
            // Количество полученных байт.
            Int32 bytes_count;
            // Буффер для хранения сообщения клиента.
            byte[] pack = new byte[1024];
            // Буффер для получения ответа.
            pack = new Byte[256];


            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = new NetworkStream(client))
            {
                do
                {

                    // Прочесть ответ сервера.
                    bytes_count = stream.Read(pack, 0, pack.Length);
                    assist = Encoding.ASCII.GetString(pack, 0, bytes_count);

                    // !!!Обработать с помощью Message_Handler!!!
                    if (decrypted != "End of file")
                    {
                        // Запись в файл.
                        outFile.Write(pack, 0, bytes_count);
                        // !!!Отправка отчета!!!
                        // stream.Write(pack, 0, pack.Length);
                    }
                } while (decrypted != "End of file");
                Console.WriteLine("Downloading is complete!");
            }
        }

        // Протокол отправки файла.
        public void Sending()
        {
            // Строка для хранения необработанного ответа.
            string assis = string.Empty;
            // Строка для хранения обработанного ответа.
            string decrypted = string.Empty;
            // Количество считанных байт.
            Int32 bytes_count;
            // Буффер для хранения полученного ответа.
            byte[] bytes = new byte[1024];
            // Объем буффера передаваемых данных.
            const int buffersz = 16384;
            // Буффер содержащий передаваемый пакет данных.
            byte[] buffer = new byte[buffersz];
            //Количество считанных байт.
            int btscpd = 0;
            // Строка для отправки уведомлений.
            byte[] message = new byte[1024];

            // Выделение пути к запрашиваемому файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            // Провека существования запрашиваемого файла.
            if (File.Exists(path))
            {
                // Начало передачи.
                using (FileStream inFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (NetworkStream stream = new NetworkStream(client))
                {
                    do
                    {
                        // Считывание данных из файла.
                        btscpd = inFile.Read(buffer, 0, buffersz);
                        // Проверка на наличие неотправленных данных.
                        if (btscpd > 0)
                        {
                            // Отправка пакета.
                            stream.Write(buffer, 0, btscpd);

                            // Получение подтверждения.
                            while (true)
                            {
                                // Считываение данных из потока.
                                bytes_count = stream.Read(bytes, 0, bytes.Length);
                                assis = Encoding.ASCII.GetString(bytes, 0, bytes_count);

                                // !!!Обработка Message_Handler и проверка подтверждения!!!

                            }
                        }
                    } while (btscpd > 0);

                    // Отправка уведомления о конце файла.
                    message = Encoding.ASCII.GetBytes("End of file");
                    stream.Write(message, 0, message.Length);

                    // Уведомление о завершении процесса.
                    Console.WriteLine("Translation thread: File has been sent.");
                }

            }
            else
            {
                // Выделение потока для отправки сообщения.
                using (NetworkStream stream = new NetworkStream(client))
                {
                    // !!!Встасить увделомление об отсутствии запрашиваемого файла!!!                
                    message = Encoding.ASCII.GetBytes("File has not been found.");
                    stream.Write(message, 0, message.Length);
                }
            }

        }

    }

    // Главное тело сервера. Обработка подключений.
    class Server
    {
        // Порт для подключения клиентов.
        static Int16 port = 13000;
        // Путь к файлам сервера.
        static string server_destination = @"D:\DFS";

        // Протокол обработки сообщения.
        public static void Receive_Message(object income)
        {
            // Получение сокета для общения с клиентом.
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
                    Send_Message(client);
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл из хранилища.
                    Request_Handler RH = new Request_Handler(client);
                    new Thread(RH.Handle_Send).Start();
                    Console.WriteLine("Waiting for choosing the file...");
                    break;
                }
                else
                {
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл в хранилище.
                    Request_Handler RH = new Request_Handler(client);
                    new Thread(RH.Handle_Recieve).Start();
                    Console.WriteLine("Waiting for downloading the file...");
                    break;
                }
            }
        }

        // Протокол отправки сообщения.
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

        // Протокол обработки подключений.
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
            // Запуск сервера.
            StartListening();
        }
    }
}
