using API;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System;

namespace Client
{
    class ViewModel
    {
        private bool checkSend = false;
        private string text = ""; //Соответсвует значению текста переменной textView
        public EventHandler observer; //Наблюдатель для отправки уведомлений
        private int IdAction; //Id действия

        public string textGet() //Получение значения текста
        {
            return text;
        }

        private Socket Connecting() //Подключение к серверу
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            //IPAddress ipAddr = IPAddress.Parse("213.59.151.6");
            const int port = 8080;
            IPEndPoint iPEndPoint = new IPEndPoint(/*IPAddress.Parse(ip)*/ipAddr, port);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sender.Connect(iPEndPoint);
            }
            catch
            {
                MessageBox.Show("Ошибка соединения");
                MatrixDet.ActiveForm.Close();
            }
            return sender;
        }

        private void Disconnecting(Socket sender) //Отключение от сервера
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private string GetData(Socket sender) //Получение данных от сервера
        {
            var buffer = new byte[1024*1024];
            var size = 0;
            var answer = new StringBuilder();

            do
            {
                size = sender.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (sender.Available > 0);
            return answer.ToString();
        }

        public void MatrixSend(DataGridView dgv) //Отправка матрицы
        {
            try
            {
                IdAction = 0;
                double[][] a = new double[dgv.ColumnCount][];
                string matrix = "";

                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    a[i] = new double[dgv.ColumnCount];
                    for (int j = 0; j < dgv.ColumnCount; j++)
                    {
                        a[i][j] = double.Parse(dgv.Rows[i].Cells[j].Value.ToString());
                        matrix += " " + a[i][j].ToString(); //Запись элементов матрицы через пробел в строку для отправки
                    }
                }

                Socket sender = Connecting();

                var data = Encoding.UTF8.GetBytes(IdAction.ToString()+$"{dgv.ColumnCount}{matrix}");
                sender.Send(data);

                string answer = GetData(sender);
                text = answer;
                checkSend = true;

                Disconnecting(sender);
            }
            catch
            {
                text = "Ошибка";
            }

            observer.Invoke(this, null);
            
        }

        public void StatusRecieve() //Получение статуса задачи
        {
            IdAction = 1;
            Socket sender = Connecting();

            var data = Encoding.UTF8.GetBytes(IdAction.ToString());
            sender.Send(data);

            string answer = GetData(sender);
            text = answer;

            Disconnecting(sender);

            observer.Invoke(this, null);
        }

        public void ResultRecieve() //Получение результата
        {
            if (checkSend)
            {
                IdAction = 2;
                Socket sender = Connecting();

                var data = Encoding.UTF8.GetBytes(IdAction.ToString());
                sender.Send(data);

                string answer = GetData(sender);
                text = answer;

                Disconnecting(sender);
            }
            else
                text = "Ошибка";

            observer.Invoke(this, null);
        }

        public void TaskCancel() //Отмена задачи
        {
            IdAction = 3;
            Socket sender = Connecting();

            var data = Encoding.UTF8.GetBytes(IdAction.ToString());
            sender.Send(data);

            string answer = GetData(sender);
            text = answer;
            checkSend = false;

            Disconnecting(sender);

            observer.Invoke(this, null);
        }
    }
}