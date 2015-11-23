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
        // Отвечает за отправку/получение сообщений.
        static NetWorking NW = new NetWorking();

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
            Inform_of_Rec_Message mes = new Inform_of_Rec_Message("Rec");
            // Сообщение c именем файла из списка.
            Messages.Message LM = new ListMessage("Rec");

            try
            {
                // Оправка уведомления о типе клиента.
                NW.Send(mes, stream);

                // Получение списка файлов.
                while (!(LM is EndMessage))
                {
                    // Прочесть ответ сервера.
                    LM = NW.Recieve(stream);

                    // Проверить является ли ответ именем файла.
                    if (!(LM is EndMessage))
                    {
                        // Добавление файла в список.
                        a.Items.Add(LM.Get_Data());

                        // Отослать назад уведомление.
                        NW.Send(LM, stream);
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
            Inform_of_Down_Message mes = new Inform_of_Down_Message("Down");
            
            // Оправка уведомления о типе клиента.
            NW.Send(mes, stream);

            // Представление файлов клиента в виде списка.
            a.Items.Clear();
            string[] dirs = Directory.GetFiles(Folder);
            foreach (string dir in dirs)
            {
                ListViewItem item1 = new ListViewItem(Path.GetFileName(dir), 0);
                a.Items.AddRange(new ListViewItem[] { item1 });
            }
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

    // Часть клиента, ответственная за отправку файлов.
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
        // Отвечает за отправку/получение сообщений.
        NetWorking NW = new NetWorking();

        public File_Sender(TcpClient c)
        {
            clnt = c;
        }
        
        // Отправка файла.
        public void Send_File(string s)
        {
            // Сообщение содержащее часть файла.
            Messages.Message mes;
            // Сообщение содержащее имя файла.
            RequestMessage name = new RequestMessage(s);
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
                    NW.Send(name, stream);

                    // Ожидание ответа.
                    do
                    {
                        resp = NW.Recieve(stream);
                    }
                    while (resp.Get_Data() == string.Empty);

                    do
                    {
                        // Считывание данных с файла.
                        btscpd = inFile.Read(buffer, 0, buffersz);                        
                        // Проверка на пустоту считанных данных.
                        if (btscpd > 0)
                        {
                            mes = new FilePartMessage(Encoding.ASCII.GetString(buffer));
                            // Отправка пакета.
                            NW.Send(mes, stream);

                            // Получение подтверждения.
                            while (true)
                            {
                                resp = NW.Recieve(stream);

                                if (resp.Get_Data() != "")
                                {
                                    resp = new ResponseMessage(string.Empty);
                                    break;
                                }
                            }
                        }
                    } while (btscpd > 0);

                    // Для устранения ошибки преждевременного обрывания.
                    Thread.Sleep(50);

                    // Отправка уведомления о конце файла.
                    mes = new EndMessage("End of file");
                    NW.Send(mes, stream);
                }

            }
            else
            {
                using (NetworkStream stream = clnt.GetStream())
                {
                    mes = new ErrorMessage("File is not exsiting");
                    NW.Send(mes, stream);
                }
            }
        }
    }

    // Часть клиента, ответственная за скачивание файлов.
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
        // Отвечает за получени и отправку собщенийю
        NetWorking NW = new NetWorking();

        public File_Reciever(TcpClient c)
        {
            clnt = c;
        }
        
        // Протокол получения файла.
        public void Receiving(string s)
        {
            // Обработанное входящее сообщение.
            Messages.Message mess = new FilePartMessage(string.Empty);
            // Сообщение содержащее имя.
            RequestMessage name;
            // Уведомительное сообщение.
            ResponseMessage response = new ResponseMessage("Response");
            // Буффер для получения ответа.
            byte[] pack = new byte[1024];

            // Выделение пути к файлу.
            string path = Path.Combine(cl_path, s);
            // Выделение потоков для получения файла.
            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = clnt.GetStream())
            {
                name = new RequestMessage(s);
                // Отправка имени.
                NW.Send(name, stream);
                do
                {
                    // Получение части файла.
                    mess = NW.Recieve(stream);

                    // Проверка на завершение скачивания.
                    if (!(mess is EndMessage))
                    {
                        // Запись в файл.
                        outFile.Write(mess.Get_Info(), 0, mess.Get_Info().Length);
                        // Отправка уведомления.
                        NW.Send(response, stream);
                    }

                } while (!(mess is EndMessage));

                // Отчет об успешном скачивании.
                MessageBox.Show("Downloading is complete!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
