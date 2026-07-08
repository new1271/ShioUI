using System.Collections.Generic;

using RiceTea.Core.Helpers;

using ShioUI.Controls;
using ShioUI.Extensions;
using ShioUI.Input;
using ShioUI.Layout;

namespace ShioUI.Test;

partial class MainWindow
{
    private readonly List<UIElement>[] _elementLists = new List<UIElement>[3];
    private InputMethod? _ime;
    private ProgressBar? _progressBar;

    protected override IEnumerable<UIElement?> EnumerateActiveElements(uint pageIndex)
        => _elementLists[pageIndex];

    protected override void InitializeElements()
    {
        _ime = new InputMethod(this);

        InitializeFirstPageElements();
        InitializeSecondPageElements();
        InitializeThirdPageElements();
    }

    private void InitializeFirstPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();

        Button button = new Button(this)
        {
            Text = "請點我!",
            LeftExpression = UIConstants.ElementMargin,
            TopExpression = UIConstants.ElementMargin,
        }.WithAutoWidth().WithAutoHeight();
        button.Click += Button_Click;
        elementList.Add(button);

        TextBox textBox = new TextBox(this, _ime)
        {
            LeftExpression = button.RightDefinition + UIConstants.ElementMargin,
            TopExpression = UIConstants.ElementMargin,
            RightExpression = PageWidthDefinition - UIConstants.ElementMargin,
            HeightExpression = button.HeightDefinition,
            Watermark = "這裡可以輸入文字喔!"
        };
        textBox.KeyDown += TextBox_KeyDown;
        elementList.Add(textBox);

        ListBox listBox = new ListBox(this)
        {
            Mode = ListBoxMode.Some,
            LeftExpression = UIConstants.ElementMargin,
            TopExpression = textBox.BottomDefinition + UIConstants.ElementMargin,
            WidthExpression = 250,
            BottomExpression = PageHeightDefinition - UIConstants.ElementMargin,
        };
        IList<string> items = listBox.Items;
        for (int i = 1; i <= 200; i++)
            items.Add("物件 " + i.ToString());
        elementList.Add(listBox);

