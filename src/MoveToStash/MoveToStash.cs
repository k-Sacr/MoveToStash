using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using PoeHUD.Framework;
using PoeHUD.Hud;
using PoeHUD.Hud.Menu;
using PoeHUD.Hud.UI;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using SharpDX.Direct3D9;

namespace MoveToStash
{
    public class MoveToStash : BaseSettingsPlugin<MoveToStashSetting>
    {
        private Thread _moveThread;
        private IngameState _ingameState;
        private Inventory _inventory;
        private Inventory _stash;

        private bool _run;
        private bool _holdKey;

        private readonly string[] _oneClick = { "BodyArmours", "Helmets", "Boots", "Gloves", "Belts", "Amulets" };
        private readonly string[] _twoClick = { "Weapons", "Rings" };

        private int[,] _invArr = new int[5, 12];
        private List<FilterItem> _filterItems = new List<FilterItem>();

        public static Graphics UGraphics;
        public static SharpDX.Vector2 MousePosition;
        private RectangleF _rectMoveButton;
        private RectangleF _rectChaosButton;
        private RectangleF _inventoryZone;
        private MouseEventID? _mouseEventId;

        public override void Initialise()
        {
            var fileName = PluginDirectory + @"/IgnoreCellSetting.json";
            if (!File.Exists(fileName))
            {
                var arr = JsonConvert.SerializeObject(_invArr).Replace("],[", $"],{Environment.NewLine} [");
                File.WriteAllText(fileName, arr);
            }

            string json = File.ReadAllText(fileName);
            _invArr = JsonConvert.DeserializeObject<int[,]>(json);

            _filterItems.Add(new FilterItem { Type = "Belt3", ItemRarity = 0, Tab = 1, Comment = "Chance a Headhunter" });
            fileName = PluginDirectory + @"/AdvansedFilterSetting.json";
            if (!File.Exists(fileName))
            {
                var arr = JsonConvert.SerializeObject(_filterItems);
                File.WriteAllText(fileName, arr);
            }

            json = File.ReadAllText(fileName);
            _filterItems = JsonConvert.DeserializeObject<List<FilterItem>>(json);

            UGraphics = Graphics;
            MenuPlugin.eMouseEvent += OnMouseEvent;
        }

        private void OnMouseEvent(MouseEventID id, SharpDX.Vector2 position)
        {
            _mouseEventId = null;
            if (!Settings.Enable)
                return;
            MousePosition = position;
            if (id != MouseEventID.LeftButtonUp)
                return;
            _mouseEventId = id;
        }

