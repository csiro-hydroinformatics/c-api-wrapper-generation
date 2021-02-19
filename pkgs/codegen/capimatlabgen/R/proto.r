
#' Extract C API functions from a header file
#'
#' Extract C API functions from a header file
#'
#' @param header_file location of the C API header file
#' @param api_marker string that, if found first in a line, identifies the line as a function to wrap 
#' @return character vector, the API functions
#' @export
get_api_functions <- function(header_file, api_marker='SWIFT_API')
{
    api_hdf <- readLines(header_file)
    api_hdf <- stringr::str_trim(api_hdf)
    api_fcts <- stringr::str_subset(api_hdf, pattern=paste0('^', api_marker))
    api_fcts <- stringr::str_replace_all(api_fcts, pattern=api_marker, replacement='')
    # remove spaces before pointer's '*'
    api_fcts <- make_uniform_ptr(api_fcts)
    api_fcts <- make_uniform_opaque_ptr(api_fcts)
    return(api_fcts)
}

#' @export
make_uniform_ptr <- function(api_fcts)
{
    # remove spaces before pointer's '*'
    api_fcts <- stringr::str_replace_all(api_fcts, pattern=' *\\*', replacement='*')
    return(api_fcts)
}

#' @export
make_uniform_opaque_ptr <- function(api_fcts, m4macro_pattern='[A-Z_]+ ', non_opaque_ptrs_patterns=character(0))
{
    # Replace things such as TIME_SERIES_PTR with void*
    api_fcts <- stringr::str_replace_all(api_fcts, pattern=m4macro_pattern, replacement='void* ')

    # Replace things such as OptimizerLogData* with void*    
    for(p in non_opaque_ptrs_patterns)
    {
        api_fcts <- stringr::str_replace_all(api_fcts, pattern=p, replacement='void*')
    }
    api_fcts <- stringr::str_trim(api_fcts)
    return(api_fcts)
}

#' @export
to_opaque_pointers <- function(api_fcts, m4macro_pattern='[A-Z_]+ ', non_opaque_ptrs_patterns=character(0))
{
    api_fcts <- make_uniform_ptr(api_fcts)
    api_fcts <- make_uniform_opaque_ptr(api_fcts, m4macro_pattern, non_opaque_ptrs_patterns)
    return(api_fcts)
}

#' @export
write_simple<-function(what, short_fn, directory) {
    writeLines(
        c(
            '// ******* THIS FILE IS GENERATED *********',
            '',
            what
        ),
        file.path(directory, short_fn))
}

#' @export
subset_structdef <- function(x) {
    m <- which(stringr::str_detect(x, "typedef"))
    stopifnot(length(m) > 0)
    x[(m[1]):(length(x))]
}
