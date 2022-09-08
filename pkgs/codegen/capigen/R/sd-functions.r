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
  capihelp::fcopy(dirPath, pattern, toDir, overwrite)
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
#' @import capihelp
load_wrapper_gen_lib <- function(wgenDir='') {
  if(wgenDir=='') wgenDir <- file.path(find_env_github_root_path(), 'c-api-wrapper-generation')
  wgenDir <- check_dir_exists(wgenDir)
  wgenDll <- file.path(wgenDir, 'ApiWrapperGenerator/bin/Debug/net6.0/ApiWrapperGenerator.dll')
  if(!file.exists(wgenDll)) 
  {
    msg <- (paste(wgenDll, 'not found - you probably need to compile the C# project\n'))
    msg <- (paste(msg,'In a DOS command prompt try something like:\n'))
    msg <- (paste(msg,'   cd F:\\src\\csiro\\stash\\per202\\swift\n'))
    msg <- (paste(msg,'   .\\Externals\\config-utils\\msvs\\setup_vcpp.cmd\n'))
    msg <- (paste(msg,'   cd F:\\src\\csiro\\stash\\per202\\swift\\Externals\\c-api-wrapper-generation\\ApiWrapperGenerator\n'))
    msg <- (paste(msg,'   msbuild ApiWrapperGenerator.csproj /p:Configuration=Debug /p:Platform=AnyCPU\n'))
    stop(msg)
  }
  rClr::clrLoadAssembly(file.path(wgenDll))
  invisible(NULL)
}
