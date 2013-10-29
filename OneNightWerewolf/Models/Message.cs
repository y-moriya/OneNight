using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OneNightWerewolf.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Nullable<DateTime> DeletedAt { get; set; }

        public Game Game { get; set; }

        [ForeignKey("Game")]
        public int GameId { get; set; }

        public int PlayerId { get; set; }
        /// <summary>
        /// 一意なユーザー名
        /// </summary>
        public string PlayerUserName { get; set; }
        /// <summary>
        /// 表示用の名称
        /// </summary>
        public string PlayerName { get; set; }
        public string IconUri { get; set; }
        public MessageType MessageType { get; set; }

        public Message()
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");
            this.CreatedAt = now;
        }

        public string GetMessageTypeForClass()
        {
            switch (this.MessageType)
            {
                case MessageType.System:
                    return "system";
                case MessageType.Debug:
                    return "debug";
                case MessageType.Player:
                    return "player";
                case MessageType.Secret:
                    return "secret";
                default:
                    return "default";
            }
        }

        public bool Visible(int playerId, Phase phase)
        {
            switch (this.MessageType)
            {
                case MessageType.System:
                    return true;
                case MessageType.Debug:
                    return false;
                case MessageType.Player:
                    return true;
                case MessageType.Secret:
                    return (!(phase < Phase.Epilogue && (this.PlayerId != playerId)));
                default:
                    return false;
            }

        }
    }

    public enum MessageType
    {
        System,
        Debug,
        Player,
        Secret
    }
}