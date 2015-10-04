// Guids.cs
// MUST match guids.h
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccessLayer.vs2015
{

    internal static class GuidList
    {
        public const string GuidSimpleDataAccessLayerVs2015PkgString = "0072ab57-bce5-4934-8c39-bfbe682d1b88";
        public const string GuidSimpleDataAccessLayerVs2015EditorFactoryString = "b11176e3-a570-46ce-82c5-56ab677dd2f3";

        public static readonly Guid GuidSimpleDataAccessLayerVs2015EditorFactory =
            new Guid(GuidSimpleDataAccessLayerVs2015EditorFactoryString);
    };
}
