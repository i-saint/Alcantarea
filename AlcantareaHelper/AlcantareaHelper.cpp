#include "stdafx.h"
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <ws2tcpip.h>
#include <winsock2.h>
#include <dbghelp.h>
#include <tlhelp32.h>
#include <cstdio>
#include <vector>
#include <string>
#include <algorithm>
#include <regex>
#include <vcclr.h>
#include "AlcantareaHelper.h"
#include "../dpInternal.h"

#pragma comment(lib, "dbghelp.lib")
#pragma comment(lib, "psapi.lib")
#pragma comment (lib, "Ws2_32.lib")


#define InjectorExecutable32 "AlcantareaInjector32.exe"
#define InjectorExecutable64 "AlcantareaInjector64.exe"
#define CoreDLL32 "AlcantareaCore32.dll"
#define CoreDLL64 "AlcantareaCore64.dll"

String^ AlcantareaHelper::GetLastError()
{
    return gcnew String(dpGetLastError().c_str());
}

DWORD AlcantareaHelper::ExecuteSuspended(
    String^ _path_to_exe, String^ _workdir, String^ _addin_dir, String^ _environment, String^ _platform )
{
    IntPtr path_to_exe = Marshal::StringToHGlobalAnsi(_path_to_exe);
    IntPtr workdir = Marshal::StringToHGlobalAnsi(_workdir);
    IntPtr addin_dir = Marshal::StringToHGlobalAnsi(_addin_dir);
    IntPtr environment = Marshal::StringToHGlobalAnsi(_environment);

    // todo
    //std::string envstr;
    //{
    //    char* s = (char*)environment.ToPointer();
    //    alcEachWords(s, strlen(s)+1, [&](const char *l){
    //        envstr += l;
    //        envstr += '\0';
    //    });
    //}
    //envstr.push_back(0);
    //envstr.push_back(0);

    char injector[MAX_PATH];
    char coredll[MAX_PATH];
    char command[4096];
    if(String::Compare(_platform, "Win32", true)==0) {
        sprintf(injector, "%s\\%s", (const char*)addin_dir.ToPointer(), InjectorExecutable32);
        sprintf(coredll, "%s\\%s", (const char*)addin_dir.ToPointer(), CoreDLL32);
    }
    else {
        sprintf(injector, "%s\\%s", (const char*)addin_dir.ToPointer(), InjectorExecutable64);
        sprintf(coredll, "%s\\%s", (const char*)addin_dir.ToPointer(), CoreDLL64);
    }
    sprintf(command, "\"%s\" \"%s\" \"%s\" /suspended", injector, (const char*)path_to_exe.ToPointer(), coredll);

    STARTUPINFOA si;
    PROCESS_INFORMATION pi;
    ::ZeroMemory(&si, sizeof(si));
    ::ZeroMemory(&pi, sizeof(pi));
    si.cb = sizeof(si);
    BOOL ret = ::CreateProcessA(nullptr, command, nullptr, nullptr, FALSE,
        NORMAL_PRIORITY_CLASS, nullptr, (const char*)workdir.ToPointer(), &si, &pi);
    if(ret) {
        DWORD ec;
        ::WaitForSingleObject(pi.hProcess, INFINITE);
        ::GetExitCodeProcess(pi.hProcess, &ec);
        ::CloseHandle(pi.hThread);
        ::CloseHandle(pi.hProcess);
        return ec;
    }
    return 0;
}

bool AlcantareaHelper::Resume( DWORD pid )
{
    bool r = false;
    alcEnumerateThreads(pid, [&](DWORD thread_id){
        if(HANDLE hthread = ::OpenThread(THREAD_ALL_ACCESS, FALSE, thread_id)) {
            DWORD ret = ::ResumeThread(hthread);
            if(ret!=DWORD(-1)) { r=true; }
            ::CloseHandle(hthread);
        }
    });
    return r;
}