        GroupBox groupBox = new GroupBox(this)
        {
            LeftExpression = listBox.RightDefinition + UIConstants.ElementMargin,
            TopExpression = textBox.BottomDefinition + UIConstants.ElementMargin,
            RightExpression = PageWidthDefinition - UIConstants.ElementMargin,
            Title = "群組容器",
        }.WithAutoHeight();
        using (var scope = groupBox.EnterContentPageScope())
        {
            LayoutNode leftNode = scope.PageLeftDefinition + UIConstants.ElementMarginDefinition;
            LayoutNode topNode = scope.PageTopDefinition + UIConstants.ElementMarginDefinition;
            LayoutNode rightNode = scope.PageRightDefinition - UIConstants.ElementMarginDefinition;

            groupBox.AddChild(new CheckBox(this)
            {
                LeftExpression = leftNode,
                TopExpression = topNode,
                Text = "可以勾選的方塊"
            }.WithAutoWidth().WithAutoHeight());

            ComboBox comboBox = new ComboBox(this)
            {
                LeftExpression = leftNode,
                TopExpression = groupBox.FirstChild!.BottomDefinition + UIConstants.ElementMarginDefinition,
                WidthExpression = 200
            }.WithAutoHeight();
            Label label = new Label(this)
            {
                LeftExpression = leftNode,
                TopExpression = comboBox.BottomDefinition + UIConstants.ElementMarginDefinition,
                RightExpression = rightNode,
                Text = "底下是進度條測試",
                Alignment = TextAlignment.MiddleCenter
            }.WithAutoHeight();
            Button leftButton = new Button(this)
            {
                LeftExpression = label.LeftDefinition,
                TopExpression = label.BottomDefinition + UIConstants.ElementMarginDefinition,
                Text = "-"
            }.WithAutoWidth().WithAutoHeight();
            Button rightButton = new Button(this)
            {
                TopExpression = label.BottomDefinition + UIConstants.ElementMarginDefinition,
                RightExpression = label.RightDefinition,
                Text = "+"
            }.WithAutoWidth().WithAutoHeight();
            ProgressBar progressBar = new ProgressBar(this)
            {
                LeftExpression = leftButton.RightDefinition + UIConstants.ElementMarginDefinition,
                TopExpression = label.BottomDefinition + UIConstants.ElementMarginDefinition,
                RightExpression = rightButton.LeftDefinition - UIConstants.ElementMarginDefinition,
                HeightExpression = leftButton.HeightDefinition,
                Maximium = 100.0f,
                Value = 50.0f
            };
            TextBox textBox2 = new TextBox(this, _ime)
            {
                LeftExpression = leftNode,
                TopExpression = progressBar.BottomDefinition + UIConstants.ElementMargin,
                RightExpression = rightNode,
                Watermark = "這裡也可以輸入文字喔!"
            }.WithAutoHeight();
            groupBox.AddChildren(comboBox, label, leftButton, rightButton, progressBar, textBox2);

            _progressBar = progressBar;
            comboBox.RequestDropdownListOpening += ComboBox_RequestDropdownListOpening;
            items = comboBox.Items;
            for (int i = 1; i <= 200; i++)
                items.Add("選項 " + i.ToString());
            leftButton.Click += LeftButton_Click;
            rightButton.Click += RightButton_Click;
        }
        GroupBox groupBox2 = new GroupBox(this)
        {
            LeftExpression = listBox.RightDefinition + UIConstants.ElementMargin,
            //TopExpression = groupBox.BottomDefinition + UIConstants.ElementMargin,
            BottomExpression = PageHeightDefinition - UIConstants.ElementMargin,
            RightExpression = PageWidthDefinition - UIConstants.ElementMargin,
            Title = "群組容器(倒轉)",
        }.WithAutoHeight();
        using (var scope = groupBox2.EnterContentPageScope())
        {
            LayoutNode leftNode = scope.PageLeftDefinition + UIConstants.ElementMarginDefinition;
            LayoutNode bottomNode = scope.PageBottomDefinition - UIConstants.ElementMarginDefinition;

            CheckBox a, b, c;
            groupBox2.AddChild(a = new CheckBox(this)
            {
                LeftExpression = leftNode,
                BottomExpression = bottomNode,
                Text = "可以勾選的方塊 A"
            }.WithAutoWidth().WithAutoHeight());
            groupBox2.AddChild(b = new CheckBox(this)
            {
                LeftExpression = leftNode,
                BottomExpression = a.TopDefinition - UIConstants.ElementMarginDefinition,
                Text = "可以勾選的方塊 B"
            }.WithAutoWidth().WithAutoHeight());
            groupBox2.AddChild(c = new CheckBox(this)
            {
                LeftExpression = leftNode,
                BottomExpression = b.TopDefinition - UIConstants.ElementMarginDefinition,
                Text = "可以勾選的方塊 C"
            }.WithAutoWidth().WithAutoHeight());
        }
        elementList.Add(groupBox);
        elementList.Add(groupBox2);
        _elementLists[0] = elementList;
    }

    private void InitializeSecondPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();
        TextBox textbox = new TextBox(this, _ime)
        {
            LeftExpression = UIConstants.ElementMarginDefinition,
            TopExpression = UIConstants.ElementMarginDefinition,
            RightExpression = PageWidthDefinition - UIConstants.ElementMarginDefinition,
            Watermark = "盡情輸入文字吧!",
            MultiLine = true
        };
        textbox.HeightExpression = MathHelper.Min(textbox.AutoHeightDefinition, PageHeightDefinition / 2 - textbox.TopDefinition);
        textbox.TextChanging += (object sender, ref TextChangingEventArgs _) => (sender as UIElement)?.ResetLayoutTimestamp(); // 強制讓元件下次渲染時重算布局

        Label label = new Label(this)
        {
            Text = "不過這條標籤將永遠在文字框的底下，而且文字框最大只能到頁面的一半高度",
            Alignment = TextAlignment.MiddleCenter,
            LeftExpression = textbox.LeftDefinition,
            TopExpression = textbox.BottomDefinition + UIConstants.ElementMarginDefinition,
            RightExpression = textbox.RightDefinition,
        }.WithAutoHeight();

        elementList.Add(textbox);
        elementList.Add(label);
        _elementLists[1] = elementList;
    }

    private void InitializeThirdPageElements()
    {
        List<UIElement> elementList = new List<UIElement>();
        Button rollingButton = new Button(this)
        {
            Text = "按鈕!"
        }.WithAutoWidth().WithAutoHeight();
        rollingButton.LeftExpression = (PageWidthDefinition - rollingButton.WidthDefinition) / 2;
        rollingButton.TopExpression = (PageHeightDefinition - rollingButton.HeightDefinition) / 2;
        rollingButton.Click += RollingButton_Click;
        elementList.Add(rollingButton);
        _elementLists[2] = elementList;
    }
}
