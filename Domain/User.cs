using System;
using System.Collections.Generic;
using System.Text;


namespace Domain
{
    public class User
    {
        private static int idCounter = 0;

        public int Id { get; }

        public User(int id)
        {
            this.Id = id;
        }

        public User() : this(++idCounter)
        { }

        public string Name { get; set; }

        public List<Game> PurchasedGame { get; set; } = new List<Game>();

        public List<Game> Games { get; set; } = new List<Game>();

        public bool AddPurchasedGame(Game game)
        {
            PurchasedGame.Add(game);
            return true;
        }

        public override string ToString()
        {
            return $"\tName:{Name}";
        }


    }
}
