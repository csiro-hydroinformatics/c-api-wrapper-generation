#' @export
generated_cpp_comment_header <- function() {
  return(
'////////////////////////////////////
// 
// *** THIS FILE IS GENERATED ****
// DO NOT MODIFY IT MANUALLY, AS YOU ARE VERY LIKELY TO LOSE WORK
// 
////////////////////////////////////

'
  )
}

#' @export
default_xptr_wrapper_prepend <- function() {
  return(
'##################
# 
# *** THIS FILE IS GENERATED ****
# DO NOT MODIFY IT MANUALLY, AS YOU ARE VERY LIKELY TO LOSE WORK
# 
##################

'
  )
}

#' @export
default_py_cffi_wrapper_prepend <- function() {
  x <- default_xptr_wrapper_prepend()
  x <- paste0(x, 
'

from refcount.interop import *

'
  )
  return(x)
}


#' @export
default_matlab_wrapper_prepend <- function() {
  return(
'%%%%%%%%%%%%%%%%%%
% 
% *** THIS FILE IS GENERATED ****
% DO NOT MODIFY IT MANUALLY, AS YOU ARE VERY LIKELY TO LOSE WORK
% 
%%%%%%%%%%%%%%%%%%

'
  )
}

#' Perform a modified Rcpp::compileAttributes on an R package
#' 
#' Perform a modified Rcpp::compileAttributes on an R package, to open package compilation with Visual C++, and optionally change the name of the C API native library being loaded/used by Rcpp
#' 
#' @import Rcpp
#' @export
custom_rcpp_compile_attributes <- function(pkg_path='', overriding_package_name=NA) {
  Rcpp::compileAttributes(pkg_path)  
  RcppExportRfile <- file.path( pkg_path, 'R', 'RcppExports.R')
  if(!is.na(overriding_package_name)) {
    str_replace_file(RcppExportRfile, "PACKAGE = '.*'", paste0("PACKAGE = '",overriding_package_name,"'"))
  }
  RcppExportRcppfile <- file.path( pkg_path, 'src', 'RcppExports.cpp')
  pattern <- '#include <Rcpp.h>'
  # Do note that this is important to have extern "C" preceding __declspec(dllexport) (i.e. not the other way around)
  replacement <- '#include <Rcpp.h>\n// following line added by R function custom_rcpp_compile_attributes\n#ifdef _WIN32\n#define RcppExport extern "C" __declspec(dllexport)\n#endif\n\n'
  str_replace_file(RcppExportRcppfile, pattern, replacement)
}

