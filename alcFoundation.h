#ifndef alcFoundation_h
#define alcFoundation_h

#include "dpInternal.h"

#define alcVersionMajor 0x01
#define alcVersionMinor 0x00
#define alcVersionPatch 0x00

#define alcRegRoot          HKEY_CURRENT_USER
#define alcRegSub           "SOFTWARE\\primitive\\Alcantarea"
#define alcRegConfig        "Config"
#define alcRegLicenseKey    "LicenseKey"
#define alcMaxTrialDays     30

typedef std::time_t alcDate;
static_assert(sizeof(alcDate)==8, "");


struct alcVersion
{
    uint8_t Major;
    uint8_t Minor;
    uint8_t Patch;
    uint8_t Padding;

    alcVersion(uint8_t ma=0, uint8_t mi=0, uint8_t pa=0)
        : Major(ma), Minor(mi), Patch(pa)
    {}
};

struct alcLicenseData
{
    uint64_t ID;
    alcDate  ExpireDate;
    uint64_t Dummy;
    uint64_t Sum;

    alcLicenseData();
};

enum {
    alcE_SysHookLoadLibrary = 0x0001,
};

struct alcRegistryData
{
    uint32_t    Size;
    alcVersion  Version;
    alcDate     InitialDate;
    alcDate     LastRemindedDate;
    uint32_t    Flags;
    uint16_t    TCPPort;

    alcRegistryData();
};

alcDate alcGetCurrentDate();
alcDate alcDaysToDate(int date);
alcDate alcGetMTime(const char *path);

// hk: HKEY_CURRENT_USER, etc
size_t alcReadRegistry(HKEY hk, const char *pos, const char *name, void *out_buf, size_t bufsize);
bool alcWriteRegistry(HKEY hk, const char *pos, const char *name, const void *out_buf, size_t bufsize);

void* alcOverrideDLLExport(HMODULE module, const char *funcname, void *replacement, void *trampoline_space=nullptr);
void alcEnumerateDLLImport(HMODULE module, const char *dllname, const std::function<void (const char*, void *&)> &f);


// F: [](DWORD thread_id)->void
template<class F>
inline void alcEnumerateThreads(DWORD pid, const F &f)
{
    HANDLE ss = ::CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
    if(ss!=INVALID_HANDLE_VALUE) {
        THREADENTRY32 te;
        te.dwSize = sizeof(te);
        if(::Thread32First(ss, &te)) {
            do {
                if(te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID)+sizeof(te.th32OwnerProcessID) &&
                    te.th32OwnerProcessID==pid)
                {
                    f(te.th32ThreadID);
                }
                te.dwSize = sizeof(te);
            } while(::Thread32Next(ss, &te));
        }
        ::CloseHandle(ss);
    }
}

template<class T>
inline void alcForceWrite(T &dst, const T &src)
{
    DWORD old_flag;
    ::VirtualProtect(&dst, sizeof(T), PAGE_EXECUTE_READWRITE, &old_flag);
    dst = src;
    ::VirtualProtect(&dst, sizeof(T), old_flag, &old_flag);
}

#endif // alcFoundation_h
