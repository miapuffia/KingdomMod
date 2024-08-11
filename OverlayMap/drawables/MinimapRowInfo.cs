using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomMod {
    internal class MinimapRowInfo {
        public MinimapRowInfo() {

        }

        public float RowHeight = 0;
        public float RowTop = 0;
        public List<MarkInfo> RowMarkList = [];
        public bool HasCastle = false;
    }
}
