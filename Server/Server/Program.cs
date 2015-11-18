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
        NetworkStream stream;
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();
        // Отвечает за отправку и получение сообщений.
        NetWorking NW = new NetWorking();
        
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
                    name = NW.Recieve(stream).Get_Data();
                }

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start sending protocol for " + name);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, name);
                Thread h = new Thread(FT.Sending);
                h.Start();

                // Ожидание конца передачи файла.
                h.Join();
                h.Abort();
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
                flnme = NW.Recieve(stream).Get_Data();

                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start receiving protocol for " + flnme);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, flnme);
                Thread h = new Thread(FT.Receiving);
                h.Start();

                // Ожидание окончания передачи.
                h.Join();
                h.Abort();
                flnme = string.Empty;
            }
        }
                
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
        // Отвечает за отправку/поучение сообщений.
        NetWorking NW = new NetWorking();

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
            // Буффер для хранения сообщения клиента.
            byte[] pack = new byte[1024];
            // Уведомление о получении.
            Message response = new ResponseMessage("Response");
            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS", flname);

            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = new NetworkStream(client))
            {
                do
                {
                    mes = NW.Recieve(stream);

                    if (!(mes is EndMessage))
                    {
                        // Запись в файл.
                        outFile.Write(mes.Get_Info(), 0, mes.Get_Info().Length);

                        // Отправка уведомления клиенту.
                        NW.Send(response, stream);
                    }
                } while (!(mes is EndMessage));

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
                        file_part = new FilePartMessage(Encoding.ASCII.GetString(buffer));

                        // Проверка на наличие неотправленных данных.
                        if (btscpd > 0)
                        {
                            // Отправка пакета.
                            NW.Send(file_part, stream);

                            // Получение подтверждения.
                            while (true)
                            {
                                response = NW.Recieve(stream);

                                if (response.Get_Data()!=string.Empty)
                                {
                                    response = new ResponseMessage(string.Empty);
                                    break;
                                }
                            }
                        }
                    } while (btscpd > 0);

                    // Отправка уведомления о конце файла.
                    file_part = new EndMessage("End of file");
                    NW.Send(file_part, stream);

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
        // Отвечает за отправку и получение сообщений.
        static NetWorking NW = new NetWorking();

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
                mess = NW.Recieve(stream);
                
                // Проверка типа сообщния.
                if (mess is Inform_of_Rec_Message)
                {
                    // Отправка списка файлов.
                    Send_List(stream);

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
        public static void Send_List(object income)
        {
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            // Строка содержащая имя файла.
            string file_name;
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
                // Отправка сообщения.
                NW.Send(LM, stream);
                // Уведомление об успешной отправке.
                Console.WriteLine("Main thread: Sent info of: {0}", file_name);

                // Ожидание получения подтверждения о приеме информации.
                while (true)
                {
                    M = NW.Recieve(stream);

                    // Проверка на пустоту.
                    if (M.Get_Data() != "")
                    {
                        break;
                    }
                }
            }

            // Формирование отчета о завершении.
            M = new EndMessage("End of list");
            // Отправка отчета о передаче всего списка.
            NW.Send(M, stream);
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

    public class NetWorking
    {
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();

        // Отправка сообщения.
        public void Send(Message m, NetworkStream ns)
        {
            // Отправка сообщения.
            ns.Write(Mes_Hand.Encrypt(m), 0, Mes_Hand.Encrypt(m).Length);
        }

        // ПОлучение сообщения.
        public Message Recieve(NetworkStream ns)
        {
            // Буффер для получения ответа.
            Byte[] data = new Byte[1024];
            // Полученное сообщение.
            Message mes;

            do
            {
                // Прочесть запрос клиента.
                Int32 bytes_1 = ns.Read(data, 0, data.Length);
                mes = Mes_Hand.Decrypt(data);
            }
            while (mes.Get_Data() == string.Empty);
            return mes;
        }
    }
}
