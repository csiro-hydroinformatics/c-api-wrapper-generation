

## sample code

## 

```r
library(capigen)
load_wrapper_gen_lib('c:/src/github_jm/rcpp-wrapper-generation')
moirai_src_path <- "C:/src/github_jm/moirai"

moirai_test_api_headerfile <- file.path(moirai_src_path, "tests/moirai_test_lib/c_interop_api.h")

api_filter <- create_api_filter(export_modifier_pattern='SPECIES_API')

prepend_header <- default_py_cffi_wrapper_prepend()
infile <- moirai_test_api_headerfile
outfile <- "c:/tmp/testmoirai.py"

generate_py_cffi_wrappers(
  prepend_header,
  infile,
  outfile,
  api_filter=api_filter
)
```
