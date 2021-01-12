#' roxygenize R package
#' 
#' roxygenize R package
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param pathRelativeToSwift Location of the package to roxygenize
#' @import roxygen2
#' @export
roxy_pkg <- function(swiftSrcPath='', pathRelativeToSwift='R/pkgs/swift') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  roxygen2::roxygenize(file.path( swiftSrcPath, pathRelativeToSwift), load = "source") # https://jira.csiro.au/browse/WIRADA-592
}

#' roxygenize swift R package
#' 
#' roxygenize swift R package
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import roxygen2
#' @export
roxy_swiftr <- function(swiftSrcPath='') {
  roxy_pkg( swiftSrcPath, 'bindings/R/pkgs/swift')
}

#' roxygenize ncSwift R package
#' 
#' roxygenize ncSwift R package
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param pathRelativeToSwift Location of the package to roxygenize
#' @import roxygen2
#' @export
roxy_ncSwift <- function(swiftSrcPath='', pathRelativeToSwift='../netcdf-tools/R/pkgs/ncSwift') {
  roxy_pkg(swiftSrcPath, pathRelativeToSwift)
}

#' roxygenize calibragem R package
#' 
#' roxygenize calibragem R package
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import roxygen2
#' @export
roxy_calibragem <- function(swiftSrcPath='') {
  roxy_pkg( swiftSrcPath, 'bindings/R/pkgs/calibragem')
}

#' roxygenize swiftdev
#' 
#' roxygenize swiftdev
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import roxygen2
#' @export
roxy_swiftdev <- function(swiftSrcPath='') {
  roxy_pkg( swiftSrcPath, 'bindings/R/pkgs/swiftdev')
}

#' roxygenize joki
#' 
#' roxygenize joki
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import roxygen2
#' @export
roxy_joki <- function(swiftSrcPath='') {
  roxy_pkg( swiftSrcPath, 'bindings/R/pkgs/joki')
}

#' roxygenize rpp
#' 
#' roxygenize rpp
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param pathRelativeToSwift Location of the package to roxygenize
#' @import roxygen2
#' @export
roxy_rpp <- function(swiftSrcPath='', pathRelativeToSwift='../rpp-cpp/bindings/R/pkgs/rpp') {
  roxy_pkg(swiftSrcPath, pathRelativeToSwift)
}

#' roxygenize qpp
#' 
#' roxygenize qpp
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param pathRelativeToSwift Location of the package to roxygenize
#' @import roxygen2
#' @export
roxy_qpp <- function(swiftSrcPath='', pathRelativeToSwift='../qpp/bindings/R/pkgs/qpp') {
  roxy_pkg(swiftSrcPath, pathRelativeToSwift)
}

#' roxygenize uchronia
#' 
#' roxygenize uchronia
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param pathRelativeToSwift Location of the package to roxygenize
#' @import roxygen2
#' @export
roxy_uchronia <- function(swiftSrcPath='', pathRelativeToSwift='../datatypes/bindings/R/pkgs/uchronia') {
  roxy_pkg(swiftSrcPath, pathRelativeToSwift)
}
