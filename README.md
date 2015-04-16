# rcpp-wrapper-generation

Generate Rcpp functions around an existing C/C++ API

```S
spath <- 'f:/path/to/this/directory'
spath <- 'F:/src/github_jm/rcpp-wrapper-generation'
library(rClr) # rclr.codeplex.com

clrLoadAssembly(file.path(spath, 'RcppGen/bin/Debug/RcppGen.dll'))
gen <- clrNew('Rcpp.CodeGen.WrapperGenerator')
clrGetProperties(gen)

clrSet(gen, 'OpaquePointers', TRUE)
clrSet(gen, 'AddRcppExport', TRUE)
clrSet(gen, 'DeclarationOnly', FALSE)
clrSet(gen, 'ContainsAny', c('SWIFT_API','_API'))
clrSet(gen, 'ContainsNone', c('DeleteAnsiStringArray','SomeOtherFunctionName'))
clrSet(gen, 'ToRemove', c('SWIFT_API','_API'))

clrSet(gen, 'FunctionNamePostfix', '_R')

clrGet(gen, 'TypeMap')
clrCall(gen, 'SetTypeMap', 'const char**', 'CharacterVector')



infile <- file.path( spath, 'libswift/extern_c_api.h')
outfile <- 'f:/tmp/outlines.txt'
clrCall(gen, 'CreateWrapperHeader', infile, outfile)
# or
inlines = readLines(file.path( spath, 'libswift/extern_c_api.h'))
inlines <- inlines[!is.na(str_match( inlines, 'SWIFT_API'))]
outlines <- sapply(inlines, function(line) {clrCall(gen, 'CHeaderToRcpp', line, 'SWIFT_API')})
writeLines(outlines, 'f:/tmp/outlines.txt', sep='')
```
