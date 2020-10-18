using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore.Shared.Nodes;
using ImGuiNET;

namespace MoveToStash
{
    public class ImGuiExtension
    {
        // Int Sliders
        public static int IntSlider(string labelString, int value, int minValue, int maxValue)
        {
            var refValue = value;
            ImGui.SliderInt(labelString, ref refValue, minValue, maxValue, "%.00f");
            return refValue;
        }

        public static int IntSlider(string labelString, string sliderString, int value, int minValue, int maxValue)
        {
            var refValue = value;
            ImGui.SliderInt(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}");
            return refValue;
        }

        public static int IntSlider(string labelString, RangeNode<int> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderInt(labelString, ref refValue, setting.Min, setting.Max);
            return refValue;
        }

        public static int IntSlider(string labelString, string sliderString, RangeNode<int> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderInt(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}");
            return refValue;
        }

        // float Sliders
        public static float FloatSlider(string labelString, float value, float minValue, float maxValue)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, "%.00f", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, float value, float minValue, float maxValue, float power)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, "%.00f", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, float value, float minValue, float maxValue)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, float value, float minValue, float maxValue, float power)
        {
            var refValue = value;
            ImGui.SliderFloat(labelString, ref refValue, minValue, maxValue, $"{sliderString}: {value}", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, RangeNode<float> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, "%.00f", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, RangeNode<float> setting, float power)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, "%.00f", power);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, RangeNode<float> setting)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}", 1f);
            return refValue;
        }

        public static float FloatSlider(string labelString, string sliderString, RangeNode<float> setting, float power)
        {
            var refValue = setting.Value;
            ImGui.SliderFloat(labelString, ref refValue, setting.Min, setting.Max, $"{sliderString}: {setting.Value}", power);
            return refValue;
        }

        // Checkboxes
        public static bool Checkbox(string labelString, bool boolValue)
        {
            ImGui.Checkbox(labelString, ref boolValue);
            return boolValue;
        }

        public static bool Checkbox(string labelString, bool boolValue, out bool outBool)
        {
            ImGui.Checkbox(labelString, ref boolValue);
            outBool = boolValue;
            return boolValue;
        }

        // Hotkey Selector
        public static IEnumerable<Keys> KeyCodes()
        {
            return Enum.GetValues(typeof(Keys)).Cast<Keys>();
        }

        
        // Combo Box

        public static string ComboBox(string sideLabel, string currentSelectedItem, List<string> objectList,
            ImGuiComboFlags comboFlags = ImGuiComboFlags.HeightRegular)
        {
            if (ImGui.BeginCombo(sideLabel, currentSelectedItem, comboFlags))
            {
                var refObject = currentSelectedItem;

                for (var n = 0; n < objectList.Count; n++)
                {
                    var isSelected = refObject == objectList[n];

                    if (ImGui.Selectable(objectList[n], isSelected))
                    {
                        ImGui.EndCombo();
                        return objectList[n];
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            return currentSelectedItem;
        }

        public static string ComboBox(string sideLabel, string currentSelectedItem, List<string> objectList, out bool didChange,
            ImGuiComboFlags comboFlags = ImGuiComboFlags.HeightRegular)
        {
            if (ImGui.BeginCombo(sideLabel, currentSelectedItem, comboFlags))
            {
                var refObject = currentSelectedItem;

                for (var n = 0; n < objectList.Count; n++)
                {
                    var isSelected = refObject == objectList[n];

                    if (ImGui.Selectable(objectList[n], isSelected))
                    {
                        didChange = true;
                        ImGui.EndCombo();
                        return objectList[n];
                    }

                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            didChange = false;
            return currentSelectedItem;
        }

        public static unsafe string InputText(string label, string currentValue, uint maxLength, ImGuiInputTextFlags flags)
        {
            var currentStringBytes = Encoding.Default.GetBytes(currentValue);
            var buffer = new byte[maxLength];
            Array.Copy(currentStringBytes, buffer, Math.Min(currentStringBytes.Length, maxLength));
            int cursor_pos = -1;
            int Callback(ImGuiInputTextCallbackData* data)
            {
                int* p_cursor_pos = (int*)data->UserData;

                if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                    *p_cursor_pos = data->CursorPos;
                return 0;
            }

            ImGui.InputText(label, buffer, maxLength, flags, Callback, (IntPtr)(&cursor_pos));
            return Encoding.Default.GetString(buffer).TrimEnd('\0');
        }

        // Color menu tabs
        //public static void ImGuiExtension_ColorTabs(string idString, int height, IReadOnlyList<string> settingList, ref int selectedItem, ref int uniqueIdPop)
        //{
        //    var newcontentRegionArea = new System.Numerics.Vector2();
        //    newcontentRegionArea = ImGuiNative.igGetContentRegionAvail();
        //    var boxRegion = new System.Numerics.Vector2(newcontentRegionArea.X, height);
        //    if (ImGui.BeginChild(idString, boxRegion, true, ImGuiWindowFlags.HorizontalScrollbar))
        //    {
        //        for (var i = 0; i < settingList.Count; i++)
        //        {
        //            ImGui.PushID(uniqueIdPop);
        //            var hue = 1f / settingList.Count * i;
        //            ImGui.PushStyleColor(ImGuiCol.Button, ImColor_HSV(hue, 0.6f, 0.6f, 0.8f));
        //            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImColor_HSV(hue, 0.7f, 0.7f, 0.9f));
        //            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImColor_HSV(hue, 0.8f, 0.8f, 1.0f));
        //            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);
        //            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, 2.0f);
        //            if (i > 0) ImGui.SameLine();
        //            if (ImGui.Button(settingList[i])) selectedItem = i;
        //            uniqueIdPop++;
        //            ImGui.PopStyleVar();
        //            ImGui.PopStyleColor(3);
        //            ImGui.PopID();
        //        }

        //    }
        //    ImGui.EndChild();
        //}

        //public static System.Numerics.Vector4 ImColor_HSV(float h, float s, float v, float a)
        //{
        //    ImGui.ColorConvertHSVtoRGB(h, s, v, out var r, out var g, out var b);
        //    return new System.Numerics.Vector4(r, g, b, a);
        //}
    }
}
