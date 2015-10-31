using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace CPULoadClient
{
    public partial class Form1 : Form
    {

        public TcpClient client;
        public string ServiceCompName;  // адрес службы
        public int ServiceCompPort;     // порт сулжбы
        //  поток для записи
        public NetworkStream writeStream;
        //  поток для чтения
        public StreamReader readStream;

        private string AppPath; // путь к приложению
        public Form1()
        {
            InitializeComponent();
            ServiceCompPort = 3124;//Convert.ToInt32(3124);
            ServiceCompName = "127.0.0.1";

            AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient(ServiceCompName, ServiceCompPort);
                readStream = new StreamReader(client.GetStream());
                writeStream = client.GetStream();
                //формируем посылку
                string sendMessage = "200\r\n";
                sendMessage += dateTimePicker1.Value.ToString() + "#";
                sendMessage += dateTimePicker2.Value.ToString() + "\r\n";

                // отправляем
                byte[] dataWrite = Encoding.UTF8.GetBytes(sendMessage + "\r\n");
                writeStream.Write(dataWrite, 0, dataWrite.Length);

                // ловим ответ
                string answer = readStream.ReadLine();
                // если пришел
                if (answer.Length > 0)
                {
                    //WriteToLog(answer);
                    // разбираем
                    //if (answer.StartsWith("200"))
                    //    MessageBox.Show("Параметры сохранены");
                    //if (answer.StartsWith("404"))
                    //    MessageBox.Show("Файл не существует");
                    string[] paramsList = answer.Split('#');
                    listView1.Items.Clear();
                    if (paramsList[0].StartsWith("ans"))
                    {
                        foreach (string str in paramsList)
                        {
                            string[] valList = str.Split(';');
                            if (valList.Length > 2)
                            {
                                ListViewItem lvi = new ListViewItem(valList[0]);
                                lvi.SubItems.Add(valList[1]);
                                lvi.SubItems.Add(valList[2]);
                                listView1.Items.Add(lvi);
                            }
                        }
                    }

                }
                writeStream.Close();
                readStream.Close();
                client.Close();
                //WriteToLog("Подключились к " + ServiceCompName + ":" + ServiceCompPort);
            }
            catch (Exception exp)
            {
                //WriteToLog("Не удается подключиться к " + ServiceCompName + ":" + ServiceCompPort + exp.Message);
                //return false;
            }
            //return true;
        }
    }
}
