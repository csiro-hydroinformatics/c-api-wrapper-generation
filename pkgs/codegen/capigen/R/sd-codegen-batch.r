#' generate all wrappers for the swift R package
#' 
#' generate all wrappers for the swift R package, derived from the SWIFT C API. Generates the c++ wrapers, 
#'   uses Rcpp compileAttributes, then completes by adding a layer that thinly wraps R external pointers to SWIFT objects.
#'  
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_rcpp_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @param prepend_r_xptr_header the text to prepend to the generated R code.
#' @export
generate_swiftr_all_wrappers <- function(swiftSrcPath='', 
    prepend_rcpp_header=default_rcppgen_header_prepend(),
    prepend_r_xptr_header=default_xptr_wrapper_prepend()
    ) {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  generate_swift_rcpp_glue(swiftSrcPath, prepend_rcpp_header)
  swiftr_compileAttributes(swiftSrcPath)
  generate_xptr_wrappers(swiftSrcPath, prepend_r_xptr_header,api_filter=create_swift_api_filter())
  generate_xptr_wrappers_from_rcppfunc(swiftSrcPath, prepend_r_xptr_header)
}


#' @export
generate_rppr_all_wrappers <- function(rppSrcPath='', 
    prepend_rcpp_header=default_rcppgen_header_prepend(),
    prepend_r_xptr_header=default_xptr_wrapper_prepend()
    ) {
  if(rppSrcPath=='') rppSrcPath <- find_rpp_src_path()
  generate_rpp_rcpp_glue(rppSrcPath)
  rppr_compileAttributes(rppSrcPath)

  generate_xptr_wrappers('', prepend_r_xptr_header,
	  infile=extern_c_api_header_file_rppr(rppSrcPath),
	  outfile=outfile_xptr_wrappers_rppr(rppSrcPath),api_filter=create_rpp_api_filter()
  )
  rppPkgPath <- check_dir_exists(file.path(rppSrcPath, 'bindings/R/pkgs/rpp'))
  generate_xptr_wrappers_from_rcppfunc(rppSrcPath, prepend_r_xptr_header,
    infile  =file.path(rppPkgPath, 'src/rcpp_custom.cpp'),
    outfile =file.path(rppPkgPath, 'R/rpp-pkg-wrap-generated.r')
  )
}

#' @export
generate_uchronia_r_all_wrappers <- function(uchroniaSrcPath='', 
    prepend_rcpp_header=default_rcppgen_header_prepend(),
    prepend_r_xptr_header=default_xptr_wrapper_prepend()
    ) {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  generate_uchronia_rcpp_glue(uchroniaSrcPath)
  uchronia_r_compileAttributes(uchroniaSrcPath)
  generate_xptr_wrappers('', prepend_r_xptr_header,
	  infile=extern_c_api_header_file_uchronia_r(uchroniaSrcPath),
	  outfile=outfile_xptr_wrappers_uchronia_r(uchroniaSrcPath), api_filter=create_uchronia_api_filter()
  )
  uchroniaPkgPath <- check_dir_exists(file.path(uchroniaSrcPath, 'bindings/R/pkgs/uchronia'))
  generate_xptr_wrappers_from_rcppfunc(uchroniaSrcPath, prepend_r_xptr_header,
    infile  =file.path(uchroniaPkgPath, 'src/rcpp_custom.cpp'),
    outfile =file.path(uchroniaPkgPath, 'R/uchronia-pkg-wrap-generated.r')
  )
}

#' @export
generate_qppr_all_wrappers <- function(qppSrcPath='', 
    prepend_rcpp_header=default_rcppgen_header_prepend(),
    prepend_r_xptr_header=default_xptr_wrapper_prepend()
    ) {
  if(qppSrcPath=='') qppSrcPath <- find_qpp_src_path()
  generate_qpp_rcpp_glue(qppSrcPath)
  qppr_compileAttributes(qppSrcPath)

  generate_xptr_wrappers('', prepend_r_xptr_header,
	  infile=extern_c_api_header_file_qppr(qppSrcPath),
	  outfile=outfile_xptr_wrappers_qppr(qppSrcPath),api_filter=create_qpp_api_filter()
  )
  qppPkgPath <- check_dir_exists(file.path(qppSrcPath, 'bindings/R/pkgs/qpp'))
  generate_xptr_wrappers_from_rcppfunc(qppSrcPath, prepend_r_xptr_header,
    infile  =file.path(qppPkgPath, 'src/rcpp_custom.cpp'),
    outfile =file.path(qppPkgPath, 'R/qpp-pkg-wrap-generated.r')
  )
}

#' @export
generate_uchronia_python_all_wrappers <- function(uchroniaSrcPath='', 
    # prepend_rcpp_header=default_rcppgen_header_prepend(),
    prepend_r_xptr_header=default_py_cffi_wrapper_prepend()
    ) {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  generate_py_cffi_wrappers(prepend_r_xptr_header,
	  infile=extern_c_api_header_file_uchronia_r(uchroniaSrcPath),
	  outfile=outfile_py_cffi_wrappers_uchronia(uchroniaSrcPath), api_filter=create_uchronia_api_filter()
  )
  # uchroniaPkgPath <- check_dir_exists(file.path(uchroniaSrcPath, 'bindings/R/pkgs/uchronia'))
  # generate_xptr_wrappers_from_rcppfunc(uchroniaSrcPath, prepend_r_xptr_header,
  #   infile  =file.path(uchroniaPkgPath, 'src/rcpp_custom.cpp'),
  #   outfile =file.path(uchroniaPkgPath, 'R/uchronia-pkg-wrap-generated.r')
  # )
}

