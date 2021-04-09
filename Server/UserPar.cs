using System.Net.Sockets;

namespace Server
{
    internal class UserPar
    {
        public string gameName { get; set; }
        public string UserNameX { get; set; }
        public string UserNameO { get; set; }
        public TcpClient CLientX { get; set; }
        public TcpClient CLientO { get; set; }

        public UserPar()
        {
            UserNameX = "XXX";
            UserNameO = "OOO";
        }
    }
}