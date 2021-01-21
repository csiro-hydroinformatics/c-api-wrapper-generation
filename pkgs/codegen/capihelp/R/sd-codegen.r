#' remove_rcpp_generated_files
#'
#' remove_rcpp_generated_files
#'
#' @export
remove_rcpp_generated_files <- function(pkg_path) {
file.remove(
  file.path(pkg_path, 'R', 'RcppExports.R'),
  file.path(pkg_path, 'src', 'RcppExports.R') )
}

#' default_rcppgen_header_prepend
#'
#' default_rcppgen_header_prepend
#'
#' @export
default_rcppgen_header_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include "swift.h"
#include "swift_wrapper_setup.h"
#include "swift_struct_interop.h"

using namespace Rcpp;
using namespace cinterop::utils;
using moirai::opaque_pointer_handle;

//////////// End of preamble ////////////
'
    )
  )
}

#' default_cppgen_prepend
#'
#' default_cppgen_prepend
#'
#' @export
default_cppgen_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include "swift_cpp_api_generated.h"
#include "swift_cpp_typeconverters.h"

//////////// End of preamble ////////////
'
    )
  )
}

#' default_cppgen_header_prepend
#'
#' default_cppgen_header_prepend
#'
#' @export
default_cppgen_header_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include <string>
#include <vector>
#include "swift_wrapper_setup.h"

//////////// End of preamble ////////////
'
    )
  )
}

#' default_cppgen_prepend
#'
#' default_cppgen_prepend
#'
#' @export
default_rpp_cppgen_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include "rpp_wrapper_setup.h"
#include "cinterop/rcpp_interop.hpp"
#include "cinterop/rcpp_timeseries_interop.hpp"
#include "rpp_r_exports.h"
#include "rpp_struct_interop.h"
#include "wila/interop_rcpp.hpp"

using namespace Rcpp;
using namespace cinterop::utils;
using moirai::opaque_pointer_handle;

//////////// End of preamble ////////////
'
    )
  )
}


#' @export
default_qpp_cppgen_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include "qpp_wrapper_setup.h"
#include "cinterop/rcpp_interop.hpp"
#include "cinterop/rcpp_timeseries_interop.hpp"
#include "qpp_r_exports.h"
#include "qpp_struct_interop.h"

using namespace Rcpp;
using namespace cinterop::utils;
using moirai::opaque_pointer_handle;

//////////// End of preamble ////////////
'
    )
  )
}

#' @export
default_uchronia_cppgen_prepend <- function() {
  return(
    paste0(
      generated_cpp_comment_header(),
'#include "uchronia.h"
#include "uchronia_wrapper_setup.h"
#include "uchronia_struct_interop.h"

using namespace Rcpp;
using namespace cinterop::utils;
using moirai::opaque_pointer_handle;

//////////// End of preamble ////////////
'
    )
  )
}

#' @export
common_exclude_api_functions <- function() {
  c(
    'DeleteAnsiStringArray', 
    'DeleteAnsiString',
    'DisposeSharedPointer' # We do not really need to surface this: handled at the C/C++ level of the R api for garbage collection
  )
}

#' Perform Rcpp::compileAttributes on SWIFT R
#' 
#' Perform Rcpp::compileAttributes on SWIFT R
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import Rcpp
#' @export
swiftr_compileAttributes <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  pkg_path <- file.path( swiftSrcPath, 'bindings/R/pkgs/swift')
  custom_rcpp_compile_attributes(pkg_path, overriding_package_name='swift_r')  
}

#' @export
rppr_compileAttributes <- function(rppSrcPath='') {
  pkg_path <- pkg_path_rppr( rppSrcPath )
  custom_rcpp_compile_attributes(pkg_path, overriding_package_name='rpp_r')  
}

#' @export
qppr_compileAttributes <- function(qppSrcPath='') {
  pkg_path <- pkg_path_qppr( qppSrcPath )
  custom_rcpp_compile_attributes(pkg_path, overriding_package_name='qpp_r')  
}

