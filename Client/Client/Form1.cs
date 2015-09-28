using System;
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

namespace Client
{
    public partial class Form1 : Form
    {
        private string folder = @"D:\DFS_Client";
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

            IPAddress ServAddr = IPAddress.Parse("192.168.7.101");
            Int32 port = 13000;
            TcpClient clnt = new TcpClient(ServAddr.ToString(), port);

            Byte[] data = Encoding.ASCII.GetBytes("Hello!");

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
                    }
                    checker = responseData;


                    // Декодирование данных.
                    responseData = responseData.ToUpper();

                    byte[] msg = Encoding.ASCII.GetBytes(responseData);

                    // Отослать назад уведомление.
                    stream.Write(msg, 0, msg.Length);
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
            string act = listView1.SelectedItems[0].SubItems[0].Text.ToString();

            try
            {
                File_Reciever FR = new File_Reciever(14000);
                FR.Receiving(folder);
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong", "Warning");
            }
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

        public void Receiving(string s)
        {
            name_of_file = s;
        }
    }
}
