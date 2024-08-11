using System;
using System.Linq;
using UnityEngine;
using Il2CppSystem.Runtime.Remoting.Messaging;


#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif

namespace KingdomMod
{
    public partial class OverlayMap : MonoBehaviour
    {
        public static OverlayMap Instance { get; private set; }

        private readonly GUIStyle guiStyle = new();
        private float timeSinceLastGuiUpdate = 0;
        private bool enabledOverlayMap = true;
        private bool showFullMap = false;
        private GameObject gameLayer = null;
        private static int _campaignIndex = 0;
        private static int _land = 0;
        private static int _challengeId = 0;
        private static string _archiveFilename;
        private static ExploredRegion _exploredRegion;

        private System.Collections.Generic.List<MarkInfo> minimapMarkList = [];
        private System.Collections.Generic.List<LineInfo> drawLineList = [];
        private readonly StatsInfo statsInfo = new();

        private const int boxMargin = 5;

        private const int imageHeight = 30;
        private const int imageHMargin = 3;
        private const int minimapRowMargin = 5;

        private const int minimapLineThickness = 1;
        private const int minimapLineMargin = 1;

        private const int textMargin = 5;
        private float textHeight;

        private float minimapHeight = 0;
        private float extraInfoHeight = 0;

        public OverlayMap()
        {
            try
            {
                guiStyle.alignment = TextAnchor.UpperLeft;
                guiStyle.normal.textColor = Color.white;
                guiStyle.fontSize = 12;
            }
            catch (Exception exception)
            {
                LogUtil.Info(exception);
                throw;
            }
        }

        public static void Initialize(OverlayMapPlugin plugin)
        {
            GlobalConfigs.ConfigBind(plugin.Config);
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<OverlayMap>();
#endif
            GameObject obj = new(nameof(OverlayMap));
            DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<OverlayMap>();
        }

        private void Start()
        {
            LogUtil.Message($"{this.GetType().Name} Start.");
            Patcher.PatchAll(this);
            Game.OnGameStart += (Action)OnGameStart;
            NetworkBigBoss.Instance._postCatchupEvent += (Action)this.OnClientCaughtUp;

            //GlobalSaveData.add_OnCurrentCampaignSwitch((Action)OnCurrentCampaignSwitch);

            // log.LogMessage($"resSet test Alfred: {Strings.Alfred}");
            // log.LogMessage($"resSet test Culture: {Strings.Culture?.Name}");
            // var resSet = Strings.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, true);
            // if (resSet != null)
            // {
            //     var dict = new SortedDictionary<string, string>();
            //     log.LogMessage($"resSet: ");
            //     var defines = "";
            //     var binds = "";
            //     foreach (DictionaryEntry dictionaryEntry in resSet)
            //     {
            //         dict.Add(dictionaryEntry.Key.ToString() ?? "", dictionaryEntry.Value?.ToString() ?? "");
            //         // defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
            //         // binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
            //         
            //         // log.LogMessage($"resSet: {dictionaryEntry.Key}, {dictionaryEntry.Value}");
            //     }
            //
            //     foreach (var dictionaryEntry in dict)
            //     {
            //         defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
            //         binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
            //
            //     }
            //     // log.LogMessage($"defines: {defines}");
            //     // log.LogMessage($"binds: {binds}");
            // }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                foreach(Payable payable in Managers.Inst.payables.AllPayables) {
                    if(payable == null)
                        continue;

                    if(Math.Abs(payable.transform.position.x - Managers.Inst.kingdom.playerOne.transform.position.x) < 1) {
                        LogUtil.Error(payable.tag + " " + payable.GetScriptClassName() + " " + payable.name + " " + payable.GetIl2CppType().FullName + " " + payable.GetComponent<PrefabID>()?.prefabID);

                        foreach(var component in payable.gameObject.GetComponents<Component>()) {
                            LogUtil.Error("    " + component.GetIl2CppType().FullName);
                        }
                    }
                }

                //LogUtil.Message("M key pressed.");
                //enabledOverlayMap = !enabledOverlayMap;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                foreach(Enemy payable in Managers.Inst.enemies.GetFieldOrPropertyValue<HashSet<Enemy>>("_enemies")) {
                    if(payable == null)
                        continue;

                    if(Math.Abs(payable.transform.position.x - Managers.Inst.kingdom.playerOne.transform.position.x) < 1) {
                        LogUtil.Error(payable.tag + " " + payable.GetScriptClassName() + " " + payable.name + " " + payable.GetIl2CppType().FullName + " " + payable.GetComponent<PrefabID>()?.prefabID);

                        foreach(var component in payable.gameObject.GetComponents<Component>()) {
                            LogUtil.Error("    " + component.GetIl2CppType().FullName);
                        }
                    }
                }

                //LogUtil.Message("F key pressed.");
                //showFullMap = !showFullMap;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                LogUtil.Message($"Try to reload game.");

                Managers.Inst.game.Reload();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                LogUtil.Message($"Try to save game.");

