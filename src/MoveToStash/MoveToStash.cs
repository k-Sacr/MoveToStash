using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PoeHUD.Framework;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;

namespace MoveToStash
{
    public class MoveToStash : BaseSettingsPlugin<MoveToStashSetting>
    {
        private Thread _moveThread;
        private IngameState _ingameState;
        private Element _inventoryZone;
        private Inventory _stashZone;

        private bool _run;
        private bool _holdKey;

        private readonly string[] _oneClick = { "BodyArmours", "Helmets", "Boots", "Gloves", "Belts", "Amulets" };
        private readonly string[] _twoClick = { "Weapons", "Rings" };

        public override void Initialise()
        {
        }

        public override void Render()
        {
            try
            {
                _ingameState = GameController.Game.IngameState;

                if (!_ingameState.IngameUi.InventoryPanel.IsVisible && !_ingameState.IngameUi.OpenLeftPanel.IsVisible)
                    return;

                _inventoryZone = _ingameState.ReadObject<Element>(_ingameState.IngameUi.InventoryPanel.Address + Element.OffsetBuffers + 0x42C);

                if (!_holdKey && WinApi.IsKeyDown(Keys.F3))
                {
                    _holdKey = true;
                    if (_moveThread == null || !_moveThread.IsAlive)
                    {
                        _moveThread = new Thread(ScanInventory);
                        _moveThread.Start();
                    }
                    else if (_run && _moveThread != null && _moveThread.IsAlive)
                        _run = false;
                }
                else if (_holdKey && !WinApi.IsKeyDown(Keys.F3))
                    _holdKey = false;


                if (!_holdKey && WinApi.IsKeyDown(Keys.F2))
                {
                    _holdKey = true;
                    if (_moveThread == null || !_moveThread.IsAlive)
                    {
                        _moveThread = new Thread(ChaosRecipe);
                        _moveThread.Start();
                    }
                    else if (_run && _moveThread != null && _moveThread.IsAlive)
                        _run = false;
                }
                else if (_holdKey && !WinApi.IsKeyDown(Keys.F2))
                    _holdKey = false;
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.Source);
                throw;
            }
        }

        private void ScanInventory()
        {
            if (!_ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                _run = false;
                return;
            }
            LogMessage("MoveToStash start!", 3);
            Thread.Sleep(Settings.Speed * 3);
            _run = true;
            var tabsValue = ReadTabSetting();

            if (Settings.Indentity)
                Indentity();
            FirstTab();

            if (!_run)
                return;

            int currentTab = 1;
            while (currentTab <= Settings.TabCount && _run)
            {
                var items = _inventoryZone.Children;
                foreach (var child in items)
                {
                    if (!_run)
                        return;

                    var item = child.AsObject<NormalInventoryItem>().Item;
                    var position = child.GetClientRect().Center;

                    if (string.IsNullOrEmpty(item?.Path))
                        continue;
                    var itemClass = CheckItem(item);
                    if (!tabsValue.ContainsKey(currentTab) || !tabsValue[currentTab].Contains(itemClass))
                        continue;
                    if (!ChechIngoredCell(position))
                        continue;
                    MouseClickCtrl(position);
                }
                NextTab(++currentTab);
            }
            LogMessage("MoveToStash Stop!", 3);
        }

        private void ChaosRecipe()
        {
            if (!GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible)
            {
                _run = false;
                return;
            }
            LogMessage("Chaos Recipe start!", 3);
            Thread.Sleep(Settings.Speed * 3);
            _run = true;
            var tabsValue = ReadTabSetting();

            FirstTab();

            if (!_run)
                return;

            var currentTab = 1;
            while (currentTab <= Settings.TabCount && _run)
            {
                if (tabsValue.ContainsKey(currentTab))
                    foreach (string type in tabsValue[currentTab])
                    {
                        if (_oneClick.Contains(type))
                        {
                            if (!ClickItem(type))
                                break;
                        }
                        else if (_twoClick.Contains(type))
                        {
                            if (!ClickItem(type, 2))
                                break;
                        }
                    }
                NextTab(++currentTab);
            }
            LogMessage("MoveToStash Stop!", 3);
        }

        private bool ClickItem(string type, int count = 1)
        {
            var itemInInv = _inventoryZone.Children.Select(element => element.AsObject<NormalInventoryItem>().Item);
            count -=
                itemInInv.Where(t => t != null && t.Path.Contains(type))
                    .Select(entity => entity.GetComponent<Mods>())
                    .Count(mods => mods.ItemRarity == ItemRarity.Rare && mods.ItemLevel >= 60);

            if (count <= 0)
                return true;

            var items = _stashZone.VisibleInventoryItems.DefaultIfEmpty();
            foreach (var child in items)
            {
                if (!_run)
                    return false;

                var item = child.AsObject<NormalInventoryItem>().Item;
                var position = child.GetClientRect().Center;
                var modsComponent = item?.GetComponent<Mods>();

                if (string.IsNullOrEmpty(item?.Path) || item.Path.Contains("Talisman"))
                    continue;
                var itemClass = CheckItem(item);
                if (type == itemClass && modsComponent.ItemRarity == ItemRarity.Rare && modsComponent.ItemLevel >= 60)
                {
                    MouseClickCtrl(position);
                    if (--count <= 0)
                        return true;
                }
            }

            LogMessage($">>> Not found item: {type} in current tab", 5);
            return false;
        }

        private bool ChechIngoredCell(SharpDX.Vector2 position)
        {
            var invPoint = _inventoryZone.GetClientRect();
            var invBottomRight = invPoint.BottomRight;
            float wCell = invPoint.Width / 12;
            float hCell = invPoint.Height / 5;
            if (position.X > invBottomRight.X - wCell)
                if (position.Y > invBottomRight.Y - hCell * Settings.IgnoreCell.Value)
                    return false;
            return true;
        }

