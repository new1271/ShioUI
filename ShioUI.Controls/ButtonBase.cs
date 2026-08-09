using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Extensions;

namespace ShioUI.Controls;

public abstract partial class ButtonBase : UIElement, IMouseInteractHandler, IMouseMoveHandler
{
    private uint _pressState, _enabled;
    private bool _isPressed;

    public ButtonBase(IElementContainer parent, string themePrefix) : base(parent, themePrefix)
    {
        _enabled = Booleans.TrueIntUnsigned;
        _pressState = (uint)ButtonTriState.None;
    }

    public override void OnSizeChanged() => Update();

    void IMouseMoveHandler.OnMouseMove(in MouseEventArgs args)
    {
        uint pressState;
        if (Enabled && args.IsInSpecificSize(Size))
            pressState = _isPressed ? (uint)ButtonTriState.Pressed : (uint)ButtonTriState.Hovered;
        else
            pressState = (uint)ButtonTriState.None;

        if (Atomics.Exchange(ref _pressState, pressState) != pressState)
            Update();
    }

    void IMouseInteractHandler.OnMouseDown(ref HandleableMouseEventArgs args)
    {
        if (!Enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;
        args.Handle();
        _isPressed = true;
        if (Atomics.Exchange(ref _pressState, (uint)ButtonTriState.Pressed) != (uint)ButtonTriState.Pressed)
            Update();
    }

    void IMouseInteractHandler.OnMouseUp(in MouseEventArgs args)
    {
        if (!Enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;

        _isPressed = false;
        if (PressState != ButtonTriState.Pressed)
            return;

        if (Atomics.Exchange(ref _pressState, (uint)ButtonTriState.Hovered) != (uint)ButtonTriState.Hovered)
            Update();
        OnClick(in args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnClick(in MouseEventArgs args) => Click?.Invoke(this, args);
}
