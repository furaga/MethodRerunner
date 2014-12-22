// Guids.cs
// MUST match guids.h
using System;

namespace furaga.MethodRerunner
{
    static class GuidList
    {
        public const string guidMethodRerunnerPkgString = "3b45e7e4-aeaa-48b5-bda2-35fcd7034dff";
        public const string guidMethodRerunnerCmdSetString = "2ced066d-8b17-4ea2-9808-38db45883c5e";

        public static readonly Guid guidMethodRerunnerCmdSet = new Guid(guidMethodRerunnerCmdSetString);
    };
}