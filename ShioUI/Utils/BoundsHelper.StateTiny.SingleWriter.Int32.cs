using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

namespace ShioUI.Utils;

partial class BoundsHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetX(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.X;
            else
                return reference.GetValueReferenceUnsafe().X;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.X;
            else
                return reference.GetValueReferenceUnsafe().X;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetY(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Y;
            else
                return reference.GetValueReferenceUnsafe().Y;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Y;
            else
                return reference.GetValueReferenceUnsafe().Y;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetWidth(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Width;
            else
                return reference.GetValueReferenceUnsafe().Width;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Width;
            else
                return reference.GetValueReferenceUnsafe().Width;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastGetHeight(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(int);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Height;
            else
                return reference.GetValueReferenceUnsafe().Height;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Height;
            else
                return reference.GetValueReferenceUnsafe().Height;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point FastGetLocation(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(ulong);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Location;
            else
                return reference.GetValueReferenceUnsafe().Location;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Location;
            else
                return reference.GetValueReferenceUnsafe().Location;
        }
#pragma warning restore CS0162
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size FastGetSize(ref readonly StateTiny.SingleWriter<Rectangle> reference)
    {
#pragma warning disable CS0162
        const int Size = sizeof(ulong);

        if (UnsafeHelper.PointerSizeConstant == UnsafeHelper.PointerSizeConstant_Indeterminate)
        {
            if (UnsafeHelper.PointerSize < Size)
                return reference.Value.Size;
            else
                return reference.GetValueReferenceUnsafe().Size;
        }
        else
        {
            if (UnsafeHelper.PointerSizeConstant < Size)
                return reference.Value.Size;
            else
                return reference.GetValueReferenceUnsafe().Size;
        }
#pragma warning restore CS0162
    }
}
