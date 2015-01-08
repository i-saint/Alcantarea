#include "alcFoundation.h"
#pragma comment(lib, "advapi32.lib")
#include <sys/types.h>
#include <sys/stat.h>

alcDate alcGetCurrentDate()
{
    return std::time(nullptr);
}

alcDate alcDaysToDate(int date)
{
    return 60*60*24*date;
}

alcDate alcGetMTime(const char *path)
{
    struct _stat fs;
    if(_stat(path, &fs)!=-1) {
        return fs.st_mtime;
    }
    return 0;
}


size_t alcReadRegistry(HKEY hk, const char *pos, const char *name, void *out_buf, size_t bufsize)
{
    DWORD ret = 0;
    HKEY key;
    DWORD size = (DWORD)bufsize;
    if(::RegOpenKeyExA(hk, pos, 0, KEY_READ, &key)==ERROR_SUCCESS) {
        if(::RegQueryValueExA(key, name, nullptr, nullptr, (LPBYTE)out_buf, &size )==ERROR_SUCCESS) {
            ret = size;
        }
        ::RegCloseKey( key );
    }
    return ret;
}

bool alcWriteRegistry(HKEY hk, const char *pos, const char *name, const void *buf, size_t bufsize)
{
    bool ret = false;
    HKEY key;
    if( ::RegCreateKeyExA(hk, pos, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, nullptr, &key, nullptr)==ERROR_SUCCESS) {
        if(::RegSetValueExA(key, name, 0, 0, (LPBYTE)buf, bufsize)==ERROR_SUCCESS  ) {
            ret = true;
        }
        ::RegCloseKey( key );
    }
    return ret;
}


alcRegistryData::alcRegistryData()
    : Size(sizeof(*this))
    , Version(alcVersionMajor, alcVersionMinor, alcVersionPatch)
    , InitialDate(alcGetCurrentDate())
    , LastRemindedDate(0)
    , Flags(alcE_SysHookLoadLibrary)
    , TCPPort(dpDefaultCommunicatorPort)
{
}

#ifndef __cplusplus_cli

inline bool alcIsValidMemory(void *p)
{
    if(p==nullptr) { return false; }
    MEMORY_BASIC_INFORMATION meminfo;
    return ::VirtualQuery(p, &meminfo, sizeof(meminfo))!=0 && meminfo.State!=MEM_FREE;
}


// dll が export している関数 (への RVA) を書き換える
// それにより、GetProcAddress() が返す関数をすり替える
// 元の関数へのポインタを返す
void* alcOverrideDLLExport(HMODULE module, const char *funcname, void *hook_, void *trampoline_space)
{
    if(!alcIsValidMemory(module)) { return nullptr; }

    HANDLE proc = ::GetCurrentProcess();

    size_t ImageBase = (size_t)module;
    PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)ImageBase;
    if(pDosHeader->e_magic!=IMAGE_DOS_SIGNATURE) { return nullptr; }

    PIMAGE_NT_HEADERS pNTHeader = (PIMAGE_NT_HEADERS)(ImageBase + pDosHeader->e_lfanew);
    DWORD RVAExports = pNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
    if(RVAExports==0) { return nullptr; }

    IMAGE_EXPORT_DIRECTORY *pExportDirectory = (IMAGE_EXPORT_DIRECTORY *)(ImageBase + RVAExports);
    DWORD *RVANames = (DWORD*)(ImageBase+pExportDirectory->AddressOfNames);
    WORD *RVANameOrdinals = (WORD*)(ImageBase+pExportDirectory->AddressOfNameOrdinals);
    DWORD *RVAFunctions = (DWORD*)(ImageBase+pExportDirectory->AddressOfFunctions);
    for(DWORD i=0; i<pExportDirectory->NumberOfFunctions; ++i) {
        char *pName = (char*)(ImageBase+RVANames[i]);
        if(strcmp(pName, funcname)==0) {
            BYTE *hook = (BYTE*)hook_;
            BYTE *target = (BYTE*)(ImageBase+RVAFunctions[RVANameOrdinals[i]]);
            if(trampoline_space) {
                BYTE *trampoline = (BYTE*)trampoline_space;
                dpAddJumpInstruction(trampoline, hook);
                ::FlushInstructionCache(proc, trampoline, 32);
                alcForceWrite<DWORD>(RVAFunctions[RVANameOrdinals[i]], (DWORD)(trampoline - ImageBase));
            }
            else {
                alcForceWrite<DWORD>(RVAFunctions[RVANameOrdinals[i]], (DWORD)(hook - ImageBase));
            }
            return target;
        }
    }
    return nullptr;
}


void alcEnumerateDLLImport(HMODULE module, const char *dllname, const std::function<void (const char*, void *&)> &f)
{
    if(module==nullptr) { return; }

    size_t ImageBase = (size_t)module;
    PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)ImageBase;
    if(pDosHeader->e_magic!=IMAGE_DOS_SIGNATURE) { return; }

    PIMAGE_NT_HEADERS pNTHeader = (PIMAGE_NT_HEADERS)(ImageBase + pDosHeader->e_lfanew);
    DWORD RVAImports = pNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
    if(RVAImports==0) { return; }

    IMAGE_IMPORT_DESCRIPTOR *pImportDesc = (IMAGE_IMPORT_DESCRIPTOR*)(ImageBase + RVAImports);
    while(pImportDesc->Name!=0) {
        const char *pDLLName = (const char*)(ImageBase+pImportDesc->Name);
        if(dllname==nullptr || _stricmp(pDLLName, dllname)==0) {
            IMAGE_THUNK_DATA* pThunkOrig = (IMAGE_THUNK_DATA*)(ImageBase + pImportDesc->OriginalFirstThunk);
            IMAGE_THUNK_DATA* pThunk = (IMAGE_THUNK_DATA*)(ImageBase + pImportDesc->FirstThunk);
            while(pThunkOrig->u1.AddressOfData!=0) {
                if((pThunkOrig->u1.Ordinal & 0x80000000) > 0) {
                    DWORD Ordinal = pThunkOrig->u1.Ordinal & 0xffff;
                    // nameless function
                }
                else {
                    IMAGE_IMPORT_BY_NAME* pIBN = (IMAGE_IMPORT_BY_NAME*)(ImageBase + pThunkOrig->u1.AddressOfData);
                    f((char*)pIBN->Name, *(void**)pThunk);
                }
                ++pThunkOrig;
                ++pThunk;
            }
        }
        ++pImportDesc;
    }
}

#endif // __cplusplus_cli
