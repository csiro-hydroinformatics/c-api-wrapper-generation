

See /home/per202/src/csiro/stash/per202/datatypes/bindings/python for a use case

# Elaborate on Matlab bindings gen

```R
library(rClr)
library(devtools)
load_all('c:/src/csiro/stash/c-api-bindings/codegen/capigen')
load_wrapper_gen_lib('c:/src/github_jm/rcpp-wrapper-generation')


api_filter <- create_api_filter(export_modifier_pattern='TEST_API')
# rClr::clrSet(api_filter, 'ContainsNone', common_exclude_api_functions())

capigen::generate_matlab_wrappers(infile='c:/src/csiro/stash/c-api-bindings/codegen/tests/test_api.h', outfolder='c:/src/csiro/stash/c-api-bindings/codegen/tests/lowlevel', api_filter=api_filter, libraryName = 'qpp')
```

Prioritise:

* one function, one file. This is not how it used to be.
* TDD
* Documentation
* Refactor/Move packages to open source repo.

C:\src\csiro\stash\qpp\bindings\matlab\create_simple_header_files.r

```R
library(rClr)
library(devtools)
load_all('c:/src/csiro/stash/c-api-bindings/codegen/capigen')
load_wrapper_gen_lib('c:/src/github_jm/rcpp-wrapper-generation')

api_filter <- create_api_filter(export_modifier_pattern='QPP_API')
# rClr::clrSet(api_filter, 'ContainsNone', common_exclude_api_functions())

qpp_src <- 'C:/src/csiro/stash/qpp'
qpp_matlab_src <- file.path(qpp_src, 'bindings/matlab')

capigen::generate_matlab_wrappers(infile=file.path(qpp_src, 'libqpp/qpp_extern_c_api.h'), outfolder=file.path(qpp_matlab_src, 'lowlevel'), api_filter=api_filter)
```
