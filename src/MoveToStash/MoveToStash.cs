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
        private bool _holdKey;
        private IngameState _ingameState;
        private Element _inventoryZone;
        private Thread _moveThread;
        private bool _run;
        private int _tabCount;

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
                    if (_moveThread == null || !_moveThread.IsAlive)
                    {
                        _moveThread = new Thread(ScanInventory);
                        _moveThread.Start();
                    }
                    else if (_run && _moveThread != null && _moveThread.IsAlive)
                    {
                        _run = false;
                    }
                }
                else if (_holdKey && !WinApi.IsKeyDown(Keys.F3))
                {
                    _holdKey = false;
                }
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
            if (!GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible)
            {
                _run = false;
                return;
            }
            LogMessage("MoveToStash start!", 3);
            Thread.Sleep(500);
            _run = true;
            _tabCount = Settings.TabCount;
            var tabsValue = ReadTabSetting();

            Indentity();
            FirstTab();

            if (!_run)
                return;

            var currentTab = 1;
            while (currentTab <= _tabCount)
            {
                var items = _inventoryZone.Children;
                foreach (var child in items)
                {
                    if (!_run)
                        return;

                    var item = child.AsObject<InventoryItemIcon>().Item;
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
                Thread.Sleep(100);
                NextTab();
                currentTab++;
                Thread.Sleep(500);
            }
            LogMessage("MoveToStash Stop!", 3);
        }

        private bool ChechIngoredCell(SharpDX.Vector2 position)
        {
            var invPoint = _inventoryZone.GetClientRect();
            var invBottomRight = invPoint.BottomRight;
            var wCell = invPoint.Width / 12;
            var hCell = invPoint.Height / 5;
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

                var item = child.AsObject<InventoryItemIcon>().Item;
                var st = item?.Path.Split('/');
                var modsComponent = item?.GetComponent<Mods>();
                if (modsComponent != null && (modsComponent.ItemRarity != ItemRarity.Rare || modsComponent.Identified || st[2] == "Maps"))
                    continue;

                var position = child.GetClientRect().Center;
                MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 10);
                Thread.Sleep(100);
                MouseTools.MouseLeftClickEvent();
                Thread.Sleep(60);
            }
            Thread.Sleep(100);
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
            var item = child.AsObject<InventoryItemIcon>().Item;
            var bit = GameController.Files.BaseItemTypes.Translate(item?.Path);
            return bit.BaseName == baseName;
        }

        private void MouseClickCtrl(SharpDX.Vector2 position)
        {
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 20);
            Thread.Sleep(100);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLControlVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
            Thread.Sleep(60);
            MouseTools.MouseLeftClickEvent();
            Thread.Sleep(60);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLControlVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
        }

        private void NextTab()
        {
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
            Thread.Sleep(60);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
        }

        private void FirstTab()
        {
            ReadTabSetting();
            int tabCount = Settings.TabCount;
            do
            {
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
                Thread.Sleep(60);
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                Thread.Sleep(60);

                tabCount--;
            }
            while (tabCount > 0);
        }

        private void UseScrollWisdom(RectangleF orbPosition)
        {
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(orbPosition.Center.X, orbPosition.Center.Y), 10);
            Thread.Sleep(100);
            MouseTools.MouseRightClickEvent();
            Thread.Sleep(60);
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