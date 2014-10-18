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

        private Game GetGame(int gameId)
        {
            return this.games.First(g => g.GameId == gameId);
        }

        public Player GetPlayer(int gameId)
        {
            return this.players.First(p => p.GameId == gameId);
        }

        public int CountGames(bool? win = null, bool? wolfside = null, bool? wolf = null, bool? villager = null, bool? seer = null,
            bool? thief = null, bool? loony = null, bool deleted = false)
        {
            var pl = this.players;
            if (win.HasValue)
            {
                pl = pl.Where(p => p.Won == win.Value);
            }
            if (wolfside.HasValue)
            {
                pl = pl.Where(p => IsWolfSide(p) == wolfside.Value);
            }
            if (wolf.HasValue)
            {
                pl = pl.Where(p => ((p.CurrentCardId == CardFactory.WEREWOLF) == wolf.Value));
            }
            if (villager.HasValue)
            {
                pl = pl.Where(p => ((p.CurrentCardId == CardFactory.VILLAGER) == villager.Value));
            }
            if (seer.HasValue)
            {
                pl = pl.Where(p => ((p.CurrentCardId == CardFactory.SEER) == seer.Value));
            }
            if (thief.HasValue)
            {
                pl = pl.Where(p => ((p.CurrentCardId == CardFactory.THIEF) == thief.Value));
            }
            if (loony.HasValue)
            {
                pl = pl.Where(p => ((p.CurrentCardId == CardFactory.LOONY) == loony.Value));
            }
            return pl.Count();
        }

        private bool IsWolfSide(Player p)
        {
            return (p.CurrentCardId == CardFactory.LOONY || p.CurrentCardId == CardFactory.WEREWOLF);
        }
    }
}