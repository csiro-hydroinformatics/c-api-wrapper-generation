# library(devtools) ; load_all('F:/src/csiro/stash/per202/swift/R/pkgs/swiftdev')
# document('F:/src/csiro/stash/per202/swift/R/pkgs/swiftdev')
# build('F:/src/csiro/stash/per202/swift/R/pkgs/swiftdev')
# install('F:/src/csiro/stash/per202/swift/R/pkgs/swiftdev')



#' Extract the R code from R Markdown vignettes
#' 
#' Extract the R code from R Markdown vignettes
#' 
#' @param pkgName package name
#' @export
purlPkgVignettes <- function(pkgName='swift') {
  cwd <- getwd()
  thisPkgDir <- file.path(find_env_SwiftSrcPath(), 'bindings/R/pkgs', pkgName)
  if(file.exists(thisPkgDir))
  {
    setwd(file.path(thisPkgDir, 'vignettes'))
    purlVignettes('.','..')
    setwd(cwd)
  }
}

#' Extract the R code from R Markdown vignettes
#' 
#' Extract the R code from R Markdown vignettes
#' 
#' @param outDir the output directory
#' @param pkgDir the location of the source package, where a 'vignettes' folder is expected.
#' @export
purlVignettes <- function(outDir, pkgDir = tryFindPkgDir()) {
  # TODO: refactor with rclrDevtools
  stopifnot(file.exists(pkgDir)) 
  vignDir <- file.path(pkgDir, 'vignettes')
  purlAll(outDir, inDir=vignDir, pattern='\\.Rmd$')
}

#' Extract the R code from R Markdown
#' 
#' Extract the R code from R Markdown
#' 
#' @param outDir the output directory
#' @param inDir The directory with the .Rmd files
#' @param pattern a file pattern to look for
#' @export
purlAll <- function(outDir, inDir, pattern='\\.Rmd$') {
  stopifnot(file.exists(inDir)) 
  stopifnot(file.exists(outDir)) 
  rmdFullnames <- list.files(inDir, pattern=pattern, full.names=TRUE)
  rmdFiles <- list.files(inDir, pattern=pattern, full.names=FALSE)
  rFiles <- stringr::str_replace(rmdFiles, pattern=pattern, '\\.r')
  rFullnames <- file.path(outDir, rFiles)
  for (j in 1:length(rmdFiles)) {
    purl(input=rmdFullnames[j], output = rFullnames[j], text = NULL, quiet = FALSE)
  }
}

fcopy <- function(dirPath, pattern, toDir, overwrite= TRUE) {
file.copy(
  from=list.files(dirPath, pattern=pattern, full.names=TRUE), 
  to= toDir,
  overwrite= overwrite,
  recursive= FALSE,
  copy.mode=FALSE)
}

#' Gets the location of the native binaries of SWIFT
#' 
#' Gets the location of the native binaries of SWIFT
#' 
#' @param srcDir root of the SWIFT codebase
#' @param sixtyFour if TRUE look for the x64 architecture, 32 bits otherwise.
#' @param buildConfig The build configuration to use; 'Debug' or 'Release'
#' @export
swiftNativeBinDir <- function(srcDir = find_env_SwiftSrcPath(), sixtyFour=TRUE, buildConfig='Debug') {
  stopifnot(file.exists(srcDir))
  slnDir <- file.path(srcDir, 'Solutions', 'SWIFT')
  binDir <- ifelse(sixtyFour, file.path(slnDir, 'x64'), slnDir)
  file.path(binDir,buildConfig)
}

#' Gets the location of the CLR binaries of SWIFT
#' 
#' Gets the location of the CLR binaries of SWIFT
#' 
#' @param srcDir root of the SWIFT codebase
#' @param buildConfig The build configuration to use; 'Debug' or 'Release'
#' @export
swiftDotnetBinDir <- function(srcDir = find_env_SwiftSrcPath(), buildConfig='Debug') {
  file.path(srcDir,'Swift.Calibration','bin',buildConfig)
}

check_path <- function(p) {
  if(!file.exists(p)) stop(paste0('path not found: ', p))
}

check_dir <- function(p) {
  if(!dir.exists(p)) stop(paste0('path not found: ', p))
}

#' Find the path specified by the env var SwiftSrcPath
#'
#' Find the path specified by the env var SwiftSrcPath
#'
#' @export
find_env_var_as_path <- function(env_var_name) {
  srcPath <- Sys.getenv(env_var_name)
  if(srcPath=='') {
    stop(paste0('environment variable ',env_var_name,' seems not defined'))
  } else if (!dir.exists(srcPath)) {
    stop(paste0('environment variable ',env_var_name,' is defined, but directory ', srcPath , ' does not exist' ))
  }
  return(srcPath);
}

