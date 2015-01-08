// Guids.cs
// MUST match guids.h
using System;

namespace primitive.Alcantarea
{
    static class GuidList
    {
        public const string guidAlcantareaPkgString = "76241999-3c86-47ee-ad8a-b50a3f8c7c4c";
        public const string guidAlcantareaCmdSetString = "76241999-3c86-47ee-ad8a-b50a3f8c7c4d";

        public static readonly Guid guidAlcantareaCmdSet = new Guid(guidAlcantareaCmdSetString);
    };
}