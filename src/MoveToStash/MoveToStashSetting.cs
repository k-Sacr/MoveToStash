using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace MoveToStash
{
    public class MoveToStashSetting : ISettings
    {
        #region Setting

        [Menu("Tab num(start at left):", 10)]
        public ToggleNode TabNum { get; set; } = new ToggleNode(true);

        [Menu("Identify items?")]
        public ToggleNode Indentity { get; set; } = new ToggleNode(true);

        [Menu("Chaos set priority?")]
        public ToggleNode ChaosSet { get; set; } = new ToggleNode(true);

        [Menu("Speed:")]
        public RangeNode<int> Speed { get; set; } = new RangeNode<int>(50, 10, 200);

        [Menu("Tab Count:")]
        public RangeNode<int> TabCount { get; set; } = new RangeNode<int>(11, 4, 20);

        [Menu("Сome back to:")]
        public RangeNode<int> BackTo { get; set; } = new RangeNode<int>(1, 0, 20);

        [Menu("Show buttons?")]
        public ToggleNode ShowButtons { get; set; } = new ToggleNode(true);

        [Menu("Move to stash Key")]
        public HotkeyNode MoveKey { get; set; } = Keys.F2;

        [Menu("Chaos recipe Key")]
        public HotkeyNode ChaosKey { get; set; } = Keys.F3;

        #endregion

        #region Setting Items

        [Menu("Currency", 12, 10)]
        public RangeNode<int> Currency { get; set; } = new RangeNode<int>(2, 0, 20);

        [Menu("Divination Cards", 11, 10)]
        public RangeNode<int> DivinationCards { get; set; } = new RangeNode<int>(1, 0, 20);

        [Menu("Gems", 13, 10)]
        public RangeNode<int> Gems { get; set; } = new RangeNode<int>(3, 0, 20);

        [Menu("Jewels", 14, 10)]
        public RangeNode<int> Jewels { get; set; } = new RangeNode<int>(5, 0, 20);

        [Menu("Map Fragments", 15, 10)]
        public RangeNode<int> MapFragments { get; set; } = new RangeNode<int>(4, 0, 20);

        [Menu("Leaguestones", 16, 10)]
        public RangeNode<int> Leaguestones { get; set; } = new RangeNode<int>(4, 0, 20);

        [Menu("Maps", 17, 10)]
        public RangeNode<int> Maps { get; set; } = new RangeNode<int>(4, 0, 20);

        [Menu("Essence", 18, 10)]
        public RangeNode<int> Essence { get; set; } = new RangeNode<int>(3, 0, 20);

        [Menu("Flasks", 19, 10)]
        public RangeNode<int> Flasks { get; set; } = new RangeNode<int>(5, 0, 20);

        [Menu("Weapons", 20, 10)]
        public RangeNode<int> Weapons { get; set; } = new RangeNode<int>(6, 0, 20);

        [Menu("Shields", 21, 10)]
        public RangeNode<int> Shields { get; set; } = new RangeNode<int>(6, 0, 20);

        [Menu("Quivers", 22, 10)]
        public RangeNode<int> Quivers { get; set; } = new RangeNode<int>(6, 0, 20);

        [Menu("Body Armours", 23, 10)]
        public RangeNode<int> BodyArmours { get; set; } = new RangeNode<int>(7, 0, 20);

        [Menu("Helmets", 24, 10)]
        public RangeNode<int> Helmets { get; set; } = new RangeNode<int>(8, 0, 20);

        [Menu("Boots", 25, 10)]
        public RangeNode<int> Boots { get; set; } = new RangeNode<int>(8, 0, 20);

        [Menu("Gloves", 26, 10)]
        public RangeNode<int> Gloves { get; set; } = new RangeNode<int>(8, 0, 20);

        [Menu("Belts", 27, 10)]
        public RangeNode<int> Belts { get; set; } = new RangeNode<int>(9, 0, 20);

        [Menu("Rings", 28, 10)]
        public RangeNode<int> Rings { get; set; } = new RangeNode<int>(9, 0, 20);

        [Menu("Amulets", 29, 10)]
        public RangeNode<int> Amulets { get; set; } = new RangeNode<int>(9, 0, 20);

        [Menu("Item with Veiled affix", 30, 10)]
        public RangeNode<int> ItemWithVeiled { get; set; } = new RangeNode<int>(10, 0, 20);

        [Menu("Stone Hammer", 30, 10)]
        public RangeNode<int> StoneHammer { get; set; } = new RangeNode<int>(10, 0, 20);

        #endregion

        public ToggleNode Enable { get; set; } = new ToggleNode(false);
    }
}