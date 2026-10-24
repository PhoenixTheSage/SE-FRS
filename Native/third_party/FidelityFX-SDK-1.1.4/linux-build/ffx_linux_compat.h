// Force-included on Linux: MSVC CRT names the FidelityFX SDK 1.1.4 sources use.
#pragma once
#ifndef _WIN32
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <wchar.h>
#include <stdarg.h>
#ifdef __cplusplus
#include <cstring>
#include <cstdlib>
#include <cwchar>
#include <new>
#include <codecvt>
#include <locale>
#include <string>
#endif

#include <math.h>
#ifdef __cplusplus
#include <cmath>
#endif

typedef void ID3D12Device;
#define FFX_UNUSED(x)               ((void)(x))

#ifndef _countof
#define _countof(a) (sizeof(a) / sizeof((a)[0]))
#endif

#define sprintf_s(buf, size, ...) snprintf((buf), (size), __VA_ARGS__)
#define swprintf_s(buf, size, ...) swprintf((buf), (size), __VA_ARGS__)
#define vsprintf_s(buf, size, fmt, args) vsnprintf((buf), (size), (fmt), (args))
#define _vsnprintf_s(buf, size, count, fmt, args) vsnprintf((buf), (size), (fmt), (args))
#define strcat_s(dst, size, src) strncat((dst), (src), (size) - strlen(dst) - 1)

static inline int ffx_compat_wcscpy_s(wchar_t* dst, size_t size, const wchar_t* src)
{
    if (!dst || size == 0)
        return 22;
    wcsncpy(dst, src ? src : L"", size - 1);
    dst[size - 1] = L'\0';
    return 0;
}
static inline int ffx_compat_strcpy_s(char* dst, size_t size, const char* src)
{
    if (!dst || size == 0)
        return 22;
    strncpy(dst, src ? src : "", size - 1);
    dst[size - 1] = '\0';
    return 0;
}
static inline int ffx_compat_wcsncpy_s(wchar_t* dst, size_t size, const wchar_t* src, size_t count)
{
    if (!dst || size == 0)
        return 22;
    size_t n = count < size - 1 ? count : size - 1;
    wcsncpy(dst, src ? src : L"", n);
    dst[n] = L'\0';
    return 0;
}
#ifdef __cplusplus
template <size_t N> static inline int ffx_compat_wcscpy_s(wchar_t (&dst)[N], const wchar_t* src) { return ffx_compat_wcscpy_s(dst, N, src); }
template <size_t N> static inline int ffx_compat_strcpy_s(char (&dst)[N], const char* src) { return ffx_compat_strcpy_s(dst, N, src); }
#endif
#define wcscpy_s ffx_compat_wcscpy_s
#define strcpy_s ffx_compat_strcpy_s
#define wcsncpy_s ffx_compat_wcsncpy_s
#endif
