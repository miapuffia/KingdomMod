namespace KingdomMod {
    internal class ExploredRegion {
        private float _exploredLeft;
        private float _exploredRight;

        public float ExploredLeft {
            get { return _exploredLeft; }
            set {
                _exploredLeft = value;
                SetExploredLeft(value);
            }
        }

        public float ExploredRight {
            get { return _exploredRight; }
            set {
                _exploredRight = value;
                SetExploredRight(value);
            }
        }

        public ExploredRegion(string archiveFilename) {
            ExploredRegionConfig.ConfigBind(archiveFilename);
            if(HasAvailableConfig()) {
                _exploredLeft = ExploredRegionConfig.ExploredLeft;
                _exploredRight = ExploredRegionConfig.ExploredRight;
            } else {
                var player = GameExtensions.GetLocalPlayer();
                _exploredLeft = player.transform.localPosition.x;
                _exploredRight = player.transform.localPosition.x;
            }
        }

        private static void SetExploredLeft(float value) {
            ExploredRegionConfig.ExploredLeft.Value = value;
            ExploredRegionConfig.Time.Value = Managers.Inst.director.currentTime;
            ExploredRegionConfig.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
        }

        private static void SetExploredRight(float value) {
            ExploredRegionConfig.ExploredRight.Value = value;
            ExploredRegionConfig.Time.Value = Managers.Inst.director.currentTime;
            ExploredRegionConfig.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
        }

        private static bool HasAvailableConfig() {
            if(ExploredRegionConfig.ExploredLeft == 0 && ExploredRegionConfig.ExploredRight == 0)
                return false;

            if(ExploredRegionConfig.Days > Managers.Inst.director.CurrentDayForSpawning)
                return false;

            if(ExploredRegionConfig.Days == Managers.Inst.director.CurrentDayForSpawning)
                if(ExploredRegionConfig.Time > Managers.Inst.director.currentTime)
                    return false;

            return true;
        }
    }
}
