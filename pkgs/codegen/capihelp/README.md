# capihelp package

## Purpose

This package facilitates the generation of low-level language bindings from a C API for a variety of languages.

* R
* Matlab
* C#
* Python
* C++

## Sample


## Building

```R
library(devtools)
h <- Sys.getenv('HOME')
f <- file.path
pkg_path <- f(h, 'src/c-api-wrapper-generation/pkgs/codegen/capihelp')
```

```R
document(pkg_path)
load_all(pkg_path)
```

```R
build(pkg_path)
install(pkg_path)
```