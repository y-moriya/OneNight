using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OneNightWerewolf.Models
{
    public class Entry
    {
        public UserProfile UserProfile { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }

        [Key]
        [ForeignKey("UserProfile")]
        [Column(Order = 0)]
        public int UserId { get; set; }

        [Key]
        [ForeignKey("Game")]
        [Column(Order = 1)]
        public int GameId { get; set; }

        [ForeignKey("Player")]
        [Column(Order = 2)]
        public int PlayerId { get; set; }


    }
}