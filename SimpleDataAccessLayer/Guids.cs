// Guids.cs
// MUST match guids.h

using System;

namespace SimpleDataAccessLayer
{

    internal static class GuidList
    {
        public const string GuidSimpleDataAccessLayerPkgString = "0072ab57-bce5-4934-8c39-bfbe682d1b88";
        public const string GuidSimpleDataAccessLayerEditorFactoryString = "b11176e3-a570-46ce-82c5-56ab677dd2f3";

        public static readonly Guid GuidSimpleDataAccessLayerEditorFactory =
            new Guid(GuidSimpleDataAccessLayerEditorFactoryString);
    };
}
