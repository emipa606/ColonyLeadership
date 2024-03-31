using System;
using System.Reflection;

namespace Nandonalt_ColonyLeadership.Detour;

[AttributeUsage(AttributeTargets.Method)]
internal class DetourAttribute(Type source) : Attribute
{
    public BindingFlags bindingFlags;
    public Type source = source;
}