bool AlcantareaHelper::InjectCoreDLL(DWORD pid, String^ _addin_dir)
{
    IntPtr addin_dir = Marshal::StringToHGlobalAnsi(_addin_dir);
    char injector[MAX_PATH];
    char coredll[MAX_PATH];
    char command[4096];

    HANDLE process = ::OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    BOOL is32bitprocess;
    if(::IsWow64Process(process, &is32bitprocess)) {
        if(is32bitprocess) {
            sprintf(injector, "%s\\%s", (const char*)addin_dir.ToPointer(), InjectorExecutable32);
            sprintf(coredll, "%s\\%s", (const char*)addin_dir.ToPointer(), CoreDLL32);
        }
        else {
            sprintf(injector, "%s\\%s", (const char*)addin_dir.ToPointer(), InjectorExecutable64);
            sprintf(coredll, "%s\\%s", (const char*)addin_dir.ToPointer(), CoreDLL64);
        }
    }
    else {
        return false;
    }
    ::CloseHandle(process);
    sprintf(command, "\"%s\" /pid:%u \"%s\"", injector, pid, coredll);

    STARTUPINFOA si;
    PROCESS_INFORMATION pi;
    ::ZeroMemory(&si, sizeof(si));
    ::ZeroMemory(&pi, sizeof(pi));
    si.cb = sizeof(si);
    BOOL ret = ::CreateProcessA(nullptr, command, nullptr, nullptr, FALSE, NORMAL_PRIORITY_CLASS, nullptr, nullptr, &si, &pi);
    if(ret) {
        DWORD ec;
        ::WaitForSingleObject(pi.hProcess, INFINITE);
        ::GetExitCodeProcess(pi.hProcess, &ec);
        ::CloseHandle(pi.hThread);
        ::CloseHandle(pi.hProcess);
        return ec==pid;
    }
    return false;
}



static bool alcSendCommandImpl(const char *data, size_t datasize, std::string *out_res=nullptr)
{
    dpInitializeNetwork();

    dpProtocolSocket socket("127.0.0.1", AlcantareaHelper::TCPPort);
    if(socket.write(data, datasize)) {
        if(out_res) {
            socket.read(*out_res);
        }
        return true;
    }
    return false;
}

bool AlcantareaHelper::SendCommand( String^ _command)
{
    IntPtr command_ptr = Marshal::StringToHGlobalAnsi(_command);
    const char *command = (const char*)command_ptr.ToPointer();
    int command_len = _command->Length;
    return alcSendCommandImpl(command, command_len);
}


static inline const char* dpGetSymbolName(PSTR pStringTable, PIMAGE_SYMBOL pSym)
{
    return pSym->N.Name.Short!=0 ? (const char*)&pSym->N.ShortName : (const char*)(pStringTable + pSym->N.Name.Long);
}

struct TIMAGE_SYMBOL
{
    const char *Name;
    DWORD   Value;
    LONG    SectionNumber;
    WORD    Type;
    BYTE    StorageClass;
    BYTE    NumberOfAuxSymbols;

    TIMAGE_SYMBOL(void *data, PSTR pStringTable, bool is_bigobj)
    {
        if(is_bigobj) {
            PIMAGE_SYMBOL_EX pSym = (PIMAGE_SYMBOL_EX)data;
            Name = dpGetSymbolName(pStringTable, (PIMAGE_SYMBOL)pSym);
            Value = pSym->Value;
            SectionNumber = pSym->SectionNumber;
            Type = pSym->Type;
            StorageClass = pSym->StorageClass;
            NumberOfAuxSymbols = pSym->NumberOfAuxSymbols;
        }
        else {
            PIMAGE_SYMBOL pSym = (PIMAGE_SYMBOL)data;
            Name = dpGetSymbolName(pStringTable, pSym);
            Value = pSym->Value;
            SectionNumber = pSym->SectionNumber;
            Type = pSym->Type;
            StorageClass = pSym->StorageClass;
            NumberOfAuxSymbols = pSym->NumberOfAuxSymbols;
        }
    }
};

