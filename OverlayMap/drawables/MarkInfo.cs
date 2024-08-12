using UnityEngine;

namespace KingdomMod {
    internal class MarkInfo(float worldPosX, Color color, string sign, string name, int? count = null, string[] textLines = null, Texture2D image = null, bool isCastle = false, int rowNum = 0, bool flipImage = false, bool alwaysVisible = false) {
        private readonly bool AlwaysVisible = alwaysVisible;

        public float WorldPosX = worldPosX;
        public float Pos;
        public Color Color = color;
        public string Sign = sign;
        public string Name = name;
        public int? Count = count;
        public string[] TextLines = textLines ?? [];
        public Texture2D Image = image;
        public bool IsCastle = isCastle;
        public int RowNum = rowNum;
        public bool FlipImage = flipImage;

        private bool _Visible = true;
        public bool Visible { get => AlwaysVisible || _Visible; set => _Visible = value; }
    }
}
