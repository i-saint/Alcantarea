// created by i-saint
// distributed under Creative Commons Attribution (CC BY) license.
// https://github.com/i-saint/DynamicPatcher

#include "dpInternal.h"
#include "alcFoundation.h"

dpAPI void dpExecExclusive(const std::function<void ()> &f);

typedef HMODULE (WINAPI *LoadLibraryAT)(LPCSTR lpFileName);
typedef HMODULE (WINAPI *LoadLibraryWT)(LPCWSTR lpFileName);
typedef HMODULE (WINAPI *LoadLibraryExAT)(LPCSTR lpFileName, HANDLE hFile, DWORD dwFlags);
typedef HMODULE (WINAPI *LoadLibraryExWT)(LPCWSTR lpFileName, HANDLE hFile, DWORD dwFlags);
static LoadLibraryAT OrigLoadLibraryA;
static LoadLibraryWT OrigLoadLibraryW;
static LoadLibraryExAT OrigLoadLibraryExA;
static LoadLibraryExWT OrigLoadLibraryExW;

void alcLoadSymbols(HMODULE mod)
{
    if(mod) {
        char path[MAX_PATH] = {0};
        ::GetModuleFileNameA(mod, path, sizeof(path));
        if(::SymLoadModuleEx(::GetCurrentProcess(), nullptr, path, nullptr, (DWORD64)mod, 0, nullptr, 0)!=0) {
            dpPrintInfo("SymLoadModuleEx() succeeded. %s\n", path);
        }

        std::string mappath = std::regex_replace(std::string(path), std::regex("\\.[^.]+$"), std::string(".map"));
        dpGetCurrentContext()->getLoader()->loadMapFile(mappath.c_str(), mod);
    }
}

HMODULE WINAPI alcLoadLibraryA(LPCSTR lpFileName)
{
    HMODULE mod = OrigLoadLibraryA(lpFileName);
    alcLoadSymbols(mod);
    return mod;
}

HMODULE WINAPI alcLoadLibraryW(LPCWSTR lpFileName)
{
    HMODULE mod = OrigLoadLibraryW(lpFileName);
    alcLoadSymbols(mod);
    return mod;
}

HMODULE WINAPI alcLoadLibraryExA(LPCSTR lpFileName, HANDLE hFile, DWORD dwFlags)
{
    HMODULE mod = OrigLoadLibraryExA(lpFileName, hFile, dwFlags);
    alcLoadSymbols(mod);
    return mod;
}

HMODULE WINAPI alcLoadLibraryExW(LPCWSTR lpFileName, HANDLE hFile, DWORD dwFlags)
{
    HMODULE mod = OrigLoadLibraryExW(lpFileName, hFile, dwFlags);
    alcLoadSymbols(mod);
    return mod;
}


void alcSetHooks()
{
    if(HMODULE mod = ::LoadLibraryA("kernel32.dll")) {
        BYTE *trampoline_space = (BYTE*)dpAllocateForward(16*4, mod);
        OrigLoadLibraryA = (LoadLibraryAT)alcOverrideDLLExport(mod, "LoadLibraryA", &alcLoadLibraryA, trampoline_space+(16*0));
        OrigLoadLibraryW = (LoadLibraryWT)alcOverrideDLLExport(mod, "LoadLibraryW", &alcLoadLibraryW, trampoline_space+(16*1));
        OrigLoadLibraryExA = (LoadLibraryExAT)alcOverrideDLLExport(mod, "LoadLibraryExA", &alcLoadLibraryExA, trampoline_space+(16*2));
        OrigLoadLibraryExW = (LoadLibraryExWT)alcOverrideDLLExport(mod, "LoadLibraryExW", &alcLoadLibraryExW, trampoline_space+(16*3));
    }
    dpEachModules([](HMODULE mod){
        alcEnumerateDLLImport(mod, "kernel32.dll", [&](const char *name, void *&func){
            if     (strcmp(name, "LoadLibraryA")  ==0) { alcForceWrite<void*>(func, (void*)&alcLoadLibraryA); }
            else if(strcmp(name, "LoadLibraryW")  ==0) { alcForceWrite<void*>(func, (void*)&alcLoadLibraryW); }
            else if(strcmp(name, "LoadLibraryExA")==0) { alcForceWrite<void*>(func, (void*)&alcLoadLibraryExA); }
            else if(strcmp(name, "LoadLibraryExW")==0) { alcForceWrite<void*>(func, (void*)&alcLoadLibraryExW); }
        });
    });
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    if(fdwReason==DLL_PROCESS_ATTACH) {
        dpExecExclusive([](){
            dpConfig conf;
            conf.sys_flags |= dpE_SysRunCommunicator | dpE_SysCommunicatorAutoFlush;
            conf.sys_flags &= ~dpE_SysPatchExports;

            alcRegistryData reg;
            if(alcReadRegistry(alcRegRoot, alcRegSub, alcRegConfig, &reg, sizeof(reg))) {
                conf.communicator_port = reg.TCPPort;
                if(reg.Flags&alcE_SysHookLoadLibrary) {
                    alcSetHooks();
                }
            }

            dpInitialize(conf);
        });
    }
    else if(fdwReason==DLL_PROCESS_DETACH) {
        dpFinalize();
    }
    return TRUE;
}
