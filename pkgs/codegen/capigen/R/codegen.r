#' @export
generated_cpp_comment_header <- function() {
  capihelp::generated_cpp_comment_header()
}

#' @export
default_xptr_wrapper_prepend <- function() {
  capihelp::default_xptr_wrapper_prepend()
}

#' @export
default_py_cffi_wrapper_prepend <- function() {
  capihelp::default_py_cffi_wrapper_prepend()
}

#' @export
default_matlab_wrapper_prepend <- function() {
  capihelp::default_matlab_wrapper_prepend()
}


#' create_api_filter
#'
#' create_api_filter
#'
#' @param class_name the full class name of a CodeFileFilter.
#' @param export_modifier_pattern (regex?) pattern(s) to use as a marker for API functions.
#' @return a CodeFileFilter
#' @export
create_api_filter <- function(class_name='ApiWrapperGenerator.HeaderFilter', export_modifier_pattern) {
  api_filter <- rClr::clrNew(class_name)
  rClr::clrSet(api_filter, 'NotStartsWith', c('#', '//', '*'))    # '*' is to exclude from multi-line comment blocks including doxygen doc.
  stopifnot(length(export_modifier_pattern) > 0)
  # Note: I need to have at least two elements in criteria (to match C# method arg as a string[]). This is just a workaround.
  if (length(export_modifier_pattern) > 1) { 
    pat <- export_modifier_pattern
  } else  { 
    pat <- rep(export_modifier_pattern, 2)
  }
  rClr::clrSet(api_filter, 'ContainsAny', pat)
  rClr::clrSet(api_filter, 'ToRemove', pat)
  return(api_filter)
}

#' Creates a .NET object that parses and matches Rcpp functions declarations marked with the RcppExport attribute.
#'
#' Creates a .NET object that parses and matches Rcpp functions declarations marked with the RcppExport attribute.
#'
#' @import rClr
#' @export
create_rcpp_exported_func <- function() {
  api_filter <- create_api_filter('ApiWrapperGenerator.RcppExportedCppFunctions', export_modifier_pattern='[[Rcpp::export]] ')
  return(api_filter)
}

#' create_wrapper_generator
#'
#' create_wrapper_generator
#'
#' @param api_filter a HeaderFilter
#' @param converter an ApiWrapperGenerator converter
#' @return a WrapperGenerator
#' @export
create_wrapper_generator <- function(converter, api_filter) {
  rClr::clrNew('ApiWrapperGenerator.WrapperGenerator', converter, api_filter)
}

#' set_wrapper_type_map
#'
#' set_wrapper_type_map
#'
#' @param api_type a string, the C API type, e.g. 'char**'
#' @param wrapper_type a string, the corresponding type in the target language, e.g. 'const std::vector<std::string>&'
#' @param converter an ApiWrapperGenerator converter
#' @export
set_wrapper_type_map <- function(converter, api_type, wrapper_type) {
 invisible(rClr::clrCall(converter, 'SetTypeMap', api_type, wrapper_type))
}

#' Defines how to create transient variables to call the wrapped API function
#'
#' Defines how to create transient variables to call the wrapped API function
#'
#' @param converter the API converter object
#' @param api_type the C API type, e.g. 'char**'
#' @param var_postfix a character to use as postfix to generate the local variable names, e.g. '_charpp'
#' @param setup_template C/C++ statement that specifies the creation of the local variable e.g. 'char** C_ARGNAME = createAnsiStringArray(RCPP_ARGNAME);'
#' @param cleanup_template  C/C++ statement that specifies how to delete the local variable e.g. 'freeAnsiStringArray(C_ARGNAME, RCPP_ARGNAME.length());'
#' @export
set_wrapper_type_converter <- function(converter, api_type, var_postfix, setup_template, cleanup_template) {
 rClr::clrCall(converter, 'SetTransientArgConversion', api_type, var_postfix, setup_template, cleanup_template)
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


add_common_custom_wrappers <- function(converter) {
  returnscharptrptr <- rClr::clrCall(converter, 'ReturnsCharPtrPtrWrapper')
  rClr::clrCall(converter, 'AddCustomWrapper', returnscharptrptr)
  returnsdoubleptr <- rClr::clrCall(converter, 'ReturnsDoublePtrWrapper')
  rClr::clrCall(converter, 'AddCustomWrapper', returnsdoubleptr)
  converter
}

