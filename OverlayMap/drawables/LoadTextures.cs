using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KingdomMod {
    internal class LoadTextures {
        private static readonly string ArcherTowerPath = "KingdomMod.images.archerTower.png";
        private static readonly string ArcherTowerUnbuiltPath = "KingdomMod.images.archerTowerUnbuilt.png";
        private static readonly string BakeryPath = "KingdomMod.images.bakery.png";
        private static readonly string BeachPath = "KingdomMod.images.beach.png";
        private static readonly string BeachPortalPath = "KingdomMod.images.beachPortal.png";
        private static readonly string BeggarCampPath = "KingdomMod.images.beggarCamp.png";
        private static readonly string BeggarCampRubblePath = "KingdomMod.images.beggarCampRubble.png";
        private static readonly string BoatPath = "KingdomMod.images.boat.png";
        private static readonly string BombPath = "KingdomMod.images.bomb.png";
        private static readonly string CastlePath = "KingdomMod.images.castle.png";
        private static readonly string ChestPath = "KingdomMod.images.chest.png";
        private static readonly string ChestGemPath = "KingdomMod.images.chestGem.png";
        private static readonly string ChestGemPayablePath = "KingdomMod.images.chestGemPayable.png";
        private static readonly string CitizenHousePath = "KingdomMod.images.citizenHouse.png";
        private static readonly string CliffPath = "KingdomMod.images.cliff.png";
        private static readonly string DeerPath = "KingdomMod.images.deer.png";
        private static readonly string DogPath = "KingdomMod.images.dog.png";
        private static readonly string EnemyPath = "KingdomMod.images.enemy.png";
        private static readonly string EnemyBossPath = "KingdomMod.images.enemyBoss.png";
        private static readonly string EnemyBossWithCrownStealerPath = "KingdomMod.images.enemyBossWithCrownStealer.png";
        private static readonly string EnemyCrownStealerPath = "KingdomMod.images.enemyCrownStealer.png";
        private static readonly string EnemySquidPath = "KingdomMod.images.enemySquid.png";
        private static readonly string FarmHousePath = "KingdomMod.images.farmHouse.png";
        private static readonly string HermitPath = "KingdomMod.images.hermit.png";
        private static readonly string LighthousePath = "KingdomMod.images.lighthouse.png";
        private static readonly string PlayerPath = "KingdomMod.images.player.png";
        private static readonly string PortalPath = "KingdomMod.images.portal.png";
        private static readonly string Portal_BPath = "KingdomMod.images.portal_b.png";
        private static readonly string Portal_GPath = "KingdomMod.images.portal_g.png";
        private static readonly string Portal_MPath = "KingdomMod.images.portal_m.png";
        private static readonly string Portal_OPath = "KingdomMod.images.portal_o.png";
        private static readonly string Portal_PPath = "KingdomMod.images.portal_p.png";
        private static readonly string Portal_YPath = "KingdomMod.images.portal_y.png";
        private static readonly string QuarryPath = "KingdomMod.images.quarry.png";
        private static readonly string RiverPath = "KingdomMod.images.river.png";
        private static readonly string ShopArcherPath = "KingdomMod.images.shopArcher.png";
        private static readonly string ShopFarmerPath = "KingdomMod.images.shopFarmer.png";
        private static readonly string ShopHammerPath = "KingdomMod.images.shopHammer.png";
        private static readonly string ShopPikePath = "KingdomMod.images.shopPike.png";
        private static readonly string ShopSwordPath = "KingdomMod.images.shopSword.png";
        private static readonly string StablePath = "KingdomMod.images.stable.png";
        private static readonly string StatuePath = "KingdomMod.images.statue.png";
        private static readonly string SteedPath = "KingdomMod.images.steed.png";
        private static readonly string WallPath = "KingdomMod.images.wall.png";
        private static readonly string WallUnbuiltPath = "KingdomMod.images.wallUnbuilt.png";
        private static readonly string WreckPath = "KingdomMod.images.wreck.png";
        private static readonly string TestPath = "KingdomMod.images.test.png";

        private static Texture2D _ArcherTowerTexture;
        public static Texture2D ArcherTowerTexture {
            get => GetLoadedTexture(ArcherTowerPath, ref _ArcherTowerTexture);
            set => _ArcherTowerTexture = value;
        }

        private static Texture2D _ArcherTowerUnbuiltTexture;
        public static Texture2D ArcherTowerUnbuiltTexture {
            get => GetLoadedTexture(ArcherTowerUnbuiltPath, ref _ArcherTowerUnbuiltTexture);
            set => _ArcherTowerUnbuiltTexture = value;
        }

        private static Texture2D _BakeryTexture;
        public static Texture2D BakeryTexture {
            get => GetLoadedTexture(BakeryPath, ref _BakeryTexture);
            set => _BakeryTexture = value;
        }

        private static Texture2D _BeachTexture;
        public static Texture2D BeachTexture {
            get => GetLoadedTexture(BeachPath, ref _BeachTexture);
            set => _BeachTexture = value;
        }

        private static Texture2D _BeachPortalTexture;
        public static Texture2D BeachPortalTexture {
            get => GetLoadedTexture(BeachPortalPath, ref _BeachPortalTexture);
            set => _BeachPortalTexture = value;
        }

        private static Texture2D _BeggarCampTexture;
        public static Texture2D BeggarCampTexture {
            get => GetLoadedTexture(BeggarCampPath, ref _BeggarCampTexture);
            set => _BeggarCampTexture = value;
        }

        private static Texture2D _BeggarCampRubbleTexture;
        public static Texture2D BeggarCampRubbleTexture {
            get => GetLoadedTexture(BeggarCampRubblePath, ref _BeggarCampRubbleTexture);
            set => _BeggarCampRubbleTexture = value;
        }

        private static Texture2D _BoatTexture;
        public static Texture2D BoatTexture {
            get => GetLoadedTexture(BoatPath, ref _BoatTexture);
            set => _BoatTexture = value;
        }

        private static Texture2D _BombTexture;
        public static Texture2D BombTexture {
            get => GetLoadedTexture(BombPath, ref _BombTexture);
            set => _BombTexture = value;
        }

        private static Texture2D _CastleTexture;
        public static Texture2D CastleTexture {
            get => GetLoadedTexture(CastlePath, ref _CastleTexture);
            set => _CastleTexture = value;
        }

        private static Texture2D _ChestTexture;
        public static Texture2D ChestTexture {
            get => GetLoadedTexture(ChestPath, ref _ChestTexture);
            set => _ChestTexture = value;
        }

        private static Texture2D _ChestGemTexture;
        public static Texture2D ChestGemTexture {
            get => GetLoadedTexture(ChestGemPath, ref _ChestGemTexture);
            set => _ChestGemTexture = value;
        }
        private static Texture2D _ChestGemPayableTexture;
        public static Texture2D ChestGemPayableTexture {
            get => GetLoadedTexture(ChestGemPayablePath, ref _ChestGemPayableTexture);
            set => _ChestGemPayableTexture = value;
        }

        private static Texture2D _CitizenHouseTexture;
        public static Texture2D CitizenHouseTexture {
            get => GetLoadedTexture(CitizenHousePath, ref _CitizenHouseTexture);
            set => _CitizenHouseTexture = value;
        }

        private static Texture2D _CliffTexture;
        public static Texture2D CliffTexture {
            get => GetLoadedTexture(CliffPath, ref _CliffTexture);
            set => _CliffTexture = value;
        }

        private static Texture2D _DeerTexture;
        public static Texture2D DeerTexture {
            get => GetLoadedTexture(DeerPath, ref _DeerTexture);
            set => _DeerTexture = value;
        }

        private static Texture2D _DogTexture;
        public static Texture2D DogTexture {
            get => GetLoadedTexture(DogPath, ref _DogTexture);
            set => _DogTexture = value;
        }

        private static Texture2D _EnemyTexture;
        public static Texture2D EnemyTexture {
            get => GetLoadedTexture(EnemyPath, ref _EnemyTexture);
            set => _EnemyTexture = value;
        }

        private static Texture2D _EnemyBossTexture;
        public static Texture2D EnemyBossTexture {
            get => GetLoadedTexture(EnemyBossPath, ref _EnemyBossTexture);
            set => _EnemyBossTexture = value;
        }

        private static Texture2D _EnemyBossWithCrownStealerTexture;
        public static Texture2D EnemyBossWithCrownStealerTexture {
            get => GetLoadedTexture(EnemyBossWithCrownStealerPath, ref _EnemyBossWithCrownStealerTexture);
            set => _EnemyBossWithCrownStealerTexture = value;
        }

        private static Texture2D _EnemyCrownStealerTexture;
        public static Texture2D EnemyCrownStealerTexture {
            get => GetLoadedTexture(EnemyCrownStealerPath, ref _EnemyCrownStealerTexture);
            set => _EnemyCrownStealerTexture = value;
        }

        private static Texture2D _EnemySquidTexture;
        public static Texture2D EnemySquidTexture {
            get => GetLoadedTexture(EnemySquidPath, ref _EnemySquidTexture);
            set => _EnemySquidTexture = value;
        }

        private static Texture2D _FarmHouseTexture;
        public static Texture2D FarmHouseTexture {
            get => GetLoadedTexture(FarmHousePath, ref _FarmHouseTexture);
            set => _FarmHouseTexture = value;
        }

        private static Texture2D _HermitTexture;
        public static Texture2D HermitTexture {
            get => GetLoadedTexture(HermitPath, ref _HermitTexture);
            set => _HermitTexture = value;
        }

        private static Texture2D _LighthouseTexture;
        public static Texture2D LighthouseTexture {
            get => GetLoadedTexture(LighthousePath, ref _LighthouseTexture);
            set => _LighthouseTexture = value;
        }

        private static Texture2D _PlayerTexture;
        public static Texture2D PlayerTexture {
            get => GetLoadedTexture(PlayerPath, ref _PlayerTexture);
            set => _PlayerTexture = value;
        }

        private static Texture2D _PortalTexture;
        public static Texture2D PortalTexture {
            get => GetLoadedTexture(PortalPath, ref _PortalTexture);
            set => _PortalTexture = value;
        }

        private static Texture2D _Portal_BTexture;
        public static Texture2D Portal_BTexture {
            get => GetLoadedTexture(Portal_BPath, ref _Portal_BTexture);
            set => _Portal_BTexture = value;
        }

        private static Texture2D _Portal_GTexture;
        public static Texture2D Portal_GTexture {
            get => GetLoadedTexture(Portal_GPath, ref _Portal_GTexture);
            set => _Portal_GTexture = value;
        }

        private static Texture2D _Portal_MTexture;
        public static Texture2D Portal_MTexture {
            get => GetLoadedTexture(Portal_MPath, ref _Portal_MTexture);
            set => _Portal_MTexture = value;
        }

        private static Texture2D _Portal_OTexture;
        public static Texture2D Portal_OTexture {
            get => GetLoadedTexture(Portal_OPath, ref _Portal_OTexture);
            set => _Portal_OTexture = value;
        }

        private static Texture2D _Portal_PTexture;
        public static Texture2D Portal_PTexture {
            get => GetLoadedTexture(Portal_PPath, ref _Portal_PTexture);
            set => _Portal_PTexture = value;
        }

        private static Texture2D _Portal_YTexture;
        public static Texture2D Portal_YTexture {
            get => GetLoadedTexture(Portal_YPath, ref _Portal_YTexture);
            set => _Portal_YTexture = value;
        }

        private static Texture2D _QuarryTexture;
        public static Texture2D QuarryTexture {
            get => GetLoadedTexture(QuarryPath, ref _QuarryTexture);
            set => _QuarryTexture = value;
        }

        private static Texture2D _RiverTexture;
        public static Texture2D RiverTexture {
            get => GetLoadedTexture(RiverPath, ref _RiverTexture);
            set => _RiverTexture = value;
        }

        private static Texture2D _ShopArcherTexture;
        public static Texture2D ShopArcherTexture {
            get => GetLoadedTexture(ShopArcherPath, ref _ShopArcherTexture);
            set => _ShopArcherTexture = value;
        }

        private static Texture2D _ShopFarmerTexture;
        public static Texture2D ShopFarmerTexture {
            get => GetLoadedTexture(ShopFarmerPath, ref _ShopFarmerTexture);
            set => _ShopFarmerTexture = value;
        }

        private static Texture2D _ShopHammerTexture;
        public static Texture2D ShopHammerTexture {
            get => GetLoadedTexture(ShopHammerPath, ref _ShopHammerTexture);
            set => _ShopHammerTexture = value;
        }

        private static Texture2D _ShopPikeTexture;
        public static Texture2D ShopPikeTexture {
            get => GetLoadedTexture(ShopPikePath, ref _ShopPikeTexture);
            set => _ShopPikeTexture = value;
        }

        private static Texture2D _ShopSwordTexture;
        public static Texture2D ShopSwordTexture {
            get => GetLoadedTexture(ShopSwordPath, ref _ShopSwordTexture);
            set => _ShopSwordTexture = value;
        }

        private static Texture2D _StableTexture;
        public static Texture2D StableTexture {
            get => GetLoadedTexture(StablePath, ref _StableTexture);
            set => _StableTexture = value;
        }

        private static Texture2D _StatueTexture;
        public static Texture2D StatueTexture {
            get => GetLoadedTexture(StatuePath, ref _StatueTexture);
            set => _StatueTexture = value;
        }

        private static Texture2D _SteedTexture;
        public static Texture2D SteedTexture {
            get => GetLoadedTexture(SteedPath, ref _SteedTexture);
            set => _SteedTexture = value;
        }

        private static Texture2D _WallTexture;
        public static Texture2D WallTexture {
            get => GetLoadedTexture(WallPath, ref _WallTexture);
            set => _WallTexture = value;
        }

        private static Texture2D _WallUnbuiltTexture;
        public static Texture2D WallUnbuiltTexture {
            get => GetLoadedTexture(WallUnbuiltPath, ref _WallUnbuiltTexture);
            set => _WallUnbuiltTexture = value;
        }

        private static Texture2D _WreckTexture;
        public static Texture2D WreckTexture {
            get => GetLoadedTexture(WreckPath, ref _WreckTexture);
            set => _WreckTexture = value;
        }

        private static Texture2D _TestTexture;
        public static Texture2D TestTexture {
            get => GetLoadedTexture(TestPath, ref _TestTexture);
            set => _TestTexture = value;
        }

        public static void LoadAllTextures() {
            LoadTexture(ArcherTowerPath, ref _ArcherTowerTexture);
            LoadTexture(ArcherTowerUnbuiltPath, ref _ArcherTowerUnbuiltTexture);
            LoadTexture(BakeryPath, ref _BakeryTexture);
            LoadTexture(BeachPath, ref _BeachTexture);
            LoadTexture(BeachPortalPath, ref _BeachPortalTexture);
            LoadTexture(BeggarCampPath, ref _BeggarCampTexture);
            LoadTexture(BeggarCampRubblePath, ref _BeggarCampRubbleTexture);
            LoadTexture(BoatPath, ref _BoatTexture);
            LoadTexture(BombPath, ref _BombTexture);
            LoadTexture(CastlePath, ref _CastleTexture);
            LoadTexture(ChestPath, ref _ChestTexture);
            LoadTexture(ChestGemPath, ref _ChestGemTexture);
            LoadTexture(ChestGemPayablePath, ref _ChestGemPayableTexture);
            LoadTexture(CitizenHousePath, ref _CitizenHouseTexture);
            LoadTexture(CliffPath, ref _CliffTexture);
            LoadTexture(DeerPath, ref _DeerTexture);
            LoadTexture(DogPath, ref _DogTexture);
            LoadTexture(EnemyPath, ref _EnemyTexture);
            LoadTexture(EnemyBossPath, ref _EnemyBossTexture);
            LoadTexture(EnemyBossWithCrownStealerPath, ref _EnemyBossWithCrownStealerTexture);
            LoadTexture(EnemyCrownStealerPath, ref _EnemyCrownStealerTexture);
            LoadTexture(EnemySquidPath, ref _EnemySquidTexture);
            LoadTexture(FarmHousePath, ref _FarmHouseTexture);
            LoadTexture(HermitPath, ref _HermitTexture);
            LoadTexture(LighthousePath, ref _LighthouseTexture);
            LoadTexture(PlayerPath, ref _PlayerTexture);
            LoadTexture(PortalPath, ref _PortalTexture);
            LoadTexture(Portal_BPath, ref _Portal_BTexture);
            LoadTexture(Portal_GPath, ref _Portal_GTexture);
            LoadTexture(Portal_MPath, ref _Portal_MTexture);
            LoadTexture(Portal_OPath, ref _Portal_OTexture);
            LoadTexture(Portal_PPath, ref _Portal_PTexture);
            LoadTexture(Portal_YPath, ref _Portal_YTexture);
            LoadTexture(QuarryPath, ref _QuarryTexture);
            LoadTexture(RiverPath, ref _RiverTexture);
            LoadTexture(ShopArcherPath, ref _ShopArcherTexture);
            LoadTexture(ShopFarmerPath, ref _ShopFarmerTexture);
            LoadTexture(ShopHammerPath, ref _ShopHammerTexture);
            LoadTexture(ShopPikePath, ref _ShopPikeTexture);
            LoadTexture(ShopSwordPath, ref _ShopSwordTexture);
            LoadTexture(StablePath, ref _StableTexture);
            LoadTexture(StatuePath, ref _StatueTexture);
            LoadTexture(SteedPath, ref _SteedTexture);
            LoadTexture(WallPath, ref _WallTexture);
            LoadTexture(WallUnbuiltPath, ref _WallUnbuiltTexture);
            LoadTexture(WreckPath, ref _WreckTexture);
            LoadTexture(TestPath, ref _TestTexture);
        }

        private static void LoadTexture(string path, ref Texture2D texture) {
            texture = new Texture2D(2, 2);
            var rawData = FileUtils.GetEmbeddedResourceBytes(path);
            texture.LoadImage(rawData);
        }

        private static Texture2D GetLoadedTexture(string path, ref Texture2D texture) {
            if(texture == null) {
                LoadTexture(path, ref texture);
            }

            return texture;
        }
    }
}
