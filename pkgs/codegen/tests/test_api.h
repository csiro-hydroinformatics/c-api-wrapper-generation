/** \file test_api.h
 * Realistic test API for codegen.
*/

#pragma once

#ifndef TEST_USE_OPAQUE_POINTERS
#ifndef TEST_USE_CPP_POINTERS
#ifndef USING_TEST_CORE
#define TEST_USE_CPP_POINTERS
#endif
#endif
#endif

#ifndef TEST_DLL_LIB_MODIFIERS

#ifdef _WIN32
#ifdef USING_TEST_CORE
#define TEST_CORE_DLL_LIB __declspec(dllimport)
#else // USING_TEST_CORE
#define TEST_CORE_DLL_LIB __declspec(dllexport)
#endif // USING_TEST_CORE
#else //_WIN32
#define TEST_CORE_DLL_LIB // nothing
#endif //_WIN32
#endif // TEST_DLL_LIB_MODIFIERS

//#include "interop_struct.h"
struct TestStructure {};

#if defined(TEST_USE_CPP_POINTERS)

class TestOject { };

#define TEST_OBJECT_TRANSPARENT_PTR TestOject*
#define TEST_OBJECT_PTR                    TEST_OBJECT_TRANSPARENT_PTR

#elif defined(TEST_USE_OPAQUE_POINTERS)

#define TEST_OBJECT_PTR void*
#else
#error macro TEST_USE_OPAQUE_POINTERS or TEST_USE_CPP_POINTERS must be defined
#endif

// see http://msdn.microsoft.com/en-us/library/as6wyhwt.aspx, best practice
#define TEST_API  TEST_CORE_DLL_LIB 

#ifdef __cplusplus
extern "C" {
#endif
	TEST_API char* ReturnsCharArray();
	TEST_API char** ReturnsCharArrayArray(int* size);
	TEST_API void TakesConstVoidPtr(const void* callback);

	TEST_API void TakesAnsiStringArray(char** values, int arrayLength);
	TEST_API void TakesAnsiString(const char* value);

	TEST_API TEST_OBJECT_PTR ReturnsPtr();
	TEST_API void TakesPtr(TEST_OBJECT_PTR simulation);

	TEST_API TestStructure* ReturnsStructurePtr(TEST_OBJECT_PTR simulation);
	TEST_API void TakesStructurePtr(TestStructure* blah);
	TEST_API void TakesStructCopy(TEST_OBJECT_PTR simulation, TestStructure start, TestStructure end);

#ifdef __cplusplus
}
#endif