template<class F>
bool dpEnumerateSymbolsObj(void *data, const F &f)
{
    size_t ImageBase = (size_t)(data);
    WORD Arch = IMAGE_FILE_MACHINE_UNKNOWN;
    bool bigobj = false;
    size_t sym_size = 0;

    PIMAGE_SECTION_HEADER pSectionHeader = nullptr;
    char *pSymbolTable = nullptr;
    DWORD SectionCount = 0;
    DWORD SymbolCount = 0;
    PSTR StringTable = nullptr;
    {
        PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)ImageBase;
        ANON_OBJECT_HEADER_BIGOBJ *pBigHeader = (ANON_OBJECT_HEADER_BIGOBJ*)ImageBase;
        if( (pDosHeader->e_magic==IMAGE_FILE_MACHINE_I386 || pDosHeader->e_magic==IMAGE_FILE_MACHINE_AMD64) && pDosHeader->e_sp==0 )
        {
            Arch = pDosHeader->e_magic;
            PIMAGE_FILE_HEADER pImageHeader = (PIMAGE_FILE_HEADER)ImageBase;
            pSectionHeader = (PIMAGE_SECTION_HEADER)(ImageBase + sizeof(IMAGE_FILE_HEADER) + pImageHeader->SizeOfOptionalHeader);
            pSymbolTable = (char*)(ImageBase + pImageHeader->PointerToSymbolTable);
            SectionCount = pImageHeader->NumberOfSections;
            SymbolCount = pImageHeader->NumberOfSymbols;
        }
        else if(pBigHeader->Sig1==IMAGE_FILE_MACHINE_UNKNOWN && pBigHeader->Sig2==0xFFFF && pBigHeader->Version>=2 &&
            (pBigHeader->Machine==IMAGE_FILE_MACHINE_I386 || pBigHeader->Machine==IMAGE_FILE_MACHINE_AMD64) )
        {
            bigobj = true;
            Arch = pBigHeader->Machine;
            pSectionHeader = (PIMAGE_SECTION_HEADER)(ImageBase + sizeof(ANON_OBJECT_HEADER_BIGOBJ) + pBigHeader->SizeOfData);
            pSymbolTable = (char*)(ImageBase + pBigHeader->PointerToSymbolTable);
            SectionCount = pBigHeader->NumberOfSections;
            SymbolCount = pBigHeader->NumberOfSymbols;
        }
        else {
            return false;
        }
        sym_size = bigobj ? sizeof(IMAGE_SYMBOL_EX) : sizeof(IMAGE_SYMBOL);
        StringTable = (PSTR)(pSymbolTable + sym_size*SymbolCount);
    }

    std::vector<int> SectionFlags;
    SectionFlags.resize(SectionCount, 0);

    for( size_t i=0; i < SymbolCount; ++i ) {
        TIMAGE_SYMBOL sym(pSymbolTable + sym_size*i, StringTable, bigobj);
        if(sym.SectionNumber>0) {
            PIMAGE_SECTION_HEADER sect = pSectionHeader + (sym.SectionNumber-1);

            void *data = (void*)(ImageBase + (int)sect->PointerToRawData + sym.Value);
            if(sym.Name[0]!='.' && sym.Name[0]!='$') {
                int flags = SectionFlags[sym.SectionNumber-1];
                flags |= sym.Type==0x20 ? alcE_Code : alcE_Data;
                flags |= sym.StorageClass==IMAGE_SYM_CLASS_STATIC ? alcE_Static : 0;
                flags |= sym.StorageClass==IMAGE_SYM_CLASS_EXTERNAL ? alcE_External: 0;
                flags |= sym.StorageClass==IMAGE_SYM_CLASS_WEAK_EXTERNAL ? alcE_Weak : 0;
                if((sect->Characteristics&IMAGE_SCN_MEM_READ))    { flags|=alcE_Read; }
                if((sect->Characteristics&IMAGE_SCN_MEM_WRITE))   { flags|=alcE_Write; }
                if((sect->Characteristics&IMAGE_SCN_MEM_EXECUTE)) { flags|=alcE_Execute; }
                if((sect->Characteristics&IMAGE_SCN_MEM_SHARED))  { flags|=alcE_Shared; }
                f(sym.Name, flags);
            }
            else if(sym.Name[0]=='.') {
                if((sect->Characteristics&IMAGE_SCN_LNK_COMDAT)!=0) {
                    if(bigobj) {
                        PIMAGE_AUX_SYMBOL_EX aux = (PIMAGE_AUX_SYMBOL_EX)(pSymbolTable + sym_size*(i+1));
                        if(aux->Section.Selection==IMAGE_COMDAT_SELECT_ANY) {
                            SectionFlags[sym.SectionNumber-1] |= alcE_Weak;
                        }
                    }
                    else {
                        PIMAGE_AUX_SYMBOL aux = (PIMAGE_AUX_SYMBOL)(pSymbolTable + sym_size*(i+1));
                        if(aux->Section.Selection==IMAGE_COMDAT_SELECT_ANY) {
                            SectionFlags[sym.SectionNumber-1] |= alcE_Weak;
                        }
                    }
                }
            }
        }
        i += sym.NumberOfAuxSymbols;
    }
    return true;
}

