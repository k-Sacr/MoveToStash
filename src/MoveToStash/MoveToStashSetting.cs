using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace MoveToStash
{
    public class MoveToStashSetting : SettingsBase
    {
        public MoveToStashSetting()
        {
            Enable = true;
            TabNum = true;
            Indentity = true;
            Speed = new RangeNode<int>(50, 10, 200);

            TabCount = new RangeNode<int>(11, 4, 20);
            BackTo = new RangeNode<int>(1, 0, 20);
            MoveKey = Keys.F2;
            ChaosKey = Keys.F3;
            ShowButtons = true;
            ChaosSet = true;

            DivinationCards = new RangeNode<int>(1, 0, 20);
            Currency = new RangeNode<int>(2, 0, 20);
            Gems = new RangeNode<int>(3, 0, 20);
            Essence = new RangeNode<int>(3, 0, 20);
            Leaguestones = new RangeNode<int>(4, 0, 20);
            Maps = new RangeNode<int>(4, 0, 20);
            MapFragments = new RangeNode<int>(4, 0, 20);
            Jewels = new RangeNode<int>(5, 0, 20);
            Flasks = new RangeNode<int>(5, 0, 20);
            Weapons = new RangeNode<int>(6, 0, 20);
            Shields = new RangeNode<int>(6, 0, 20);
            Quivers = new RangeNode<int>(6, 0, 20);
            BodyArmours = new RangeNode<int>(7, 0, 20);
            Helmets = new RangeNode<int>(8, 0, 20);
            Boots = new RangeNode<int>(8, 0, 20);
            Gloves = new RangeNode<int>(8, 0, 20);
            Belts = new RangeNode<int>(9, 0, 20);
            Rings = new RangeNode<int>(9, 0, 20);
            Amulets = new RangeNode<int>(9, 0, 20);
            StoneHammer = new RangeNode<int>(10, 0, 20);
        }

        #region Setting

        [Menu("Tab num(start at left):", 10)]
        public ToggleNode TabNum { get; set; }

        [Menu("Identify items?")]
        public ToggleNode Indentity { get; set; }

        [Menu("Chaos set priority?")]
        public ToggleNode ChaosSet { get; set; }

        [Menu("Speed:")]
        public RangeNode<int> Speed { get; set; }

        [Menu("Tab Count:")]
        public RangeNode<int> TabCount { get; set; }

        [Menu("Сome back to:")]
        public RangeNode<int> BackTo { get; set; }

        [Menu("Show buttons?")]
        public ToggleNode ShowButtons { get; set; }

        [Menu("Move to stash Key")]
        public HotkeyNode MoveKey { get; set; }

        [Menu("Chaos recipe Key")]
        public HotkeyNode ChaosKey { get; set; }
        
        #endregion


        #region Setting Items

        [Menu("Currency", 12, 10)]
        public RangeNode<int> Currency { get; set; }

        [Menu("Divination Cards", 11, 10)]
        public RangeNode<int> DivinationCards { get; set; }

        [Menu("Gems", 13, 10)]
        public RangeNode<int> Gems { get; set; }

        [Menu("Jewels", 14, 10)]
        public RangeNode<int> Jewels { get; set; }

        [Menu("Map Fragments", 15, 10)]
        public RangeNode<int> MapFragments { get; set; }

        [Menu("Leaguestones", 16, 10)]
        public RangeNode<int> Leaguestones { get; set; }

        [Menu("Maps", 17, 10)]
        public RangeNode<int> Maps { get; set; }

        [Menu("Essence", 18, 10)]
        public RangeNode<int> Essence { get; set; }

        [Menu("Flasks", 19, 10)]
        public RangeNode<int> Flasks { get; set; }

        [Menu("Weapons", 20, 10)]
        public RangeNode<int> Weapons { get; set; }

        [Menu("Shields", 21, 10)]
        public RangeNode<int> Shields { get; set; }

        [Menu("Quivers", 22, 10)]
        public RangeNode<int> Quivers { get; set; }

        [Menu("Body Armours", 23, 10)]
        public RangeNode<int> BodyArmours { get; set; }

        [Menu("Helmets", 24, 10)]
        public RangeNode<int> Helmets { get; set; }

        [Menu("Boots", 25, 10)]
        public RangeNode<int> Boots { get; set; }

        [Menu("Gloves", 26, 10)]
        public RangeNode<int> Gloves { get; set; }

        [Menu("Belts", 27, 10)]
        public RangeNode<int> Belts { get; set; }

        [Menu("Rings", 28, 10)]
        public RangeNode<int> Rings { get; set; }

        [Menu("Amulets", 29, 10)]
        public RangeNode<int> Amulets { get; set; }

        [Menu("Stone Hammer", 30, 10)]
        public RangeNode<int> StoneHammer { get; set; }

        #endregion
    }
}