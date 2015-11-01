//
// Клиент для работы со службой сбора данных по процессам
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace CPULoadClient
{
    public partial class Form1 : Form
    {

        public TcpClient client;
        public string ServiceCompName;      // адрес службы
        public int ServiceCompPort;         // порт сулжбы
        public NetworkStream writeStream;   //  поток для записи
        public StreamReader readStream;     //  поток для чтения

        public Form1()
        {
            InitializeComponent();
            // читаем номер порта для запуска сокета  
            try
            {
                ServiceCompPort = Convert.ToInt32(ConfigurationManager.AppSettings["CPULoadServicePort"]);
            }
            catch { ServiceCompPort = 3125; }
            ServiceCompName = "127.0.0.1"; // локальный компьютер
            
        }

        /// <summary>
        /// Получение данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                client = new TcpClient(ServiceCompName, ServiceCompPort);
                readStream = new StreamReader(client.GetStream());
                writeStream = client.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к службе: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                toolStripStatusLabel1.Text = "Не удалось подключиться к службе";
                return;
            }
            try
            {
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
                    else
                    {
                        MessageBox.Show("Ошибка запроса данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        toolStripStatusLabel1.Text = "Ошибка запроса данных";
                    }

                }

            }
            catch (Exception exp)
            {
                MessageBox.Show("Не удается подключиться к " + ServiceCompName + ":" + ServiceCompPort + exp.Message);
            }
            toolStripStatusLabel1.Text = "Время обновления " + DateTime.Now.ToShortTimeString();
            writeStream.Close();
            readStream.Close();
            client.Close();

            Cursor = Cursors.Default;
        }
    }
}
