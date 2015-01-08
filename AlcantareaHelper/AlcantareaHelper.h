// AlcantareaHelper.h

#pragma once
#include <windows.h>
#include <cstdint>
#include "../alcFoundation.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Runtime::Serialization;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace System::Xml::Serialization;

namespace primitive {
namespace Alcantarea {

enum alcSymbolFlags
{
    alcE_Static       = 0x1,
    alcE_External     = 0x2,
    alcE_Weak         = 0x4,
    alcE_LinkageMask  = alcE_Static | alcE_External | alcE_Weak,

    alcE_Data         = 0x8,
    alcE_Code         = 0x10,
    alcE_DataTypeMask = alcE_Data | alcE_Code,

    alcE_Read    = 0x020,
    alcE_Write   = 0x040,
    alcE_Execute = 0x080,
    alcE_AccessMask = alcE_Read | alcE_Write | alcE_Execute,
    alcE_Shared  = 0x100,

    alcE_Exclude = 0x200,
};

enum alcFilterFlags
{
    alcE_IncludeUpdate     = 0x1,
    alcE_IncludeLinkToLocal= 0x2,
    alcE_IncludeOnLoad     = 0x4,
    alcE_IncludeOnUnload   = 0x8,
};

[Serializable]
public ref class alcGlobalSymFilterColumn
{
public:
    property String ^Pattern;
    property int Flags;

#define ImplFlagProperty(V) \
    bool get() { return (Flags&V)!=0; }\
    void set(bool v) {\
    if(v) { Flags |= V; }\
        else  { Flags &= ~V; }\
    }

    [XmlIgnore] property bool Function  { ImplFlagProperty(alcE_Code) }
    [XmlIgnore] property bool Variable  { ImplFlagProperty(alcE_Data) }
    [XmlIgnore] property bool Static    { ImplFlagProperty(alcE_Static) }
    [XmlIgnore] property bool External  { ImplFlagProperty(alcE_External) }
    [XmlIgnore] property bool Weak      { ImplFlagProperty(alcE_Weak) }
    [XmlIgnore] property bool Read      { ImplFlagProperty(alcE_Read) }
    [XmlIgnore] property bool Write     { ImplFlagProperty(alcE_Write) }
    [XmlIgnore] property bool Execute   { ImplFlagProperty(alcE_Execute) }
    [XmlIgnore] property bool Shared    { ImplFlagProperty(alcE_Shared) }

#undef ImplFlagProperty

    [XmlIgnore]
    property String^ Inclusion {
        String^ get()
        {
            return (Flags&alcE_Exclude)==0 ?
                gcnew String("Include") : gcnew String("Exclude");
        }
        void set(String ^v) {
            if     (v=="Include") { Flags &= ~alcE_Exclude; }
            else if(v=="Exclude") { Flags |= alcE_Exclude; }
        }
    }

    alcGlobalSymFilterColumn() {}
    alcGlobalSymFilterColumn(String ^p, int f) { Flags=f; Pattern=p; }
};

[Serializable]
public ref class alcSymbolFilterColumn
{
public:
    property int FilterFlags;
    property int SymbolFlags;
    property String ^NameMangled;

    [XmlIgnore] property String ^Name;
    [XmlIgnore] property String ^NameWithSignature;

    [XmlIgnore] property String ^AttrDataType;
    [XmlIgnore] property String ^AttrLinkType;
    [XmlIgnore] property String ^AttrAccessType;

#define ImplFlagProperty(V) \
    bool get() { return (FilterFlags&V)!=0; }\
    void set(bool v) {\
    if(v) { FilterFlags |= V; }\
        else  { FilterFlags &= ~V; }\
    }

    [XmlIgnore] property bool FlagUpdate     { ImplFlagProperty(alcE_IncludeUpdate) }
    [XmlIgnore] property bool FlagLinkToLocal{ ImplFlagProperty(alcE_IncludeLinkToLocal) }
    [XmlIgnore] property bool FlagOnLoad     { ImplFlagProperty(alcE_IncludeOnLoad) }
    [XmlIgnore] property bool FlagOnUnload   { ImplFlagProperty(alcE_IncludeOnUnload) }

    [XmlIgnore]
    property String^ Handler {
        String^ get()
        {
            if(FlagOnLoad)   { return "OnLoad"; }
            if(FlagOnUnload) { return "OnUnload"; }
            return "";
        }
        void set(String ^v) {
            if     (v=="OnLoad")   { FlagOnLoad=true;  FlagOnUnload=false; }
            else if(v=="OnUnload") { FlagOnLoad=false; FlagOnUnload=true;  }
            else                   { FlagOnLoad=false; FlagOnUnload=false; }
        }
    }

#undef ImplFlagProperty

    void SetupDemangledNames();
    bool IsFunction() { return (SymbolFlags&alcE_Code)!=0; }
};




[Serializable]
public ref class alcGlobalConfig
{
public:
    property List<alcGlobalSymFilterColumn^>^ SymIgnore;
    [XmlIgnore] property int TCPPort;

    alcGlobalConfig();
    void SetupDefaultValues();
};

} // namespace Alcantarea
} // namespace primitive


