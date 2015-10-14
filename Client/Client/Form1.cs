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

namespace Client
{
    public partial class Form1 : Form
    {
        string act;
        public static string adress = "192.168.7.101";
        private string folder = @"D:\DFS_Client";
        public static TcpClient clnt;
        public Form1()
        {
            InitializeComponent();
        }

        
        public static void StartClient(ListView a)
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];
            // Проверка.
            string checker = "";
            Int32 port = 13000;
            clnt = new TcpClient(adress, port);

            Byte[] data;

            NetworkStream stream = clnt.GetStream();

            try
            {
                while (checker != "End of list")
                {

                    // Буффер дл получения ответа.
                    data = new Byte[256];

                    // Сткрока для храниения ASCII-варианта ответа.
                    String responseData = String.Empty;

                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes_1);
                    if (responseData != "End of list")
                    {
                        a.Items.Add(responseData);

                        // Декодирование данных.
                        responseData = responseData.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(responseData);

                        // Отослать назад уведомление.
                        stream.Write(msg, 0, msg.Length);
                    }
                    checker = responseData;

                }
                // Закрыть все.
                stream.Close();
                clnt.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            StartClient(listView1);
            button1.Visible = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

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

            label1.Text = "Download this file: " + act + "?";
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
            File_Reciever FR = new File_Reciever(15000);
            FR.Receiving(clnt, act);
        }
    }

    class File_Sender
    { }
    class File_Reciever
    {
        int port;
        string name_of_file;
        public File_Reciever(int p)
        {
            port = p;
        }

        public void Receiving(TcpClient cl ,string s)
        {

            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];
            // Объявление переменных.
            string checker = "";
            const int buffersz = 16384;
            byte[] buffer = new byte[buffersz];
            int btscpd = 0;
            byte[] pack = new byte[1024];

            // Выделение сервера
            IPAddress ServAddr = IPAddress.Parse(Form1.adress);
            Int32 port = 13000;
            TcpClient clnt = new TcpClient(ServAddr.ToString(), port);

            string path = Path.Combine(@"D:\DFS_Client", s);


            using (FileStream outFile = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (NetworkStream stream = clnt.GetStream())
            {

                do
                {
                    // Буффер дл получения ответа.
                    pack = new Byte[256];

                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(pack, 0, pack.Length);
                    checker = Encoding.ASCII.GetString(pack, 0, bytes_1);
                    if (checker != "End of file")
                    {
                        outFile.Write(pack, 0, bytes_1);
                    }

                } while (checker!= "End of file") ;
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
