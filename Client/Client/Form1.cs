using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Messages;
using System.Threading;

namespace Client
{
    // Главное тело клиента.
    public partial class Form1 : Form
    {
        // Текущий выбранный файл.
        string act;
        // IP сервера.
        public static string adress = "192.168.7.105";
        // Тср-клиент для сервера.
        public static TcpClient clnt = new TcpClient(adress, 13000);
        // Директория файлов клиента.
        static private string Folder = @"D:\DFS_Client";
        // Маркер для определения типа запросов.
        static public bool receiving;
        // Обработчик сообщений.
        static Message_Handler M = new Message_Handler();        

        public Form1()
        {
            InitializeComponent();
        }

        // Режим скачивания файла.
        public static void StartClient(ListView a)
        {
            // Выделение переменных.
            byte[] bytes = new Byte[1024];
            // Строка для хранения входящих сообщений.
            string message = string.Empty;
            // Буффер дл получения ответа.
            Byte[] data = new Byte[256];
            // Сткрока для храниения ASCII-варианта ответа.
            String responseData = String.Empty;
            // Выделение потока для получения списка файлов.
            NetworkStream stream = clnt.GetStream();
            // Сообщение о типе клиента.
            Inform_of_Rec_Message mes = new Inform_of_Rec_Message(string.Empty);
            // Сообщение о типе клиента.
            ListMessage LM = new ListMessage(string.Empty);

            try
            {
                // Оправка уведомления о типе клиента.
                stream.Write(M.Encrypt(mes), 0, M.Encrypt(mes).Length);

                // Получение списка файлов.
                while (LM.Get_Data() != "End of list")
                {
                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    LM = Recieve_L_mes(stream);

                    // Проверить является ли ответ именем файла.
                    if (LM.Get_Data() != "End of list")
                    {
                        // Добавление файла в список.
                        a.Items.Add(LM.Get_Data());

                        // Отослать назад уведомление.
                        stream.Write(M.Encrypt(LM), 0, M.Encrypt(LM).Length);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }

        // Режим отправки файла.
        public static void StartUploadClient(ListView a)
        {
            // Выделение переменных.
            byte[] bytes = new Byte[1024];
            // Выделение потока для получения списка файлов.
            NetworkStream stream = clnt.GetStream();
            // Сообщение о типе клиента.
            Inform_of_Down_Message mes = new Inform_of_Down_Message(string.Empty);
            // Сообщение о типе клиента.
            ListMessage LM = new ListMessage(string.Empty);
            
            // Оправка уведомления о типе клиента.
            stream.Write(M.Encrypt(mes), 0, M.Encrypt(mes).Length);

            // Представление файлов клиента в виде списка.
            a.Items.Clear();
            string[] dirs = Directory.GetFiles(Folder);
            foreach (string dir in dirs)
            {
                ListViewItem item1 = new ListViewItem(Path.GetFileName(dir), 0);
                a.Items.AddRange(new ListViewItem[] { item1 });
            }
        }

        // Обработка входящего сообщения
        public static Messages.ListMessage Recieve_L_mes(NetworkStream stream)
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];

            // Возвращаемое сообщение.
            ListMessage mess = new ListMessage(string.Empty);

            // Проверка получения ответа.
            while (mess.Get_Data() == string.Empty)
            {
                // Считвание ответа.
                Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                mess = (ListMessage)M.Decrypt(bytes);
            }
            return mess;
        }

        // Перевод клиента в режим скачивания файлов с сервера.
        private void button1_Click(object sender, EventArgs e)
        {
            StartClient(listView1);
            panel1.Visible = false;
            receiving = true;
        }

        // Инициализация формы.
        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            // Set the view to show details.
            listView1.View = View.Details;
            // Allow the user to edit item text.
            listView1.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView1.AllowColumnReorder = true;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            // Sort the items in the list in ascending order.
            listView1.Sorting = SortOrder.Ascending;

            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            listView1.Columns.Add("Name", 1000, HorizontalAlignment.Left);
        }

        // Выбор файла и запрос подтверждения.
        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            act = listView1.SelectedItems[0].SubItems[0].Text.ToString();

            if (receiving)
                label1.Text = "Download this file: " + act + "?";
            else
                label1.Text = "Upload this file: " + act + "?";

            groupBox1.Visible = true;
        }

