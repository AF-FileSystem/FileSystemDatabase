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
    public partial class Form1 : Form
    {
        string act;
        public static string adress = "192.168.7.102";
        static Int32 port13 = 13000;
        public static TcpClient clnt;
        static private string Folder = @"D:\DFS_Client";
        static public bool receiving;

        public Form1()
        {
            InitializeComponent();
        }
        
        public static void StartClient(ListView a)
        {
            // Выделение переменных.
            byte[] bytes = new Byte[1024];            
            string checker = "";
            clnt = new TcpClient(adress, port13);
            Byte[] data;

            // Выделение потока для получения списка файлов.
            NetworkStream stream = clnt.GetStream();

            try
            {
                // Буффер дл получения ответа.
                data = new Byte[256];

                // Сткрока для храниения ASCII-варианта ответа.
                String responseData = String.Empty;


                bytes = Encoding.ASCII.GetBytes("1LST");
                stream.Write(bytes, 0, bytes.Length);
                while (checker != "End of list")
                {
                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes_1);
                    if (responseData != "End of list")
                    {
                        // Добавление файла в список.
                        a.Items.Add(responseData);

                        // Отослать назад уведомление.
                        byte[] msg = Encoding.ASCII.GetBytes(responseData);
                        stream.Write(msg, 0, msg.Length);
                    }
                    checker = responseData;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }

        public static void StartUploadClient(ListView a)
        {
                // Выделение переменных.
                byte[] bytes = new Byte[1024];
                clnt = new TcpClient(adress, port13);

                // Выделение потока для получения списка файлов.
                NetworkStream stream = clnt.GetStream();

            bytes = Encoding.ASCII.GetBytes("0LST");
            stream.Write(bytes, 0, bytes.Length);

            a.Items.Clear();
            string[] dirs = Directory.GetFiles(Folder);
            foreach (string dir in dirs)
            {
                ListViewItem item1 = new ListViewItem(Path.GetFileName(dir), 0);
                a.Items.AddRange(new ListViewItem[] { item1 });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartClient(listView1);
            panel1.Visible = false;
            receiving = true;
        }

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
            listView1.Columns.Add("Name", 130, HorizontalAlignment.Left);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            act = listView1.SelectedItems[0].SubItems[0].Text.ToString();

            if (receiving)
                label1.Text = "Download this file: " + act + "?";
            else
                label1.Text = "Upload this file: " + act + "?";

            groupBox1.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            label1.Text = "";
            act = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            label1.Text = "";
            if (receiving)
            {
                File_Reciever FR = new File_Reciever();
                FR.Receiving(act);
            }
            else
            {
                File_Sender FS = new File_Sender();
                FS.Send(act);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartUploadClient(listView1);
            panel1.Visible = false;
            receiving = false;
        }
    }

    class File_Sender
    {
        static Int32 port15 = 15000;

        public File_Sender()
        { 
        }

        public void Send(string s)
        {
            // Объявление переменных.
            string checkr = "";
            byte[] bytes = new byte[1024];
            byte[] filenames = new byte[1024];

            // Выделение пути к запрашиваемому файлу.
            string path = Path.Combine(@"D:\DFS_Client", s);

            IPAddress ServAddr = IPAddress.Parse(Form1.adress);
            TcpClient clnt = new TcpClient(ServAddr.ToString(), port15);
            // Отправка файла
            if (File.Exists(path))
            {
                // Выделение констант.
                const int buffersz = 16384;
                byte[] buffer = new byte[buffersz];
                int btscpd = 0;

                // Начало передачи.
                using (FileStream inFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (NetworkStream stream = clnt.GetStream())
                {
                    bytes = Encoding.ASCII.GetBytes("3"+s);
                    stream.Write(bytes, 0, bytes.Length);
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

                    Thread.Sleep(50);
                    // Отправка уведомления о конце файла.
                    filenames = Encoding.ASCII.GetBytes("End of file");
                    stream.Write(filenames, 0, filenames.Length);
                }

            }
        }
    }
    class File_Reciever
    {
        static Int32 port15 = 15000;
        public File_Reciever()
        {
        }

        public void Receiving(string s)
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

            // Выделение сервера
            IPAddress ServAddr = IPAddress.Parse(Form1.adress);
            TcpClient clnt = new TcpClient(ServAddr.ToString(), port15);


            // Выделение пути к файлу.
            string path = Path.Combine(@"D:\DFS_Client", s);
            
            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = clnt.GetStream())
            {
                filename = Encoding.ASCII.GetBytes(s);
                stream.Write(filename, 0, filename.Length);
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
                } while (checker!= "End of file");
                MessageBox.Show("Downloading is complete!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    class Hasher
    {
        public Hasher()
        { }
        public Int32 HashMe(byte[] a)
        {
            Int32 ans = 0;
            return ans;
        }
    }
}