        public override void Render()
        {
            try
            {
                _ingameState = GameController.Game.IngameState;

                if (!_ingameState.IngameUi.InventoryPanel.IsVisible)
                    return;

                _inventory = GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory];
                _inventoryZone = _inventory.InventoryUiElement.GetClientRect();
                if (!_holdKey && WinApi.IsKeyDown(Settings.MoveKey.Value))
                {
                    _holdKey = true;
                    if (_moveThread == null || !_moveThread.IsAlive)
                    {
                        _moveThread = _ingameState.IngameUi.OpenLeftPanel.IsVisible
                            ? new Thread(ScanInventory)
                            : new Thread(SellItems);
                        _moveThread.Start();
                        Thread.Sleep(200);
                    }
                    else if (_run && _moveThread != null && _moveThread.IsAlive)
                    {
                        _run = false;
                        LogMessage("Stop!", 3);

                        Thread.Sleep(200);
                    }
                }
                else if (_holdKey && !WinApi.IsKeyDown(Settings.MoveKey.Value))
                    _holdKey = false;

                if (!_holdKey && WinApi.IsKeyDown(Settings.ChaosKey.Value))
                {
                    _holdKey = true;
                    if (_moveThread == null || !_moveThread.IsAlive)
                    {
                        _moveThread = new Thread(ChaosRecipe);
                        _moveThread.Start();
                        Thread.Sleep(200);
                    }
                    else if (_run && _moveThread != null && _moveThread.IsAlive)
                    {
                        _run = false;
                        LogMessage("Stop!", 3);
                        Thread.Sleep(200);
                    }
                }
                else if (_holdKey && !WinApi.IsKeyDown(Settings.ChaosKey.Value))
                    _holdKey = false;

                if (Settings.ShowButtons.Value)
                {
                    var invPoint = _inventoryZone;
                    float wCell = invPoint.Width / 12 * 2;
                    float hCell = invPoint.Height / 5;
                    _rectMoveButton = new RectangleF(_inventoryZone.X, _inventoryZone.Y - hCell, wCell, hCell * 0.7f);
                    var invItem = _ingameState.UIHover.AsObject<NormalInventoryItem>();
                    if (_ingameState.IngameUi.InventoryPanel.IsVisible && (invItem.Item == null || !invItem.Item.IsValid) || _run)
                    {
                        #region MoveButton

                        Utils.DrawButton(_rectMoveButton, 1, new Color(55, 21, 0), Color.DarkGoldenrod);
                        if (_rectMoveButton.Contains(MousePosition) && _mouseEventId == MouseEventID.LeftButtonUp)
                        {
                            _mouseEventId = null;
                            if (_moveThread == null || !_moveThread.IsAlive)
                            {
                                _moveThread = _ingameState.IngameUi.OpenLeftPanel.IsVisible
                                    ? new Thread(ScanInventory)
                                    : new Thread(SellItems);
                                _moveThread.Start();
                                Thread.Sleep(65);
                            }
                            else if (_run && _moveThread != null && _moveThread.IsAlive)
                            {
                                _run = false;
                                LogMessage("Stop!", 3);
                                Thread.Sleep(200);
                            }
                        }
                        if (_moveThread == null || !_moveThread.IsAlive)
                        {
                            if (_ingameState.IngameUi.InventoryPanel.IsVisible && !_ingameState.IngameUi.OpenLeftPanel.IsVisible)
                                Graphics.DrawText("<-Sell", 18, _rectMoveButton.Center, Color.DarkGoldenrod,
                                    FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
                            else if (_ingameState.IngameUi.InventoryPanel.IsVisible && _ingameState.IngameUi.OpenLeftPanel.IsVisible)
                                Graphics.DrawText("<-Move", 18, _rectMoveButton.Center, Color.DarkGoldenrod,
                                    FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
                        }
                        else if (_run && _moveThread != null && _moveThread.IsAlive)
                        {
                            Graphics.DrawText("work...", 18, _rectMoveButton.Center, Color.DarkGoldenrod, FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
                        }

                        #endregion

                        #region ChaosButton

                        if (_ingameState.IngameUi.OpenLeftPanel.IsVisible)
                        {
                            _rectChaosButton = _rectMoveButton;
                            _rectChaosButton.Y = _rectMoveButton.Y - (_rectMoveButton.Height + 10);
                            Utils.DrawButton(_rectChaosButton, 1, new Color(55, 21, 0), Color.DarkGoldenrod);
                            if (_rectChaosButton.Contains(MousePosition) && _mouseEventId == MouseEventID.LeftButtonUp)
                            {
                                _mouseEventId = null;
                                if (_moveThread == null || !_moveThread.IsAlive)
                                {
                                    _moveThread = new Thread(ChaosRecipe);
                                    _moveThread.Start();
                                    Thread.Sleep(65);
                                }
                                else if (_run && _moveThread != null && _moveThread.IsAlive)
                                {
                                    _run = false;
                                    LogMessage("Stop!", 3);
                                    Thread.Sleep(200);
                                }
                            }
                            if (_moveThread == null || !_moveThread.IsAlive)
                            {
                                if (_ingameState.IngameUi.InventoryPanel.IsVisible && _ingameState.IngameUi.OpenLeftPanel.IsVisible)
                                    Graphics.DrawText("Set->", 18, _rectChaosButton.Center, Color.DarkGoldenrod,
                                        FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
                            }
                            else if (_run && _moveThread != null && _moveThread.IsAlive)
                            {
                                Graphics.DrawText("work...", 18, _rectChaosButton.Center, Color.DarkGoldenrod,
                                    FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
                            }
                        }

                        #endregion
                    }
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
            if (!_ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                _run = false;
                return;
            }
            LogMessage("MoveToStash start!", 3);
            Thread.Sleep(Settings.Speed * 3);
            _run = true;
            var tabsValue = MyDictionary.ReadTabSetting(Settings);

            if (Settings.Indentity)
                Indentity();
            FirstTab();

            if (!_run)
                return;

            int currentTab = 1;
            while (currentTab <= Settings.TabCount && _run)
            {
                var items = _inventory.VisibleInventoryItems;
                if (items.All(i => !CheckIngoredCell(i.GetClientRect())))
                {
                    BackToTab(currentTab, Settings.BackTo);
                    return;
                }
                foreach (var child in items)
                {
                    if (!_run)
                        return;

                    var item = child.AsObject<NormalInventoryItem>().Item;
                    var position = child.GetClientRect();


                    if (string.IsNullOrEmpty(item?.Path))
                        continue;
                    var itemClass = CheckItem(item);

                    if (CheckAdvansedFilter(currentTab, position, item))
                        continue;

                    if (!tabsValue.ContainsKey(currentTab) || !tabsValue[currentTab].Contains(itemClass))
                        continue;

                    if (!CheckIngoredCell(position))
                        continue;

                    position.X += GameController.Window.GetWindowRectangle().X;
                    position.Y += GameController.Window.GetWindowRectangle().Y;
                    MouseClickCtrl(position.Center);
                }
                currentTab++;
                if (currentTab > _ingameState.ServerData.StashPanel.TotalStashes)
                    return;
                NextTab(currentTab);
            }
            BackToTab(currentTab, Settings.BackTo);
            LogMessage("MoveToStash Stop!", 3);
            _run = false;
        }

        private void BackToTab(int currentTab, int needTab)
        {
            if (needTab < 0)
                return;
            if (needTab > currentTab)
            {
                do
                {
                    KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
                    Thread.Sleep(Settings.Speed);
                    KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyRightVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                    Thread.Sleep(Settings.Speed);

                    if (_ingameState.ServerData.StashPanel.GetStashInventoryByIndex(needTab - 1).AsObject<Element>().IsVisible)
                        return;
                }
                while (_run);
            }
            else if (needTab < currentTab)
            {
                do
                {
                    KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
                    Thread.Sleep(Settings.Speed);
                    KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                    Thread.Sleep(Settings.Speed);

                    if (_ingameState.ServerData.StashPanel.GetStashInventoryByIndex(needTab - 1).AsObject<Element>().IsVisible)
                        return;
                }
                while (_run);
            }
        }

        private void SellItems()
        {
            if (!_ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                _run = false;
                return;
            }
            LogMessage("Sell Items start!", 3);
            _run = true;
            Thread.Sleep(Settings.Speed * 3);
            if (!_run)
                return;

            var items = _inventory.VisibleInventoryItems;
            if (items.All(i => !CheckIngoredCell(i.GetClientRect())))
                return;
            foreach (var child in items)
            {
                if (!_run)
                    return;

                var item = child.AsObject<NormalInventoryItem>().Item;
                var position = child.GetClientRect();

                if (string.IsNullOrEmpty(item?.Path))
                    continue;
                if (item.Path.Contains("Currency")
                    || item.Path.Contains("Jewels"))
                    continue;
                if (!CheckIngoredCell(position))
                    continue;

                position.X += GameController.Window.GetWindowRectangle().X;
                position.Y += GameController.Window.GetWindowRectangle().Y;
                MouseClickCtrl(position.Center);
            }
            LogMessage("Sell Items Stop!", 3);
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
            var tabsValue = MyDictionary.ReadTabSetting(Settings);

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
                currentTab++;
                if (currentTab > _ingameState.ServerData.StashPanel.TotalStashes)
                    return;
                NextTab(currentTab);
            }
            BackToTab(currentTab, Settings.BackTo);
            _run = false;
            LogMessage("MoveToStash Stop!", 3);
        }

        private bool ClickItem(string type, int count = 1)
        {
            try
            {
                var itemInInv = _inventory.VisibleInventoryItems.Select(element => element.AsObject<NormalInventoryItem>().Item);
                count -=
                    itemInInv.Where(t => t != null && t.Path.Contains(type))
                        .Select(entity => entity.GetComponent<Mods>())
                        .Count(mods => mods.ItemRarity == ItemRarity.Rare && mods.ItemLevel >= 60);

                if (count <= 0)
                    return true;

                var items = _stash.VisibleInventoryItems;
                if (items != null && items.Count > 0 && type == "Weapons")
                    foreach (var child in items)
                    {
                        var item = child.AsObject<NormalInventoryItem>().Item;
                        if (string.IsNullOrEmpty(item?.Path) || item.Path.Contains("TwoHandWeapons"))
                        {
                            if (!_run)
                                return false;

                            var position = child.GetClientRect().Center;
                            position.X += GameController.Window.GetWindowRectangle().X;
                            position.Y += GameController.Window.GetWindowRectangle().Y;
                            var modsComponent = item?.GetComponent<Mods>();

                            if (modsComponent != null
                                && modsComponent.ItemRarity == ItemRarity.Rare
                                && modsComponent.ItemLevel >= 60)
                            {
                                MouseClickCtrl(position);
                                return true;
                            }
                        }
                    }
                if (items != null && items.Count > 0)
                    foreach (var child in items)
                    {
                        if (!_run)
                            return false;

                        var item = child.AsObject<NormalInventoryItem>().Item;
                        var position = child.GetClientRect().Center;
                        position.X += GameController.Window.GetWindowRectangle().X;
                        position.Y += GameController.Window.GetWindowRectangle().Y;
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
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.Source);
                throw;
            }
            LogMessage($">>> Not found item: {type} in current tab", 5);
            return false;
        }

        private bool CheckIngoredCell(RectangleF position)
        {
            var invPoint = _inventoryZone;
            float wCell = invPoint.Width / 12;
            float hCell = invPoint.Height / 5;
            int x = (int) ((1f + position.X - invPoint.X) / wCell);
            int y = (int) ((1f + position.Y - invPoint.Y) / hCell);
            return _invArr[y, x] == 0;
        }

        private void Indentity()
        {
            if (!_run)
                return;

            var scroll = _inventory.VisibleInventoryItems.FirstOrDefault(element => CheckNameItem(element, "Scroll of Wisdom"));
            if (scroll == null)
                return;
            var scrollPos = scroll.GetClientRect().Center;
            scrollPos.X += GameController.Window.GetWindowRectangle().X;
            scrollPos.Y += GameController.Window.GetWindowRectangle().Y;
            UseScrollWisdom(scrollPos);

            var items = _inventory.VisibleInventoryItems;
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
                position.X += GameController.Window.GetWindowRectangle().X;
                position.Y += GameController.Window.GetWindowRectangle().Y;
                MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 22);
                Thread.Sleep(Settings.Speed);
                MouseTools.MouseLeftClickEvent();
                Thread.Sleep(Settings.Speed);
            }
            Thread.Sleep(Settings.Speed);
            KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyEventShiftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
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
            if ((st[2] == "Rings" || st[2] == "Amulets" || st[2] == "Belts") && modsComponent.ItemRarity == ItemRarity.Magic)
                return "";
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
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(position.X, position.Y), 22);
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
            int delay = Settings.Speed;
            do
            {
                Thread.Sleep(delay);
                delay -= delay / 5;
                inv = _ingameState.ServerData.StashPanel.GetStashInventoryByIndex(stashNum - 1);
            }
            while (inv == null && _run && delay > 0);
            _stash = inv;
        }

        private void FirstTab()
        {
            do
            {
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyDown);
                Thread.Sleep(Settings.Speed);
                KeyTools.KeyEvent(WinApiMouse.KeyEventFlags.KeyLeftVirtual, WinApiMouse.KeyEventFlags.KeyEventKeyUp);
                Thread.Sleep(Settings.Speed);

                if (_ingameState.ServerData.StashPanel.GetStashInventoryByIndex(0).AsObject<Element>().IsVisible)
                    return;
            }
            while (_run);
        }

        private bool CheckAdvansedFilter(int currentTab, RectangleF position, Entity itemClass)
        {
            foreach (var item in _filterItems)
            {
                if (!itemClass.Path.Contains(item.Type))
                    continue;

                var modsComponent = itemClass.GetComponent<Mods>();
                if (item.ItemRarity < 4 && modsComponent != null && modsComponent.ItemRarity != (ItemRarity) item.ItemRarity)
                    return false;

                if (item.Tab == currentTab)
                {
                    if (!CheckIngoredCell(position))
                        continue;
                    position.X += GameController.Window.GetWindowRectangle().X;
                    position.Y += GameController.Window.GetWindowRectangle().Y;
                    MouseClickCtrl(position.Center);
                }
                return true;
            }
            return false;
        }

        private void UseScrollWisdom(SharpDX.Vector2 scrollPos)
        {
            MouseTools.MoveCursor(MouseTools.GetMousePosition(), new Vector2(scrollPos.X, scrollPos.Y), 22);
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

    

    struct FilterItem
    {
        public string Type;
        public int ItemRarity;
        public int Tab;
        public string Comment;
    }
}