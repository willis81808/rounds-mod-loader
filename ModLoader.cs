using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Photon.Pun;
using HarmonyLib;
using RoundsMDK;

namespace RoundsModLoader
{
    public class ModLoader : MonoBehaviourPunCallbacks
    {
        private static ModLoader _instance;
        public static ModLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Mod Loader Manager").AddComponent<ModLoader>();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
        
        private Canvas _canvas;
        public Canvas canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = new GameObject("").AddComponent<Canvas>();
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _canvas.pixelPerfect = false;
                    DontDestroyOnLoad(_canvas);
                }
                return _canvas;
            }
        }

        private static bool initialized = false;
        private static string MOD_DIRECTORY { get { return $@"{Application.dataPath}/Mods"; } }

        internal static CardInfo templateCard;
        internal static CardInfo[] defaultCards;
        internal static List<CardInfo> moddedCards = new List<CardInfo>();

        struct NetworkEventType
        {
            public const string
                StartHandshake = "ModLoader_HandshakeStart",
                FinishHandshake = "ModLoader_HandshakeFinish";
        }

        public static bool IsInitialized()
        {
            return initialized;
        }
        public static void Initialize()
        {
            BuildInfoPopup("Mod Loader Initialized");
            initialized = true;

            // Create mod directory
            if (!Directory.Exists(MOD_DIRECTORY))
            {
                Directory.CreateDirectory(MOD_DIRECTORY);
            }

            // Patch game with Harmony
            var harmony = new Harmony("com.willis.modloader");
            harmony.PatchAll();

            // Initialize mods
            Instance.ExecuteAfterSeconds(1, () =>
            {
                InitializeMods(Directory.GetFiles(MOD_DIRECTORY));
            });

            // fetch card to use as a template for all custom cards
            templateCard = (from c in CardChoice.instance.cards
                            where c.cardName.ToLower() == "huge"
                            select c).FirstOrDefault();

            defaultCards = CardChoice.instance.cards;
            moddedCards.AddRange(defaultCards);
        }

        public override void OnJoinedRoom()
        {
            BuildInfoPopup("Mod handshake requested");
            NetworkingManager.RaiseEventOthers(NetworkEventType.StartHandshake);
        }
        public override void OnLeftRoom()
        {
            CardChoice.instance.cards = defaultCards;
        }

        void Awake()
        {
            defaultCards = CardChoice.instance.cards;

            // request mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.StartHandshake, (data) =>
            {
                NetworkingManager.RaiseEvent(NetworkEventType.FinishHandshake);
                CardChoice.instance.cards = defaultCards;
                BuildInfoPopup("Mod handshake recieved");
            });
            // recieve mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.FinishHandshake, (data) =>
            {
                CardChoice.instance.cards = moddedCards.ToArray();
                BuildInfoPopup("Mod handshake finished");
            });
        }

        void Update()
        {
            if (GameManager.instance.isPlaying && PhotonNetwork.OfflineMode && CardChoice.instance.cards == defaultCards)
            {
                CardChoice.instance.cards = moddedCards.ToArray();
            }
        }
        
        public static void BuildInfoPopup(string message)
        {
            var popup = new GameObject("Info Popup").AddComponent<InfoPopup>();
            popup.rectTransform.SetParent(Instance.canvas.transform);
            popup.Build(message);
        }
        
        private static void InitializeMods(string[] paths)
        {
            // Load mods
            int count = 0;
            foreach (var path in paths)
            {
                var dll = Assembly.LoadFrom(path);
                foreach (var type in dll.GetExportedTypes())
                {
                    if (typeof(IMod).IsAssignableFrom(type) && type.Name != "IMod")
                    {
                        var modEntryPoint = Activator.CreateInstance(type);
                        var method = type.GetMethod("Initialize");
                        string result = (string)method.Invoke(modEntryPoint, null);

                        Instance.ExecuteAfterSeconds(count / 3f, () =>
                        {
                            BuildInfoPopup(result);
                        });

                        count++;
                    }
                }
            }
        }
    }
}
