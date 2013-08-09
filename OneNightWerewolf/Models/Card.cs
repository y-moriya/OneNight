using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneNightWerewolf.Models
{
    public abstract class Card
    {
        public int CardId { get; private set; }
        public string CardName { get; private set; }

        public Card()
        {
        }

        public Card(int cardId, string cardName)
        {
            this.CardId = cardId;
            this.CardName = cardName;
        }
    }

    public static class CardFactory
    {
        public const int VILLAGER = 1;
        public const int SEER = 2;
        public const int THIEF = 3;
        public const int WEREWOLF = 4;
        public const int LOONY = 5;
        public const string SEPARATOR = ",";

        public static Card CreateCardFromCardId(int id)
        {
            switch (id)
            {
                case SEER:
                    return new SeerCard();
                case THIEF:
                    return new ThiefCard();
                case WEREWOLF:
                    return new WerewolfCard();
                case VILLAGER:
                    return new VillagerCard();
                case LOONY:
                    return new LoonyCard();
                default:
                    return null;
            }
        }

        public static string CreateCardsString(List<Card> cards)
        {
            return string.Join(SEPARATOR, cards.Select(c => c.CardId));
        }

        public static List<Card> CreateCardsFromString(string cards)
        {
            if (string.IsNullOrEmpty(cards))
            {
                return new List<Card>();
            }
            List<int> cardIds = CreateCardIdsFromString(cards);
            return cardIds.Select(id => CreateCardFromCardId(id)).ToList();
        }

        public static List<int> CreateCardIdsFromString(string cards)
        {
            string[] cardIds = cards.Split(SEPARATOR.ToCharArray());
            return cardIds.Select(s => int.Parse(s)).ToList();
        }

        public static string CreateCardIdSetString(int playerNum)
        {
            List<int> cardIdSet = CreateCardIdSet(playerNum);
            return string.Join(SEPARATOR, cardIdSet);
        }

        private static List<int> CreateCardIdSet(int playerNum)
        {
            int cardsNum = playerNum + 2;
            if (cardsNum > 8)
            {
                cardsNum = 8;
            }

            List<int> cardIdSet = new List<int>(cardsNum);

            cardIdSet.Add(WEREWOLF);
            cardIdSet.Add(LOONY);
            //cardIdSet.Add(WEREWOLF);
            cardIdSet.Add(SEER);
            cardIdSet.Add(THIEF);
            for (int i = 0; i < cardsNum - 4; i++)
            {
                cardIdSet.Add(VILLAGER);
            }

            return cardIdSet.OrderBy(i => Guid.NewGuid()).ToList();
        }
    }

    public class VillagerCard : Card
    {
        public VillagerCard()
            : base(CardFactory.VILLAGER, "村人")
        { }
    }

    public class SeerCard : Card
    {
        public SeerCard()
            : base(CardFactory.SEER, "占い師")
        { }
    }

    public class ThiefCard : Card
    {
        public ThiefCard()
            : base(CardFactory.THIEF, "怪盗")
        { }
    }

    public class WerewolfCard : Card
    {
        public WerewolfCard()
            : base(CardFactory.WEREWOLF, "人狼")
        { }
    }

    public class LoonyCard : Card
    {
        public LoonyCard()
            : base(CardFactory.LOONY, "狂人")
        { }
    }
}