inline void alcSymbolAttributesToString(alcSymbolFilterColumn ^col)
{
    int flags = col->SymbolFlags;
#define AddSeparator if(pos>0) { pos+=sprintf(buf+pos, ", "); }
    {
        char buf[256] = {0};
        int pos = 0;
        if(flags&alcE_Data)    { AddSeparator; pos+=sprintf(buf+pos, "variable"); }
        if(flags&alcE_Code)    { AddSeparator; pos+=sprintf(buf+pos, "function"); }
        col->AttrDataType = gcnew String(buf);
    }
    {
        char buf[256] = {0};
        int pos = 0;
        if(flags&alcE_Weak)    { AddSeparator; pos+=sprintf(buf+pos, "weak"); }
        if(flags&alcE_Static)  { AddSeparator; pos+=sprintf(buf+pos, "local"); }
        if(flags&alcE_External){ AddSeparator; pos+=sprintf(buf+pos, "external"); }
        col->AttrLinkType = gcnew String(buf);
    }
    {
        char buf[256] = {0};
        int pos = 0;
        if(flags&alcE_Read)   { buf[pos++]='r'; }
        if(flags&alcE_Write)  { buf[pos++]='w'; }
        if(flags&alcE_Execute){ buf[pos++]='x'; }
        if(flags&alcE_Shared) { AddSeparator; pos+=sprintf(buf+pos, "shared"); }
        col->AttrAccessType = gcnew String(buf);
    }
#undef AddSeparator
}

bool ShouldBeLinkToLocal(alcSymbolInfoN *sym)
{
    if((sym->Flags & (alcE_Data|alcE_Read|alcE_Write)) == (alcE_Data|alcE_Read) &&
        !std::regex_search(sym->NameShort, std::regex("[$@`#]"))) { return true; }
    if(std::regex_search(sym->NameShort, std::regex("^__(real|xmm|mask)@"))) { return true; }
    if(std::regex_search(sym->NameShort, std::regex("^`string'"))) { return true; }
    if(std::regex_search(sym->NameShort, std::regex("<lambda"))) { return true; }
    return false;
}

List<alcSymbolFilterColumn^>^ AlcantareaHelper::GetDefaultSymbolFilter(String^ path_to_objfile__, bool enable_filter)
{
    List<alcSymbolFilterColumn^>^ ret = nullptr;

    IntPtr path_to_objfile_ = Marshal::StringToHGlobalAnsi(path_to_objfile__);
    const char *path_to_objfile = (const char*)path_to_objfile_.ToPointer();

    void *objdata = nullptr;
    size_t objsize = 0;
    if(dpMapFile(path_to_objfile, objdata, objsize, malloc)) {
        ret = gcnew List<alcSymbolFilterColumn^>();

        alcGlobalConfigN &conf = GetGlobalConfig();

        std::vector<alcSymbolInfoN*> symbols;
        bool succeeded = dpEnumerateSymbolsObj(objdata, [&](const char *symname, int flags){
            char name_demangled[MAX_SYM_NAME+1];
            char name_short[MAX_SYM_NAME+1];
            dpDemangleSignatured(symname, name_demangled, sizeof(name_demangled));
            dpDemangleNameOnly(symname, name_short, sizeof(name_short));

            alcSymbolInfoN *sym = new alcSymbolInfoN(std::string(name_demangled), std::string(name_short), std::string(symname), flags);
            if(!enable_filter || !conf.SymIgnore.match(*sym)) {
                symbols.push_back(sym);
            }
        });
        if(!succeeded) {
            throw gcnew Exception("Loading failed. Possibly this obj file was built with /LTCG (link time code generation) option.");
        }

        std::string tmp_str;
        for(size_t i=0; i<symbols.size(); ++i) {
            alcSymbolFilterColumn ^tmp = gcnew alcSymbolFilterColumn();
            tmp->SymbolFlags = symbols[i]->Flags;
            tmp->FlagLinkToLocal = ShouldBeLinkToLocal(symbols[i]);
            tmp->NameWithSignature = gcnew String(symbols[i]->Name.c_str());
            tmp->NameMangled = gcnew String(symbols[i]->NameMangled.c_str());
            tmp->Name = gcnew String(symbols[i]->NameShort.c_str());
            alcSymbolAttributesToString(tmp);
            tmp_str.clear();

            ret->Add(tmp);
        }
        std::for_each(symbols.begin(), symbols.end(), [](alcSymbolInfoN *s){ delete s; });
        free(objdata);
    }

    return ret;
}

