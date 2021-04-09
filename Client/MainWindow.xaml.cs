using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private IPEndPoint endPoint;
        private IPAddress address;
        private string userName;

        public MainWindow()
        {
            InitializeComponent();
            StartView();
        }

        /// <summary>
        /// настраивает все елементы формы для начала роботы
        /// </summary>
        private void StartView()
        {
            GameList.IsEnabled = false;
            Ratings.IsEnabled = false;

            Games1.Items.Clear();
            Games1.Items.Add("Все игры");
            Games1.Items.Add("Шашки");
            Games1.Items.Add("Крестики Нолики");

            Criteria.Items.Clear();
            Criteria.Items.Add("Все мои игры");
            Criteria.Items.Add("Все мои игры в крестики нолики");
            Criteria.Items.Add("Все мои игры в шашки");
            Criteria.Items.Add("Все победы во всех играх");
            Criteria.Items.Add("Все проиграшы во всех играх");
            Criteria.Items.Add("Все победы в шашках");
            Criteria.Items.Add("Все победы в крестиках ноликах");
            Criteria.Items.Add("Все проиграшы в шашках");
            Criteria.Items.Add("Все проиграшы в крестиках ноликах");
        }

        /// <summary>
        /// отправка сообщений на сервер
        /// </summary>
        /// <param name="message">сообщение каторое нужно отправить на сервер</param>
        private void Send(string message)
        {
            NetworkStream network = client.GetStream();
            byte[] buf = Encoding.Unicode.GetBytes(message);
            network.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// иницыалезация нового TcpClient и установка подключения к серверу
        /// </summary>
        private void InitClient()
        {
            if (address == null)
                address = IPAddress.Parse("127.0.0.1");
            if (endPoint == null)
                endPoint = new IPEndPoint(address, 8081);
            if (client == null)
                client = new TcpClient();
            client.Connect(endPoint);
            new Thread(new ThreadStart(ReceivingMessages))
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Анализирует даные получиные от сервера
        /// </summary>
        private void ReceivingMessages()
        {
            while (true)
                lock (client)
                {
                    string s = GetMessage(client);
                    if (s == "Вход выполнен")
                        Dispatcher.Invoke(new Action(() => { MessageBox.Show(s); GameList.IsEnabled = Ratings.IsEnabled = true; }));
                    else if (s.Contains("побед") || s.Contains("против"))
                        Dispatcher.Invoke(new Action(() => { RatingRezault.Text = s; }));
                    else
                        MessageBox.Show(s);
                }
        }

        /// <summary>
        /// Получение даных из TcpClient и их конвертация в строку
        /// </summary>
        /// <param name="client">клиент у каторого мы получаем данные</param>
        /// <returns>Конвертированая строка</returns>
        private string GetMessage(TcpClient client)
        {
            NetworkStream network = client.GetStream();
            byte[] bytes = new byte[1024];
            network.Read(bytes, 0, bytes.Length);
            return Encoding.Unicode.GetString(bytes).Replace("\0", "");
        }

        private void StartGameXO_Click(object sender, RoutedEventArgs e)
        {
            GameXO gameXO;
            if (client == null)
                gameXO = new GameXO(userName);
            else
                gameXO = new GameXO(client, endPoint, address, userName);
            gameXO.ShowDialog();
        }

        private void SaveToSystem_Click(object sender, RoutedEventArgs e)
        {
            InitClient();
            Send($"DB|newUser|{RegistrationLogin.Text}|{RegistrationPassword.Text}");
            RegistrationLogin.Text = RegistrationPassword.Text = ConfirmThePassword.Text = "";
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            InitClient();
            Send($"DB|Enter|{EnterLogin.Text}|{EnterPassword.Text}");
            userName = EnterLogin.Text;
            EnterPassword.Text = EnterLogin.Text = "";
        }

        private void QuestLogsIn_Click(object sender, RoutedEventArgs e)
        {
            userName = UserName.Text;
            GameList.IsEnabled = true;
            UserName.Text = "";
        }

        private void StartGameCheckers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данная функция находится на стадии розработки!!!");
            GamesCheckers checkers;
            if (client == null)
                checkers = new GamesCheckers(userName);
            else
                checkers = new GamesCheckers(client, endPoint, address, userName);
            checkers.ShowDialog();
        }

        private void SearchTop_Click(object sender, RoutedEventArgs e)
        {
            int tmp = 0;
            if (int.TryParse(TopCount.Text, out tmp))
                Send($"{Games1.SelectedItem}|{TopCount.Text}|Request|Top");
            else
                MessageBox.Show($"{TopCount.Text} не является целочесленым числом!!!");
        }

        private void SearchMyRating_Click(object sender, RoutedEventArgs e)
        {
            Send($"{userName}|{Criteria.SelectedItem}|Request|My");
        }
    }
}