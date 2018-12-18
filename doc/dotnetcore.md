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
