# rcpp-wrapper-generation

## Background

Around 2014 I needed in a project to generate bindings for C++ scientific research modelling code _via a C API_ for at least R, Python, Matlab and C#. After scouting around I tried to apply or adapt a few of the many third party options (incl. some heavyweights in the wrapping field) at the time. I ended up with unsatisfactory feasibility. Hence, this repo.

It may, or may not, suit your needs. It is used on an ongoing basis for quite large APIs of hundreds of functions. If it can help alleviate your language interop glue code maintenance, all the better.

## Purpose

```c++
	SWIFT_API HYPERCUBE_PTR CreateHypercubeParameterizer(const char* strategy);
	SWIFT_API void AddParameterDefinition(HYPERCUBE_PTR hypercubeParameterizer, const char* variableName, double min, double max, double value);
	SWIFT_API double GetParameterValue(HYPERCUBE_PTR hypercubeParameterizer, const char* variableName);
	SWIFT_API void DisposeSharedPointer(VOID_PTR_PROVIDER_PTR ptr);
```

This code generation tool was created to generate bindings around native libraries with C API signatures looking (from outside the native library). An extract from a real world, [hydrologic modelling](https://www.mssanz.org.au/modsim2015/L15/perraud.pdf) example:

```c++
extern void* CreateHypercubeParameterizer(const char* strategy);
extern void AddParameterDefinition(void* hypercubeParameterizer, const char* variableName, double min, double max, double value);
extern double GetParameterValue(void* hypercubeParameterizer, const char* variableName);
extern void DisposeSharedPointer(void* ptr);
```

Say you want to surface this API in R using [Rcpp](http://www.rcpp.org/). There are plenty of design options of course, but chances are you will need some boilerplate `C++` code with Rcpp classes (`XPtr`, `CharacterVector`, etc.) looking like:

```c++
// [[Rcpp::export]]
XPtr<opaque_pointer_handle> CreateHypercubeParameterizer_Rcpp(CharacterVector strategy)
{
    auto result = CreateHypercubeParameterizer(strategy[0]);
    auto x = XPtr<opaque_pointer_handle>(new opaque_pointer_handle(result));
    return x;
}
```

and boilerplate `R` code such as:

```R
#' CreateHypercubeParameterizer_R
#'
#' CreateHypercubeParameterizer_R Wrapper function for CreateHypercubeParameterizer
#'
#' @param strategy R type equivalent for C++ type const char*
#' @export
CreateHypercubeParameterizer_R <- function(strategy) {
  strategy <- cinterop::getExternalXptr(strategy)
  result <- CreateHypercubeParameterizer_Rcpp(strategy)
  return(cinterop::mkExternalObjRef(result, 'HYPERCUBE_PTR'))
}
```

`opaque_pointer_handle` and `mkExternalObjRef` are wrapping helpers for managing native object lifetime, but besides the repository.

## Getting started

Generate Rcpp functions around an existing C/C++ API.

As of April 2015, this is used on a specific C API of ~ 100 functions. I tried to make some things generic, but this is very much a prototype.

I put this on GitHub as there seems not to be tools that I could find to parse a C API and generate functions that use the Rcpp objects as arguments. 

The code is in C# because, well, this is the most productive. I'll see whether I port to an R package later on.

Note to self: something like T4 for code generation could be more versatile and scalable. 

```bash
cd /home/per202/src/github_jm/rcpp-wrapper-generation/ApiWrapperGenerator
msbuild ApiWrapperGenerator.csproj /p:Configuration=Debug /p:Platform=AnyCPU
```

```R
wgenDir <- 'f:/path/to/this/directory'
wgenDir <- 'F:/src/github_jm/rcpp-wrapper-generation'
library(rClr) # rclr.codeplex.com

clrLoadAssembly(file.path(wgenDir, 'RcppGen/bin/Debug/RcppGen.dll'))
gen <- clrNew('Rcpp.CodeGen.WrapperGenerator')
clrGetProperties(gen)
```

```R
clrSet(gen, 'OpaquePointers', TRUE)
clrSet(gen, 'OpaquePointerClassName', 'OpaquePointer')
clrSet(gen, 'AddRcppExport', TRUE)
clrSet(gen, 'DeclarationOnly', FALSE)
clrSet(gen, 'FunctionNamePostfix', '_R')
```

```S
# Note: I need to have at least two elements in criteria. This is just a workaround.
clrSet(gen, 'ContainsAny', c('SWIFT_API','SWIFT_API'))
clrSet(gen, 'ContainsNone', c('DeleteAnsiStringArray', 'DeleteAnsiString', 'MarshaledDateTime', 'TS_GEOMETRY_PTR'))
clrSet(gen, 'NotStartsWith', c('#', '//'))
clrSet(gen, 'ToRemove', c('SWIFT_API','SWIFT_API'))
# clrGet(gen, 'TypeMap')
```

```S
spath <- 'f:/src/csiro/stash/per202/swift'
infile <- file.path( spath, 'libswift/extern_c_api.h')
outfile <- 'f:/tmp/outlines.txt'
clrCall(gen, 'CreateWrapperHeader', infile, outfile)
# or
# inlines = readLines(file.path( wgenDir, 'libswift/extern_c_api.h'))
# inlines <- inlines[!is.na(str_match( inlines, 'SWIFT_API'))]
# outlines <- sapply(inlines, function(line) {clrCall(gen, 'CHeaderToRcpp', line, 'SWIFT_API')})
# writeLines(outlines, 'f:/tmp/outlines.txt', sep='')
```
