using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Server_
{
    class Model
    {
        private string status = "Не задана";
        private int n = 0; //Размерность матрицы
        private double[][] matrix; //Матрица
        public string StatusGet()
        {
            return status;
        }
        public void Cancel() //Отмена Задачи
        {
            n = 0;
            matrix = new double[n][];
            status = "Не задана";
        }
        public void MatrixFill(string[] data) //Заполнение матрицы
        {
            status = "Не решена";
            n = int.Parse(data[0]);
            int k = 1;
            matrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    matrix[i][j] = double.Parse(data[k]);
                    k++;
                }
            }
        }
        public string Det() //Метод Гаусса для вычисления определителя матрицы
        {
            double det = 1;
            double[][] a = new double[n][];
            for (int i = 0; i < n; i++)
            {
                a[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    a[i][j] = matrix[i][j];
                }
            }
            double[][] b = new double[1][];
            b[0] = new double[n];
            for (int i = 0; i < n; ++i)
            {
                int k = i;
                for (int j = i + 1; j < n; ++j)
                    if (Math.Abs(a[j][i]) > Math.Abs(a[k][i]))
                        k = j;
                if (Math.Abs(a[k][i]) < 1E-9)
                {
                    det = 0;
                    break;
                }
                b[0] = a[i];
                a[i] = a[k];
                a[k] = b[0];
                if (i != k)
                    det = -det;
                det *= a[i][i];
                for (int j = i + 1; j < n; ++j)
                    a[i][j] /= a[i][i];
                for (int j = 0; j < n; ++j)
                    if ((j != i) && (Math.Abs(a[j][i]) > 1E-9))
                        for (k = i + 1; k < n; ++k)
                            a[j][k] -= a[i][k] * a[j][i];
            }
            status = "Решена";

            return Math.Round(det, 3).ToString();
        }
    }

    class Server
    {
        static void Main(string[] args)
        {
            Model model = new Model();

            //Запуск сервера
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            const int port = 8080;
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddr, port);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Bind(iPEndPoint);
            sender.Listen(2); //Максимальное количество одновременных пользователей
            while (true) //Поток
            {
                var accepter = sender.Accept();
                var buffer = new byte[1024*1024];
                var size = 0;
                var data = new StringBuilder();

                do //Получение даннных
                {
                    size = accepter.Receive(buffer);
                    data.Append(Encoding.UTF8.GetString(buffer, 0, size));
                }
                while (accepter.Available > 0);

                string id = data[0].ToString(); //Id действия

                switch (id)
                {
                    case "0": //Получение задачи
                        model.MatrixFill(data.ToString().Split(' ')); //Разделение размерности и элементов матрицы
                        accepter.Send(Encoding.UTF8.GetBytes("Матрица получена"));
                        break;

                    case "1": //Определение текущего статуса задачи                   
                        accepter.Send(Encoding.UTF8.GetBytes(model.StatusGet()));
                        break;

                    case "2": //Вычисление и отправка результа
                        accepter.Send(Encoding.UTF8.GetBytes(model.Det()));
                        break;

                    case "3": //Отмена задачи
                        model.Cancel();
                        accepter.Send(Encoding.UTF8.GetBytes("Задача отменена"));
                        break;
                }
                accepter.Shutdown(SocketShutdown.Both);
                accepter.Close();
            }
        }
    }
}