                Managers.Inst.game.TriggerSave();
            }

            timeSinceLastGuiUpdate += Time.deltaTime;

            if (timeSinceLastGuiUpdate > (1 / GlobalConfigs.GUIUpdatesPerSecond))
            {
                timeSinceLastGuiUpdate = 0;

                if (!IsPlaying()) return;

                if (enabledOverlayMap)
                {
                    UpdateMinimapMarkList();
                    UpdateStatsInfo();
                }
            }
        }

        private static bool IsPlaying()
        {
            var game = Managers.Inst?.game;
            if (game == null) return false;
            return game.state is Game.State.Playing or Game.State.NetworkClientPlaying;
        }

        private void OnGUI()
        {
            if (!IsPlaying()) return;

            if (enabledOverlayMap) {
                textHeight = guiStyle.CalcSize(new GUIContent("A")).y;

                DrawGuiForPlayer(0);
                DrawGuiForPlayer(1);
            }
        }

        private void DrawGuiForPlayer(int playerId)
        {
            var player = Managers.Inst.kingdom.GetPlayer(playerId);
            if (player == null) return;
            if (player.isActiveAndEnabled == false) return;
            if (player.hasLocalAuthority == false && NetworkBigBoss.IsOnline) return;

            var groupY = 0.0f;
            var groupHeight = Screen.height;

            if (Managers.COOP_ENABLED)
            {
                groupHeight = Screen.height / 2;
                if (playerId == 1)
                    groupY = Screen.height / 2.0f;
            }

            GUI.BeginGroup(new Rect(0, groupY, Screen.width, groupHeight));
            DrawMinimap(playerId);
            DrawExtraInfo(playerId);
            DrawStatsInfo(playerId);
            GUI.EndGroup();
        }

        private void OnClientCaughtUp()
        {
            LogUtil.Message("host_OnClientCaughtUp.");

            // OnGameStart();
        }

        private void OnGameStart()
        {
            LogUtil.Message("OnGameStart.");

            gameLayer = GameObject.FindGameObjectWithTag(Tags.GameLayer);

            _campaignIndex = GlobalSaveData.loaded.currentCampaign;
            _land = CampaignSaveData.current.CurrentLand;
            _challengeId = GlobalSaveData.loaded.currentChallenge;
            _archiveFilename = IslandSaveData.GetFilePropsForLand(_campaignIndex, _land, _challengeId).filename;

            LogUtil.Message($"OnGameStart: _archiveFilename {_archiveFilename}, Campaign {_campaignIndex}, CurrentLand {_land}, currentChallenge {_challengeId}");

            _exploredRegion = new ExploredRegion(_archiveFilename);

            LoadTextures.LoadAllTextures();
            minimapMarkList.Clear();
        }

        private void OnCurrentCampaignSwitch()
        {
            LogUtil.Message($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

        }

        private void UpdateMinimapMarkList()
        {
            var world = Managers.Inst.world;
            if (world == null) return;
            var level = Managers.Inst.level;
            if (level == null) return;
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;
            var payables = Managers.Inst.payables;
            if (payables == null) return;

            minimapMarkList.Clear();
            var poiList = new System.Collections.Generic.List<MarkInfo>();
            var leftWalls = new System.Collections.Generic.List<WallPoint>();
            var rightWalls = new System.Collections.Generic.List<WallPoint>();

            var hermitNames = new System.Collections.Generic.Dictionary<Hermit.HermitType, string> {
                { Hermit.HermitType.Baker,      ConfigStrings.HermitBaker },
                { Hermit.HermitType.Ballista,   ConfigStrings.HermitBallista },
                { Hermit.HermitType.Horn,       ConfigStrings.HermitHorn },
                { Hermit.HermitType.Horse,      ConfigStrings.HermitHorse },
                { Hermit.HermitType.Knight,     ConfigStrings.HermitKnight },
            };

            //Players
            foreach(var player in new System.Collections.Generic.List<Player> { kingdom.playerOne, kingdom.playerTwo }) {
                if(player == null)
                    continue;
                if(player.isActiveAndEnabled == false)
                    continue;
                var mover = player.mover;
                if(mover == null)
                    continue;

                var playerTextLines = new System.Collections.Generic.List<string> {
                    player.playerId == 0 ? ConfigStrings.P1 : ConfigStrings.P2
                };

                foreach(var dog in kingdom.dogs) {
                    if(dog._targPlayer == player) {
                        playerTextLines.Add(ConfigStrings.DogSpawn);
                    }
                }

                if(player.passenger != null) {
                    playerTextLines.Add(hermitNames[player.passenger.hermitType]);
                }

                poiList.Add(new MarkInfo(mover.transform.position.x, StyleConfigs.Player.Color, StyleConfigs.Player.Sign, "", textLines: playerTextLines.ToArray(), image: LoadTextures.PlayerTexture, rowNum: 2));
                float l = mover.transform.position.x - 12;
                float r = mover.transform.position.x + 12;
                if(l < _exploredRegion.ExploredLeft)
                    _exploredRegion.ExploredLeft = l;
                if(r > _exploredRegion.ExploredRight)
                    _exploredRegion.ExploredRight = r;
            }

            //Center castle
            var castle = kingdom.castle;
            if(castle != null) {
                var payable = castle.GetFieldOrPropertyValue<PayableUpgrade>("_payableUpgrade");
                var reason = payable.IsLocked(GameExtensions.GetLocalPlayer());
                bool canPay = reason == PayableUpgrade.LockedReason.NotLocked;
                bool isLocked = reason != PayableUpgrade.LockedReason.NotLocked && reason != PayableUpgrade.LockedReason.NoUpgrade;
                bool isLockedForInvalidTime = reason == PayableUpgrade.LockedReason.InvalidTime;
                var price = isLockedForInvalidTime ? (int) (payable.GetFieldOrPropertyValue<float>("timeAvailableFrom") - Time.time) : canPay ? payable.price : 0;
                var color = isLocked ? StyleConfigs.Castle.Locked.Color : StyleConfigs.Castle.Color;
                poiList.Add(new MarkInfo(castle.transform.position.x, color, StyleConfigs.Castle.Sign, "", textLines: [ (Array.IndexOf(Enum.GetValues(castle.level.GetType()), castle.level) + 1) + "" ], image: LoadTextures.CastleTexture, isCastle: true));

                leftWalls.Add(new WallPoint(castle.transform.position, StyleConfigs.WallLine.Color));
                rightWalls.Add(new WallPoint(castle.transform.position, StyleConfigs.WallLine.Color));
            }

            var campfire = kingdom.campfire;
            if(campfire != null) {
                poiList.Add(new MarkInfo(campfire.transform.position.x, StyleConfigs.Campfire.Color, StyleConfigs.Campfire.Sign, ConfigStrings.Campfire));
            }

            //Archer shop
            var shopArcher = GameObject.FindGameObjectWithTag(Tags.ShopBow);
            if(shopArcher != null) {
                poiList.Add(new MarkInfo(shopArcher.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopArcherTexture));
            }

            //Hammer shop
            var shopHammer = GameObject.FindGameObjectWithTag(Tags.ShopHammer);
            if(shopHammer != null) {
                poiList.Add(new MarkInfo(shopHammer.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopHammerTexture));
            }

            //Farmer shop
            var shopFarmer = GameObject.FindGameObjectWithTag(Tags.ShopScythe);
            if(shopFarmer != null) {
                poiList.Add(new MarkInfo(shopFarmer.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopFarmerTexture));
            }

            //Pike shop left
            var shopPikeLeft = GameObject.FindGameObjectWithTag(Tags.ShopPikeLeft);
            if(shopPikeLeft != null) {
                poiList.Add(new MarkInfo(shopPikeLeft.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopPikeTexture));
            }

            //Pike shop right
            var shopPikeRight = GameObject.FindGameObjectWithTag(Tags.ShopPikeRight);
            if(shopPikeRight != null) {
                poiList.Add(new MarkInfo(shopPikeRight.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopPikeTexture));
            }

            //Sword shop
            var shopForge = GameObject.FindGameObjectWithTag(Tags.ShopForge);
            if(shopForge != null) {
                poiList.Add(new MarkInfo(shopForge.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.ShopSwordTexture));
            }

            //Teleporters
            System.Collections.Generic.List<Texture2D> availablePortalTextures = [
                LoadTextures.Portal_GTexture,
                LoadTextures.Portal_PTexture,
                LoadTextures.Portal_OTexture,
                LoadTextures.Portal_MTexture,
                LoadTextures.Portal_YTexture,
                LoadTextures.Portal_BTexture,
            ];

            System.Collections.Generic.Dictionary<int, Texture2D> assignedPortalTextures = [];

            foreach(var obj in GameExtensions.GetPayablesOfType<PayableTeleporter>()) {
                if(assignedPortalTextures.ContainsKey(obj.linkedTo.GetHashCode())) {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: assignedPortalTextures[obj.linkedTo.GetHashCode()], rowNum: 1));
                    continue;
                }

                if(availablePortalTextures.Count == 0) {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: LoadTextures.PortalTexture, rowNum: 1));
                    continue;
                }

                assignedPortalTextures.Add(obj.GetHashCode(), availablePortalTextures[0]);
                availablePortalTextures.RemoveAt(0);

                poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", image: assignedPortalTextures[obj.GetHashCode()], rowNum: 1));
            }

            //Active beggar camps
            foreach (var beggarCamp in kingdom.BeggarCamps)
            {
                int count = 0;
                foreach (var beggar in beggarCamp.GetFieldOrPropertyValue<List<Beggar>>("_beggars"))
                {
                    if (beggar != null && beggar.isActiveAndEnabled)
                        count++;
                }
                poiList.Add(new MarkInfo(beggarCamp.transform.position.x, StyleConfigs.BeggarCamp.Color, StyleConfigs.BeggarCamp.Sign, "", textLines: [ count + "" ], image: LoadTextures.BeggarCampTexture));
            }

            //Built citizen houses
            foreach(var obj in GameObject.FindGameObjectsWithTag(Tags.CitizenHouse)) {
                var citizenHouse = obj.GetComponent<CitizenHousePayable>();
                if(citizenHouse != null) {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.CitizenHouse.Color, StyleConfigs.CitizenHouse.Sign, "", textLines: [ citizenHouse.GetPropertyValue<int>("_numberOfAvaliableCitizens") + "" ], image: LoadTextures.CitizenHouseTexture, rowNum: 0));
                }
            }

            foreach (var beggar in kingdom.beggars)
            {
                if (beggar == null) continue;

                if (beggar.hasFoundBaker)
                {
                    poiList.Add(new MarkInfo(beggar.transform.position.x, StyleConfigs.Beggar.Color, StyleConfigs.Beggar.Sign, ConfigStrings.Beggar, 0));
                }
            }

            //Deer
            foreach (var deer in GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife))
            {
                if (!deer.GetFieldOrPropertyValue<Damageable>("_damageable").isDead)
                    poiList.Add(new MarkInfo(deer.transform.position.x, deer.GetFieldOrPropertyValue<StateMachine>("_fsm").current == 5 ? StyleConfigs.DeerFollowing.Color : StyleConfigs.Deer.Color, StyleConfigs.Deer.Sign, "", image: LoadTextures.DeerTexture, rowNum: 1, flipImage: deer.transform.localScale.x > 0));
            }

            //Enemy portals
            Portal dock = null;
            foreach (var obj in kingdom.AllPortals)
            {
                if (obj.type == Portal.Type.Regular)
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Portal.Color, StyleConfigs.Portal.Sign, ""));
                else if (obj.type == Portal.Type.Cliff)
                    poiList.Add(new MarkInfo(obj.transform.position.x, obj.state switch{ Portal.State.Destroyed => StyleConfigs.Cliff.Destroyed.Color, Portal.State.Rebuilding => StyleConfigs.Cliff.Rebuilding.Color, _=> StyleConfigs.Cliff.Color }, StyleConfigs.Cliff.Sign, "", image: LoadTextures.CliffTexture, rowNum: 1));
                else if (obj.type == Portal.Type.Dock)
                    dock = obj;
            }

            //Beach
            var beach = gameLayer.GetComponentInChildren<Beach>();
            if(beach != null) {
                if(dock && (dock.state != Portal.State.Destroyed))
                    poiList.Add(new MarkInfo(beach.transform.position.x, StyleConfigs.Beach.Color, StyleConfigs.Beach.Sign, "", image: LoadTextures.BeachPortalTexture, rowNum: 1));
                else
                    poiList.Add(new MarkInfo(beach.transform.position.x, StyleConfigs.Beach.Destroyed.Color, StyleConfigs.Beach.Sign, "", image: LoadTextures.BeachTexture));
            }

            //Lighthouse
            var lighthouseLevel = new System.Collections.Generic.Dictionary<TechnologyAge, int> {
                { TechnologyAge.None,   0 },
                { TechnologyAge.Wood,   1 },
                { TechnologyAge.Stone,  2 },
                { TechnologyAge.Iron,   3 },
            };

            var lighthouse = kingdom.lighthouse;
            if(lighthouse != null) {
                poiList.Add(new MarkInfo(lighthouse.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", textLines: [lighthouseLevel[lighthouse.techAge] + "" ], image: LoadTextures.LighthouseTexture));
            }

            //Enemies and bosses
            var enemies = Managers.Inst.enemies.GetFieldOrPropertyValue<HashSet<Enemy>>("_enemies");
            if (enemies != null && enemies.Count > 0)
            {
                var leftEnemies = new System.Collections.Generic.List<float>();
                var leftBosses = new System.Collections.Generic.List<float>();
                var leftBossesWithCrownStealers = new System.Collections.Generic.List<float>();
                var leftSquids = new System.Collections.Generic.List<float>();
                var leftCrownStealers = new System.Collections.Generic.List<float>();

                var rightEnemies = new System.Collections.Generic.List<float>();
                var rightBosses = new System.Collections.Generic.List<float>();
                var rightBossesWithCrownStealers = new System.Collections.Generic.List<float>();
                var rightSquids = new System.Collections.Generic.List<float>();
                var rightCrownStealers = new System.Collections.Generic.List<float>();

                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    var damageable = enemy.GetComponent<Damageable>();
                    if (damageable != null && damageable.isDead)
                        continue;

                    var bossComponent = enemy.GetComponent<Boss>();

                    var enemyX = enemy.transform.position.x;
                    if (kingdom.GetBorderSideForPosition(enemyX) == Side.Left) {
                        if(enemy.GetComponent<Squid>() != null)
                            leftSquids.Add(enemyX);
                        else if (bossComponent != null && bossComponent.crownStealerPrefab != null)
                            leftBossesWithCrownStealers.Add(enemyX);
                        else if(enemy.GetComponent<CrownStealer>() != null)
                            leftCrownStealers.Add(enemyX);
                        else if(bossComponent != null)
                            leftBosses.Add(enemyX);
                        else
                            leftEnemies.Add(enemyX);
                    } else {
                        if(enemy.GetComponent<Squid>() != null)
                            rightSquids.Add(enemyX);
                        else if(bossComponent != null && bossComponent.crownStealerPrefab != null)
                            rightBossesWithCrownStealers.Add(enemyX);
                        else if(enemy.GetComponent<CrownStealer>() != null)
                            rightCrownStealers.Add(enemyX);
                        else if(bossComponent != null)
                            rightBosses.Add(enemyX);
                        else
                            rightEnemies.Add(enemyX);
                    }
                }

                leftEnemies.Sort();
                leftEnemies.Reverse();
                leftBosses.Sort();
                leftBosses.Reverse();
                leftBossesWithCrownStealers.Sort();
                leftBossesWithCrownStealers.Reverse();
                leftSquids.Sort();
                leftSquids.Reverse();
                leftCrownStealers.Sort();
                leftCrownStealers.Reverse();

                rightEnemies.Sort();
                rightBosses.Sort();
                rightBossesWithCrownStealers.Sort();
                rightSquids.Sort();
                rightCrownStealers.Sort();

                if(leftEnemies.Count > 0) {
                    poiList.Add(new MarkInfo(leftEnemies[0], StyleConfigs.Enemy.Color, StyleConfigs.Enemy.Sign, ConfigStrings.Enemy, textLines: [ leftEnemies.Count + "" ], image: LoadTextures.EnemyTexture, rowNum: 1));
                }

                if(leftBosses.Count > 0) {
                    poiList.Add(new MarkInfo(leftBosses[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ leftBosses.Count + "" ], image: LoadTextures.EnemyBossTexture, rowNum: 1));
                }

                if(leftBossesWithCrownStealers.Count > 0) {
                    poiList.Add(new MarkInfo(leftBossesWithCrownStealers[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ leftBossesWithCrownStealers.Count + "" ], image: LoadTextures.EnemyBossWithCrownStealerTexture, rowNum: 1));
                }

                if(leftSquids.Count > 0) {
                    poiList.Add(new MarkInfo(leftSquids[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ leftSquids.Count + "" ], image: LoadTextures.EnemySquidTexture, rowNum: 1));
                }

                if(leftCrownStealers.Count > 0) {
                    poiList.Add(new MarkInfo(leftCrownStealers[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ leftCrownStealers.Count + "" ], image: LoadTextures.EnemyCrownStealerTexture, rowNum: 1));
                }

                if(rightEnemies.Count > 0) {
                    poiList.Add(new MarkInfo(rightEnemies[0], StyleConfigs.Enemy.Color, StyleConfigs.Enemy.Sign, ConfigStrings.Enemy, textLines: [ rightEnemies.Count + "" ], image: LoadTextures.EnemyTexture, rowNum: 1, flipImage: true));
                }

                if(rightBosses.Count > 0) {
                    poiList.Add(new MarkInfo(rightBosses[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ rightBosses.Count + "" ], image: LoadTextures.EnemyBossTexture, rowNum: 1));
                }

                if(rightBossesWithCrownStealers.Count > 0) {
                    poiList.Add(new MarkInfo(rightBossesWithCrownStealers[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [rightBossesWithCrownStealers.Count + "" ], image: LoadTextures.EnemyBossWithCrownStealerTexture, rowNum: 1));
                }

                if(rightSquids.Count > 0) {
                    poiList.Add(new MarkInfo(rightSquids[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ rightSquids.Count + "" ], image: LoadTextures.EnemySquidTexture, rowNum: 1));
                }

                if(rightCrownStealers.Count > 0) {
                    poiList.Add(new MarkInfo(rightCrownStealers[0], StyleConfigs.Boss.Color, StyleConfigs.Boss.Sign, ConfigStrings.Boss, textLines: [ rightCrownStealers.Count + "" ], image: LoadTextures.EnemyCrownStealerTexture, rowNum: 1));
                }
            }

            //Chests
            foreach (var obj in gameLayer.GetComponentsInChildren<Chest>())
            {
                if (obj.coins == 0) continue;

                if (obj.isGems)
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.GemChest.Color, StyleConfigs.GemChest.Sign, "", textLines: [ obj.coins + "" ], image: LoadTextures.ChestGemTexture, rowNum: 1));
                else
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Chest.Color, StyleConfigs.Chest.Sign, "", textLines: [ obj.coins + "" ], image: LoadTextures.ChestTexture, rowNum: 1));
            }

            //Payable gem chest next to boat
            var payableGemChest = GameExtensions.GetPayableOfType<PayableGemChest>();
            if(payableGemChest != null) {
                var gemsCount = payableGemChest.infiniteGems ? payableGemChest.GetFieldOrPropertyValue<PayableGemGuard>("guardRef").price : payableGemChest.gemsStored;
                poiList.Add(new MarkInfo(payableGemChest.transform.position.x, StyleConfigs.GemMerchant.Color, StyleConfigs.GemMerchant.Sign, "", textLines: [ gemsCount + "" ], image: LoadTextures.ChestGemPayableTexture, rowNum: 1));
            }

            //Archer towers
            foreach(var obj in GameExtensions.FindObjectsWithTagOfType<Tower>(Tags.Tower)) {
                if(obj.level > 0) {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Deer.Color, StyleConfigs.Deer.Sign, "", textLines: [ obj.level + "" ], image: LoadTextures.ArcherTowerTexture));
                } else {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Deer.Color, StyleConfigs.Deer.Sign, "", image: LoadTextures.ArcherTowerUnbuiltTexture));
                }
            }

            //Active walls
            foreach (var obj in kingdom.GetFieldOrPropertyValue<HashSet<Wall>>("_walls"))
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Wall.Color, StyleConfigs.Wall.Sign, "", textLines: [ obj.level + "" ], image: LoadTextures.WallTexture));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, StyleConfigs.WallLine.Color));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, StyleConfigs.WallLine.Color));
            }

            //Broken walls
            foreach(var obj in GameObject.FindGameObjectsWithTag(Tags.WallWreck)) {
                poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Wall.Wrecked.Color, StyleConfigs.Wall.Sign, "", image: LoadTextures.WallUnbuiltTexture));
                if(kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, StyleConfigs.WallLine.Wrecked.Color));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, StyleConfigs.WallLine.Wrecked.Color));
            }

            //Unbuilt walls
            foreach(var obj in GameObject.FindGameObjectsWithTag(Tags.WallFoundation)) {
                poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.WallFoundation.Color, StyleConfigs.WallFoundation.Sign, "", image: LoadTextures.WallUnbuiltTexture));
            }

            //Steeds
            var steedNames = new System.Collections.Generic.Dictionary<Steed.SteedType, string> {
                { Steed.SteedType.Bear,                  ConfigStrings.Bear },
                { Steed.SteedType.P1Griffin,             ConfigStrings.Griffin },
                { Steed.SteedType.Lizard,                ConfigStrings.Lizard },
                { Steed.SteedType.Reindeer,              ConfigStrings.Reindeer },
                { Steed.SteedType.Spookyhorse,           ConfigStrings.Spookyhorse },
                { Steed.SteedType.Stag,                  ConfigStrings.Stag },
                { Steed.SteedType.Unicorn,               ConfigStrings.Unicorn },
                { Steed.SteedType.P1Warhorse,            ConfigStrings.Warhorse },
                { Steed.SteedType.P1Default,             ConfigStrings.DefaultSteed },
                { Steed.SteedType.P2Default,             ConfigStrings.DefaultSteed },
                { Steed.SteedType.HorseStamina,          ConfigStrings.HorseStamina },
                { Steed.SteedType.HorseBurst,            ConfigStrings.HorseBurst },
                { Steed.SteedType.HorseFast,             ConfigStrings.HorseFast },
                { Steed.SteedType.P1Wolf,                ConfigStrings.Wolf },
                { Steed.SteedType.Trap,                  ConfigStrings.Trap },
                { Steed.SteedType.Barrier,               ConfigStrings.Barrier },
                { Steed.SteedType.Bloodstained,          ConfigStrings.Bloodstained },
                { Steed.SteedType.P2Wolf,                ConfigStrings.Wolf },
                { Steed.SteedType.P2Griffin,             ConfigStrings.Griffin },
                { Steed.SteedType.P2Warhorse,            ConfigStrings.Warhorse },
                { Steed.SteedType.P2Stag,                ConfigStrings.Stag },
                { Steed.SteedType.Gullinbursti,          ConfigStrings.Gullinbursti },
                { Steed.SteedType.Sleipnir,              ConfigStrings.Sleipnir },
                { Steed.SteedType.Reindeer_Norselands,   ConfigStrings.Reindeer },
                { Steed.SteedType.CatCart,               ConfigStrings.CatCart },
                { Steed.SteedType.Kelpie,                ConfigStrings.Kelpie },
                { Steed.SteedType.DayNight,              ConfigStrings.DayNight },
                { Steed.SteedType.P2Kelpie,              ConfigStrings.Kelpie },
                { Steed.SteedType.P2Reindeer_Norselands, ConfigStrings.Reindeer },
            };

            //Farm houses
            var farmHouses = kingdom.GetFarmHouses();
            foreach(var obj in farmHouses) {
                if(obj.isStable) {
                    string[] stableSteedNames = new string[obj.stabledSteeds.Count];

                    for(int i = 0; i < obj.stabledSteeds.Count; i++) {
                        stableSteedNames[i] = steedNames[obj.stabledSteeds[i].steedType];
                    }

                    stableSteedNames = [.. stableSteedNames.OrderBy(s => s.Length)];

                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Farmhouse.Color, StyleConfigs.Farmhouse.Sign, "", textLines: stableSteedNames, image: LoadTextures.StableTexture));
                } else {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Farmhouse.Color, StyleConfigs.Farmhouse.Sign, "", textLines: [ obj.level + "" ], image: LoadTextures.FarmHouseTexture));
                }
            }

            //Rivers (unbuilt farm houses)
            foreach(var river in gameLayer.GetComponentsInChildren<River>()) {
                bool isFarmHouse = false;

                foreach(var farmHouse in farmHouses) {
                    if(Math.Abs(farmHouse.transform.position.x - river.transform.position.x) <= 1) {
                        isFarmHouse = true;
                        break;
                    }
                }

                if(isFarmHouse)
                    continue;

                poiList.Add(new MarkInfo(river.transform.position.x, StyleConfigs.River.Color, StyleConfigs.River.Sign, "", image: LoadTextures.RiverTexture));
            }

            foreach (var obj in Managers.Inst.world.GetFieldOrPropertyValue<List<PayableBush>>("_berryBushes"))
            {
                if (obj.GetFieldOrPropertyValue<bool>("paid"))
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.BerryBushPaid.Color, StyleConfigs.BerryBushPaid.Sign, ""));
                else
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.BerryBush.Color, StyleConfigs.BerryBush.Sign, ""));
            }

            //Dog
            var dogSpawn = GameExtensions.GetPayableBlockerOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn.GetPropertyValue<bool>("_dogFreed"))
                poiList.Add(new MarkInfo(dogSpawn.transform.position.x, StyleConfigs.DogSpawn.Color, StyleConfigs.DogSpawn.Sign, "", image: LoadTextures.DogTexture));

            var boarSpawn = world.GetFieldOrPropertyValue<BoarSpawnGroup>("boarSpawnGroup");
            if (boarSpawn != null)
            {
                poiList.Add(new MarkInfo(boarSpawn.transform.position.x, StyleConfigs.BoarSpawn.Color, StyleConfigs.BoarSpawn.Sign,
                    ConfigStrings.BoarSpawn, boarSpawn.GetFieldOrPropertyValue<bool>("_spawnedBoar") ? 0 : 1));
            }

            //Bomb
            var caveHelper = Managers.Inst.caveHelper;
            if (caveHelper != null && caveHelper.CurrentlyBombingPortal != null)
            {
                var bomb = caveHelper.Getbomb(caveHelper.CurrentlyBombingPortal.side);
                if (bomb != null)
                {
                    poiList.Add(new MarkInfo(bomb.transform.position.x, StyleConfigs.Bomb.Color, StyleConfigs.Bomb.Sign, "", image: LoadTextures.BombTexture, rowNum: 1));
                }
            }

            //Spawned steeds
            /*foreach (var obj in kingdom.spawnedSteeds)
            {
                if (obj.CurrentMode != Steed.Mode.Player)
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Steeds.Color, StyleConfigs.Steeds.Sign, steedNames[obj.steedType], obj.price));
            }*/

            //Unspawned steeds
            foreach (var obj in kingdom.steedSpawns)
            {
                var info = "";
                foreach (var steedTmp in obj.GetFieldOrPropertyValue<List<Steed>>("steedPool"))
                {
                    info = steedNames[steedTmp.steedType];
                }

                if (!obj.GetPropertyValue<bool>("_hasSpawned"))
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.SteedSpawns.Color, StyleConfigs.SteedSpawns.Sign, info, obj.price, textLines: [ info ], image: LoadTextures.SteedTexture));
            }

            //Hermits
            foreach (var obj in GameExtensions.GetPayablesOfType<Cabin>())
            {
                if (obj.GetPropertyValue<bool>("canPay"))
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.HermitCabins.Color, StyleConfigs.HermitCabins.Sign, "", obj.price, textLines: [ hermitNames[obj.hermitType] ], image: LoadTextures.HermitTexture));
            }

            foreach(var obj in kingdom.hermits) {
                if(obj._playerPassengerTo == null) {
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.HermitCabins.Color, StyleConfigs.HermitCabins.Sign, "", obj.price, textLines: [hermitNames[obj.hermitType]], image: LoadTextures.HermitTexture, rowNum: 1));
                }
            }

            //Statues
            var statueNames = new System.Collections.Generic.Dictionary<Statue.Deity, string> {
                { Statue.Deity.Archer,  ConfigStrings.StatueArcher },
                { Statue.Deity.Worker,  ConfigStrings.StatueWorker },
                { Statue.Deity.Knight,  ConfigStrings.StatueKnight },
                { Statue.Deity.Farmer,  ConfigStrings.StatueFarmer },
                { Statue.Deity.Time,    ConfigStrings.StatueTime },
            };

            foreach (var obj in GameExtensions.GetPayablesOfType<Statue>())
            {
                if (obj.deityStatus != Statue.DeityStatus.Activated)
                    poiList.Add(new MarkInfo(obj.transform.position.x, StyleConfigs.Statues.Color, StyleConfigs.Statues.Sign, statueNames[obj.deity], obj.price, textLines: [ statueNames[obj.deity] ], image: LoadTextures.StatueTexture));
            }

            var timeStatue = kingdom.timeStatue;
            if (timeStatue)
                poiList.Add(new MarkInfo(timeStatue.transform.position.x, StyleConfigs.StatueTime.Color, StyleConfigs.StatueTime.Sign, ConfigStrings.StatueTime, timeStatue.daysRemaining));

            //Boat/wreck
            var boat = kingdom.boat;
            if (boat)
                poiList.Add(new MarkInfo(boat.transform.position.x, StyleConfigs.Boat.Color, StyleConfigs.Boat.Sign, ConfigStrings.Boat, image: LoadTextures.BoatTexture, flipImage: boat.transform.localScale.x > 1, rowNum: 1));
            else
            {
                var wreck = kingdom.wreckPlaceholder;
                if (wreck)
                    poiList.Add(new MarkInfo(wreck.transform.position.x, StyleConfigs.Boat.Wrecked.Color, StyleConfigs.Boat.Sign, "", image: LoadTextures.WreckTexture, rowNum: 1));
            }

            var playerModelName = new System.Collections.Generic.Dictionary<Player.Model, string> {
                { Player.Model.None,        "" },
                { Player.Model.King,        ConfigStrings.King },
                { Player.Model.Queen,       ConfigStrings.Queen },
                { Player.Model.Prince,      ConfigStrings.Prince },
                { Player.Model.Princess,    ConfigStrings.Princess },
                { Player.Model.Hooded,      ConfigStrings.Hooded },
                { Player.Model.Zangetsu,    ConfigStrings.Zangetsu },
                { Player.Model.Alfred,      ConfigStrings.Alfred },
                { Player.Model.Gebel,       ConfigStrings.Gebel },
                { Player.Model.Miriam,      ConfigStrings.Miriam },
                { Player.Model.Total,       "" },
            };

            foreach (var obj in payables.
#if IL2CPP
                         AllPayables
#else
                         GetFieldOrPropertyValue<Payable[]>("AllPayables")
#endif
                     )
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                //Unpurchased quarry
                if (prefab.prefabID == (int)PrefabIDEnum.Quarry_undeveloped) {
                    poiList.Add(new MarkInfo(go.transform.position.x, StyleConfigs.Quarry.Locked.Color, StyleConfigs.Quarry.Sign, "", textLines: [ obj.price + "" ], image: LoadTextures.QuarryTexture));
                }
                else if (prefab.prefabID == (int)PrefabIDEnum.Mine_undeveloped) {
                    poiList.Add(new MarkInfo(go.transform.position.x, StyleConfigs.Mine.Locked.Color, StyleConfigs.Mine.Sign, ConfigStrings.Mine, obj.price));
                }
                //Unbuilt citizen houses
                else if(prefab.prefabID == (int)PrefabIDEnum.Citizen_House && prefab.name.Contains("Rubble")) {
                    poiList.Add(new MarkInfo(go.transform.position.x, StyleConfigs.BeggarCamp.Color, StyleConfigs.BeggarCamp.Sign, "", image: LoadTextures.BeggarCampRubbleTexture));
                }
                //Baker shops
                else if(prefab.prefabID == (int) PrefabIDEnum.Tower_Baker) {
                    var baker = go.GetComponent<Baker>();
                    if(baker != null)
                        poiList.Add(new MarkInfo(go.transform.position.x, StyleConfigs.ShopForge.Color, StyleConfigs.ShopForge.Sign, "", textLines: [ baker._breads.Count + "" ], image: LoadTextures.BakeryTexture));
                }
                else {
                    var unlockNewRulerStatue = go.GetComponent<UnlockNewRulerStatue>();
                    if (unlockNewRulerStatue != null)
                    {
                        var color = unlockNewRulerStatue.status switch
                        {
                            UnlockNewRulerStatue.Status.Locked => StyleConfigs.RulerSpawns.Locked.Color,
                            UnlockNewRulerStatue.Status.WaitingForArcher => StyleConfigs.RulerSpawns.Building.Color,
                            _ => StyleConfigs.RulerSpawns.Unlocked.Color
                        };
                        if (color != StyleConfigs.RulerSpawns.Unlocked.Color)
                        {
                            poiList.Add(new MarkInfo(go.transform.position.x, color, StyleConfigs.RulerSpawns.Sign, playerModelName[unlockNewRulerStatue.rulerToUnlock], obj.price));
                        }
                    }
                }
            }

            foreach (var obj in payables.GetFieldOrPropertyValue<List<PayableBlocker>>("_allBlockers"))
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;

                var thorPuzzleController = go.GetComponent<ThorPuzzleController>();
                if (thorPuzzleController != null)
                {
                    var color = thorPuzzleController.State == 0 ? StyleConfigs.ThorPuzzleStatue.Locked.Color : StyleConfigs.ThorPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(thorPuzzleController.transform.position.x, color, StyleConfigs.ThorPuzzleStatue.Sign, ConfigStrings.ThorPuzzleStatue));
                }

                var helPuzzleController = go.GetComponent<HelPuzzleController>();
                if (helPuzzleController != null)
                {
                    var color = helPuzzleController.State == 0 ? StyleConfigs.HelPuzzleStatue.Locked.Color : StyleConfigs.HelPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(helPuzzleController.transform.position.x, color, StyleConfigs.HelPuzzleStatue.Sign, ConfigStrings.HelPuzzleStatue));
                }
            }

            foreach (var blocker in payables.GetFieldOrPropertyValue<List<PayableBlocker>>("_allBlockers"))
            {
                if (blocker == null) continue;
                var scaffolding = blocker.GetComponent<Scaffolding>();
                if (scaffolding == null) continue;
                var go = scaffolding.building;
                if (go == null) continue;

                var wall = go.GetComponent<Wall>();
                if (wall)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, StyleConfigs.Wall.Building.Color, StyleConfigs.Wall.Sign, ""));
                    if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                        leftWalls.Add(new WallPoint(go.transform.position, StyleConfigs.WallLine.Building.Color));
                    else
                        rightWalls.Add(new WallPoint(go.transform.position, StyleConfigs.WallLine.Building.Color));
                }
            }

            // var mine = GameObject.Find("Mine_undeveloped(Clone)");
            // if (mine)
            // {
            //     poiList.Add(new MarkInfo(mine.transform.position, Color.red, Strings.Mine));
            //     log.LogMessage($"mine prefabID: {mine.GetComponent<PrefabID>().prefabID}");
            // }
            
            // explored area

            float wallLeft = Managers.Inst.kingdom.GetBorderSide(Side.Left);
            float wallRight = Managers.Inst.kingdom.GetBorderSide(Side.Right);

            foreach (var poi in poiList)
            {
                if (showFullMap)
                    poi.Visible = true;
                else if(poi.WorldPosX >= _exploredRegion.ExploredLeft && poi.WorldPosX <= _exploredRegion.ExploredRight)
                    poi.Visible = true;
                else if (poi.WorldPosX >= wallLeft && poi.WorldPosX <= wallRight)
                    poi.Visible = true;
                else
                    poi.Visible = false;
            }

            // Calc screen pos

            if (poiList.Count == 0)
                return;

            var startPos = poiList[0].WorldPosX;
            var endPos = poiList[0].WorldPosX;

            foreach (var poi in poiList)
            {
                startPos = Math.Min(startPos, poi.WorldPosX);
                endPos = Math.Max(endPos, poi.WorldPosX);
            }

            var mapWidth = endPos - startPos;
            var clientWidth = Screen.width - 40;
            var scale = clientWidth / mapWidth;

            foreach (var poi in poiList)
            {
                poi.Pos = (poi.WorldPosX - startPos) * scale + 16;
            }
            
            minimapMarkList = poiList;

            // Make wall lines

            var lineList = new System.Collections.Generic.List<LineInfo>();
            if (leftWalls.Count > 1)
            {
                leftWalls.Sort((a, b) => b.Pos.x.CompareTo(a.Pos.x));
                var beginPoint = leftWalls[0];
                for (int i = 1; i < leftWalls.Count; i++)
                {
                    var endPoint = leftWalls[i];
                    var info = new LineInfo
                    {
                        LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, boxMargin + minimapLineMargin),
                        LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, boxMargin + minimapLineMargin),
                        Color = endPoint.Color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            if (rightWalls.Count > 1)
            {
                rightWalls.Sort((a, b) => a.Pos.x.CompareTo(b.Pos.x));
                var beginPoint = rightWalls[0];
                for (int i = 1; i < rightWalls.Count; i++)
                {
                    var endPoint = rightWalls[i];
                    var info = new LineInfo
                    {
                        LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, boxMargin + minimapLineMargin),
                        LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, boxMargin + minimapLineMargin),
                        Color = endPoint.Color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            drawLineList = lineList;
        }

        private static bool IsYourSelf(int playerId, string name)
        {
            if (name == ConfigStrings.P1)
            {
                if (playerId == 0 && NetworkBigBoss.HasWorldAuth)
                {
                    return true;
                }
            }
            else if (name == ConfigStrings.P2)
            {
                if (playerId == 1 && (Managers.COOP_ENABLED || ProgramDirector.IsClient))
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawMinimap(int playerId) {
            if(minimapMarkList.Count == 0)
                return;

            guiStyle.alignment = TextAnchor.UpperCenter;

            var rowList = new System.Collections.Generic.List<MinimapRowInfo>();

            foreach(var markInfo in minimapMarkList) {
                if(markInfo.Image == null) {
                    continue;
                }

                //Make sure row exists
                if(markInfo.RowNum > rowList.Count - 1) {
                    for(int i = rowList.Count; i <= markInfo.RowNum; i++) {
                        rowList.Add(new MinimapRowInfo());
                    }
                }

                if(markInfo.IsCastle)
                    rowList[markInfo.RowNum].HasCastle = true;

                rowList[markInfo.RowNum].RowMarkList.Add(markInfo);
                rowList[markInfo.RowNum].RowHeight = Math.Max(rowList[markInfo.RowNum].RowHeight, imageHeight + (textHeight * markInfo.TextLines.Length));
            }

            float rowTop = boxMargin + minimapLineMargin + minimapLineThickness + minimapLineMargin;
            float allRowsHeight = 0;

            for(int i = 0; i < rowList.Count; i++) {
                rowList[i].RowTop = rowTop;
                rowTop += rowList[i].RowHeight + minimapRowMargin;

                if(i < rowList.Count - 1)
                    allRowsHeight += rowList[i].RowHeight + minimapRowMargin;
                else
                    allRowsHeight += rowList[i].RowHeight;
            }

            minimapHeight = minimapLineMargin + minimapLineThickness + minimapLineMargin + allRowsHeight + boxMargin;

            Rect boxRect = new Rect(boxMargin, boxMargin, Screen.width - boxMargin - boxMargin, minimapHeight);
            GUI.Box(boxRect, "");
            GUI.Box(boxRect, "");

            foreach(var line in drawLineList) {
                GuiHelper.DrawLine(line.LineStart, line.LineEnd, line.Color, minimapLineThickness);
            }

            foreach(var row in rowList) {
                DrawMinimapRow(playerId, row);
            }

            foreach (var markInfo in minimapMarkList) {
                if (!markInfo.Visible)
                    continue;

                if(markInfo.Image != null) {
                    continue;
                }

                var markName = markInfo.Name;
                var color = markInfo.Color;

                guiStyle.normal.textColor = color;

                if (markInfo.Sign != "")
                    GUI.Label(new Rect(markInfo.Pos, 8, 0, 20), markInfo.Sign, guiStyle);

                float namePosY = 24;

                if (markInfo.Name != "")
                    GUI.Label(new Rect(markInfo.Pos, namePosY, 0, 20), markName, guiStyle);

                if (markInfo.Count != 0)
                    GUI.Label(new Rect(markInfo.Pos, namePosY + 16, 0, 20), markInfo.Count.ToString(), guiStyle);

                // draw self vec.x

                // if (markInfo.pos.y == 50.0f)
                // {
                //     Rect pos = markInfo.pos;
                //     pos.y = pos.y + 20;
                //     GUI.Label(pos, markInfo.vec.x.ToString(), SpotMarkGUIStyle);
                // }
            }
        }

        private void DrawMinimapRow(int playerId, MinimapRowInfo row) {
            if(row.HasCastle) {
                DrawMinimapRowWithCastle(row);
            } else {
                DrawMinimapRowWithoutCastle(playerId, row);
            }
        }

        private void DrawMinimapRowWithCastle(MinimapRowInfo row) {
            MarkInfo castleMarkInfo = row.RowMarkList.FirstOrDefault(mi => mi.IsCastle);

            if(castleMarkInfo.Visible) {
                GUI.DrawTexture(new Rect(castleMarkInfo.Pos - (castleMarkInfo.Image.width / 2), row.RowTop, castleMarkInfo.Image.width, castleMarkInfo.Image.height), castleMarkInfo.Image);

                if(castleMarkInfo.TextLines.Length > 0) {
                    for(int i = 0; i < castleMarkInfo.TextLines.Length; i++) {
                        GUI.Label(new Rect(castleMarkInfo.Pos, row.RowTop + castleMarkInfo.Image.height + (textHeight * i), 0, textHeight), castleMarkInfo.TextLines[i], guiStyle);
                    }
                }
            }

            float occupiedPixelsLeft = castleMarkInfo.Pos - (castleMarkInfo.Image.width / 2) - imageHMargin;
            float occupiedPixelsRight = castleMarkInfo.Pos + (castleMarkInfo.Image.width / 2) + imageHMargin;

            IOrderedEnumerable<MarkInfo> imageMarkListLeft = row.RowMarkList.Where(mi => mi.Pos < castleMarkInfo.Pos).OrderByDescending(mi => mi.Pos);
            IOrderedEnumerable<MarkInfo> imageMarkListRight = row.RowMarkList.Where(mi => mi.Pos > castleMarkInfo.Pos).OrderBy(mi => mi.Pos);

            DrawMinimapImagesLeft(imageMarkListLeft, occupiedPixelsLeft, row.RowTop);

            DrawMinimapImagesRight(imageMarkListRight, occupiedPixelsRight, row.RowTop);
        }

        private void DrawMinimapRowWithoutCastle(int playerId, MinimapRowInfo row) {
            IOrderedEnumerable<MarkInfo> imageMarkListRight = row.RowMarkList.OrderBy(mi => mi.Pos);

            DrawMinimapImagesRight(imageMarkListRight, 0, row.RowTop);
        }

        private void DrawMinimapImagesLeft(IOrderedEnumerable<MarkInfo> imageMarkListLeft, float occupiedPixelsLeft, float imageTop) {
            float markInfoCorrectedPos;

            foreach(MarkInfo markInfo in imageMarkListLeft) {
                if(!markInfo.Visible)
                    continue;

                if(markInfo.Pos + (markInfo.Image.width / 2) > occupiedPixelsLeft) {
                    markInfoCorrectedPos = occupiedPixelsLeft - markInfo.Image.width;
                    occupiedPixelsLeft -= markInfo.Image.width + imageHMargin;
                } else {
                    markInfoCorrectedPos = markInfo.Pos - (markInfo.Image.width / 2);
                    occupiedPixelsLeft = markInfo.Pos - (markInfo.Image.width / 2) - imageHMargin;
                }

                int imageWidth = markInfo.Image.width;

                if(markInfo.FlipImage) {
                    markInfoCorrectedPos += imageWidth;
                    imageWidth *= -1;
                }

                GUI.DrawTexture(new Rect(markInfoCorrectedPos, imageTop, imageWidth, markInfo.Image.height), markInfo.Image);

                if(markInfo.TextLines.Length > 0) {
                    for(int i = 0; i < markInfo.TextLines.Length; i++) {
                        GUI.Label(new Rect(markInfoCorrectedPos + (markInfo.Image.width / 2), imageTop + markInfo.Image.height + (textHeight * i), 0, textHeight), markInfo.TextLines[i], guiStyle);
                    }
                }
            }
        }

        private void DrawMinimapImagesRight(IOrderedEnumerable<MarkInfo> imageMarkListRight, float occupiedPixelsRight, float imageTop) {
            float markInfoCorrectedPos;

            foreach(MarkInfo markInfo in imageMarkListRight) {
                if(!markInfo.Visible)
                    continue;

                if(markInfo.Pos - (markInfo.Image.width / 2) < occupiedPixelsRight) {
                    markInfoCorrectedPos = occupiedPixelsRight;
                    occupiedPixelsRight += markInfo.Image.width + imageHMargin;
                } else {
                    markInfoCorrectedPos = markInfo.Pos - (markInfo.Image.width / 2);
                    occupiedPixelsRight = markInfo.Pos + (markInfo.Image.width / 2) + imageHMargin;
                }

                int imageWidth = markInfo.Image.width;

                if(markInfo.FlipImage) {
                    markInfoCorrectedPos += imageWidth;
                    imageWidth *= -1;
                }

                GUI.DrawTexture(new Rect(markInfoCorrectedPos, imageTop, imageWidth, markInfo.Image.height), markInfo.Image);

                if(markInfo.TextLines.Length > 0) {
                    for(int i = 0; i < markInfo.TextLines.Length; i++) {
                        GUI.Label(new Rect(markInfoCorrectedPos + (markInfo.Image.width / 2), imageTop + markInfo.Image.height + (textHeight * i), 0, textHeight), markInfo.TextLines[i], guiStyle);
                    }
                }
            }
        }

        private void DrawExtraInfo(int playerId) {
            guiStyle.normal.textColor = StyleConfigs.ExtraInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            float top = boxMargin + minimapHeight + boxMargin;

            extraInfoHeight = boxMargin + textHeight + boxMargin;

            //Island and days
            float islandTextWidth = guiStyle.CalcSize(new GUIContent(ConfigStrings.Land + ": " + (Managers.Inst.game.currentLand + 1))).x;
            float daysTextWidth = guiStyle.CalcSize(new GUIContent(ConfigStrings.Days + ": " + (Managers.Inst.director.CurrentDayForSpawning))).x;

            var leftRect = new Rect(boxMargin, top, boxMargin + islandTextWidth + boxMargin + daysTextWidth + boxMargin, extraInfoHeight);
            GUI.Box(leftRect, "");
            GUI.Box(leftRect, "");

            GUI.Label(new Rect(boxMargin + boxMargin, top + boxMargin, islandTextWidth, textHeight), ConfigStrings.Land + ": " + (Managers.Inst.game.currentLand + 1), guiStyle);
            GUI.Label(new Rect(boxMargin + boxMargin + islandTextWidth + boxMargin, top + boxMargin, daysTextWidth, textHeight), ConfigStrings.Days + ": " + (Managers.Inst.director.CurrentDayForSpawning), guiStyle);

            //Time
            float currentTime = Managers.Inst.director.currentTime;
            var currentHour = Math.Truncate(currentTime);
            var currentMints = Math.Truncate((currentTime - currentHour) * 60);
            float centerTextWidth = guiStyle.CalcSize(new GUIContent($"{currentHour:00.}:{currentMints:00.}")).x;
            float centerTextLeft = (Screen.width - centerTextWidth) / 2;

            var centerRect = new Rect(centerTextLeft - boxMargin, top, boxMargin + centerTextWidth + boxMargin, extraInfoHeight);
            GUI.Box(centerRect, "");
            GUI.Box(centerRect, "");

            GUI.Label(new Rect(centerTextLeft, top + boxMargin, centerTextWidth, textHeight), $"{currentHour:00.}:{currentMints:00.}", guiStyle);

            //Gems and coins
            var player = Managers.Inst.kingdom.GetPlayer(playerId);
            float gemsTextWidth = guiStyle.CalcSize(new GUIContent(ConfigStrings.Gems + ": " + player.gems)).x;
            float coinsTextWidth = guiStyle.CalcSize(new GUIContent(ConfigStrings.Coins + ": " + player.coins)).x;

            var rightRect = new Rect(Screen.width - boxMargin - boxMargin - coinsTextWidth - boxMargin - gemsTextWidth - boxMargin, top, boxMargin + gemsTextWidth + boxMargin + coinsTextWidth + boxMargin, extraInfoHeight);
            GUI.Box(rightRect, "");
            GUI.Box(rightRect, "");

            if(player != null) {
                GUI.Label(new Rect(Screen.width - boxMargin - boxMargin - coinsTextWidth - boxMargin - gemsTextWidth, top + boxMargin, gemsTextWidth, textHeight), ConfigStrings.Gems + ": " + player.gems, guiStyle);
                GUI.Label(new Rect(Screen.width - boxMargin - boxMargin - coinsTextWidth, top + boxMargin, coinsTextWidth, textHeight), ConfigStrings.Coins + ": " + player.coins, guiStyle);
            }
        }

        private void UpdateStatsInfo() {
            var kingdom = Managers.Inst.kingdom;
            if(kingdom == null)
                return;

            var peasantList = GameObject.FindGameObjectsWithTag(Tags.Peasant);
            statsInfo.PeasantCount = peasantList.Length;

            var workerList = kingdom.GetFieldOrPropertyValue<HashSet<Worker>>("_workers");
            statsInfo.WorkerCount = workerList.Count;

            var archerList = kingdom.GetFieldOrPropertyValue<HashSet<Archer>>("_archers");
            statsInfo.ArcherCount = archerList.Count;

            var farmerList = kingdom.Farmers;
            statsInfo.FarmerCount = farmerList.Count;

            var farmhouseList = kingdom.GetFarmHouses();
            int maxFarmlands = 0;
            foreach(var obj in farmhouseList) {
                maxFarmlands += obj.GetMethodDelegate<Func<int>>("CurrentMaxFarmlands")();
            }
            statsInfo.MaxFarmlands = maxFarmlands;
        }

        private void DrawStatsInfo(int playerId)
        {
            guiStyle.normal.textColor = StyleConfigs.StatsInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            var kingdom = Managers.Inst.kingdom;

            string[] statsStrings = {
                ConfigStrings.Peasant + ": " + statsInfo.PeasantCount,
                ConfigStrings.Worker + ": " + statsInfo.WorkerCount,
                $"{ConfigStrings.Archer.Value}: {statsInfo.ArcherCount} ({GameExtensions.GetArcherCount(ArcherTypeEnum.Free)}|{GameExtensions.GetArcherCount(ArcherTypeEnum.GuardSlot)}|{GameExtensions.GetArcherCount(ArcherTypeEnum.KnightSoldier)})",
                ConfigStrings.Pikeman + ": " + kingdom.Pikemen.Count,
                $"{ConfigStrings.Knight.Value}: {kingdom.knights.Count} ({GameExtensions.GetKnightCount(true)})",
                ConfigStrings.Farmer + ": " + statsInfo.FarmerCount,
                ConfigStrings.Farmlands + ": " + statsInfo.MaxFarmlands
            };

            float boxTop = boxMargin + minimapHeight + boxMargin + extraInfoHeight + boxMargin;
            float boxWidth = boxMargin + statsStrings.Select(s => guiStyle.CalcSize(new GUIContent(s)).x).Max() + boxMargin;
            float boxHeight = boxMargin + ((textHeight + textMargin) * statsStrings.Length) + boxMargin;

            if(statsStrings.Length > 0) {
                boxHeight -= textMargin;
            }

            var boxRect = new Rect(boxMargin, boxTop, boxWidth, boxHeight);
            GUI.Box(boxRect, "");
            GUI.Box(boxRect, "");

            for(int i = 0; i < statsStrings.Length; i++) {
                float textTop = boxTop + boxMargin + ((textHeight + textMargin) * i);

                GUI.Label(new Rect(boxMargin + boxMargin, textTop, boxWidth, textHeight), statsStrings[i], guiStyle);
            }
        }
    }
}