using namespace primitive::Alcantarea;

struct alcSymbolInfoN
{
    std::string Name;
    std::string NameShort;
    std::string NameMangled;
    int Flags;

    alcSymbolInfoN(const std::string &n="", const std::string &sn="",const std::string &mn="", int f=0)
        : Name(n), NameShort(sn), NameMangled(mn), Flags(f)
    {
    }
};

struct alcGlobalSymFilterColumnN
{
    std::string PatternStr;
    std::regex Pattern;
    std::wregex PatternW;
    int Flags;

    alcGlobalSymFilterColumnN(const std::string &p="", int f=0)
        : PatternStr(p), Pattern(p), Flags(f)
    {
        if(!p.empty()) {
            size_t len = _mbstrlen(p.c_str());
            wchar_t *wcs = (wchar_t*)_alloca(sizeof(wchar_t)*(len+1));
            mbstowcs(wcs, p.c_str(), len);
            wcs[len] = L'\0';
            PatternW = std::wregex(wcs);
        }
    }
};

class alcGlobalFilterN
{
private:
    std::vector<alcGlobalSymFilterColumnN*> Patterns;

public:
    alcGlobalFilterN();
    ~alcGlobalFilterN();
    void clear();
    void addPattern(alcGlobalSymFilterColumnN *pattern);
    void setPatterns(List<alcGlobalSymFilterColumn^>^ patterns);
    void toManaged(List<alcGlobalSymFilterColumn^>^ o_patterns);
    bool match(const alcSymbolInfoN &sym);
    bool match(alcSymbolFilterColumn^ col);
};


struct alcGlobalConfigN
{
    std::string AddinDir;
    alcDate ModuleMTime;
    alcGlobalFilterN SymIgnore;
    alcRegistryData RegData;
};

public ref class AlcantareaHelper
{
public:
    static String^ GetLastError();

    static DWORD ExecuteSuspended(String^ path_to_exe, String^ workdir, String^ addin_dir, String^ environment, String^ _platform);
    static bool Resume(DWORD pid);
    static bool InjectCoreDLL(DWORD pid, String^ addin_dir);

    static bool SendCommand( String^ _command );
    static List<alcSymbolFilterColumn^>^ GetDefaultSymbolFilter(String^ path_to_objfile, bool enable_filter);
    static List<alcSymbolFilterColumn^>^ ApplyIgnorePattern(List<alcSymbolFilterColumn^>^ list);
    static bool RequestSetSymbolFilter(String^ path_to_objfile, List<alcSymbolFilterColumn^>^ out_symdata);

    static alcGlobalConfigN& GetGlobalConfig();
    static void Startup(String ^addindir);
    static void SaveConfig();


    typedef List<alcGlobalSymFilterColumn^> GlobalFilter;

    static property int TCPPort {
        int get() { return GetGlobalConfig().RegData.TCPPort; }
        void set(int v) { GetGlobalConfig().RegData.TCPPort=v; }
    };

    static property bool HookLoadLibrary {
        bool get() { return (GetGlobalConfig().RegData.Flags&alcE_SysHookLoadLibrary)!=0; }
        void set(bool v) {
            if(v) {
                GetGlobalConfig().RegData.Flags |= alcE_SysHookLoadLibrary;
            }
            else {
                GetGlobalConfig().RegData.Flags &= ~alcE_SysHookLoadLibrary;
            }
        }
    };

    static property GlobalFilter^ SymGlobalIgnore
    {
        GlobalFilter^ get() {
            GlobalFilter^ r = gcnew GlobalFilter();
            GetGlobalConfig().SymIgnore.toManaged(r);
            return r;
        }
        void set(GlobalFilter^ v)
        {
            GetGlobalConfig().SymIgnore.setPatterns(v);
        }
    }
};