List<alcSymbolFilterColumn^>^ AlcantareaHelper::ApplyIgnorePattern(List<alcSymbolFilterColumn^>^ list)
{
    alcGlobalConfigN &conf = GetGlobalConfig();
    List<alcSymbolFilterColumn^>^ ret = gcnew List<alcSymbolFilterColumn^>();
    for each(alcSymbolFilterColumn^ col in list) {
        if(!conf.SymIgnore.match(col)) {
            ret->Add(col);
        }
    }
    return ret;
}



bool AlcantareaHelper::RequestSetSymbolFilter( String^ path_to_objfile, List<alcSymbolFilterColumn^>^ symdata )
{
    String ^command = gcnew String("dpSetSymbolFilter\n");
    command += path_to_objfile + "\n";
    for(int i=0; i<symdata->Count; ++i) {
        alcSymbolFilterColumn ^column = symdata[i];
        if(column->FilterFlags!=0) {
            command += String::Format("{0} {1}\n", column->FilterFlags, column->NameMangled);
        }
    }
    command += "\n";
    return SendCommand(command);
}



alcGlobalConfigN& AlcantareaHelper::GetGlobalConfig()
{
    static alcGlobalConfigN s_conf;
    return s_conf;
}


void AlcantareaHelper::Startup(String ^addindir_)
{
    alcGlobalConfigN &conf = GetGlobalConfig();
    {
        IntPtr addindir = Marshal::StringToHGlobalAnsi(addindir_);
        conf.AddinDir = (const char*)addindir.ToPointer();
        conf.ModuleMTime = alcGetMTime((conf.AddinDir+"\\Alcantarea.dll").c_str());
    }
    {
        alcRegistryData &regdata = conf.RegData;
        bool needs_write = false;
        if(!alcReadRegistry(alcRegRoot, alcRegSub, alcRegConfig, &regdata, sizeof(alcRegistryData))) {
            needs_write = true;
        }
        else if(regdata.InitialDate==0) {
            regdata.InitialDate = alcGetCurrentDate();
            needs_write = true;
        }
        if(needs_write) {
            alcWriteRegistry(alcRegRoot, alcRegSub, alcRegConfig, &regdata, sizeof(alcRegistryData));
        }
    }
    {
        alcGlobalConfig ^mconf = nullptr;
        try {
            System::IO::StreamReader ^reader = gcnew System::IO::StreamReader(addindir_+"\\config.alc");
            try {
                System::Xml::Serialization::XmlSerializer ^serializer = gcnew System::Xml::Serialization::XmlSerializer(alcGlobalConfig::typeid);
                mconf = (alcGlobalConfig^)serializer->Deserialize(reader);
            }
            catch(Exception^) {}
            reader->Close();
        }
        catch(Exception^) {}

        if(mconf==nullptr) {
            mconf = gcnew alcGlobalConfig();
            mconf->SetupDefaultValues();
        }
        conf.SymIgnore.setPatterns(mconf->SymIgnore);
    }
}

void AlcantareaHelper::SaveConfig()
{
    try {
        alcGlobalConfigN &conf = GetGlobalConfig();
        alcGlobalConfig ^mconf = gcnew alcGlobalConfig();
        conf.SymIgnore.toManaged(mconf->SymIgnore);
        mconf->TCPPort = conf.RegData.TCPPort;
        {
            alcRegistryData &regdata = conf.RegData;
            alcWriteRegistry(alcRegRoot, alcRegSub, alcRegConfig, &regdata, sizeof(alcRegistryData));
        }

        String ^path = gcnew String((conf.AddinDir+"\\config.alc").c_str());
        System::IO::StreamWriter ^writer = gcnew System::IO::StreamWriter(path);
        try {
            System::Xml::Serialization::XmlSerializer ^serializer = gcnew System::Xml::Serialization::XmlSerializer(alcGlobalConfig::typeid);
            serializer->Serialize(writer, mconf);
        }
        catch(Exception^) {}
        writer->Close();
    }
    catch(Exception^) {}
}



alcGlobalFilterN::alcGlobalFilterN()
{
}

alcGlobalFilterN::~alcGlobalFilterN()
{
    clear();
}

