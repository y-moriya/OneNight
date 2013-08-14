using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OneNightWerewolf.Models
{
    [Table("Players")]
    public class Player
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int OriginalCardId { get; set; }
        public int CurrentCardId { get; set; }
        public int VotePlayerId { get; set; }
        public int GameId { get; set; }
        public bool Won { get; set; }
        public bool Executed { get; set; }
        public bool Commited { get; set; }

        public int SkillTarget { get; set; }
        public string SkillResult { get; set; }

        public List<int> GetSkillResults()
        {
            if (string.IsNullOrEmpty(this.SkillResult))
            {
                return new List<int>();
            }
            return CardFactory.CreateCardIdsFromString(this.SkillResult);
        }

        public void AddSkillResult(int cardId)
        {
            var results = this.GetSkillResults();
            results.Add(cardId);
            this.SkillResult = string.Join(CardFactory.SEPARATOR, results);
        }
    }

    public class PlayerModel
    {
        public GameModel Game { get; set; }
        public Player Player { get; set; }
        public Card OriginalCard { get; private set; }
        public Card CurrentCard { get; private set; }
        public int VotedCount { get; set; }

        public PlayerModel(string name)
        {
            this.Player = new Player();
            this.Player.PlayerName = name;
        }

        public PlayerModel(Player p, GameModel game)
        {
            this.Player = p;
            this.Game = game;
            this.OriginalCard = CardFactory.CreateCardFromCardId(p.OriginalCardId);
            this.CurrentCard = CardFactory.CreateCardFromCardId(p.CurrentCardId);
        }

        public bool IsWolfTeam()
        {
            return (this.CurrentCard is WerewolfCard || this.CurrentCard is LoonyCard);
        }

        public void SetOriginalCard(Card card)
        {
            this.OriginalCard = card;
            this.Player.OriginalCardId = card.CardId;
        }

        public void SetCurrentCard(Card card)
        {
            this.CurrentCard = card;
            this.Player.CurrentCardId = card.CardId;
        }

        public void Vote(int targetId)
        {
            this.Player.VotePlayerId = targetId;
        }

        public bool CanVote()
        {
            if (this.Game.Game.Phase != Phase.Voting)
            {
                return false;
            }

            if (this.Player.VotePlayerId > 0)
            {
                return false;
            }

            return true;
        }

        public bool CanUseSkill()
        {
            if (this.Game.Game.Phase != Phase.Night)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(this.Player.SkillResult))
            {
                return false;
            }

            if (this.OriginalCard is VillagerCard || this.OriginalCard is LoonyCard)
            {
                return false;
            }

            return true;
        }

        public bool CanCommit()
        {
            if (this.Game.Game.Phase != Phase.Day)
            {
                return false;
            }

            return true;
        }

        public bool CanSendMessage()
        {
            if (this.Game.Game.Phase == Phase.Night ||
                this.Game.Game.Phase == Phase.Voting ||
                this.Game.Game.Phase == Phase.Close)
            {
                return false;
            }

            return true;
        }

        public Message CreateMessage()
        {
            return new Message()
            {
                GameId = this.Player.GameId,
                MessageType = MessageType.Player,
                PlayerId = this.Player.PlayerId,
                PlayerName = this.Player.PlayerName
            };
        }

        public Message CreateMessage(string content)
        {
            var message = this.CreateMessage();
            message.Content = content;

            return message;
        }

        public SelectList CreateVoteTarget()
        {
            var targets = this.Game.Players.Where(p => p.Player.PlayerId != this.Player.PlayerId);

            return new SelectList(targets, "Player.PlayerId", "Player.PlayerName");
        }

        public List<SelectListItem> CreateSkillTarget()
        {
            var targets = this.Game.Players.Where(p => p.Player.PlayerId != this.Player.PlayerId)
                                           .Select(p => new SelectListItem()
                                           {
                                               Text = p.Player.PlayerName,
                                               Value = p.Player.PlayerId.ToString()
                                           }).ToList();

            if (this.OriginalCard is SeerCard)
            {
                targets.Add(new SelectListItem()
                {
                    Text = "伏せカード",
                    Value = "-1"
                });
            }
            else if (this.OriginalCard is ThiefCard)
            {
                targets.Add(new SelectListItem()
                {
                    Text = "交換しない",
                    Value = "-1"
                });
            }

            return targets;
        }

        public string GetCardNameForPlayer()
        {
            if (this.Game.Game.Phase == Phase.Prologue)
            {
                return string.Empty;
            }
            else if (this.Game.Game.Phase < Phase.Epilogue)
            {
                if (this.OriginalCard is ThiefCard)
                {
                    if (this.CurrentCard != null)
                    {
                        return string.Format("{0} （元{1}）", this.CurrentCard.CardName, this.OriginalCard.CardName);
                    }
                }
            }
            else
            {
                if (this.OriginalCard == null || this.CurrentCard == null)
                {
                    return string.Empty;
                }
                if (this.OriginalCard.CardId != this.CurrentCard.CardId)
                {
                    return string.Format("{0} （元{1}）", this.CurrentCard.CardName, this.OriginalCard.CardName);
                }
            }

            return this.OriginalCard.CardName;
        }
    }
}