#' @export
uchronia_r_compileAttributes <- function(uchroniaSrcPath='') {
  pkg_path <- pkg_path_uchronia_r( uchroniaSrcPath )
  custom_rcpp_compile_attributes(pkg_path, overriding_package_name='uchronia_r')  
}

#' @export
apply_c_preprocessor <- function(include_dirs, api_importer_file, outfile, execute=TRUE) {
  include_dirs <- normalizePath(include_dirs)
  api_importer_file <- normalizePath(api_importer_file)
  outfile <- normalizePath(outfile)
  f <- function(...) {paste(..., sep = " " , collapse = " ")}
  include_options <- f( "-I", include_dirs)
  codedef_options <- f( "-std=c++0x" , "-E", api_importer_file)
  cmd_line <- f( "gcc", include_options, codedef_options, " -o ", outfile)
  if(execute) {
    x <- system(cmd_line, intern=TRUE)
    return(invisible(x))
  } else {
    return(cmd_line)
  }
}


#' @export
extract_cffi_cdefs <- function(preprocessed_cpp_file_lines, pattern_start_structs="typedef struct.*", 
  extern_c_start_match='extern .C. \\{', extern_c_end_match = '^\\}') {
  a <- preprocessed_cpp_file_lines

  struct_start_line <- (which(stringr::str_detect(a, pattern_start_structs)))[1]
  extern_c_start_line <- (which(stringr::str_detect(a, extern_c_start_match)))[1]

  end_brackets <- which(stringr::str_detect(a, extern_c_end_match))
  extern_c_end_line <- (end_brackets[end_brackets > extern_c_start_line])[1]

  structs <- a[struct_start_line:(extern_c_start_line-1)]
  funcs <- a[(extern_c_start_line+1):(extern_c_end_line-1)]

  not_empty <- function(x) { x[stringr::str_detect(x, '.+')] }
  not_comment <- function(x) { x[!stringr::str_detect(x, '^#')] }

  structs <- structs %>% (stringr::str_trim) %>% not_empty %>% not_comment
  funcs <- funcs %>% (stringr::str_trim) %>% not_empty %>% not_comment
  # Depending on whether we are on linux or windows the __attribute__ may differ - cater for both:
  # [1] "__attribute__((dllimport)) void RegisterExceptionCallback(const void* callback);" 
  funcs <- stringr::str_replace(funcs, '__attribute__\\(\\(dllimport\\)\\)', 'extern')
  #  __attribute__((dllexport)) multi_regular_time_series_data* ToStructEnsembleTimeSeriesData(void* ensSeries);
  funcs <- stringr::str_replace(funcs, '__attribute__\\(\\(dllexport\\)\\)', 'extern')
  # clean up ptr and other formating
  funcs <- stringr::str_replace_all(funcs, ' *\\* *', "\\* ")
  # clean up things like char* * 
  funcs <- stringr::str_replace_all(funcs, '\\* \\*', "\\*\\*")
  funcs <- stringr::str_replace_all(funcs, ' +\\);', "\\);")
  funcs <- stringr::str_replace_all(funcs, '\\( +', "\\(")
  funcs <- stringr::str_replace_all(funcs, ' +', " ")

  # Why did I have this. Probably superflous:
  # funcs <- paste(funcs, collapse= " ")
  # funcs <- stringr::str_replace_all(funcs, ' *; *', ';\n')

  return(list(structs=structs,funcs=funcs))
}

#' @export
create_cffi_cdefs <- function(preprocessed_cpp_file, outdir, pattern_start_structs, 
  extern_c_start_match='extern .C. \\{', extern_c_end_match = '^\\}') {
  a <- readLines(preprocessed_cpp_file)
  extracted <- extract_cffi_cdefs(a, pattern_start_structs, extern_c_start_match, extern_c_end_match)
  structs <- extracted[[1]]
  funcs <- extracted[[2]]
  writeLines( structs, file.path(outdir, 'structs_cdef.h'))
  writeLines( funcs, file.path(outdir, 'funcs_cdef.h'))
}

# #' @export
# create_swig_idl <- function() {
#   out_dir <- '~/src/csiro/stash/per202/datatypes/bindings/python/swig/out'
#   # STUB TODO
# }