void alcGlobalFilterN::clear()
{
    std::for_each(Patterns.begin(), Patterns.end(), [&](alcGlobalSymFilterColumnN *p){
        delete p;
    });
    Patterns.clear();
}

void alcGlobalFilterN::addPattern(alcGlobalSymFilterColumnN *pattern)
{
    Patterns.push_back(pattern);
}

void alcGlobalFilterN::setPatterns(List<alcGlobalSymFilterColumn^>^ patterns)
{
    clear();
    for(int i=0; i<patterns->Count; ++i) {
        alcGlobalSymFilterColumn^ pattern = (*patterns)[i];
        IntPtr strpr = Marshal::StringToHGlobalAnsi(pattern->Pattern);
        const char *str = (const char*)strpr.ToPointer();
        addPattern(new alcGlobalSymFilterColumnN(str ? str : "", pattern->Flags));
    }
}

void alcGlobalFilterN::toManaged(List<alcGlobalSymFilterColumn^>^ o_patterns)
{
    for(size_t i=0; i<Patterns.size(); ++i) {
        alcGlobalSymFilterColumnN &pattern = *Patterns[i];
        o_patterns->Add( gcnew alcGlobalSymFilterColumn(gcnew String(pattern.PatternStr.c_str()), pattern.Flags) );
    }
}

bool alcGlobalFilterN::match(const alcSymbolInfoN &sym)
{
    bool result = false;
    std::for_each(Patterns.begin(), Patterns.end(), [&](alcGlobalSymFilterColumnN *p){
        if(( result && (p->Flags & alcE_Exclude)==0) ||
            (!result && (p->Flags & alcE_Exclude)!=0))
        {
            return;
        }

        bool m = (sym.Flags & p->Flags)!=0;
        if(m && !p->PatternStr.empty()) {
            m = std::regex_search(sym.NameShort, p->Pattern);
        }

        if(p->Flags & alcE_Exclude) {
            result = result && !m;
        }
        else {
            result = result || m;
        }
    });
    return result;
}

bool alcGlobalFilterN::match(alcSymbolFilterColumn^ col)
{
    bool result = false;
    for(size_t i=0; i<Patterns.size(); ++i) {
        alcGlobalSymFilterColumnN *p = Patterns[i];
        if(( result && (p->Flags & alcE_Exclude)==0) ||
            (!result && (p->Flags & alcE_Exclude)!=0))
        {
            break;;
        }

        bool m = (col->SymbolFlags & p->Flags)!=0;
        if(m && !p->PatternStr.empty()) {
            pin_ptr<const wchar_t> name = PtrToStringChars(col->Name);
            m = std::regex_search((const wchar_t*)name, p->PatternW);
        }

        if(p->Flags & alcE_Exclude) {
            result = result && !m;
        }
        else {
            result = result || m;
        }
    }
    return result;
}


namespace primitive {
namespace Alcantarea {

void alcSymbolFilterColumn::SetupDemangledNames()
{
    IntPtr ptr = Marshal::StringToHGlobalAnsi(NameMangled);
    const char *mangled = (const char*)ptr.ToPointer();

    char name_demangled[MAX_SYM_NAME+1];
    char name_short[MAX_SYM_NAME+1];
    dpDemangleSignatured(mangled, name_demangled, sizeof(name_demangled));
    dpDemangleNameOnly(mangled, name_short, sizeof(name_short));

    NameWithSignature = gcnew String(name_demangled);
    Name = gcnew String(name_short);
    alcSymbolAttributesToString(this);
}

alcGlobalConfig::alcGlobalConfig()
{
    SymIgnore = gcnew List<alcGlobalSymFilterColumn^>();
    TCPPort = dpDefaultCommunicatorPort;
}

void alcGlobalConfig::SetupDefaultValues()
{
    SymIgnore->Add(gcnew alcGlobalSymFilterColumn("[$@`#]", alcE_DataTypeMask));
    SymIgnore->Add(gcnew alcGlobalSymFilterColumn("<lambda", alcE_DataTypeMask));
    SymIgnore->Add(gcnew alcGlobalSymFilterColumn("^std::", alcE_DataTypeMask));
    SymIgnore->Add(gcnew alcGlobalSymFilterColumn("^boost::", alcE_DataTypeMask));
}


} // namespace Alcantarea
} // namespace primitive
