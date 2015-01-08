#include <string>
#include <vector>
#include <algorithm>
#include <windows.h>


bool InjectDLL(HANDLE hProcess, const char* dllname)
{
    SIZE_T bytesRet = 0;
    DWORD oldProtect = 0;
    LPVOID remote_addr = NULL;
    HANDLE hThread = NULL;
    size_t len = strlen(dllname) + 1;

    remote_addr = ::VirtualAllocEx(hProcess, 0, 1024, MEM_COMMIT|MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if(remote_addr==NULL) { return false; }
    ::VirtualProtectEx(hProcess, remote_addr, len, PAGE_EXECUTE_READWRITE, &oldProtect);
    ::WriteProcessMemory(hProcess, remote_addr, dllname, len, &bytesRet);
    ::VirtualProtectEx(hProcess, remote_addr, len, oldProtect, &oldProtect);

    hThread = ::CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)((void*)&LoadLibraryA), remote_addr, 0, NULL);
    ::WaitForSingleObject(hThread, 5000); 
    ::VirtualFreeEx(hProcess, remote_addr, 0, MEM_RELEASE);
    return true;
}

const char* GetMainModuleFilename()
{
    static char s_full_path[MAX_PATH+1];
    static char *s_filename = nullptr;
    if(!s_filename) {
        ::GetModuleFileNameA(nullptr, s_full_path, sizeof(s_full_path));
        for(int i=0; s_full_path[i]!='\0'; ++i) {
            if(s_full_path[i]=='\\') {
                s_filename = s_full_path+i+1;
            }
        }
    }
    return s_filename;
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE prev, LPWSTR cmd, int show)
{
    if(__argc<2) {
        printf("usage: %s [path-to-exe or pid:ProcessID] [path to dll(s)] (/suspended)\n", GetMainModuleFilename());
        return 0;
    }
    std::vector<std::string> dll_paths;
    std::wstring exe_path;
    DWORD exe_pid = 0;
    DWORD flags = NORMAL_PRIORITY_CLASS;

    if(wcsncmp(__wargv[1], L"/pid:", 5)==0) {
        swscanf(__wargv[1]+5, L"%u", &exe_pid);
    }
    else {
        exe_path = __wargv[1];
    }
    
    for(int i=2; i<__argc; ++i) {
        if(wcscmp(__wargv[i], L"/suspended")==0) {
            flags |= CREATE_SUSPENDED;
        }
        else {
            char mbs[MAX_PATH+1];
            size_t l = wcstombs(mbs, __wargv[i], sizeof(mbs));
            mbs[l] = '\0';
            dll_paths.push_back(mbs);
        }
    }

    if(!exe_path.empty()) {
        STARTUPINFOW si;
        PROCESS_INFORMATION pi;
        ::ZeroMemory(&si, sizeof(si));
        ::ZeroMemory(&pi, sizeof(pi));
        si.cb = sizeof(si);
        BOOL ret = ::CreateProcessW(nullptr, (wchar_t*)exe_path.c_str(), nullptr, nullptr, FALSE,
            flags, nullptr, nullptr, &si, &pi);
        if(ret) {
            std::for_each(dll_paths.begin(), dll_paths.end(), [&](const std::string &dll){
                InjectDLL(pi.hProcess, dll.c_str());
            });
            ::CloseHandle(pi.hThread);
            ::CloseHandle(pi.hProcess);
            return pi.dwProcessId;
        }
    }
    else if(exe_pid!=0) {
        HANDLE process = ::OpenProcess(PROCESS_ALL_ACCESS, FALSE, exe_pid);
        if(process!=nullptr) {
            std::for_each(dll_paths.begin(), dll_paths.end(), [&](const std::string &dll){
                InjectDLL(process, dll.c_str());
            });
            ::CloseHandle(process);
            return exe_pid;
        }
    }
    return 0;
}
