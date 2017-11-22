using System.Collections.Generic;
using SharpDX;

namespace MoveToStash
{
    public static class Utils
    {
        public static void DrawButton(RectangleF rect, float borderWidth, Color boxColor, Color frameColor)
        {
            if (rect.Contains(MoveToStash.MousePosition))
                boxColor = Color.Lerp(boxColor, Color.White, 0.4f);

            DrawFrameBox(rect, borderWidth, boxColor, frameColor);
        }

        private static void DrawFrameBox(RectangleF rect, float borderWidth, Color boxColor, Color frameColor)
        {
            MoveToStash.UGraphics.DrawBox(rect, boxColor);
            MoveToStash.UGraphics.DrawFrame(rect, borderWidth, frameColor);
            MoveToStash.UGraphics.DrawImage("menu-background.png", rect, boxColor);
        }


    }

    internal static class MyDictionary
    {
        public static void AddToDic(this Dictionary<int, HashSet<string>> dic, int s, string name)
        {
            if (!dic.ContainsKey(s))
                dic[s] = new HashSet<string> { name };
            else
                dic[s].Add(name);
        }

        public static Dictionary<int, HashSet<string>> ReadTabSetting(MoveToStashSetting settings)
        {
            var dict = new Dictionary<int, HashSet<string>>();
            dict.AddToDic(settings.Amulets.Value, nameof(settings.Amulets));
            dict.AddToDic(settings.Belts.Value, nameof(settings.Belts));
            dict.AddToDic(settings.BodyArmours.Value, nameof(settings.BodyArmours));
            dict.AddToDic(settings.Boots.Value, nameof(settings.Boots));
            dict.AddToDic(settings.Currency.Value, nameof(settings.Currency));
            dict.AddToDic(settings.DivinationCards.Value, nameof(settings.DivinationCards));
            dict.AddToDic(settings.Flasks.Value, nameof(settings.Flasks));
            dict.AddToDic(settings.Gems.Value, nameof(settings.Gems));
            dict.AddToDic(settings.Gloves.Value, nameof(settings.Gloves));
            dict.AddToDic(settings.Shields.Value, nameof(settings.Shields));
            dict.AddToDic(settings.Weapons.Value, nameof(settings.Weapons));
            dict.AddToDic(settings.Quivers.Value, nameof(settings.Quivers));
            dict.AddToDic(settings.Helmets.Value, nameof(settings.Helmets));
            dict.AddToDic(settings.Rings.Value, nameof(settings.Rings));
            dict.AddToDic(settings.StoneHammer.Value, nameof(settings.StoneHammer));
            dict.AddToDic(settings.Jewels.Value, nameof(settings.Jewels));
            dict.AddToDic(settings.MapFragments.Value, nameof(settings.MapFragments));
            dict.AddToDic(settings.Maps.Value, nameof(settings.Maps));
            dict.AddToDic(settings.Leaguestones.Value, nameof(settings.Leaguestones));
            dict.AddToDic(settings.Essence.Value, nameof(settings.Essence));

            return dict;
        }
    }


}