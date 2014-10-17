using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneNightWerewolf.Models
{
    public class Record
    {
        public string UserName { get; set; }
        private IEnumerable<Player> players;
        private IEnumerable<Game> games;

        public Record(string username, IEnumerable<Player> players, IEnumerable<Game> games)
        {
            this.UserName = username;
            this.players = players;
            this.games = games;
        }

        public IEnumerable<Game> GetGames()
        {
            return this.games;
        }
    }
}