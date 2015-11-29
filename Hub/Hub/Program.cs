using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Messages;

namespace Hub
{
    // Основное тело хаба. Отвечает за запросы.
    class Program
    {
        // IP сервера.
        public static string adress = "192.168.7.103";
        // Тср-клиент для сервера.
        public static TcpClient clnt = new TcpClient(adress, 13000);
        // Директория файлов клиента.
        static private string Folder = @"D:\DFS";
        // Обработчик сообщений.
        static Message_Handler M = new Message_Handler();
        // Отвечает за отправку/получение сообщений.
        static NetWorking NW = new NetWorking();


        // Подключение сервера.
        public static void Start_Hub()
        {
            // Буффер обмена.
            byte[] bytes = new Byte[1024];
            // Буффер дл получения ответа.
            Byte[] data = new Byte[256];
            // Выделение потока для получения списка файлов.
            NetworkStream stream = clnt.GetStream();
            // Сообщение о типе клиента.
            Messages.ListMessage mes = new ListMessage("Hub");
            // Сообщение c именем файла из списка.
            Message LM;

            try
            {
                NW.Send(mes, stream);
                // Прочесть ответ сервера.
                LM = NW.Recieve(stream);

                // Проверить является ли ответ именем файла.
                if (LM is RequestMessage)
                {
                    Send_List(stream);
                    Start_Working(stream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }

        // Отправка списка файлов.
        public static void Send_List(object income)
        {
            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            // Строка содержащая имя файла.
            string file_name;
            // Выделение потока для обмена информацией с клиентом.
            NetworkStream stream = (NetworkStream)income;
            // Определение файлов в директории.
            string[] dirs = Directory.GetFiles(Folder);
            // Сообщение полученное от клиента.
            Message M;

            // Отправка информации о каждом файле.
            for (int q = 0; q < dirs.Length; q++)
            {
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

        // Обработка запросов.
        public static void Start_Working(NetworkStream stream)
        {
            // Выделение экземпляра класса Message для распознавания ответа.
            Message mess;

            while (true)
            {
                mess = NW.Recieve(stream);

                // Проверка типа сообщния.
                if (mess is Inform_of_Rec_Message)
                {
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл из хранилища.
                    Translator TR = new Translator(stream, mess.Get_Data());
                    new Thread(TR.Recieve).Start();
                    Console.WriteLine("Waiting for choosing the file...");
                    break;
                }
                else
                {
                    // Выделение потока для обработки запросов с указанием того, что клиент хочет загрузить файл в хранилище.
                    Translator TR = new Translator(stream, mess.Get_Data());
                    new Thread(TR.Recieve).Start();
                    Console.WriteLine("Waiting for downloading the file...");
                    break;
                }
            }
        }

        // Инициализация хаба.
        static void Main(string[] args)
        {
            Start_Hub();
        }
    }

    // РЕализация запросов.
    public class Translator
    {
        // Директория файлов клиента.
        static private string Folder = @"D:\DFS";
        // Обработчик сообщений.
        static Message_Handler M = new Message_Handler();
        // Отвечает за отправку/получение сообщений.
        static NetWorking NW = new NetWorking();
        // Поток для общения с сервером.
        NetworkStream stream;
        // Сообщение для коммуникации с клиентом.
        Message mes;
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();
        string name;

        public Translator(NetworkStream ns, string s)
        {
            stream = ns;
            name = s;
        }

        // Отправка файла.
        public void Send()
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
            string path = Path.Combine(@"D:\DFS", name);

            // Провека существования запрашиваемого файла.
            if (File.Exists(path))
            {
                // Начало передачи.
                using (FileStream inFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
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

                                if (response.Get_Data() != string.Empty)
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
        }

        // Получение файла.
        public void Recieve()
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
            string path = Path.Combine(@"D:\DFS", name);

            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                NW.Send(response, stream);
                do
                {
                    mes = NW.Recieve(stream);
                    if (mes != null)
                    {
                        if (!(mes is EndMessage))
                        {
                            // Запись в файл.
                            outFile.Write(mes.Get_Info(), 0, mes.Get_Info().Length);

                            // Отправка уведомления клиенту.
                            NW.Send(response, stream);
                        }
                    }
                } while (!(mes is EndMessage));

                // Уведомление об окончании операции.
                Console.WriteLine("Downloading is complete!");
            }
        }
    }

    // Отвечает за передачу сообщений.
    public class NetWorking
    {
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();

        // Отправка сообщения.
        public void Send(Messages.Message m, NetworkStream ns)
        {
            // Отправка сообщения.
            ns.Write(Mes_Hand.Encrypt(m), 0, Mes_Hand.Encrypt(m).Length);
        }

        // Пoлучение сообщения.
        public Messages.Message Recieve(NetworkStream ns)
        {
            // Буффер для получения ответа.
            Byte[] data = new Byte[1024];
            // Полученное сообщение.
            Messages.Message mes;

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
