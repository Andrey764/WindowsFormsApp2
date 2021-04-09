using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для GameXO.xaml
    /// </summary>
    public partial class GameXO : Window
    {
        private const string gameName = "Крестики Нолики";
        private const string V = "O";
        private const string X = "X";
        private string userName;
        private string PlayerChar;
        private TcpClient client;
        private IPEndPoint endPoint;
        private IPAddress address;
        private Dictionary<string, Button> buttons;

        public GameXO()
        {
            InitializeComponent();
            InitDictionary();
            StartScreen();
        }

        public GameXO(string userName) : this()
        {
            this.userName = userName;
        }

        public GameXO(TcpClient client, IPEndPoint endPoint, IPAddress address, string userName) : this(userName)
        {
            this.client = client;
            this.endPoint = endPoint;
            this.address = address;
        }

        private void Conect_Click(object sender, RoutedEventArgs e)
        {
            PlayerChar = comboBox1.SelectedItem.ToString() == "Крестики" ? X : V;
            Conect.IsEnabled = comboBox1.IsEnabled = false;
            PlayingField.IsEnabled = true;
            Conected();
        }

        /// <summary>
        /// Инициалезирует колекцию кнопок доступных для игры
        /// </summary>
        private void InitDictionary()
        {
            buttons = new Dictionary<string, Button>
            {
                { "B1", B1 },
                { "B2", B2 },
                { "B3", B3 },
                { "B4", B4 },
                { "B5", B5 },
                { "B6", B6 },
                { "B7", B7 },
                { "B8", B8 },
                { "B9", B9 }
            };
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
                    if (s == "противник найден")
                        MessageBox.Show($"Ваш {s}");
                    else if (s == "Close")
                    {
                        client.Close();
                        Dispatcher.Invoke(new Action(() => { StartScreen(); }));
                        break;
                    }
                    else if (s.Contains("!!!"))
                        Dispatcher.Invoke(new Action(() => { PlayingField.IsEnabled = false; MessageBox.Show(s); }));
                    else
                        Dispatcher.Invoke(new Action(() => { DataSynchronization(s.Split('|')); }));
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

        /// <summary>
        /// Синхронезирует данные получиные от сервера с пользователем и розблокирует пользователя
        /// </summary>
        /// <param name="strs">Массив обозначений полученных с сервера</param>
        private void DataSynchronization(string[] strs)
        {
            var temp = buttons[strs[strs.Length - 1]];
            temp.Content = strs[1];
            temp.IsEnabled = false;

            PlayingField.IsEnabled = true;
        }

        /// <summary>
        /// Проверяет победил ли кто нибуть
        /// </summary>
        /// <returns>true если один из играков победил иначе false</returns>
        private bool GameOver()
        {
            if (B3.Content.ToString() == X && B5.Content.ToString() == X && B7.Content.ToString() == X || B1.Content.ToString() == X && B5.Content.ToString() == X && B9.Content.ToString() == X || B3.Content.ToString() == X && B6.Content.ToString() == X && B9.Content.ToString() == X || B2.Content.ToString() == X && B5.Content.ToString() == X && B8.Content.ToString() == X || B1.Content.ToString() == X && B4.Content.ToString() == X && B7.Content.ToString() == X || B7.Content.ToString() == X && B8.Content.ToString() == X && B9.Content.ToString() == X || B1.Content.ToString() == X && B2.Content.ToString() == X && B3.Content.ToString() == X || B4.Content.ToString() == X && B5.Content.ToString() == X && B6.Content.ToString() == X)
            {
                GameOverMessage(X);
                return false;
            }
            if (B3.Content.ToString() == V && B5.Content.ToString() == V && B7.Content.ToString() == V || B1.Content.ToString() == V && B5.Content.ToString() == V && B9.Content.ToString() == V || B3.Content.ToString() == V && B6.Content.ToString() == V && B9.Content.ToString() == V || B2.Content.ToString() == V && B5.Content.ToString() == V && B8.Content.ToString() == V || B1.Content.ToString() == V && B4.Content.ToString() == V && B7.Content.ToString() == V || B7.Content.ToString() == V && B8.Content.ToString() == V && B9.Content.ToString() == V || B1.Content.ToString() == V && B2.Content.ToString() == V && B3.Content.ToString() == V || B4.Content.ToString() == V && B5.Content.ToString() == V && B6.Content.ToString() == V)
            {
                GameOverMessage(V);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверяет в результате игры получилась ничья или нет
        /// </summary>
        /// <returns>false если ничья, иначе победил какой-то игрок</returns>
        private bool Draw()
        {
            foreach (var item in buttons)
                if (item.Value.IsEnabled)
                    return false;
            return true;
        }

        /// <summary>
        /// Отправляет сообщение обом игрокам о завершении игры
        /// </summary>
        /// <param name="str">Обозначение победителя(должен принимать либо X либо O)</param>
        private void GameOverMessage(string str)
        {
            string message = "";
            if (str == X)
                message = "Победили крестики!!!";
            else if (str == V)
                message = "Победили нолики!!!";
            else if (str == "ничья")
                message = $"{str} победила дружба!!!";
            MessageBox.Show(message);
            Send($"{userName}|{message}");
            Segregation();
            StartScreen();
        }

        /// <summary>
        /// отправляет сообщение на сервер
        /// </summary>
        /// <param name="message">Сообщение каторое отправица на сервер</param>
        /// <param name="playingFieldEnable">опредиляет будет ли заблокирован пользователь если true то пользователь НЕ заблокируется(Стоит по умолчанию),
        /// а если False то пользователь заблокируется до момента пока его противник не сделает ход</param>
        private void Send(string message)
        {
            NetworkStream network = client.GetStream();
            byte[] buf = Encoding.Unicode.GetBytes(message);
            network.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Создает нового клиента и создает подключение
        /// </summary>
        private void InitClient()
        {
            address = IPAddress.Parse("127.0.0.1");
            endPoint = new IPEndPoint(address, 8081);
            client = new TcpClient();
            client.Connect(endPoint);
            new Thread(new ThreadStart(ReceivingMessages))
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Обработчик клика, на кнопку, на поле для игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Turn(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.Content = PlayerChar;
                button.IsEnabled = false;
                Send($"{userName}|{PlayerChar}|{button.Name}");
                if (GameOver() && Draw())
                    GameOverMessage("ничья");
                PlayingField.IsEnabled = false;
            }
        }

        /// <summary>
        /// отправляет на сервер запрос на добавление пользователя в список играков
        /// </summary>
        private void Conected()
        {
            InitClient();
            Send($"{userName}&{gameName}&{PlayerChar}");
        }

        /// <summary>
        /// Настаивает елементы управления до настроек по умолчанию
        /// </summary>
        private void StartScreen()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Крестики");
            comboBox1.Items.Add("Нолики");
            comboBox1.IsEnabled = true;
            PlayingField.IsEnabled = false;
            Conect.IsEnabled = true;

            foreach (var item in buttons)
            {
                item.Value.IsEnabled = true;
                item.Value.Content = "";
            }
        }

        /// <summary>
        /// отправляет запрос на сервер для отсоиденения
        /// </summary>
        private void Segregation()
        {
            Send($"{userName}|DeleteMe");
        }
    }
}