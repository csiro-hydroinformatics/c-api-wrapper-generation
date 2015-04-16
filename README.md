# rcpp-wrapper-generation

Generate Rcpp functions around an existing C/C++ API.

As of April 2015, this is used on a specific C API of ~ 100 functions. I tried to make some things generic, but this is very much a prototype.

I put this on GitHub as there seems not to be tools that I could find to parse a C API and generate functions that use the Rcpp objects as arguments. 

The code is in C# because, well, this is the most productive. I'll see whether I port to an R package later on.

Note to self: something like T4 for code generation could be more versatile and scalable. 

```S
wgenDir <- 'f:/path/to/this/directory'
wgenDir <- 'F:/src/github_jm/rcpp-wrapper-generation'
library(rClr) # rclr.codeplex.com

clrLoadAssembly(file.path(wgenDir, 'RcppGen/bin/Debug/RcppGen.dll'))
gen <- clrNew('Rcpp.CodeGen.WrapperGenerator')
clrGetProperties(gen)
```

```S
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