        private void Indentity()
        {
            if (!_run)
                return;

            var scroll = _inventoryZone.Children.FirstOrDefault(element => CheckNameItem(element, "Scroll of Wisdom"));
            if (scroll == null)
                return;
            UseScrollWisdom(scroll.GetClientRect());

            var items = _inventoryZone.Children;
            foreach (var child in items)
            {
                if (!_run)
                {
                    KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyEventShiftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                    return;
                }

                var item = child.AsObject<NormalInventoryItem>().Item;
                var st = item?.Path.Split('/');
                var modsComponent = item?.GetComponent<Mods>();
                if (modsComponent == null || modsComponent.ItemRarity != ItemRarity.Rare || modsComponent.Identified || string.IsNullOrEmpty(item.Path)
                    || st[2] == "Maps")
                    continue;

                var position = child.GetClientRect().Center;
                MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 10);
                Thread.Sleep(Settings.Speed);
                MouseTools.MouseLeftClickEvent();
                Thread.Sleep(Settings.Speed);
            }
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyEventShiftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
        }

        private Dictionary<int, HashSet<string>> ReadTabSetting()
        {
            var dict = new Dictionary<int, HashSet<string>>();
            dict.AddToDic(Settings.Amulets.Value, nameof(Settings.Amulets));
            dict.AddToDic(Settings.Belts.Value, nameof(Settings.Belts));
            dict.AddToDic(Settings.BodyArmours.Value, nameof(Settings.BodyArmours));
            dict.AddToDic(Settings.Boots.Value, nameof(Settings.Boots));
            dict.AddToDic(Settings.Currency.Value, nameof(Settings.Currency));
            dict.AddToDic(Settings.DivinationCards.Value, nameof(Settings.DivinationCards));
            dict.AddToDic(Settings.Flasks.Value, nameof(Settings.Flasks));
            dict.AddToDic(Settings.Gems.Value, nameof(Settings.Gems));
            dict.AddToDic(Settings.Gloves.Value, nameof(Settings.Gloves));
            dict.AddToDic(Settings.Shields.Value, nameof(Settings.Shields));
            dict.AddToDic(Settings.Weapons.Value, nameof(Settings.Weapons));
            dict.AddToDic(Settings.Quivers.Value, nameof(Settings.Quivers));
            dict.AddToDic(Settings.Helmets.Value, nameof(Settings.Helmets));
            dict.AddToDic(Settings.Rings.Value, nameof(Settings.Rings));
            dict.AddToDic(Settings.StoneHammer.Value, nameof(Settings.StoneHammer));
            dict.AddToDic(Settings.Jewels.Value, nameof(Settings.Jewels));
            dict.AddToDic(Settings.MapFragments.Value, nameof(Settings.MapFragments));
            dict.AddToDic(Settings.Maps.Value, nameof(Settings.Maps));
            dict.AddToDic(Settings.Leaguestones.Value, nameof(Settings.Leaguestones));
            dict.AddToDic(Settings.Essence.Value, nameof(Settings.Essence));

            return dict;
        }

        private static string CheckItem(Entity item)
        {
            var modsComponent = item?.GetComponent<Mods>();
            if (modsComponent == null)
                return "";
            if (modsComponent.ItemRarity == ItemRarity.Unique)
                return "";
            var st = item.Path.Split('/');
            if (st.Length < 3)
                return "";
            if (st[2] == "Armours")
                return modsComponent.ItemRarity == ItemRarity.Rare ? st[3] : "";
            if (st[2] == "Weapons" && (st.Last() == "OneHandMace4" || st.Last() == "OneHandMace11" || st.Last() == "OneHandMace18"))
                return "StoneHammer";
            if (st[2] == "Weapons" && modsComponent.ItemRarity != ItemRarity.Rare)
                return "";
            if (st[2] == "Currency" && st.Last().Contains("Essence"))
                return "Essence";
            return st[2];
        }

        private bool CheckNameItem(Element child, string baseName)
        {
            if (child == null)
                return false;
            var item = child.AsObject<NormalInventoryItem>().Item;
            var bit = GameController.Files.BaseItemTypes.Translate(item?.Path);
            return bit != null && bit.BaseName == baseName;
        }

        private void MouseClickCtrl(SharpDX.Vector2 position)
        {
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 20);
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLControlVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
            Thread.Sleep(Settings.Speed);
            MouseTools.MouseLeftClickEvent();
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLControlVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
        }

        private void NextTab(int stashNum)
        {
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);

            Inventory inv;
            do
            {
                Thread.Sleep(Settings.Speed);
                inv = _ingameState.ServerData.StashPanel.getStashInventory(stashNum - 1);
            }
            while (inv == null && _run);
            _stashZone = inv;
        }

        private void FirstTab()
        {
            do
            {
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
                Thread.Sleep(Settings.Speed);
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                Thread.Sleep(Settings.Speed);

                if (_ingameState.ServerData.StashPanel.getStashInventory(0).AsObject<Element>().IsVisible)
                    return;
            }
            while (_run);
        }

        private void UseScrollWisdom(RectangleF orbPosition)
        {
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(orbPosition.Center.X, orbPosition.Center.Y), 10);
            Thread.Sleep(Settings.Speed);
            MouseTools.MouseRightClickEvent();
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyEventShiftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
        }

        private static void Log(string message)
        {
            File.AppendAllText("log.txt", message + "\n");
        }
    }

    internal static class DictionaryExtensions
    {
        public static void AddToDic(this Dictionary<int, HashSet<string>> dic, int s, string name)
        {
            if (!dic.ContainsKey(s))
                dic[s] = new HashSet<string> { name };
            else
                dic[s].Add(name);
        }
    }
}