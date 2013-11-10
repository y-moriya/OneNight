using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OneNightWerewolf.Models;

namespace OneNightWerewolf.Hubs
{
    public class GamePipelineModule : HubPipelineModule
    {
        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            System.Diagnostics.Trace.TraceError(exceptionContext.Error.Message);
            System.Diagnostics.Trace.TraceError(exceptionContext.Error.StackTrace);

            invokerContext.Hub.Clients.Caller.Error();
        
 	    base.OnIncomingError(exceptionContext, invokerContext);
        }
    }

    [HubName("game")]
    public class GameHub : Hub
    {
        public void Start(int gameId)
        {
            var game = new GameModel(gameId);
            string msg = string.Empty;

            if (game.Game.Phase > Phase.Prologue)
            {
                return;
            }
            game.UpdatePhase();

            if (game.Game.Phase == Phase.Prologue)
            {
                msg = "ゲームを開始出来ませんでした。";
                Clients.Caller.Reload(msg);
                return;
            }

            msg = "ゲームが開始されました。画面を更新します。";
            Clients.Group(gameId.ToString()).Reload(msg);
        }

        public void Commit(int gameId, int playerId)
        {
            var game = new GameModel(gameId);

            if (game.Game.Phase != Phase.Day)
            {
                return;
            }

            var commited = game.Commit(playerId);
            if (game.DoUpdate())
            {
                game.UpdatePhase();
                string msg = "時間が進められました。画面を更新します。";
                Clients.Group(gameId.ToString()).Reload(msg);
            }
            else
            {
                Clients.Caller.Toggle(commited);
            }
        }

        public void Join(int gameId, string playerName, bool reload)
        {
            var game = new GameModel(gameId);
            Groups.Add(Context.ConnectionId, gameId.ToString());
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            if (game.Game.Phase == Phase.Prologue && !reload)
            {
                Clients.OthersInGroup(gameId.ToString()).Info(game.GetPlayersInformation());
                //Clients.OthersInGroup(gameId.ToString()).Recieve("system", "System", string.Format("{0} が参加しました。", playerName), now.ToString());
            }
        }

        public void Exit(int gameId, int playerId, string playerName)
        {
            var game = new GameModel(gameId);
            game.RemovePlayer(playerId);

            Groups.Remove(Context.ConnectionId, gameId.ToString());
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");

            Clients.Group(gameId.ToString()).Info(game.GetPlayersInformation());
            //Clients.Group(gameId.ToString()).Recieve("system", "System", string.Format("{0} が退出しました。", playerName), now.ToString());
            Clients.Caller.Reload(string.Empty);
        }

        public void SendMessage(int gameId, int playerId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var game = new GameModel(gameId);

            if (game.Game.Phase == Phase.Voting)
            {
                string msg = "時間が進められました。画面を更新します。";
                Clients.Group(gameId.ToString()).Reload(msg);
                return;
            }

            var player = game.Players.Find(p => p.Player.PlayerId == playerId);
            var message = player.CreateMessage(content);

            game.SendMessage(message);

            //Clients.Group(gameId.ToString()).Recieve(message.GetMessageTypeForClass(), message.PlayerName, message.Content, message.CreatedAt.ToString());
        }

        public void GetMessages(int gameId, int playerId, int messageId)
        {
            var game = new GameModel(gameId);
            var messages = game.GetMessages(playerId, messageId);

            if (messages.Count == 0)
            {
                Clients.Caller.UpdateMax(messageId);
                return;
            }

            var max = messages.Max(m => m.MessageId);
            var json = GetJsonMessages(messages);

            Clients.Caller.WriteMsg(json, max);
        }

        private string GetJsonMessages(List<Message> messages)
        {
            var objects = messages.OrderBy(m => m.MessageId)
                        .Select(m =>
                        new JsonMessage()
                        {
                            Type = m.GetMessageTypeForClass(),
                            Name = m.PlayerName,
                            Content = m.Content,
                            Date = m.CreatedAt.ToString(),
                            UserName = m.PlayerUserName,
                            IconUri = m.IconUri
                        }).ToList();
            return System.Web.Helpers.Json.Encode(objects);
        }

        public void CheckUpdate(int gameId, int phase)
        {
            var game = new GameModel(gameId);

            if ((int)game.Game.Phase > phase)
            {
                string msg = "時間が進められました。画面を更新します。";
                Clients.Caller.Reload(msg);
                return;
            }

            Clients.Caller.Adjust(game.GetLeftTime().ToString(@"%m'分 '%s'秒'"));
        }
    }

    public class JsonMessage
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
        public string IconUri { get; set; }
    }
}