        // Отмена выбора.
        private void button3_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            label1.Text = "";
            act = "";
        }

        // Подтверждение выбора.
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            label1.Text = "";
            if (receiving)
            {
                // Запуск протокола скачивания файла.
                File_Reciever FR = new File_Reciever(clnt);
                FR.Receiving(act);
            }
            else
            {
                // Запуск протокола отправки файла.
                File_Sender FS = new File_Sender(clnt);
                FS.Send_File(act);
            }
        }

        // Перевод клиента в режим загрузки файлов на сервер.
        private void button4_Click(object sender, EventArgs e)
        {
            StartUploadClient(listView1);
            panel1.Visible = false;
            receiving = false;
        }
    }

    //Часть клиента, ответственная за отправку файлов.
    class File_Sender
    {
        // Путь к файлам клиента.
        string cl_path = @"D:\DFS_Client";
        // Тср-клиент для связи с сервером.
        TcpClient clnt;
        // Буффер для отправки данных на сервер.
        byte[] bytes = new byte[1024];
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();

        public File_Sender(TcpClient c)
        {
            clnt = c;
        }

        // Отправка сообщения.
        public void Send_Name(string s, NetworkStream stream)
        {
            // Создание сообщения содержащего имя файла.
            Messages.RequestMessage name = new RequestMessage(s);

            // Отправка части файла.
            stream.Write(Mes_Hand.Encrypt(name), 0, Mes_Hand.Encrypt(name).Length);
        }

        // Получение ответа.
        public Messages.Message Recieve_resp(NetworkStream stream)
        {
            // Сообщение для хранения ответа.
            Messages.Message mess = new RequestMessage(string.Empty);

            // Проверка получения ответа.
            while (mess.Get_Data() == string.Empty)
            {
                // Считвание ответа.
                Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                mess = Mes_Hand.Decrypt(bytes);

            }
            return mess;
        }

        // Отправка файла.
        public void Send_File(string s)
        {
            // Сообщение содержащее часть файла.
            FilePartMessage mes;
            // Сообщение содержащее подтверждение получения.
            Messages.Message resp;
            // Размер буффера считываемых данных.
            const int buffersz = 16384;
            // Буффер для частей файла.
            byte[] buffer = new byte[buffersz];
            // Размер считанных данных.
            int btscpd = 0;

            // Выделение пути к запрашиваемому файлу.
            string path = Path.Combine(cl_path, s);

            // Проверка существования файла.
            if (File.Exists(path))
            {
                // Выделение потоков для отправки файла.
                using (FileStream inFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (NetworkStream stream = clnt.GetStream())
                {
                    // Отправка имени файла.
                    Send_Name(s, stream);

                    // Ожидание ответа.
                    Recieve_resp(stream);

                    do
                    {
                        // Считывание данных с файла.
                        btscpd = inFile.Read(buffer, 0, buffersz);                        
                        // Проверка на пустоту считанных данных.
                        if (btscpd > 0)
                        {
                            mes = (FilePartMessage)Mes_Hand.Decrypt(buffer);
                            // Отправка пакета.
                            stream.Write(Mes_Hand.Encrypt(mes), 0, Mes_Hand.Encrypt(mes).Length);

                            // Получение подтверждения.
                            while (true)
                            {
                                resp = Recieve_resp(stream);

                                if (resp.Get_Data() != "")
                                {
                                    resp = new Messages.Message(string.Empty);
                                    break;
                                }
                            }
                        }
                    } while (btscpd > 0);

                    // Для устранения ошибки преждевременного обрывания.
                    Thread.Sleep(50);

                    // Отправка уведомления о конце файла.
                    mes = new FilePartMessage("End of file");
                    stream.Write(Mes_Hand.Encrypt(mes), 0, Mes_Hand.Encrypt(mes).Length);
                }

            }
            else
            {
                using (NetworkStream stream = clnt.GetStream())
                {
                    // !!!Отправить серверу сообщение об отсутствии файла!!!
                    buffer = Encoding.ASCII.GetBytes("File is not exsiting");
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }

    //Часть клиента, ответственная за скачивание файлов.
    class File_Reciever
    {
        // Тср-клиент для коммуникации с сервером.
        TcpClient clnt;
        // Путь к файлам клиента.
        string cl_path = @"D:\DFS_Client";
        // Буффер входящих данных.
        byte[] bytes = new Byte[1024];
        // Обработчик сообщений.
        Message_Handler Mes_Hand = new Message_Handler();

        public File_Reciever(TcpClient c)
        {
            clnt = c;
        }

        // Отправка сообщения.
        public void Send_Name(string s, NetworkStream stream)
        {
            // Создание сообщения содержащего имя файла.
            Messages.RequestMessage name = new RequestMessage(s);

            // Отправка части файла.
            stream.Write(Mes_Hand.Encrypt(name), 0, Mes_Hand.Encrypt(name).Length);
        } 

        // Получение ответа.
        public Messages.Message Recieve_resp(NetworkStream stream)
        {
            // Возвращаемое сообщение.
            Messages.Message mess = new Messages.Message(string.Empty);

            // Проверка получения ответа.
            while (mess.Get_Data() == string.Empty)
            {
                // Считвание ответа.
                Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                mess = Mes_Hand.Decrypt(bytes);
            }
            return mess;
        }

        // Протокол получения файла.
        public void Receiving(string s)
        {
            // Обработанное входящее сообщение.
            Messages.Message mess;
            // Обработанное входящее сообщение.
            Messages.ResponseMessage response = new ResponseMessage(string.Empty);
            // Буффер для получения ответа.
            byte[] pack = new byte[1024];

            // Выделение пути к файлу.
            string path = Path.Combine(cl_path, s);
            
            // Выделение потоков для получения файла.
            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = clnt.GetStream())
            {
                // Отправка имени.
                Send_Name(s, stream);
                do
                {
                    // Получение части файла.
                    mess = Recieve_resp(stream);

                    // Проверка на завершение скачивания.
                    if (mess.Get_Data() != "End of file")
                    {
                        // Запись в файл.
                        outFile.Write(pack, 0, pack.Length);
                        // Отправка уведомления.
                        stream.Write(Mes_Hand.Encrypt(response), 0, Mes_Hand.Encrypt(response).Length);
                    }

                } while (mess.Get_Data()!= "End of file");

                // Отчет об успешном скачивании.
                MessageBox.Show("Downloading is complete!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
