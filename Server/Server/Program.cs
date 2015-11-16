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
        // Сообщение для коммуникации с клиентом.
        Message Mes;
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();
        
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
            // Массив для считывания ответа из потока.
            Byte[] data = new Byte[256];
            // Строка для имени файла.
            string name = string.Empty;

            // Цикл ожидания ответа.
            while (true)
            {
                while (name == string.Empty)
                {
                    name = Receive_Message(stream);
                }

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start sending protocol for " + Mes.Get_Data());

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, Mes.Get_Data());
                Thread h = new Thread(FT.Sending);
                h.Start();

                // Ожидание конца передачи файла.
                h.Join();
                h.Abort();
                Mes = new Message();
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
            Mes = new RequestMessage(string.Empty);

            while (Mes.Get_Data() == string.Empty)
            {
                // Прочесть запрос клиента.
                Int32 bytes_1 = stream.Read(data, 0, data.Length);
                Mes = Mes_Hand.Decrypt(data);
            }
            return Mes.Get_Data();
        }

        // Старый метод.
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
        // Сообщение для коммуникации с клиентом.
        Message mes;
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();

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
            // Количество полученных байт.
            Int32 bytes_count;
            // Буффер для хранения сообщения клиента.
            byte[] pack = new byte[1024];

            Message response = new ResponseMessage(string.Empty);


            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = new NetworkStream(client))
            {
                do
                {

                    // Прочесть ответ сервера.
                    bytes_count = stream.Read(pack, 0, pack.Length);
                    // Обработка данных.
                    mes = new FilePartMessage(Mes_Hand.Decrypt(pack).Get_Data());

                    if (mes.Get_Data()!= "End of file")
                    {
                        // Запись в файл.
                        outFile.Write(pack, 0, bytes_count);

                        // Отправка уведомления клиенту.
                        stream.Write(Mes_Hand.Encrypt(response), 0, Mes_Hand.Encrypt(response).Length);
                    }
                } while (mes.Get_Data() != "End of file");

                // Уведомление об окончании операции.
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
            // Сообщение для коммуникации с клиентом.
            Message file_part = new FilePartMessage(string.Empty);
            // Сообщение для обработки подтверждения.
            Message response = new ResponseMessage(string.Empty);

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
                        file_part = Mes_Hand.Decrypt(buffer);

                        // Проверка на наличие неотправленных данных.
                        if (btscpd > 0)
                        {
                            // Отправка пакета.
                            stream.Write(Mes_Hand.Encrypt(file_part), 0, Mes_Hand.Encrypt(file_part).Length);

                            // Получение подтверждения.
                            while (true)
                            {
                                response = Recieve_Message(stream);

                                if (response.Get_Data()!=string.Empty)
                                {
                                    response = new ResponseMessage(string.Empty);
                                    break;
                                }
                            }
                        }
                    } while (btscpd > 0);

                    // Отправка уведомления о конце файла.
                    file_part = new FilePartMessage("End of file");
                    stream.Write(Mes_Hand.Encrypt(file_part), 0, Mes_Hand.Encrypt(file_part).Length);

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
                    ErrorMessage Err = new ErrorMessage(string.Empty);
                    stream.Write(Mes_Hand.Encrypt(Err), 0, Mes_Hand.Encrypt(Err).Length);
                }
            }

        }

        public Message Recieve_Message(NetworkStream stream)
        {
            // Количество полученных байт.
            Int32 bytes_count;
            // Буффер для хранения сообщения клиента.
            byte[] pack = new byte[1024];
            // Возвращаемое сообщение.
            ResponseMessage response;

            // Считываение данных из потока.
            bytes_count = stream.Read(pack, 0, pack.Length);
            response = new ResponseMessage(Mes_Hand.Decrypt(pack).Get_Data());
            return response;
        }

    }

    // Главное тело сервера. Обработка подключений.
    class Server
    {
        // Порт для подключения клиентов.
        static Int16 port = 13000;
        // Путь к файлам сервера.
        static string server_destination = @"D:\DFS";
        // Обработчик сообщений.
        static Message_Handler Mes_Hand = new Message_Handler();

        // Работа с подключением.
        public static void Working_With(object income)
        {
            // Получение сокета для общения с клиентом.
            Socket client = (Socket)income;
            // Выделение экземпляра класса Message для распознавания ответа.
            Message mess;
            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = new NetworkStream(client);

            // Ожидание получения сообщения.
            while (true)
            {
                mess = Recieve_Message(stream);
                
                // Проверка типа сообщния.
                if (mess.Get_Type()==1)
                {
                    // Отправка списка файлов.
                    Send_Message(stream);

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
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            // Строка содержащая имя файла.
            string file_name;
            // Длина ответа клиента.
            int response_lenght;
            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = (NetworkStream)income;
            // Определение файлов в директории.
            string[] dirs = Directory.GetFiles(server_destination);
            // Сообщение полученное от клиента.
            Message M;

            // Отправка информации о каждом файле.
            for (int q = 0; q < dirs.Length; q++)
            {
                // Подготовка информации о файле.
                FileInfo inf = new FileInfo(dirs[q]);
                // Строка хранящая имя файла.
                file_name = (Path.GetFileName(dirs[q]));
                // Создание сообщения.
                ListMessage LM = new ListMessage(file_name);
                // Кодировка
                bytes = Mes_Hand.Encrypt(LM);
                // Отправка сообщения.
                stream.Write(bytes, 0, bytes.Length);
                // Уведомление об успешной отправке.
                Console.WriteLine("Main thread: Sent info of: {0}", file_name);

                // Ожидание получения подтверждения о приеме информации.
                while (true)
                {
                    // Получение данных.
                    response_lenght = stream.Read(bytes, 0, bytes.Length);
                    // Расшифровка сообщения.
                    M = Mes_Hand.Decrypt(bytes);

                    // Проверка на пустоту.
                    if (M.Get_Data() != "")
                    {
                        break;
                    }
                }
            }

            // Формирование отчета о завершении.
            M = new EndMessage("End of list");
            bytes = Mes_Hand.Encrypt(M);

            // Отправка отчета о передаче всего списка.
            stream.Write(bytes, 0, bytes.Length);
        }

        // Протокол обработки сообщения.
        public static Message Recieve_Message(object income)
        {
            // Получение сокета для общения с клиентом.
            Stream client = (Stream)income;
            // Выделение экземпляра класса Message для распознавания ответа.
            Message mess;
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];

            // Считывание массива байт из потока.
            Int32 bytes_count = client.Read(bytes, 0, bytes.Length);
            // Обработка сообщения.
            mess = Mes_Hand.Decrypt(bytes);

            return mess;
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
                    new Thread(Working_With).Start(client);

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