#' Find the path specified by the env var SwiftSrcPath
#'
#' Find the path specified by the env var SwiftSrcPath
#'
#' @export
find_env_github_root_path <- function() {
  return(find_env_var_as_path('GithubSrcPath'))
}


#' Find the path specified by the env var SwiftSrcPath
#'
#' Find the path specified by the env var SwiftSrcPath
#'
#' @export
find_env_SwiftSrcPath <- function() {
  return(find_env_var_as_path('SwiftSrcPath'))
}

#' KLUDGE Find the src rpp path relative to the env var SwiftSrcPath
#'
#' KLUDGE Find the src rpp path relative to the env var SwiftSrcPath
#'
#' @export
find_rpp_src_path <- function() {
  srcPath <- file.path(find_env_SwiftSrcPath(), '../rpp-cpp')
  if (!dir.exists(srcPath)) {
    stop(paste0('environment variable SwiftSrcPath is defined, but directory ', srcPath , ' does not exist' ))
  }
  return(srcPath);
}

#' KLUDGE Find the src qpp path relative to the env var SwiftSrcPath
#'
#' KLUDGE Find the src qpp path relative to the env var SwiftSrcPath
#'
#' @export
find_qpp_src_path <- function() {
  srcPath <- file.path(find_env_SwiftSrcPath(), '../qpp')
  if (!dir.exists(srcPath)) {
    stop(paste0('environment variable SwiftSrcPath is defined, but directory ', srcPath , ' does not exist' ))
  }
  return(srcPath);
}

#' KLUDGE Find the src uchronia path relative to the env var SwiftSrcPath
#'
#' KLUDGE Find the src uchronia path relative to the env var SwiftSrcPath
#'
#' @export
find_uchronia_src_path <- function() {
  srcPath <- file.path(find_env_SwiftSrcPath(), '../datatypes')
  if (!dir.exists(srcPath)) {
    stop(paste0('environment variable SwiftSrcPath is defined, but directory ', srcPath , ' does not exist' ))
  }
  return(srcPath);
}

#' Load the .NET assembly ApiWrapperGenerator
#'
#' Load the .NET assembly ApiWrapperGenerator
#'
#' @param wgenDir the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used to extrapolate the expected location
#' @export
load_wrapper_gen_lib <- function(wgenDir='') {
  if(wgenDir=='') wgenDir <- file.path(find_env_github_root_path(), 'rcpp-wrapper-generation')
  check_dir(wgenDir)
  wgenDll <- file.path(wgenDir, 'ApiWrapperGenerator/bin/Debug/netstandard2.0/ApiWrapperGenerator.dll')
  if(!file.exists(wgenDll)) 
  {
    msg <- (paste(wgenDll, 'not found - you probably need to compile the C# project\n'))
    msg <- (paste(msg,'In a DOS command prompt try something like:\n'))
    msg <- (paste(msg,'   cd F:\\src\\csiro\\stash\\per202\\swift\n'))
    msg <- (paste(msg,'   .\\Externals\\config-utils\\msvs\\setup_vcpp.cmd\n'))
    msg <- (paste(msg,'   cd F:\\src\\csiro\\stash\\per202\\swift\\Externals\\rcpp-wrapper-generation\\ApiWrapperGenerator\n'))
    msg <- (paste(msg,'   msbuild ApiWrapperGenerator.csproj /p:Configuration=Debug /p:Platform=AnyCPU\n'))
    stop(msg)
  }
  rClr::clrLoadAssembly(file.path(wgenDll))
  invisible(NULL)
}


check_file_exists <- function(filename) {
  if (!file.exists(filename)) {
    stop(paste0('file expected and not found: ', filename ))
  }
  return(filename)
}
  
check_dir_exists <- function(dname) {
  if (!dir.exists(dname)) {
    stop(paste0('directory expected and not found: ', dname ))
  }
  return(dname)
}
  
#' @export
extern_c_api_header_file_swiftr <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  check_file_exists(file.path( swiftSrcPath, 'libswift/include/swift/extern_c_api.h'))
}
  
#' @export
outfile_xptr_wrappers_swiftr <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  (file.path( pkg_path_swiftr(swiftSrcPath), 'R/swift-wrap-generated.r'))
}

#' @export
outfile_py_cffi_wrappers_swiftr <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  stop("Not yet implemented")
}

#' @export
outfolder_matlab_wrappers_swiftr <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  (file.path( swiftSrcPath, 'bindings/matlab/tests/swift-matlab-generated'))
}
    
