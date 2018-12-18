Dec 2018

Documenting steps taken to set the gen code on a linux box using dotnet core.

Trying [.NET core 3.0.](https://dotnet.microsoft.com/download/dotnet-core/3.0)

`mkdir -p $HOME/dotnet && tar zxf $HOME/Downloads/dotnet-sdk-3.0.100-preview-009812-linux-x64.tar.gz -C $HOME/dotnet`

And putting in ~/config/dotnet to source from:

```sh
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet
```

Going through the [Getting started tutorial](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial?sdk-installed=true). This is written for the stable version 2.2 but seems forward compat. 

Continuing via the [.net core guide](https://docs.microsoft.com/en-us/dotnet/core/).

[Port your code from .NET Framework to .NET Core](https://docs.microsoft.com/en-us/dotnet/core/porting/) seems fresh enough

Give a try to a 3rd party tool [CsprojToVs2017](https://github.com/hvanbakel/CsprojToVs2017)

`dotnet tool install --global Project2015To2017.Migrate2017.Tool` succeeds and provides useful advice as to PATH...

`cd ~/src/github_jm/rcpp-wrapper-generation/ApiWrapperGenerator` `dotnet migrate-2017 migrate ApiWrapperGenerator.sln`

```txt
It was not possible to find any compatible framework version
The specified framework 'Microsoft.NETCore.App', version '2.1.0' was not found.
  - Check application dependencies and target a framework version installed at:
      /home/per202/dotnet
  - Installing .NET Core prerequisites might help resolve this problem:
      https://go.microsoft.com/fwlink/?LinkID=798306&clcid=0x409
  - The .NET Core framework and SDK can be installed from:
      https://aka.ms/dotnet-download
  - The following versions are installed:
      3.0.0-preview-27122-01 at [/home/per202/dotnet/shared/Microsoft.NETCore.App]
```

Fair enough.

```sh
rm Properties/AssemblyInfo.cs
dotnet build -f netstandard2.0  ApiWrapperGenerator.csproj
```
Looking good!

`cd ~/src/github_jm/rcpp-wrapper-generation/TestApiWrapperGenerator` `dotnet build -f netcoreapp2.0 TestApiWrapperGenerator.csproj`

```sh
cd ~/src/github_jm/rcpp-wrapper-generation/ApiWrapperGenerator
dotnet restore ApiWrapperGenerator.sln
```

```sh
dotnet build --configuration Release --no-restore ApiWrapperGenerator.sln
```

Tried to use myultiple target fw in proj files but this requires explicit cmd line option to compile without failure (otherwise tries to doo each FW target, which makes sense). Problematic for sulutions though, netcoreapp and netstandard cannot be both specified, right?

```xml
    <!-- <TargetFrameworks>netstandard2.0;net472;net461</TargetFrameworks> -->
```

```sh
cd ~/src/github_jm/rcpp-wrapper-generation/TestApiWrapperGenerator
dotnet test TestApiWrapperGenerator.csproj 
```

```txt 
Build started, please wait...
Build completed.

Test run for /home/per202/src/github_jm/rcpp-wrapper-generation/TestApiWrapperGenerator/bin/Debug/netcoreapp2.0/TestApiWrapperGenerator.dll(.NETCoreApp,Version=v2.0)
Microsoft (R) Test Execution Command Line Tool Version 15.9.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
```

Still waiting. 