using UnityEngine;

namespace KingdomMod {
    public struct MarkerConfigStatedImage {
        public ConfigEntryWrapper<string> Color;
        public ConfigEntryWrapper<string> Sign;
        public Texture Image;
        public MarkerConfigColor Rebuilding;
        public MarkerConfigColor Destroyed;
        public MarkerConfigColor Locked;
        public MarkerConfigColor Unlocked;
        public MarkerConfigColor Wrecked;
        public MarkerConfigColor Building;
    }
}