#' @export
pkg_path_swiftr <- function(swiftSrcPath='') {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()
  check_dir_exists(file.path( swiftSrcPath, 'bindings/R/pkgs/swift'))
}

#' @export
extern_c_api_header_file_rppr <- function(rppSrcPath='') {
  if(rppSrcPath=='') rppSrcPath <- find_rpp_src_path()
  check_file_exists(file.path( rppSrcPath, 'include/rpp/rpp_c_interop.h'))
}
  
#' @export
extern_c_api_header_file_qppr <- function(qppSrcPath='') {
  if(qppSrcPath=='') qppSrcPath <- find_qpp_src_path()
  check_file_exists(file.path( qppSrcPath, 'libqpp/include/qpp/qpp_extern_c_api.h'))
}
  
#' @export
outfile_xptr_wrappers_rppr <- function(rppSrcPath='') {
  if(rppSrcPath=='') rppSrcPath <- find_rpp_src_path()
  (file.path( pkg_path_rppr(rppSrcPath), 'R/rpp-wrap-generated.r'))
}

#' @export
outfile_xptr_wrappers_qppr <- function(qppSrcPath='') {
  if(qppSrcPath=='') qppSrcPath <- find_qpp_src_path()
  (file.path( pkg_path_qppr(qppSrcPath), 'R/qpp-wrap-generated.r'))
}

#' @export
outfile_py_cffi_wrappers_rppr <- function(swiftSrcPath='') {
  if(rppSrcPath=='') rppSrcPath <- find_rpp_src_path()
  stop("Not yet implemented")
}
 
#' @export
pkg_path_rppr <- function(rppSrcPath='') {
  if(rppSrcPath=='') rppSrcPath <- find_rpp_src_path()
  check_dir_exists(file.path( rppSrcPath, 'bindings/R/pkgs/rpp'))
}
  
#' @export
pkg_path_qppr <- function(qppSrcPath='') {
  if(qppSrcPath=='') qppSrcPath <- find_qpp_src_path()
  check_dir_exists(file.path( qppSrcPath, 'bindings/R/pkgs/qpp'))
}
  
#' @export
extern_c_api_header_file_uchronia_r <- function(uchroniaSrcPath='') {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  check_file_exists(file.path( uchroniaSrcPath, 'include/datatypes/extern_c_api.h'))
}
  
#' @export
outfile_xptr_wrappers_uchronia_r <- function(uchroniaSrcPath='') {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  (file.path( pkg_path_uchronia_r(uchroniaSrcPath), 'R/uchronia-wrap-generated.r'))
}

#' @export
outfile_py_cffi_wrappers_uchronia <- function(swiftSrcPath='') {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  (file.path( pkg_path_uchronia_py(uchroniaSrcPath), 'uchronia/wrap/uchronia_wrap_generated.py'))
}
   
#' @export
pkg_path_uchronia_r <- function(uchroniaSrcPath='') {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  check_dir_exists(file.path( uchroniaSrcPath, 'bindings/R/pkgs/uchronia'))
}
  
#' @export
pkg_path_uchronia_py <- function(uchroniaSrcPath='') {
  if(uchroniaSrcPath=='') uchroniaSrcPath <- find_uchronia_src_path()
  check_dir_exists(file.path( uchroniaSrcPath, 'bindings/python/uchronia'))
}
  

copy_header_file <- function(infile, outfile, overwrite=TRUE) {
  if(!file.exists(infile)) stop(paste0(infile, ' not found'))
  tgt_dir <- dirname(outfile)
  if(!dir.exists(tgt_dir)) dir.create(tgt_dir, recursive=TRUE)
  file.copy(from=infile, to=outfile, overwrite=overwrite)
}



#' Apply stringr::str_replace_all to all lines of a file
#' 
#' Apply stringr::str_replace_all to all lines of a file
#' 
#' @param filename    the file to parse and modify
#' @param pattern     pattern
#' @param replacement replacement 
#' @import stringr
#' @export
str_replace_file <- function(filename, pattern, replacement) {
  stopifnot(file.exists(filename))
  rcppexp <- readLines(filename)
  blah <- stringr::str_replace_all(rcppexp, pattern, replacement)
  writeLines(blah, con=filename)
}

#' @export
custom_install_shlib <- function(files, srclibname='swift_r', shlib_ext, r_arch, r_package_dir, windows, group.writable=FALSE) {
  msvs:::custom_install_shlib(files=files, srclibname=srclibname, shlib_ext=shlib_ext, r_arch=r_arch, r_package_dir=r_package_dir, windows=windows, group.writable=group.writable)
}