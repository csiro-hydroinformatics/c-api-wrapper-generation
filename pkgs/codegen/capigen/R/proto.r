
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
  capihelp::get_api_functions(header_file, api_marker)
}

#' @export
make_uniform_ptr <- function(api_fcts)
{
  capihelp::make_uniform_ptr(api_fcts)
}

#' @export
make_uniform_opaque_ptr <- function(api_fcts, m4macro_pattern='[A-Z_]+ ', non_opaque_ptrs_patterns=character(0))
{
    capihelp::make_uniform_opaque_ptr(api_fcts, m4macro_pattern=, non_opaque_ptrs_patterns)
}

#' @export
to_opaque_pointers <- function(api_fcts, m4macro_pattern='[A-Z_]+ ', non_opaque_ptrs_patterns=character(0))
{
    capihelp::to_opaque_pointers(api_fcts, m4macro_pattern=, non_opaque_ptrs_patterns)
}

#' @export
write_simple<-function(what, short_fn, directory) {
    capihelp::write_simple(what, short_fn, directory)
}

#' @export
subset_structdef <- function(x) {
    capihelp::subset_structdef(x)
}
