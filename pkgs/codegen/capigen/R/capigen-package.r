## update the documentation with:
# library(roxygen2) ; roxygenize('C:/src/csiro/stash/c-api-bindings/codegen/capigen')

#' A package to facilitate code generation to surface a C API
#' 
#' \tabular{ll}{
#' Package: \tab capigen\cr
#' Type: \tab Package\cr
#' Version: \tab 0.6.5 \cr
#' Date: \tab 2020-01-25 \cr
#' Release Notes: \tab Add codegen for swift multisite/multiobjective new API entry points \cr
#' License: \tab GPL 2\cr
#' }
#'
#' \tabular{lll}{
#' Version \tab Date \tab Notes \cr
#' 0.6.4 \tab 2019-10-12 \tab Work around a breaking change in roxygen2 - https://jira.csiro.au/browse/WIRADA-592. \cr
#' 0.6.3 \tab 2019-01-10 \tab Changes to support exploration of python bindings generation. \cr
#' 0.6.2 \tab 2019-01-02 \tab Start to adjust py bindings generation to use the refcount package. \cr
#' 0.6.1 \tab 2018-02-27 \tab Fix: codegen was catering for references, which is not a C "thing" and should not be in C APIs.\cr
#' }
#'
#' @import knitr
#' @import roxygen2
#'
#' @name capigen-package
#' @docType package
#' @title A package to facilitate code generation to surface a C API
#' @author Jean-Michel Perraud \email{jean-michel.perraud_at_csiro.au}
#' @keywords Matlab
NULL



