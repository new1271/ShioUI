using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

namespace ShioUI.Utils;

partial class BoundsHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetX(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.X;
            else
                return reference.GetValueUnsafe().X;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.X;
            else
                return reference.GetValueUnsafe().X;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetY(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Y;
            else
                return reference.GetValueUnsafe().Y;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Y;
            else
                return reference.GetValueUnsafe().Y;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetWidth(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Width;
            else
                return reference.GetValueUnsafe().Width;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Width;
            else
                return reference.GetValueUnsafe().Width;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetHeight(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Height;
            else
                return reference.GetValueUnsafe().Height;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Height;
            else
                return reference.GetValueUnsafe().Height;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point FastGetLocation(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(ulong);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Location;
            else
                return reference.GetValueUnsafe().Location;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Location;
            else
                return reference.GetValueUnsafe().Location;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size FastGetSize(State<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(ulong);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Size;
            else
                return reference.GetValueUnsafe().Size;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Size;
            else
                return reference.GetValueUnsafe().Size;
        }
#pragma warning restore CS0162
    }
}
