using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OneNightWerewolf.Models
{
    public class GamesContext : DbContext
    {
        public GamesContext()
            : base("DefaultConnection")
        {
        }

        public int GetUserId(string name)
        {
            var user = this.UserProfiles.First(u => u.UserName == name);
            return user.UserId;
        }

        public UserProfile GetUserProfile(string name)
        {
            return this.UserProfiles.First(u => u.UserName == name);
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("Games")]
    public class Game
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }
        public string GameName { get; set; }
        [Display(Name = "参加人数")]
        public int PlayerNum { get; set; }
        [Display(Name = "状態")]
        public Phase Phase { get; set; }
        public string SecretCards { get; set; }
        public string Cards { get; set; }
        public bool WerewolfWon { get; set; }
        public string Creator { get; set; }
        public GameType GameType { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<DateTime> StartedAt { get; set; }
        public Nullable<DateTime> DayAt { get; set; }
        public Nullable<DateTime> JudgeAt { get; set; }
        public Nullable<DateTime> VoteAt { get; set; }
        public Nullable<DateTime> ClosedAt { get; set; }
        public Nullable<DateTime> DeletedAt { get; set; }
        public Nullable<DateTime> NextUpdate { get; set; }

        public Game()
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");
            this.CreatedAt = now;
            this.StartedAt = null;
            this.DayAt = null;
            this.JudgeAt = null;
            this.VoteAt = null;
            this.ClosedAt = null;
            this.DeletedAt = null;
            this.NextUpdate = now.AddMinutes(GameModel.MINUTES_OF_DEL);
            this.GameType = Models.GameType.Advance;
        }

        public string PhaseToString()
        {
            if (this.DeletedAt != null)
            {
                return "削除";
            }

            switch (this.Phase)
            {
                case Phase.Prologue:
                    return "参加者募集中";
                case Phase.Night:
                    return "夜";
                case Phase.Day:
                    return "昼";
                case Phase.Voting:
                    return "投票";
                case Phase.Epilogue:
                    return "決着";
                case Phase.Close:
                    return "終了";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string WonToString()
        {
            if (this.Phase < Phase.Epilogue)
            {
                return string.Empty;
            }

            if (this.WerewolfWon)
            {
                return "人狼勝利";
            }
            else
            {
                return "村人勝利";
            }
        }
    }

    public class GameModel
    {
        #region Properties and Members
        private GamesContext db = new GamesContext();
        private Random random = new Random();
        public Game Game { get; private set; }
        public List<Card> SecretCards { get; private set; }
        private List<Card> cardset;
        public List<PlayerModel> Players { get; set; }
        private List<Message> Messages { get; set; }
        public int PlayerNum { get { return this.Game.PlayerNum; } }
        public const int MAX_PLAYERS = 7;
        public const int MIN_PLAYERS = 3;
        public const int MINUTES_OF_NIGHT = 1;
        public const int MINUTES_OF_DAY = 8;
        public const int MINUTES_OF_VOTE = 1;
        public const int MINUTES_OF_EP = 30;
        public const int MINUTES_OF_DEL = 60;
        #endregion

        #region Constructors
        public GameModel(int gameId)
        {
            this.Game = this.db.Games.Find(gameId);
            this.Initialize();
        }

        private void Initialize()
        {
            this.SecretCards = CardFactory.CreateCardsFromString(this.Game.SecretCards);
            this.cardset = CardFactory.CreateCardsFromString(this.Game.Cards);
            this.Players = GetPlayers();
            this.Messages = this.db.Messages.Where(m => m.GameId == this.Game.GameId).ToList();
            if (this.Messages.Count == 0)
            {
                this.SystemMessage("ゲームが作成されました。１時間以内に開始されなかった場合、自動的に削除されます。");
            }
        }

        public GameModel(string name)
        {
            this.Game = new Game();
            this.Game.GameName = name;
            this.Game.PlayerNum = 0;

            this.db.Games.Add(this.Game);
            this.Game.Phase = Models.Phase.Prologue;

            this.Players = new List<PlayerModel>();
            this.SecretCards = new List<Card>();
            this.cardset = new List<Card>();
            this.Messages = new List<Message>();

            this.db.SaveChanges();
        }
        #endregion

        #region Methods

        #region private utilities
        private List<PlayerModel> GetPlayers()
        {
            var players = this.db.Players.Where(p => p.GameId == this.Game.GameId);
            var result = new List<PlayerModel>(players.Count());
            foreach (Player p in players)
            {
                result.Add(new PlayerModel(p, this));
            }

            return result;
        }

        private PlayerModel GetRandomPlayerWithoutSelf(int selfId)
        {
            var targets = this.Players.Where(p => p.Player.PlayerId != selfId);
            return targets.ElementAt(random.Next(targets.Count()));
        }

        private int GetRandomSkillTarget(int selfId)
        {
            var targets = this.Players.Where(p => p.Player.PlayerId != selfId).Select(p => p.Player.PlayerId).ToList();
            targets.Add(-1);

            return targets[random.Next(targets.Count())];
        }
        #endregion

        #region Prologue
        public bool CanEntry()
        {
            return (this.Game.Phase == Models.Phase.Prologue && this.PlayerNum < MAX_PLAYERS);
        }
        private bool CanAddPlayer(PlayerModel p)
        {
            if (this.Game.Phase != Models.Phase.Prologue)
            {
                return false;
            }

            if (this.Players.Any(pl => pl.Player.PlayerUserName == p.Player.PlayerUserName))
            {
                return false;
            }

            if (this.PlayerNum > MAX_PLAYERS)
            {
                return false;
            }

            return true;
        }
        public bool AddPlayer(PlayerModel p)
        {
            if (!CanAddPlayer(p))
            {
                return false;
            }

            p.Player.GameId = this.Game.GameId;
            this.db.Players.Add(p.Player);
            this.Players.Add(p); // not need?
            this.Game.PlayerNum++;


            this.db.SaveChanges();

            int userId = this.db.GetUserId(p.Player.PlayerUserName);
            Entry entry = new Entry()
            {
                GameId = this.Game.GameId,
                UserId = userId,
                PlayerId = p.Player.PlayerId
            };

            this.db.Entries.Add(entry);
            this.db.SaveChanges();

            this.SystemMessage(string.Format("{0} が参加しました。", p.Player.PlayerUserName));

            return true;
        }

        public void RemovePlayer(int playerId)
        {
            if (this.Game.Phase != Models.Phase.Prologue)
            {
                return;
            }

            var target = this.Players.Find(p => p.Player.PlayerId == playerId);

            if (target == null)
            {
                return;
            }

            this.Players.Remove(target);
            this.db.Players.Remove(target.Player);
            this.Game.PlayerNum--;

            this.db.SaveChanges();

            this.SystemMessage(string.Format("{0} が退出しました。", target.Player.PlayerUserName));
        }

        public string GetPlayersInformation()
        {
            return string.Format("参加者 {0} 名：{1}", this.PlayerNum,
                string.Join("、", this.Players.Select(p => p.Player.PlayerUserName)));
        }

        private void StartGame()
        {
            this.Game.Cards = CardFactory.CreateCardIdSetString(this.PlayerNum);
            this.cardset = CardFactory.CreateCardsFromString(this.Game.Cards);
            this.DealCards();
            this.ShowCardMessage();

            this.SystemMessage("ゲームをスタートしました。夜時間を開始します。");
        }

        private void DealCards()
        {
            for (int i = 0; i < this.cardset.Count; i++)
            {
                if (i < this.Players.Count())
                {
                    this.Players[i].SetOriginalCard(this.cardset[i]);
                    this.TraceLog(string.Format("{0} に {1} を割り当てました。", this.Players[i].Player.PlayerUserName,
                                                                                    this.cardset[i].CardName));
                }
                else
                {
                    this.SecretCards.Add(this.cardset[i]);
                    this.TraceLog(string.Format("伏せカードに {0} を割り当てました。", this.cardset[i].CardName));
                }
            }

            this.Game.SecretCards = CardFactory.CreateCardsString(this.SecretCards);
        }

        private void ShowCardMessage()
        {
            this.SystemMessage(string.Format("このゲームの編成は {0} です。そのうち {1} 枚が伏せられています。",
                string.Join("、", this.cardset.OrderBy(c => c.CardId).Select(c => c.CardName)), this.SecretCards.Count));
        }
        #endregion

        #region Night
        public void UseWerewolfSkill(PlayerModel player,int playerId, int targetId)
        {
            if (this.cardset.Count(c => c is WerewolfCard) < 2)
            {
                // 人狼の総数が1枚なら確認は不要
                return;
            }

            var target = this.Players.FirstOrDefault(p =>
                p.OriginalCard is WerewolfCard && p.Player.PlayerId != playerId);
            if (target != null)
            {
                player.Player.SkillTarget = target.Player.PlayerId;
                player.Player.AddSkillResult(target.OriginalCard.CardId);

                this.SecretMessage(player, string.Format("あなたの仲間は {0} です。", target.Player.PlayerUserName));

                this.TraceLog(string.Format("人狼 {0} が {1} を仲間として認識しました。", player.Player.PlayerUserName, target.Player.PlayerUserName));
            }
            else
            {
                player.Player.SkillTarget = -1;
                player.Player.AddSkillResult(-1);
                this.SecretMessage(player, "あなたの仲間はいませんでした。");
                this.TraceLog(string.Format("人狼 {0} には仲間がいませんでした。", player.Player.PlayerUserName));
            }
        }

        public void UseSeerSkill(PlayerModel player, int playerId, int targetId)
        {
            if (targetId < 1)
            {
                player.Player.SkillTarget = targetId;
                foreach (Card secret in this.SecretCards)
                {
                    player.Player.AddSkillResult(secret.CardId);
                    this.SecretMessage(player, string.Format("伏せカードは {0} でした。", secret.CardName));

                    // 伏せカードのうち、1枚だけ占う
                    //break;
                }

                this.TraceLog(string.Format("占い師 {0} が伏せカードを占いました。", player.Player.PlayerUserName));
            }
            else
            {
                var target = this.Players.Find(p => p.Player.PlayerId == targetId);
                player.Player.SkillTarget = target.Player.PlayerId;
                player.Player.AddSkillResult(target.OriginalCard.CardId);

                this.SecretMessage(player, string.Format("{0} は {1} でした。", target.Player.PlayerUserName, target.OriginalCard.CardName));
                this.TraceLog(string.Format("占い師 {0} が {1} を占い、結果は {2} でした。", player.Player.PlayerUserName, target.Player.PlayerUserName, target.OriginalCard.CardName));
            }
        }

        public void UseThiefSkill(PlayerModel player, int playerId, int targetId)
        {
            // 怪盗は交換しないこともできる。
            if (targetId < 1)
            {
                player.Player.SkillTarget = targetId;
                player.Player.AddSkillResult(player.Player.OriginalCardId);
                player.SetCurrentCard(player.OriginalCard);

                this.SecretMessage(player, "あなたはカードを交換しませんでした。");
                this.TraceLog(string.Format("怪盗 {0} がカード交換しないことを選択しました。", player.Player.PlayerUserName));
            }
            else
            {
                var target = this.Players.Find(p => p.Player.PlayerId == targetId);
                player.Player.SkillTarget = target.Player.PlayerId;
                player.Player.AddSkillResult(target.OriginalCard.CardId);
                player.SetCurrentCard(target.OriginalCard);
                target.SetCurrentCard(player.OriginalCard);

                this.SecretMessage(player, string.Format("あなたは {0} とカードを交換し、 {1} になりました。", target.Player.PlayerUserName, target.OriginalCard.CardName));
                this.TraceLog(string.Format("怪盗 {0} が {1} とカードを交換しました。", player.Player.PlayerUserName, target.Player.PlayerUserName));
                this.TraceLog(string.Format("{0} のカードは {1} です。", player.Player.PlayerUserName, player.CurrentCard.CardName));
                this.TraceLog(string.Format("{0} のカードは {1} です。", target.Player.PlayerUserName, target.CurrentCard.CardName));
            }
        }

        public void UseSkill(int playerId, int targetId)
        {
            if (this.Game.Phase != Phase.Night)
            {
                return;
            }

            var player = this.Players.Find(p => p.Player.PlayerId == playerId);

            if (player.Player.GetSkillResults().Count > 0)
            {
                return;
            }

            if (player.OriginalCard is WerewolfCard)
            {
                UseWerewolfSkill(player, playerId, targetId);
            }
            else if (player.OriginalCard is SeerCard)
            {
                UseSeerSkill(player, playerId, targetId);
            }
            else if (player.OriginalCard is ThiefCard)
            {
                UseThiefSkill(player, playerId, targetId);
            }

            this.db.SaveChanges();
        }
        #endregion

        #region Day
        private void StartDay()
        {
            foreach (PlayerModel p in this.Players)
            {
                if (p.Player.GetSkillResults().Count == 0)
                {
                    var targetId = this.GetRandomSkillTarget(p.Player.PlayerId);
                    this.UseSkill(p.Player.PlayerId, targetId);
                }
            }

            foreach (PlayerModel p in this.Players)
            {
                if (p.CurrentCard == null)
                {
                    p.SetCurrentCard(p.OriginalCard);
                }
            }

            this.SystemMessage("すべての能力が使用され、夜時間が終了しました。昼時間を開始します。");
        }

        public bool Commit(int playerId)
        {
            var player = db.Players.Find(playerId);
            player.Commited = !player.Commited;
            db.SaveChanges();

            return player.Commited;
        }
        #endregion

        #region Vote
        private void StartVote()
        {
            this.SystemMessage("昼時間が終了しました。投票時間を開始します。");
        }

        public void Vote(int playerId, int targetId)
        {
            if (this.Game.Phase != Phase.Voting)
            {
                return;
            }

            var player = this.Players.Find(p => p.Player.PlayerId == playerId);
            var target = this.Players.Find(p => p.Player.PlayerId == targetId);
            player.Vote(target.Player.PlayerId);

            this.SecretMessage(player, string.Format("あなたは {0} に投票しました。", target.Player.PlayerUserName));
            this.TraceLog(string.Format("{0} が {1} に投票しました。", player.Player.PlayerUserName, target.Player.PlayerUserName));

            this.db.SaveChanges();
        }

        private void RandomVote(PlayerModel player)
        {
            var t = GetRandomPlayerWithoutSelf(player.Player.PlayerId);
            player.Vote(t.Player.PlayerId);
            this.SecretMessage(player, string.Format("あなたは {0} にランダム投票しました。", t.Player.PlayerUserName));
            this.TraceLog(string.Format("{0} が {1} にランダム投票しました。", player.Player.PlayerUserName, t.Player.PlayerUserName));
        }

        #endregion

        #region Judgement
        private void Judgement()
        {
            // 未投票者はランダム投票
            foreach (PlayerModel player in this.Players.Where(p => p.Player.VotePlayerId < 1))
            {
                RandomVote(player);
            }

            this.SystemMessage("投票時間が終了しました。処刑者を決定し、勝敗判定を行います");

            // 処刑者決定
            var executed = this.JudgeExecution();
            foreach (PlayerModel ex in executed)
            {
                this.SystemMessage(string.Format("投票の結果、{0} ({1}) が処刑されました。", ex.Player.PlayerUserName, ex.CurrentCard.CardName));
            }


            // 勝敗判定
            this.Game.WerewolfWon = this.JudgeWerewolfVictory(executed);
            string won = this.Game.WerewolfWon ? "人狼" : "村人";
            this.SystemMessage(string.Format("{0} が勝利しました。", won));

            // プレイヤーに勝敗記録
            foreach (PlayerModel player in this.Players)
            {
                player.Player.Won = (player.IsWolfTeam() == this.Game.WerewolfWon);
            }
        }

        private List<PlayerModel> JudgeExecution()
        {
            List<PlayerModel> result = new List<PlayerModel>();

            // 集計
            foreach (PlayerModel player in this.Players)
            {
                player.VotedCount = this.Players
                    .Where(p => p.Player.VotePlayerId == player.Player.PlayerId).Count();
            }

            int max = this.Players.Max(p => p.VotedCount);

            // 投票がすべてバラバラ
            if (max == 1)
            {
                return result;
            }

            var executed = this.Players.Where(p => p.VotedCount == max).ToList();
            foreach (PlayerModel player in executed)
            {
                player.Player.Executed = true;
            }

            return executed;
        }

        private bool JudgeWerewolfVictory(List<PlayerModel> executed)
        {
            if (executed.Count == 0)
            {
                this.SystemMessage("誰も処刑されませんでした。");
                return this.Players.Any(p => p.CurrentCard is WerewolfCard || p.CurrentCard is LoonyCard);
            }
            else
            {
                if (this.Players.Any(p => p.CurrentCard is WerewolfCard))
                {
                    return !executed.Any(p => p.CurrentCard is WerewolfCard);
                }
                else
                {
                    return !executed.Any(p => p.CurrentCard is LoonyCard);
                }
            }
        }
        #endregion

        #region Close
        private void Close()
        {
            this.SystemMessage("ゲームを終了しました。");
        }
        #endregion

        #region Message
        public Message CreateAdminMessage()
        {
            var m = new Message()
            {
                GameId = this.Game.GameId,
                MessageType = MessageType.Admin,
                PlayerName = "admin",
                PlayerUserName = "管理者",
                PlayerId = -1,
                IconUri = "~/Images/admicon.png"
            };

            return m;
        }

        public Message CreateAdminMessage(string content)
        {
            var m = CreateAdminMessage();
            m.Content = content;

            return m;
        }

        public void SystemMessage(string str)
        {
            var m = new Message()
            {
                Content = str,
                GameId = this.Game.GameId,
                MessageType = MessageType.System,
                PlayerName = "System",
                PlayerUserName = "System",
                IconUri = "~/Images/sysicon.png"
            };

            this.db.Messages.Add(m);
            this.db.SaveChanges();
        }

        public void TraceLog(string str, LogLevel logLevel = LogLevel.Information)
        {
            string message = string.Format("GameId:{0}, Message:{1}", this.Game.GameId, str);

            switch (logLevel)
            {
                case LogLevel.Error:
                    System.Diagnostics.Trace.TraceError(message);
                    break;
                case LogLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(message);
                    break;
                case LogLevel.Information:
                default:
                    System.Diagnostics.Trace.TraceInformation(message);
                    break;
            }
        }

        public void SendMessage(Message message)
        {
            if (this.Game.Phase == Phase.Night ||
                this.Game.Phase == Phase.Voting ||
                this.Game.Phase == Phase.Close)
            {
                return;
            }

            if (message.MessageType == MessageType.Player && !this.Players.Any(p => p.Player.PlayerId == message.PlayerId))
            {
                return;
            }

            this.db.Messages.Add(message);
            this.db.SaveChanges();
        }

        public void SecretMessage(PlayerModel player, string str)
        {
            var m = new Message()
            {
                Content = str,
                GameId = this.Game.GameId,
                MessageType = MessageType.Secret,
                PlayerId = player.Player.PlayerId,
                PlayerName = player.Player.PlayerName,
                PlayerUserName = player.Player.PlayerUserName,
                IconUri = "~/Images/secreticon.png"
            };

            this.db.Messages.Add(m);
            this.db.SaveChanges();
        }

        public List<Message> GetMessages(int playerId)
        {
            return this.Messages.Where(m => m.Visible(playerId, this.Game.Phase))
                                .OrderByDescending(m => m.MessageId).ToList();
        }

        public List<Message> GetMessages(int playerId, int messageId)
        {
            return this.GetMessages(playerId).Where(m => m.MessageId > messageId).ToList();
        }

        #endregion

        #region Update
        public void Delete()
        {
            if (this.Game.Phase != Phase.Prologue)
            {
                return;
            }

            this.SystemMessage("一定時間開始されなかったため、このゲームは終了しました。");

            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            this.Game.NextUpdate = null;
            this.Game.DeletedAt = now;
            this.Game.Phase = Phase.Close;

            this.db.SaveChanges();
        }

        public bool DoUpdate()
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            if (this.Game.NextUpdate.HasValue && this.Game.NextUpdate.Value < now)
            {
                if (this.Game.Phase == Phase.Prologue)
                {
                    this.Delete();
                    return false;
                }
                return true;
            }

            // 夜コミットはメタっぽいので廃止。
            //if (this.Game.Phase == Phase.Night)
            //{
            //    return this.Players.All(p => !p.CanUseSkill());
            //}

            if (this.Game.Phase == Phase.Day)
            {
                return this.Players.All(p => p.Player.Commited);
            }

            if (this.Game.Phase == Phase.Voting)
            {
                return this.Players.All(p => !p.CanVote());
            }

            return false;
        }

        public bool CanUpdate()
        {
            if (this.Game.Phase == Models.Phase.Prologue)
            {
                if (this.PlayerNum < MIN_PLAYERS || MAX_PLAYERS < this.PlayerNum)
                {
                    this.SystemMessage("参加人数が足りていないため、ゲームを開始出来ませんでした。");
                    return false;
                }
            }

            if (this.Game.Phase == Models.Phase.Close)
            {
                return false;
            }

            return true;
        }

        public TimeSpan GetLeftTime()
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");
            if (this.Game.Phase < Phase.Close &&
                this.Game.NextUpdate.HasValue && now < this.Game.NextUpdate.Value)
            {
                return (this.Game.NextUpdate.Value - now);
            }
            else
            {
                return new TimeSpan();
            }
        }

        public void UpdatePhase()
        {
            if (!CanUpdate())
            {
                return;
            }

            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            switch (this.Game.Phase)
            {
                case Phase.Prologue:
                    this.SetTimer(MINUTES_OF_NIGHT);
                    this.Game.StartedAt = now;
                    this.StartGame();
                    break;
                case Phase.Night:
                    this.SetTimer(MINUTES_OF_DAY);
                    this.Game.DayAt = now;
                    this.StartDay();
                    break;
                case Phase.Day:
                    this.SetTimer(MINUTES_OF_VOTE);
                    this.Game.VoteAt = now;
                    this.StartVote();
                    break;
                case Phase.Voting:
                    this.SetTimer(MINUTES_OF_EP);
                    this.Game.JudgeAt = now;
                    this.Judgement();
                    break;
                case Phase.Epilogue:
                    this.Game.ClosedAt = now;
                    this.Close();
                    break;
                default:
                    break;
            }

            this.Game.Phase = this.Game.Phase + 1;

            try
            {
                this.db.SaveChanges();
            }
            catch (Exception e)
            {
                this.TraceLog(e.Message, LogLevel.Error);
                this.TraceLog(e.StackTrace, LogLevel.Error);
            }
        }

        public void SetTimer(int minutes)
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            if (this.Game.NextUpdate == null)
            {
                this.Game.NextUpdate = now;
            }

            //this.Game.NextUpdate = this.Game.NextUpdate.Value.AddMinutes(minutes);
            this.Game.NextUpdate = now.AddMinutes(minutes);
            this.TraceLog(string.Format("次回更新時間を {0} に設定しました。", this.Game.NextUpdate.ToString()));
        }
        #endregion

        #endregion
    }

    public enum Phase
    {
        Prologue,
        Night,
        Day,
        Voting,
        Epilogue,
        Close
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Information
    }

    public enum GameType
    {
        Classic,
        Advance
    }
}