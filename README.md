# Generate bindings for a C API

[![license](http://img.shields.io/badge/license-GPLv3-red.svg)](https://raw.githubusercontent.com/csiro-hydroinformatics/c-api-wrapper-generation/master/LICENSE)
![status](https://img.shields.io/badge/status-beta-orange.svg)

From a C API with functions such as:

```c++
SWIFT_API HYPERCUBE_PTR CreateHypercubeParameterizer(const char* strategy);
```

Generate the glue code to surface it to R, Python, Matlab, .NET, etc.

## Background

Around 2014 I needed in a project to (re)generate bindings for C++ scientific research modelling code _via a C API_ for at least R, Python, Matlab and C#. A manual approach was not a sustainable option. I first tried to apply or adapt a few of the many third party options (incl. some heavyweights in the wrapping field such as SWIG). I ended up with more difficulties than I bargained for, and despite my reluctance enacted plan B, a custom solution. 

It may, or may not, suit your needs. It is used on an ongoing basis for quite large APIs with hundreds of functions. I hope it can help alleviate your language interop glue code maintenance.

## Overview

A typical C API, build on top of C++ for a better cross-language interoperability, often looks like this [hydrologic modelling](https://www.mssanz.org.au/modsim2015/L15/perraud.pdf) example:

```c++
SWIFT_API HYPERCUBE_PTR CreateHypercubeParameterizer(const char* strategy);
SWIFT_API void AddParameterDefinition(HYPERCUBE_PTR hypercubeParameterizer, const char* variableName, double min, double max, double value);
SWIFT_API double GetParameterValue(HYPERCUBE_PTR hypercubeParameterizer, const char* variableName);
SWIFT_API void DisposeSharedPointer(VOID_PTR_PROVIDER_PTR ptr);
```

with C macro expanded:

```c++
extern void* CreateHypercubeParameterizer(const char* strategy);
extern void AddParameterDefinition(void* hypercubeParameterizer, const char* variableName, double min, double max, double value);
extern double GetParameterValue(void* hypercubeParameterizer, const char* variableName);
extern void DisposeSharedPointer(void* ptr);
```

This present code generation tool was created to generate bindings around native libraries with a C API. 

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

`opaque_pointer_handle` and `mkExternalObjRef` are managing native object lifetime. They are not included in the present repository, but if useful to you in the related [cpp interop commons](https://github.com/csiro-hydroinformatics/rcpp-interop-commons) repository.

## Getting started

_Note to self; consider using `F#` for scripting rather than using R and .NET._

### Windows 

```bat
MSBuild %github_dir%\c-api-wrapper-generation\ApiWrapperGenerator\ApiWrapperGenerator.sln /t:Build /p:Configuration=Debug
```

### Linux


#### prerequisites

Check dotnet is available with `which dotnet`, and `dotnet --info` should return on of the 2.x runtimes.

If need be follow the [Manual Install](https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian#manual-install). There are other ways to obtain dotnet.

`cd ~/Downloads` `ls dotnet*`

```text
dotnet-sdk-5.0.101-linux-x64.tar.gz
dotnet-sdk-3.1.404-linux-x64.tar.gz
dotnet-sdk-2.1.811-linux-x64.tar.gz
dotnet-sdk-3.1.402-linux-x64.tar.gz
```

```bash
tar zxf dotnet-sdk-2.1.811-linux-x64.tar.gz  -C "$HOME/dotnet"
```

Optionally if you want newer versions:

```bash
tar zxf dotnet-sdk-3.1.404-linux-x64.tar.gz  -C "$HOME/dotnet"
tar zxf dotnet-sdk-5.0.101-linux-x64.tar.gz  -C "$HOME/dotnet"
```

`nano ~/.bashrc` then adding

```sh
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
```

rClr:

```
sudo apt install mono-xbuild
sudo apt install libmono-2.0-dev
sudo apt install msbuild

cd ~/src/github_jm/rClr

cd ~/src/github_jm
export BUILDTYPE=Debug
R CMD INSTALL --no-test-load rClr

cd ~/src/github_jm
R CMD build --no-build-vignettes rClr
R CMD INSTALL rClr_0.9.0.tar.gz
```

#### Building our codegen engine

```sh
cd engine/ApiWrapperGenerator
dotnet restore ApiWrapperGenerator.sln
```

```bash
dotnet build --configuration Release --no-restore ApiWrapperGenerator.sln

cd ../TestApiWrapperGenerator/
dotnet test TestApiWrapperGenerator.csproj 
```
