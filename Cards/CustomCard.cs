using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoundsModLoader.Cards
{
    public abstract class CustomCard : MonoBehaviour
    {
        public static List<CardInfo> cards = new List<CardInfo>();

        public CardInfo cardInfo;
        public Gun gun;
        public ApplyCardStats cardStats;
        public CharacterStatModifiers statModifiers;

        void Awake()
        {
            cardInfo = GetComponent<CardInfo>();
            gun = GetComponent<Gun>();
            cardStats = GetComponent<ApplyCardStats>();
            statModifiers = GetComponent<CharacterStatModifiers>();
            SetupCard(cardInfo, gun, cardStats, statModifiers);
        }
        
        protected abstract string GetTitle();
        protected abstract string GetDescription();
        protected abstract CardInfoStat[] GetStats();
        protected abstract CardInfo.Rarity GetRarity();
        protected abstract CardThemeColor.CardThemeColorType GetTheme();
        public abstract void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers);
        public abstract void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats);
        public abstract void OnRemoveCard();
        
        public static CustomCard BuildCard<T>() where T : CustomCard
        {
            // Instantiate card and mark to avoid destruction on scene change
            var newCard = Instantiate(ModLoader.templateCard.gameObject, Vector3.up * 100, Quaternion.identity);
            newCard.transform.SetParent(null, true);
            var newCardInfo = newCard.GetComponent<CardInfo>();
            DontDestroyOnLoad(newCard);

            // Add custom ability handler
            var customCard = newCard.AddComponent<T>();

            // Remove superfluous card base
            newCardInfo.ExecuteAfterFrames(5, () =>
            {
                Destroy(newCard.transform.GetChild(0).gameObject);
            });
            Utilities.DestroyChildren(newCardInfo.cardBase.GetComponent<CardInfoDisplayer>().grid);

            // Apply card data
            newCardInfo.cardStats = customCard.GetStats();
            newCard.gameObject.name = newCardInfo.cardName = customCard.GetTitle();
            newCardInfo.cardDestription = customCard.GetDescription();
            newCardInfo.sourceCard = newCardInfo;
            newCardInfo.rarity = customCard.GetRarity();
            newCardInfo.colorTheme = customCard.GetTheme();
            newCardInfo.allowMultiple = true;
            
            // Remove card art
            newCardInfo.cardArt = null;

            // Fix sort order issue
            newCardInfo.cardBase.transform.position -= Camera.main.transform.forward * 0.5f;

            // Reset stats
            newCard.GetComponent<CharacterStatModifiers>().health = 1;

            // Finish initializing
            newCardInfo.SendMessage("Awake");
            PhotonNetwork.PrefabPool.RegisterPrefab(newCard.gameObject.name, newCard);

            // Add this card to the list of all custom cards
            ModLoader.moddedCards.Add(newCardInfo);

            return customCard;
        }
    }
}
