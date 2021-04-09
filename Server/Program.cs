using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class Pair
    {
        public string Key { get; set; }
        public int Value { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            NewThread();
        }

        /// <summary>
        /// Ожидает подключение новых пользователей и в новом потоке начинает с ними работать
        /// </summary>
        private static void NewThread()
        {
            List<UserPar> userPars = new List<UserPar>();
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8081);
            server.Start();

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (server.AcceptTcpClient() is TcpClient client)
                            new Thread(() => { Logical(client, ref userPars); })
                            {
                                IsBackground = true
                            }.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }).Start();
        }

        /// <summary>
        /// Реализует передачу данных между пользователями
        /// </summary>
        /// <param name="client">новый клиент</param>
        /// <param name="userPars">Список подключенных клиентов</param>
        private static void Logical(TcpClient client, ref List<UserPar> userPars)
        {
            try
            {
                while (true)
                {
                    string s = ReadMessage(client);
                    if (s.Contains("&"))
                        AddUser(s, ref userPars, client);
                    else if (s.Contains("Request"))
                        Request(s, client);
                    else if (s.Contains("DB"))
                        DB(s, client);
                    else if (s.Contains("!!!"))
                    {
                        DataSynchronization(s, ref userPars);
                        SaveGameToBD(s, ref userPars);
                    }
                    else if (s.Contains("DeleteMe"))
                        DeleteUser(s, ref userPars);
                    else if (s.Contains("|"))
                        DataSynchronization(s, ref userPars);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Опредиление основних взаимодействий с базой данных
        /// </summary>
        /// <param name="client">новый клиент</param>
        /// <param name="userPars">Список подключенных клиентов</param>
        private static void DB(string s, TcpClient client)
        {
            if (s.Contains("newUser"))
                AddUserToDB(s, client);
            else if (s.Contains("Enter"))
                LogIn(s, client);
            Console.WriteLine("Запрос обработан");
        }

        /// <summary>
        /// Опредиление запросов для рейтингов
        /// </summary>
        /// <param name="client">новый клиент</param>
        /// <param name="userPars">Список подключенных клиентов</param>
        private static void Request(string s, TcpClient client)
        {
            string Message = "";
            if (s.Contains("Top"))
                Message = Top(s);
            else if (s.Contains("My"))
                Message = My(s);
            Console.WriteLine("Запрос обработан");
            Send(client.GetStream(), Message);
        }

        /// <summary>
        /// формирует список игр пользователя
        /// </summary>
        /// <param name="s">сообщение полученое от пользователя</param>
        /// <returns>список игр пользователя</returns>
        private static string My(string s)
        {
            string[] strs = s.Split('|');
            if ("Все мои игры" == strs[1])
                return MyGame(strs[0]);
            else if ("Все мои игры в крестики нолики" == strs[1])
                return MyGame(strs[0], "Крестики Нолики");
            else if ("Все мои игры в шашки" == strs[1])
                return MyGame(strs[0], "Шашки");
            else if ("Все победы во всех играх" == strs[1])
                return MyVictories(strs[0]);
            else if ("Все проиграшы во всех играх" == strs[1])
                return MyLosses(strs[0]);
            else if ("Все победы в шашках" == strs[1])
                return MyVictories(strs[0], "Шашки");
            else if ("Все победы в крестиках ноликах" == strs[1])
                return MyVictories(strs[0], "Крестики Нолики");
            else if ("Все проиграшы в шашках" == strs[1])
                return MyLosses(strs[0], "Крестики Нолики");
            else if ("Все проиграшы в крестиках ноликах" == strs[1])
                return MyLosses(strs[0], "Шашки");
            return "";
        }

        /// <summary>
        /// ищет все проиграши по переданному имени пользователя
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <returns>все проиграные игры</returns>
        private static string MyLosses(string userName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if ((game.Gamer1 == userName || game.Gamer2 == userName) && game.Winner != userName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// ищет все проиграши для конкретной игры по переданному имени пользователя и названию игры
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <param name="gameName">название игры</param>
        /// <returns>все проиграные игры</returns>
        private static string MyLosses(string userName, string gameName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if ((game.Gamer1 == userName || game.Gamer2 == userName) && game.Winner != userName && game.GameName == gameName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// ищет все победы по переданному имени пользователя
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <returns>все выиграные игры</returns>
        private static string MyVictories(string userName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if (game.Winner == userName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// ищет все победы для конкретной игры по переданному имени пользователя и названию игры
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <param name="gameName">название игры</param>
        /// <returns>все выигранные игры</returns>
        private static string MyVictories(string userName, string gameName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if (game.Winner == userName && game.GameName == gameName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// ищет все игры по переданному имени пользователя
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <returns>все игры</returns>
        private static string MyGame(string userName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if (game.Gamer1 == userName || game.Gamer2 == userName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// ищет все игры для конкретной игры по переданному имени пользователя и названию игры
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <param name="gameName">название игры</param>
        /// <returns>все игры</returns>
        private static string MyGame(string userName, string gameName)
        {
            string message = "";
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if ((game.Gamer1 == userName || game.Gamer2 == userName) && game.GameName == gameName)
                        message += game.ToString() + "\n";
            }
            return message;
        }

        /// <summary>
        /// Получает данные об победителях с базы данных
        /// </summary>
        /// <returns>слоаврь победителей и количество их побед</returns>
        private static Dictionary<string, int> TopUsersMethod(string condition)
        {
            Dictionary<string, int> TopUsers = new Dictionary<string, int>();
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                foreach (var game in db.Games)
                    if (condition == "Все игры")
                        if (TopUsers.ContainsKey(game.Winner))
                            TopUsers[game.Winner]++;
                        else
                            TopUsers.Add(game.Winner, 1);
                    else
                    {
                        if (TopUsers.ContainsKey(game.Winner) && game.GameName == condition)
                            TopUsers[game.Winner]++;
                        else if (game.GameName == condition)
                            TopUsers.Add(game.Winner, 1);
                    }
            }
            return TopUsers;
        }

        /// <summary>
        /// Формирует топ лучших игракову
        /// </summary>
        /// <param name="s">сообщение полученое от пользователя</param>
        /// <returns>топ играков</returns>
        private static string Top(string s)
        {
            string[] strs = s.Split('|');
            return UserNamberOne(TopUsersMethod(strs[0]), int.Parse(strs[1]));
        }

        /// <summary>
        /// Сортирует играков по их каличеству их побед и возвращает строку в каторой лучшый игрок в начале а худший в конце
        /// </summary>
        /// <param name="TopUsers">словарь з игроками и их результатами</param>
        /// <returns>строка с данными о лучшем играке</returns>
        private static string UserNamberOne(Dictionary<string, int> TopUsers, int TopCount)
        {
            string Message = "";
            for (int i = 0; i < TopCount; i++)
            {
                Pair pair = new Pair();
                pair.Key = "";
                pair.Value = -1;
                foreach (var item in TopUsers)
                {
                    if (item.Value > pair.Value)
                    {
                        pair.Value = item.Value;
                        pair.Key = item.Key;
                    }
                }
                TopUsers.Remove(pair.Key);
                Message += $"{pair.Key} :{pair.Value} побед\n";
            }
            return Message;
        }

        /// <summary>
        /// Сохраняет результат игры в базу данних
        /// </summary>
        /// <param name="Message">Сообщение отправленое клиентом</param>
        /// <param name="userPars">Имя клиента</param>
        private static void SaveGameToBD(string Message, ref List<UserPar> userPars)
        {
            string[] strs = Message.Split('|');
            if (strs.Length == 2)
            {
                foreach (var item in userPars)
                    if (item.UserNameO == strs[0] || item.UserNameX == strs[0])
                        using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
                        {
                            db.Games.Add(AddGames(item.UserNameX, item.UserNameO, item.gameName, strs[1]));
                            db.SaveChanges();
                        }
                Console.WriteLine("игра сохранена");
            }
        }

        /// <summary>
        /// создает обьект класа Games и возвращает его
        /// </summary>
        /// <param name="Gamer1">имя первого играка</param>
        /// <param name="Gamer2">имя второго играка</param>
        /// <param name="GameName">Название игры</param>
        /// <param name="Message">сообщение полученое от клиента</param>
        /// <returns>обьект класа Games</returns>
        private static Games AddGames(string Gamer1, string Gamer2, string GameName, string Message)
        {
            Games games = new Games();
            games.Gamer1 = Gamer1;
            games.Gamer2 = Gamer2;
            games.GameName = GameName;
            if (Message == "Победили крестики!!!")
                games.Winner = Gamer1;
            else if (Message == "Победили нолики!!!")
                games.Winner = Gamer2;
            else
                games.Winner = "Ничея";
            return games;
        }

        /// <summary>
        ///  выполняет вход в систему для переданного пользователя
        /// </summary>
        /// <param name="Message">сообщение клиента пытающегося войти в систему</param>
        /// <param name="client">Клиент пытающийся войти в систему</param>
        private static void LogIn(string Message, TcpClient client)
        {
            string[] strs = Message.Split('|');
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                string SendMessage = "";
                foreach (var item in db.Users)
                {
                    if (item.UserLogin == strs[strs.Length - 2])
                        if (item.UserPassword == strs[strs.Length - 1])
                        {
                            SendMessage = "Вход выполнен";
                            break;
                        }
                        else
                            SendMessage = "Ошика вход не выполнен!!!";
                    else
                        SendMessage = "Ошика вход не выполнен!!!";
                }
                Send(client.GetStream(), SendMessage);
                Console.WriteLine("Проверка пользователя окончена!");
            }
        }

        /// <summary>
        /// Добавляет пользователя в базу данных
        /// </summary>
        /// <param name="Message">Сообщение полученое от клиента каторого нужно добавить</param>
        /// <param name="client">Клиент каторого нужно добавит</param>
        private static void AddUserToDB(string Message, TcpClient client)
        {
            string[] strs = Message.Split('|');
            using (MyOnlineBoardGameTournamentEntities db = new MyOnlineBoardGameTournamentEntities())
            {
                Users users = new Users();
                users.UserLogin = strs[strs.Length - 2];
                users.UserPassword = strs[strs.Length - 1];
                db.Users.Add(users);
                db.SaveChanges();
            }
            Console.WriteLine("пользователь добавлен в базу данних!");
            Send(client.GetStream(), "пользователь добавлен в базу данних!");
        }

        /// <summary>
        /// Получение даных из TcpClient и их конвертация в строку
        /// </summary>
        /// <param name="client">клиент у каторого мы получаем данные</param>
        /// <returns>Конвертированая строка</returns>
        private static string ReadMessage(TcpClient client)
        {
            byte[] bytes = new byte[256];
            NetworkStream network = client.GetStream();
            network.Read(bytes, 0, bytes.Length);
            return Encoding.Unicode.GetString(bytes).Replace("\0", "");
        }

        /// <summary>
        /// Синхрогнизация данных между парой пользователей
        /// </summary>
        /// <param name="s">сообщение полученое от клиента клиента</param>
        /// <param name="userPars">список пользователей</param>
        private static void DataSynchronization(string s, ref List<UserPar> userPars)
        {
            string[] strs = s.Split('|');
            string message = strs.Length >= 3 ? s : strs[strs.Length - 1];

            foreach (var item in userPars)
            {
                if (item.UserNameX == strs[0])
                    Send(item.CLientO.GetStream(), message);
                else if (item.UserNameO == strs[0])
                    Send(item.CLientX.GetStream(), message);
            }
        }

        /// <summary>
        /// Отправка сообщений конкретному клиенту
        /// </summary>
        /// <param name="network">Поток по каторому будут отправленные данные</param>
        /// <param name="s">сообщение для клиента</param>
        private static void Send(NetworkStream network, string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            network.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// добавление пользователя в список играющих и поиск пары для него
        /// </summary>
        /// <param name="s">Сообщение полученое от пользователя</param>
        /// <param name="userPars">список пользователей</param>
        /// <param name="client">TCP клиент подключившегося пользователя</param>
        private static void AddUser(string s, ref List<UserPar> userPars, TcpClient client)
        {
            string[] strs = s.Split('&');
            if (strs[strs.Length - 1] == "X")
            {
                if (!FreeX(strs[0], strs[1], ref userPars, client))
                {
                    AddX(strs[0], strs[1], ref userPars, client);
                }
            }
            else if (strs[strs.Length - 1] == "O")
            {
                if (!FreeO(strs[0], strs[1], ref userPars, client))
                {
                    AddO(strs[0], strs[1], ref userPars, client);
                }
            }
        }

        /// <summary>
        /// добавляет пользователя играющего крестиками
        /// </summary>
        /// <param name="UserName">имя пользователя</param>
        /// <param name="userPars">список пользователей</param>
        /// <param name="client">TCP клиент подключившегося пользователя</param>
        private static void AddX(string UserName, string gameName, ref List<UserPar> userPars, TcpClient client)
        {
            UserPar par = new UserPar();
            par.UserNameX = UserName;
            par.CLientX = client;
            par.gameName = gameName;
            userPars.Add(par);
        }

        /// <summary>
        /// добавляет пользователя играющего ноликами
        /// </summary>
        /// <param name="UserName">имя пользователя</param>
        /// <param name="userPars">список пользователей</param>
        /// <param name="client">TCP клиент подключившегося пользователя</param>
        private static void AddO(string UserName, string gameName, ref List<UserPar> userPars, TcpClient client)
        {
            UserPar par = new UserPar();
            par.UserNameO = UserName;
            par.CLientO = client;
            par.gameName = gameName;
            userPars.Add(par);
        }

        /// <summary>
        /// Ищет свободного пользователя играющего ноликами чтобы преставить к нему пользователя играющего крестиками
        /// </summary>
        /// <param name="UserName">новый игрок крестиками без пары</param>
        /// <param name="userPars">список пар играющих пользователей</param>
        /// <returns> true если новому игроку нашлась пара иначе false</returns>
        private static bool FreeX(string UserName, string gameName, ref List<UserPar> userPars, TcpClient client)
        {
            foreach (var item in userPars)
            {
                if (item.UserNameX == "XXX" && item.gameName == gameName)
                {
                    item.UserNameX = UserName;
                    item.CLientX = client;
                    // Send(client.GetStream(), "противник найден");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ищет свободного пользователя играющего крестиками чтобы преставить к нему пользователя играющего ноликами
        /// </summary>
        /// <param name="UserName">новый игрок ноликами без пары</param>
        /// <param name="userPars">список пар играющих пользователей</param>
        /// <returns> true если новому игроку нашлась пара иначе false</returns>
        private static bool FreeO(string UserName, string gameName, ref List<UserPar> userPars, TcpClient client)
        {
            foreach (var item in userPars)
            {
                if (item.UserNameO == "OOO" && item.gameName == gameName)
                {
                    item.UserNameO = UserName;
                    item.CLientO = client;
                    //Send(client.GetStream(), "Противник найден");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Удаляет пользователя и его противника если его имя содержится в передаваемой строке
        /// </summary>
        /// <param name="s">сообщение от пользователя</param>
        /// <param name="userPars">список пар игаков</param>
        private static void DeleteUser(string s, ref List<UserPar> userPars)
        {
            string[] strs = s.Split('|');
            foreach (var item in userPars)
            {
                if (item.UserNameX == strs[0])
                    DeletePlayerPar(item);
                if (item.UserNameO == strs[0])
                    DeletePlayerPar(item);
            }
        }

        /// <summary>
        /// Удаляет пару играков из списка играющих
        /// </summary>
        /// <param name="item">пара играков каторую нужно удалить</param>
        private static void DeletePlayerPar(UserPar item)
        {
            item.UserNameO = "OOO";
            Send(item.CLientO?.GetStream(), "Close");
            item.CLientO = null;

            item.UserNameX = "XXX";
            Send(item.CLientX?.GetStream(), "Close");
            item.CLientX = null;
        }
    }
}