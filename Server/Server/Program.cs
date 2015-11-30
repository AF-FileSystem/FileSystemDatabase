using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Messages;
using System.Collections.Generic;


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
        // Список файлов.
        Dictionary<string, NetworkStream> Files;
        // Хранилища.
        static List<NetworkStream> Hubs = new List<NetworkStream>();

        public Request_Handler(Socket c, Dictionary<string, NetworkStream> d, List<NetworkStream> h)
        {
            // Сокет для коммуникации с клиентом.
            client = c;
            // Поток общения с клиентом.
            stream = new NetworkStream(client);
            // Файлы.
            Files = d;
            // Хабы.
            Hubs = h;
        }

        // Протокол обработки запроса на отправку файла.
        public void Handle_Send()
        {
            // Массив для считывания ответа из потока.
            Byte[] data = new Byte[256];
            // Строка для имени файла.
            string name = string.Empty;
            Message mes = new Message(string.Empty);

            // Цикл ожидания ответа.
            while (true)
            {
                    mes = NW.Recieve(stream);

                if (!(mes is ErrorMessage))
                {
                    name = mes.Get_Data();
                }
                else
                {
                    Console.WriteLine("Ending session");
                    Thread.CurrentThread.Abort();
                }
                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start sending protocol for " + name);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, name, Files, Hubs);
                name = string.Empty;
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
            Message mes = new Message(string.Empty);
            // Строка для хранения имени запрашиваемого файла.
            string flnme = "";
            
            while (true)
            {
                // Получение имени файла.
                mes = NW.Recieve(stream);

                if (!(mes is ErrorMessage))
                {
                    flnme = mes.Get_Data();
                }
                else
                {
                    Console.WriteLine("Ending session");
                    Thread.CurrentThread.Abort();
                }
                // Отчет о начале передачи.
                Console.WriteLine("Handling thread: Start receiving protocol for " + flnme);

                // Запуск передачи файла.
                File_Translator FT = new File_Translator(client, flnme, Files, Hubs);
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
        // Список файлов.
        Dictionary<string, NetworkStream> Files;
        // Хранилища.
        static List<NetworkStream> Hubs = new List<NetworkStream>();

        public File_Translator(Socket c, string n, Dictionary<string, NetworkStream> d, List<NetworkStream> h)
        {
            // Сокет для коммуникации с клиентом.
            client = c;
            // Имя обрабатываемого файла.
            flname = n;
            // Файлы.
            Files = d;
            // Хабы.
            Hubs = h;
        }

        // Протокол получения файла.
        public void Receiving()
        {
            // Уведомление о получении.
            Message response = new ResponseMessage("Response");
            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS", flname);
            // Уведомление о типе трансляции хаба.
            Inform_of_Rec_Message rec = new Inform_of_Rec_Message(Path.GetFileName(flname));

            NW.Send(rec, Hubs[0]);
            using (NetworkStream stream = new NetworkStream(client))
            {
                // Отправка уведомления клиенту.
                NW.Send(response, stream);
                do
                {
                    mes = NW.Recieve(stream);
                    if (mes != null)
                    {
                        if (!(mes is EndMessage))
                        {
                            // Отправка сообщения хабу.
                            NW.Send(mes, Hubs[0]);

                            // Ожидание ответа.
                            do
                            {
                                response = NW.Recieve(Hubs[0]);
                            }
                            while (response.Get_Data() == string.Empty);

                            // Отправка уведомления клиенту.
                            NW.Send(response, stream);
                        }
                        else
                        {
                            mes = new EndMessage("End of file.");
                            NW.Send(mes, Hubs[0]);
                        }
                    }
                } while (!(mes is EndMessage));

                // Уведомление об окончании операции.
                Console.WriteLine("Transmisson is complete!");
            }
        }

        // Протокол отправки файла.
        public void Sending()
        {
            // Сообщение для коммуникации с клиентом.
            Message file_part = new FilePartMessage(string.Empty);
            // Сообщение для обработки подтверждения.
            Message response = new ResponseMessage("resp");
            // Уведомление о типе трансляции хаба.
            Inform_of_Down_Message down = new Inform_of_Down_Message(Path.GetFileName(flname));
            // Поток общения с хабом.
            NetworkStream Cur_Hub;
            Files.TryGetValue(flname, out Cur_Hub);

            NW.Send(down, Cur_Hub);

                using (NetworkStream stream = new NetworkStream(client))
                {
                    NW.Send(response, Cur_Hub);
                    do
                    {
                        mes = NW.Recieve(Cur_Hub);
                        if (mes != null)
                        {
                        if (!(mes is EndMessage))
                        {
                            // Отправка сообщения клиенту.
                            NW.Send(mes, stream);

                            // Ожидание ответа.
                            do
                            {
                                response = NW.Recieve(stream);
                            }
                            while (response.Get_Data() == string.Empty);

                            // Отправка уведомления хабу.
                            NW.Send(response, Cur_Hub);
                        }
                        else
                        {
                            mes = new EndMessage("End of file.");
                            NW.Send(mes, stream);
                        }
                        }
                    } while (!(mes is EndMessage));

                    // Уведомление об окончании операции.
                    Console.WriteLine("Downloading is complete!");
                }
                
        }
        
    }

    // Главное тело сервера. Обработка подключений.
    class Server
    {
        // Порт для подключения клиентов.
        static Int16 port = 13000;
        // Обработчик сообщений.
        static Message_Handler Mes_Hand = new Message_Handler();
        // Отвечает за отправку и получение сообщений.
        static NetWorking NW = new NetWorking();
        // Список файлов.
        static Dictionary<string, NetworkStream> Files = new Dictionary<string, NetworkStream>();
        // Хранилища.
        static List<NetworkStream> Hubs = new List<NetworkStream>();

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
                    Request_Handler RH = new Request_Handler(client,Files, Hubs);
                    new Thread(RH.Handle_Send).Start();
                    Console.WriteLine("Waiting for choosing the file...");
                    break;
                }
                else
                {
                    if (mess is Inform_of_Down_Message)
                    {
                        // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл в хранилище.
                        Request_Handler RH = new Request_Handler(client, Files,Hubs);
                        new Thread(RH.Handle_Recieve).Start();
                        Console.WriteLine("Waiting for downloading the file...");
                        break;
                    }
                    else
                    {
                        Hubs.Add(stream);
                        new Thread(List_Recieving).Start(stream);
                        Console.WriteLine("Recieving list of files...");
                        break;
                    }
                }
            }
        }

        // Получение списка.
        public static void List_Recieving(object income)
        {
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = (NetworkStream)income;
            // Запрос файлов.
            RequestMessage req = new RequestMessage("Send me");
            // Ответ.
            ResponseMessage res = new ResponseMessage("Response");
            // Сообщение полученное от клиента.
            Message LM;

            try
            {
                // Оправка уведомления о готовности.
                NW.Send(req, stream);

                // Получение списка файлов.
                do
                {
                    // Прочесть ответ хаба.
                    LM = NW.Recieve(stream);

                    // Проверить является ли ответ именем файла.
                    if (!(LM is EndMessage))
                    {
                        // Добавление файла в список.
                        Files.Add(LM.Get_Data(), stream);

                        // Отослать назад уведомление.
                        NW.Send(res, stream);
                    }
                } while (!(LM is EndMessage));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }

        // Протокол отправки сообщения.
        public static void Send_List(object income)
        {
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = (NetworkStream)income;
            // Сообщение полученное от клиента.
            Message M;

            foreach(var f in Files)
            {                
                // Создание сообщения.
                ListMessage LM = new ListMessage(f.Key);
                // Отправка сообщения.
                NW.Send(LM, stream);
                // Уведомление об успешной отправке.
                Console.WriteLine("Main thread: Sent info of: {0}", f.Key);

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
