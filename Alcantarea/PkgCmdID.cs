// PkgCmdID.cs
// MUST match PkgCmdID.h

namespace primitive.Alcantarea
{
    static class PkgCmdIDList
    {
        public const uint alcStartDebugging         = 0x0200;
        public const uint alcApplyChange            = 0x0201;
        public const uint alcSymbolFilter           = 0x0202;
        public const uint alcReloadSymbols          = 0x0203;
        public const uint alcLoadObjFiles           = 0x0204;
        public const uint alcReloadObjFiles         = 0x0205;
        public const uint alcOptions                = 0x0206;
        public const uint alcToggleSuspend          = 0x0207;
        public const uint alcAttachToDebugee        = 0x0208;
    };
}