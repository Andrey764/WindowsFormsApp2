//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Server
{
    using System;
    using System.Collections.Generic;
    
    public partial class Games
    {
        public int ID { get; set; }
        public string Gamer1 { get; set; }
        public string Gamer2 { get; set; }
        public string GameName { get; set; }
        public string Winner { get; set; }
        public override string ToString()
        {
            return $"{GameName} :{Gamer1} против {Gamer2}";
        }
    }
}
