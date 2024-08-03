using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Nandonalt_ColonyLeadership.Detour;

// Detour code by Jecrell
public static class Detours
{
    private static readonly List<string> detoured = [];

    private static readonly List<string> destinations = [];

    public static unsafe void TryDetourFromTo(MethodInfo source, MethodInfo destination)
    {
        if (source == null)
        {
            Log.Error("Source MethodInfo is null: Detours");
            return;
        }

        if (destination == null)
        {
            Log.Error("Destination MethodInfo is null: Detours");
            return;
        }

        var item =
            $"{source.DeclaringType?.FullName}.{source.Name} @ 0x{source.MethodHandle.GetFunctionPointer().ToString($"X{IntPtr.Size * 2}")}";
        var item2 =
            $"{destination.DeclaringType?.FullName}.{destination.Name} @ 0x{destination.MethodHandle.GetFunctionPointer().ToString($"X{IntPtr.Size * 2}")}";
        detoured.Add(item);
        destinations.Add(item2);
        if (IntPtr.Size == 8)
        {
            var num = source.MethodHandle.GetFunctionPointer().ToInt64();
            var num2 = destination.MethodHandle.GetFunctionPointer().ToInt64();
            var ptr = (byte*)num;
            var ptr2 = (long*)(ptr + 2);
            *ptr = 72;
            if (ptr == null)
            {
                return;
            }

            ptr[1] = 184;
            *ptr2 = num2;
            ptr[10] = 255;
            ptr[11] = 224;
        }
        else
        {
            var num3 = source.MethodHandle.GetFunctionPointer().ToInt32();
            var num4 = destination.MethodHandle.GetFunctionPointer().ToInt32();
            var ptr3 = (byte*)num3;
            var ptr4 = (int*)(ptr3 + 1);
            var num5 = num4 - num3 - 5;
            *ptr3 = 233;
            *ptr4 = num5;
        }
    }
}