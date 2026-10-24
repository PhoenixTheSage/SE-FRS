// dlopen the built library and list the upscale providers via ffxQuery(GET_VERSIONS).
#include <dlfcn.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>

typedef struct ffxApiHeader { uint64_t type; struct ffxApiHeader* pNext; } ffxApiHeader;
typedef struct ffxQueryDescGetVersions {
    ffxApiHeader header;
    uint64_t createDescType;
    void* device;
    uint64_t* outputCount;
    uint64_t* versionIds;
    const char** versionNames;
} ffxQueryDescGetVersions;

#define FFX_API_QUERY_DESC_TYPE_GET_VERSIONS 4u
#define FFX_API_CREATE_CONTEXT_DESC_TYPE_UPSCALE 0x00010000u

typedef int32_t (*PfnQuery)(void* context, ffxApiHeader* desc);

int main(int argc, char** argv)
{
    void* lib = dlopen(argv[1], RTLD_NOW | RTLD_LOCAL);
    if (!lib) { printf("dlopen failed: %s\n", dlerror()); return 1; }
    const char* names[] = { "ffxCreateContext", "ffxDestroyContext", "ffxConfigure", "ffxQuery", "ffxDispatch" };
    for (int i = 0; i < 5; i++)
        printf("%s %s\n", names[i], dlsym(lib, names[i]) ? "ok" : "MISSING");

    PfnQuery query = (PfnQuery)dlsym(lib, "ffxQuery");
    uint64_t count = 0;
    ffxQueryDescGetVersions q;
    memset(&q, 0, sizeof(q));
    q.header.type = FFX_API_QUERY_DESC_TYPE_GET_VERSIONS;
    q.createDescType = FFX_API_CREATE_CONTEXT_DESC_TYPE_UPSCALE;
    q.outputCount = &count;
    int32_t rc = query(NULL, &q.header);
    printf("count rc=%d n=%llu\n", rc, (unsigned long long)count);
    uint64_t ids[8] = {0};
    const char* vn[8] = {0};
    if (count > 8) count = 8;
    q.versionIds = ids;
    q.versionNames = vn;
    rc = query(NULL, &q.header);
    for (uint64_t i = 0; i < count; i++)
        printf("  upscaler %s id=0x%llx\n", vn[i] ? vn[i] : "?", (unsigned long long)ids[i]);
    return rc;
}
