using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для GamesCheckers.xaml
    /// </summary>
    public partial class GamesCheckers : Window
    {
        private const string gameName = "Шашки";
        private const string V = "O";
        private const string X = "X";
        private string HitMessage;
        private string userName;
        private string PlayerChar;
        private TcpClient client;
        private IPEndPoint endPoint;
        private IPAddress address;
        private Button CurrentChecker;
        private Dictionary<string, Button> ListWhiteCheckers;
        private Dictionary<string, Button> ListBlackCheckers;
        private Dictionary<string, Button> CellColection;

        public GamesCheckers()
        {
            InitializeComponent();
            initChecker();
            StartWindow();
        }

        public GamesCheckers(string userName) : this()
        {
            this.userName = userName;
        }

        public GamesCheckers(TcpClient client, IPEndPoint endPoint, IPAddress address, string userName) : this(userName)
        {
            this.client = client;
            this.endPoint = endPoint;
            this.address = address;
        }

        /// <summary>
        /// Создает нового клиента и создает подключение
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
        /// отправляет на сервер запрос на добавление пользователя в список играков
        /// </summary>
        private void Conected()
        {
            InitClient();
            Send($"{userName}&{gameName}&{PlayerChar}");
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
        /// Анализирует даные получиные от сервера
        /// </summary>
        private void ReceivingMessages()
        {
            while (true)
                lock (client)
                {
                    string s = GetMessage(client);
                    Dispatcher.Invoke(new Action(() => { DataSynchronization(s.Split('|')); }));
                }
        }

        /// <summary>
        /// Синхронезирует данные получиные от сервера с пользователем и розблокирует пользователя
        /// </summary>
        /// <param name="strs">Массив обозначений полученных с сервера</param>
        private void DataSynchronization(string[] strs)
        {
            if (strs[1].Contains("White"))
                Move2(ListWhiteCheckers[strs[1]], CellColection[strs[2]]);
            else if (strs[1].Contains("Black"))
                Move2(ListBlackCheckers[strs[1]], CellColection[strs[2]]);
            if (strs.Length == 4)
            {
                if (strs[3].Contains("White"))
                {
                    Field.Children.Remove(ListWhiteCheckers[strs[3]]);
                    ListWhiteCheckers[strs[3]] = null;
                    ListWhiteCheckers.Remove(strs[3]);
                }
                if (strs[3].Contains("Black"))
                {
                    Field.Children.Remove(ListBlackCheckers[strs[3]]);
                    ListBlackCheckers[strs[3]] = null;
                    ListBlackCheckers.Remove(strs[3]);
                }
            }
        }

        private void initChecker()
        {
            ListWhiteCheckers = new Dictionary<string, Button>();
            ListBlackCheckers = new Dictionary<string, Button>();
            CellColection = new Dictionary<string, Button>();
            //////////////////////////////////////////////////////
            ListWhiteCheckers.Add(White1.Name, White1);
            ListWhiteCheckers.Add(White2.Name, White2);
            ListWhiteCheckers.Add(White3.Name, White3);
            ListWhiteCheckers.Add(White4.Name, White4);
            ListWhiteCheckers.Add(White5.Name, White5);
            ListWhiteCheckers.Add(White6.Name, White6);
            ListWhiteCheckers.Add(White7.Name, White7);
            ListWhiteCheckers.Add(White8.Name, White8);
            ListWhiteCheckers.Add(White9.Name, White9);
            ListWhiteCheckers.Add(White10.Name, White10);
            ListWhiteCheckers.Add(White11.Name, White11);
            ListWhiteCheckers.Add(White12.Name, White12);
            ////////////////////////////////////////////////////////
            ListBlackCheckers.Add(Black1.Name, Black1);
            ListBlackCheckers.Add(Black2.Name, Black2);
            ListBlackCheckers.Add(Black3.Name, Black3);
            ListBlackCheckers.Add(Black4.Name, Black4);
            ListBlackCheckers.Add(Black5.Name, Black5);
            ListBlackCheckers.Add(Black6.Name, Black6);
            ListBlackCheckers.Add(Black7.Name, Black7);
            ListBlackCheckers.Add(Black8.Name, Black8);
            ListBlackCheckers.Add(Black9.Name, Black9);
            ListBlackCheckers.Add(Black10.Name, Black10);
            ListBlackCheckers.Add(Black11.Name, Black11);
            ListBlackCheckers.Add(Black12.Name, Black12);
            ////////////////////////////////////////////////////////
            CellColection.Add(Cell1.Name, Cell1);
            CellColection.Add(Cell2.Name, Cell2);
            CellColection.Add(Cell3.Name, Cell3);
            CellColection.Add(Cell4.Name, Cell4);
            CellColection.Add(Cell5.Name, Cell5);
            CellColection.Add(Cell6.Name, Cell6);
            CellColection.Add(Cell7.Name, Cell7);
            CellColection.Add(Cell8.Name, Cell8);
            CellColection.Add(Cell9.Name, Cell9);
            CellColection.Add(Cell10.Name, Cell10);
            CellColection.Add(Cell11.Name, Cell11);
            CellColection.Add(Cell12.Name, Cell12);
            CellColection.Add(Cell13.Name, Cell13);
            CellColection.Add(Cell14.Name, Cell14);
            CellColection.Add(Cell15.Name, Cell15);
            CellColection.Add(Cell16.Name, Cell16);
            CellColection.Add(Cell17.Name, Cell17);
            CellColection.Add(Cell18.Name, Cell18);
            CellColection.Add(Cell19.Name, Cell19);
            CellColection.Add(Cell20.Name, Cell20);
            CellColection.Add(Cell21.Name, Cell21);
            CellColection.Add(Cell22.Name, Cell22);
            CellColection.Add(Cell23.Name, Cell23);
            CellColection.Add(Cell24.Name, Cell24);
            CellColection.Add(Cell25.Name, Cell25);
            CellColection.Add(Cell26.Name, Cell26);
            CellColection.Add(Cell27.Name, Cell27);
            CellColection.Add(Cell28.Name, Cell28);
            CellColection.Add(Cell29.Name, Cell29);
            CellColection.Add(Cell30.Name, Cell30);
            CellColection.Add(Cell31.Name, Cell31);
            CellColection.Add(Cell32.Name, Cell32);
        }

        private void Checker_Click(object sender, RoutedEventArgs e)
        {
            CurrentChecker = sender as Button;
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button Buf = sender as Button;
            if (IsOamFree(Buf))
                if (WasThereAMove(CurrentChecker, Buf))
                    Move(Buf);
        }

        /// <summary>
        /// Перемещает выбраную шашку в заданую позицию
        /// </summary>
        /// <param name="Where">позиция куда пользователь собирается ходить</param>
        private void Move(Button Where)
        {
            Move2(CurrentChecker, Where);
            //if (WillThereBeAMove())
            //{
            //    Field.IsEnabled = true;
            //}
            if (HitMessage == "")
                Send($"{userName}|{CurrentChecker.Name}|{Where.Name}");
            else
                Send($"{userName}|{CurrentChecker.Name}|{Where.Name}|{HitMessage}");
        }

        private void Move2(Button FromWhere, Button Where)
        {
            Grid.SetRow(FromWhere, Grid.GetRow(Where));
            Grid.SetColumn(FromWhere, Grid.GetColumn(Where));
            Field.IsEnabled = !Field.IsEnabled;
        }

        /// <summary>
        /// Прверяет позволительный ли ход в заданую позицию для конкретной шашки
        /// </summary>
        /// <param name="FromWhere">шашка каторой пользователь хочет походить</param>
        /// <param name="Where">позиция куда пользователь собирается ходить</param>
        /// <returns>true если ход позвоолительный иначе false</returns>
        private bool WasThereAMove(Button FromWhere, Button Where)
        {
            if (IsItABlow(FromWhere, Where))
                return true;
            else if (FromWhere.Name.Contains("White"))
            {
                if (Grid.GetRow(FromWhere) - 1 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) - 1 == Grid.GetColumn(Where))
                    return true;
                else if (Grid.GetRow(FromWhere) - 1 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) + 1 == Grid.GetColumn(Where))
                    return true;
            }
            else if (FromWhere.Name.Contains("Black"))
            {
                if (Grid.GetRow(FromWhere) + 1 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) - 1 == Grid.GetColumn(Where))
                    return true;
                else if (Grid.GetRow(FromWhere) + 1 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) + 1 == Grid.GetColumn(Where))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// опредиляет будет ли переход хода(Недоработан)
        /// </summary>
        /// <param name="FromWhere">шашка каторой пользователь хочет походить</param>
        /// <param name="Where">позиция куда пользователь собирается ходить</param>
        /// <returns></returns>
        private bool WillThereBeAMove()
        {
            bool rezault = false;
            if (CurrentChecker.Name.Contains("White"))
            {
                Moving(-1, -1);
                if (!BlackIsOamFree(CurrentChecker))
                {
                    Moving(-1, -1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(+1, +1);
                    if (rezault)
                        return rezault;
                }
                Moving(+1, +1);
                Moving(-1, +1);
                if (!BlackIsOamFree(CurrentChecker))
                {
                    Moving(-1, +1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(+1, -1);
                    if (rezault)
                        return rezault;
                }
                Moving(+1, -1);
                Moving(+1, -1);
                if (!BlackIsOamFree(CurrentChecker))
                {
                    Moving(+1, -1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(-1, +1);
                    if (rezault)
                        return rezault;
                }
                Moving(-1, +1);
                Moving(+1, +1);
                if (!BlackIsOamFree(CurrentChecker))
                {
                    Moving(+1, +1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(-1, -1);
                    if (rezault)
                        return rezault;
                }
                Moving(-1, -1);
            }
            if (CurrentChecker.Name.Contains("Black"))
            {
                Moving(-1, -1);
                if (!WhiteIsOamFree(CurrentChecker))
                {
                    Moving(-1, -1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(+1, +1);
                    if (rezault)
                        return rezault;
                }
                Moving(+1, +1);
                Moving(-1, +1);
                if (!WhiteIsOamFree(CurrentChecker))
                {
                    Moving(-1, +1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(+1, -1);
                    if (rezault)
                        return rezault;
                }
                Moving(+1, -1);
                Moving(+1, -1);
                if (!WhiteIsOamFree(CurrentChecker))
                {
                    Moving(+1, -1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(-1, +1);
                    if (rezault)
                        return rezault;
                }
                Moving(-1, +1);
                Moving(+1, +1);
                if (!WhiteIsOamFree(CurrentChecker))
                {
                    Moving(+1, +1);
                    rezault = IsOamFree(CurrentChecker);
                    Moving(-1, -1);
                    if (rezault)
                        return rezault;
                }
                Moving(-1, -1);
            }
            return rezault;
        }

        private void Moving(int pos1, int pos2)
        {
            Grid.SetColumn(CurrentChecker, Grid.GetColumn(CurrentChecker) + pos1);
            Grid.SetRow(CurrentChecker, Grid.GetRow(CurrentChecker) + pos2);
        }

        /// <summary>
        /// Проверяет является ли ход в заданую позицию ударом
        /// </summary>
        /// <param name="FromWhere">шашка каторой пользователь хочет походить</param>
        /// <param name="Where">позиция куда пользователь собирается ходить</param>
        /// <returns>true если этот ход удар инача false</returns>
        private bool IsItABlow(Button FromWhere, Button Where)
        {
            if (Grid.GetRow(FromWhere) - 2 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) - 2 == Grid.GetColumn(Where))
                return WillTheBlowBe(FromWhere, -1, -1);
            else if (Grid.GetRow(FromWhere) - 2 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) + 2 == Grid.GetColumn(Where))
                return WillTheBlowBe(FromWhere, -1, 1);
            else if (Grid.GetRow(FromWhere) + 2 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) - 2 == Grid.GetColumn(Where))
                return WillTheBlowBe(FromWhere, 1, -1);
            else if (Grid.GetRow(FromWhere) + 2 == Grid.GetRow(Where) && Grid.GetColumn(FromWhere) + 2 == Grid.GetColumn(Where))
                return WillTheBlowBe(FromWhere, 1, 1);
            return false;
        }

        /// <summary>
        /// Проверяет будет ли удар
        /// </summary>
        /// <param name="FromWhere">шашка каторой пользователь собирается побить шашку противника</param>
        /// <param name="position1">Предполагаемая ширина шашки протвника</param>
        /// <param name="position2">Предполагаемая высота шашки протвника</param>
        /// <returns>true если удар будет инача false</returns>
        private bool WillTheBlowBe(Button FromWhere, int position1, int position2)
        {
            Grid.SetColumn(FromWhere, Grid.GetColumn(FromWhere) + position1);
            Grid.SetRow(FromWhere, Grid.GetRow(FromWhere) + position2);
            if (FromWhere.Name.Contains("White"))
                return (RollOutChanges(!BlackIsOamFree(FromWhere, true), FromWhere, position1, position2));
            else if (FromWhere.Name.Contains("Black"))
                return (RollOutChanges(!WhiteIsOamFree(FromWhere, true), FromWhere, position1, position2));
            return false;
        }

        /// <summary>
        /// возвращает шашку на предыдущию позицию
        /// </summary>
        /// <param name="rezualt">результат действий каторые потребовали временных изменений</param>
        /// <param name="FromWhere">шашка каторой пользователь собирается побить шашку противника</param>
        /// <param name="position1">Предполагаемая ширина шашки протвника</param>
        /// <param name="position2">Предполагаемая высота шашки протвника</param>
        /// <returns></returns>
        private bool RollOutChanges(bool rezualt, Button FromWhere, int position1, int position2)
        {
            Grid.SetColumn(FromWhere, Grid.GetColumn(FromWhere) - position1);
            Grid.SetRow(FromWhere, Grid.GetRow(FromWhere) - position2);
            return rezualt;
        }

        /// <summary>
        /// проверяет можна ли походить в указаную позицию
        /// </summary>
        /// <param name="Where">Черная клеточка на каторую пользователь хочет походить</param>
        /// <returns>true если ход туда возможен иначе false</returns>
        private bool IsOamFree(Button Where)
        {
            return WhiteIsOamFree(Where) && BlackIsOamFree(Where);
        }

        /// <summary>
        /// проверяет есть ли белие шашки в переданой позиции
        /// </summary>
        /// <param name="Where">Черная клеточка на каторую пользователь хочет походить</param>
        /// <param name="HitChecker">определяет будет ли удалятся шашка в том случее если она будет находица в переданной позиции</param>
        /// <returns>true если в переданой позиции нету белых шашок иначе false</returns>
        private bool WhiteIsOamFree(Button Where, bool HitChecker = false)
        {
            foreach (var item in ListWhiteCheckers)
                if (Grid.GetRow(Where) == Grid.GetRow(item.Value) && Grid.GetColumn(Where) == Grid.GetColumn(item.Value))
                {
                    if (HitChecker)
                    {
                        HitMessage = item.Key;
                        //удаление шашки  и текущего item
                        Field.Children.Remove(ListWhiteCheckers[item.Key]);
                        ListWhiteCheckers[item.Key] = null;
                        ListWhiteCheckers.Remove(item.Key);
                    }
                    return false;
                }
            return true;
        }

        /// <summary>
        /// проверяет есть ли черние шашки в переданой позиции
        /// </summary>
        /// <param name="Where">Черная клеточка на каторую пользователь хочет походить</param>
        /// <param name="HitChecker">определяет будет ли удалятся шашка в том случее если она будет находица в переданной позиции</param>
        /// <returns>true если в переданой позиции нету черних шашок иначе false</returns>
        private bool BlackIsOamFree(Button Where, bool HitChecker = false)
        {
            foreach (var item in ListBlackCheckers)
                if (Grid.GetRow(Where) == Grid.GetRow(item.Value) && Grid.GetColumn(Where) == Grid.GetColumn(item.Value))
                {
                    if (HitChecker)
                    {
                        HitMessage = item.Key;
                        //удаление шашки  и текущего item
                        Field.Children.Remove(ListBlackCheckers[item.Key]);
                        ListBlackCheckers[item.Key] = null;
                        ListBlackCheckers.Remove(item.Key);
                    }
                    return false;
                }
            return true;
        }

        private void Conect_Click(object sender, RoutedEventArgs e)
        {
            PlayerChar = comboBox.SelectedItem.ToString() == "Белие шашки" ? X : V;
            Conect.IsEnabled = comboBox.IsEnabled = false;
            Field.IsEnabled = true;
            if (PlayerChar == X)
                CheckersDisable(ListBlackCheckers);
            else if (PlayerChar == V)
            {
                CheckersDisable(ListWhiteCheckers);
                Field.IsEnabled = false;
            }
            Conected();
        }

        /// <summary>
        /// блокирует доступ к переданому в метод списку шашок для этого пользователя
        /// </summary>
        /// <param name="collection">список шашок каторый нужно заблокировать</param>
        private void CheckersDisable(Dictionary<string, Button> collection)
        {
            foreach (var item in collection)
                item.Value.IsEnabled = false;
        }

        /// <summary>
        /// востанавливает вид преложение до начала каких либо действий
        /// </summary>
        private void StartWindow()
        {
            Field.IsEnabled = false;
            comboBox.Items.Add("Белие шашки");
            comboBox.Items.Add("черние шашки");
        }
    }
}