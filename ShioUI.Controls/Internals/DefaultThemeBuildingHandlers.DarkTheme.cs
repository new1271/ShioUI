using System;
using System.Collections.Generic;
using System.Drawing;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Theme;

namespace ShioUI.Controls.Internals;

partial class ThemeBuildingHandlers
{
    partial class DarkTheme
    {
        private static partial IEnumerable<KeyValuePair<string, IThemedBrushFactory>> InitializeBrushFactories(
            Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc)
        {
            // 右鍵選單
            yield return KeyValuePair.Create(
                key: "app.contextMenu.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.back.hovered",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(200, 200, 200)));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.back.pressed",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.fore.inactive",
                value: queryBrushFunc("app.control.fore.inactive"));
            yield return KeyValuePair.Create(
                key: "app.contextMenu.fore.hovered",
                value: ThemedBrushFactory.FromColor(Color.White));

            // 標籤
            yield return KeyValuePair.Create(
                key: "app.label.fore",
                value: queryBrushFunc("app.control.fore"));

            // 按鈕
            yield return KeyValuePair.Create(
                key: "app.button.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.button.border.hovered",
                value: queryBrushFunc("app.control.border.hovered"));
            yield return KeyValuePair.Create(
                key: "app.button.face",
                value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunc("app.control.back"), 1.5f));
            yield return KeyValuePair.Create(
                key: "app.button.face.hovered",
                value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunc("app.control.back"), 1.2f));
            yield return KeyValuePair.Create(
                key: "app.button.face.pressed",
                value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunc("app.control.back"), 0.7f));
            yield return KeyValuePair.Create(
                key: "app.button.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.button.fore.inactive",
                value: queryBrushFunc("app.control.fore.inactive"));

            // 字型圖示按鈕
            yield return KeyValuePair.Create(
                key: "app.fontIconButton.face",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));
            yield return KeyValuePair.Create(
                key: "app.fontIconButton.face.hovered",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(96, 162, 252)));
            yield return KeyValuePair.Create(
                key: "app.fontIconButton.face.pressed",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(127, 180, 252)));

            // 核取方塊
            yield return KeyValuePair.Create(
                key: "app.checkBox.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.border.hovered",
                value: queryBrushFunc("app.control.border.hovered"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.border.pressed",
                value: queryBrushFunc("app.control.border.pressed"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.border.checked",
                value: queryBrushFunc("app.checkBox.border"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.border.hovered.checked",
                value: queryBrushFunc("app.checkBox.border.hovered"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.border.pressed.checked",
                value: queryBrushFunc("app.checkBox.border.pressed"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.mark",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.fore.hovered",
                value: queryBrushFunc("app.control.fore.hovered"));
            yield return KeyValuePair.Create(
                key: "app.checkBox.fore.pressed",
                value: queryBrushFunc("app.control.fore"));

            // 開關按鈕
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.border.hovered",
                value: queryBrushFunc("app.control.border.hovered"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.border.pressed",
                value: queryBrushFunc("app.control.border.pressed"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.thumb",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.thumb.hovered",
                value: queryBrushFunc("app.control.fore.hovered"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.thumb.pressed",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.track",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(125, 125, 125)));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.track.checked",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.fore.hovered",
                value: queryBrushFunc("app.control.fore.hovered"));
            yield return KeyValuePair.Create(
                key: "app.toggleSwitch.fore.pressed",
                value: queryBrushFunc("app.control.fore"));

            // 卷軸
            yield return KeyValuePair.Create(
                key: "app.scrollBar.back",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(50, 50, 50)));
            yield return KeyValuePair.Create(
                key: "app.scrollBar.fore",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(149, 149, 149)));
            yield return KeyValuePair.Create(
                key: "app.scrollBar.fore.hovered",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(164, 164, 164)));
            yield return KeyValuePair.Create(
                key: "app.scrollBar.fore.pressed",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(200, 200, 200)));

            // 文字方塊
            yield return KeyValuePair.Create(
                key: "app.textbox.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.textbox.back.disabled",
                value: queryBrushFunc("app.control.back.disabled"));
            yield return KeyValuePair.Create(
                key: "app.textbox.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.textbox.border.focused",
                value: queryBrushFunc("app.control.border.pressed"));
            yield return KeyValuePair.Create(
                key: "app.textbox.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.textbox.fore.inactive",
                value: queryBrushFunc("app.control.fore.inactive"));
            yield return KeyValuePair.Create(
                key: "app.textbox.selection.back",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(0, 120, 215)));
            yield return KeyValuePair.Create(
                key: "app.textbox.selection.fore",
                value: ThemedBrushFactory.FromColor(Color.White));

            // 下拉式方塊
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.back.disabled",
                value: queryBrushFunc("app.control.back.disabled"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.back.hovered",
                value: queryBrushFunc("app.control.back.hovered"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.border.hovered",
                value: queryBrushFunc("app.control.border.hovered"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.border.pressed",
                value: queryBrushFunc("app.control.border.pressed"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.list.back.hovered",
                value: queryBrushFunc("app.contextMenu.back.hovered"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.list.back.pressed",
                value: queryBrushFunc("app.contextMenu.back.pressed"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.list.fore.hovered",
                value: queryBrushFunc("app.contextMenu.fore.hovered"));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.button",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(215, 215, 215)));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.button.hovered",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(190, 190, 190)));
            yield return KeyValuePair.Create(
                key: "app.dropdownBox.button.pressed",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(150, 150, 150)));

            // 進度條
            yield return KeyValuePair.Create(
                key: "app.progressBar.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.progressBar.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.progressBar.fore",
                value: queryBrushFunc("app.control.fore.inactive"));

            // 群組方塊
            yield return KeyValuePair.Create(
                key: "app.groupBox.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.groupBox.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.groupBox.fore",
                value: queryBrushFunc("app.control.fore"));
            yield return KeyValuePair.Create(
                key: "app.groupBox.fore.description",
                value: queryBrushFunc("app.control.fore.description"));
            yield return KeyValuePair.Create(
                key: "app.groupBox.card.back",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(100, 100, 100, 127)));
            yield return KeyValuePair.Create(
                key: "app.groupBox.card.fore",
                value: queryBrushFunc("app.groupBox.fore"));
            yield return KeyValuePair.Create(
                key: "app.groupBox.card.fore.description",
                value: queryBrushFunc("app.groupBox.fore.description"));

            // 列表方塊
            yield return KeyValuePair.Create(
                key: "app.listbox.back",
                value: queryBrushFunc("app.control.back"));
            yield return KeyValuePair.Create(
                key: "app.listbox.back.disabled",
                value: queryBrushFunc("app.control.back.disabled"));
            yield return KeyValuePair.Create(
                key: "app.listbox.border",
                value: queryBrushFunc("app.control.border"));
            yield return KeyValuePair.Create(
                key: "app.listbox.fore",
                value: queryBrushFunc("app.control.fore"));
        }
    }
}
