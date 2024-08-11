using UnityEngine;

namespace KingdomMod {
    internal class MarkInfo(float worldPosX, Color color, string sign, string name, int? count = null, string[] textLines = null, Texture2D image = null, bool isCastle = false, int rowNum = 0, bool flipImage = false) {
        public float WorldPosX = worldPosX;
        public float Pos;
        public Color Color = color;
        public string Sign = sign;
        public string Name = name;
        public int? Count = count;
        public string[] TextLines = textLines ?? [];
        public bool Visible;
        public Texture2D Image = image;
        public bool IsCastle = isCastle;
        public int RowNum = rowNum;
        public bool FlipImage = flipImage;
    }
}
