#' remove_rcpp_generated_files
#'
#' remove_rcpp_generated_files
#'
#' @export
remove_rcpp_generated_files <- function(pkg_path) {
  capihelp::remove_rcpp_generated_files(pkg_path)
}

#' default_rcppgen_header_prepend
#'
#' default_rcppgen_header_prepend
#'
#' @export
default_rcppgen_header_prepend <- function() {
  capihelp::default_rcppgen_header_prepend()
}

#' default_cppgen_prepend
#'
#' default_cppgen_prepend
#'
#' @export
default_cppgen_prepend <- function() {
  capihelp::default_cppgen_prepend()
}

#' default_cppgen_header_prepend
#'
#' default_cppgen_header_prepend
#'
#' @export
default_cppgen_header_prepend <- function() {
  capihelp::default_cppgen_header_prepend()
}

#' default_cppgen_prepend
#'
#' default_cppgen_prepend
#'
#' @export
default_rpp_cppgen_prepend <- function() {
  capihelp::default_rpp_cppgen_prepend()
}


#' @export
default_qpp_cppgen_prepend <- function() {
  capihelp::default_qpp_cppgen_prepend()
}

#' @export
default_uchronia_cppgen_prepend <- function() {
  capihelp::default_uchronia_cppgen_prepend()
}

common_exclude_api_functions <- function() {
  return capihelp::common_exclude_api_functions()
}

#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' @import rClr
#' @export
create_swift_api_filter <- function() {
  api_filter <- create_api_filter(export_modifier_pattern='SWIFT_API')
  rClr::clrSet(api_filter, 'ContainsNone', c(common_exclude_api_functions(), 
    'DisposeOptimizerLogDataWila', 'GetOptimizerLogDataWila',   # these use a pointer to a struct that is not opaque
    'SetItemEnsembleForecastTimeSeries', 'GetItemEnsembleForecastTimeSeries', 'DisposeMultiTimeSeriesData', # these use a pointer to a struct that is not opaque
    # these use a pointer to a struct that is not opaque
    'EvaluateScoresForParametersWila',
    'GetCatchmentStructure', 'DisposeCatchmentStructure', 'DisposeNamedValuedVectorsSwift', 'DisposeStringStringMapSwift', 'GetFeasibleMuskingumBounds', 'GetModelConfigurationSwift',
    'SortSimulationElementsByRunOrder' # needs manual wrapping for now: returns a char** but not like other names getters.
    )
  )
  return(api_filter)
}

#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' @import rClr
#' @export
create_rpp_api_filter <- function() {
  api_filter <- create_api_filter(export_modifier_pattern='RPP_API')
  rClr::clrSet(api_filter, 'ContainsNone', c(
    common_exclude_api_functions(),
    'GetScoresSerializedRpp', 'DisposeNamedValuedVectorsRpp',
    'DisposeOptimizerLogDataWila', 'GetOptimizerLogDataWila', 'GetOptimizerLogDataWilaNumericData', 'GetOptimizerLogDataWilaStringData',  # these use a pointer to a struct that is not opaque
    'AsInteropHypercubeRpp', 'SetHypercubeValuesRpp', 'DisposeInteropHypercubeRpp'
  ))
  return(api_filter)
}

#' @import rClr
#' @export
create_qpp_api_filter <- function() {
  api_filter <- create_api_filter(export_modifier_pattern='QPP_API')
  rClr::clrSet(api_filter, 'ContainsNone', c(
    common_exclude_api_functions(),
    'GetScoresSerializedRpp', 'DisposeNamedValuedVectorsRpp',
    'DisposeOptimizerLogDataWila', 'GetOptimizerLogDataWila', 'GetOptimizerLogDataWilaNumericData', 'GetOptimizerLogDataWilaStringData',  # these use a pointer to a struct that is not opaque
    'AsInteropHypercubeRpp', 'SetHypercubeValuesRpp', 'DisposeInteropHypercubeRpp'
  ))
  return(api_filter)
}

#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' Creates a .NET object that parses and matches lines in a C API header file.
#'
#' @import rClr
#' @export
create_uchronia_api_filter <- function() {
  api_filter <- create_api_filter(export_modifier_pattern='DATATYPES_API')
  rClr::clrSet(api_filter, 'ContainsNone', c('DeleteAnsiStringArray', 'DeleteAnsiString', 
    'SetItemEnsembleForecastTimeSeries', 'GetItemEnsembleForecastTimeSeries', 'DisposeMultiTimeSeriesData', # these use a pointer to a struct that is not opaque
    'GetDataDimensionsDescription', 'DisposeDataDimensionsDescriptions' # these use a pointer to a struct that is not opaque
    )
  )
  return(api_filter)
}

#' Create a skeleton wrapper code gen for R package  
#' 
#' Create a skeleton wrapper code gen for R package  
#' 
#' @param prepend_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @export
create_rcpp_generator_base <- function(prepend_header=default_rcppgen_header_prepend()) {

  rcpp_cpp_gen <- rClr::clrNew('ApiWrapperGenerator.RcppGlueWrapperGenerator')
  # clrGetProperties(rcpp_cpp_gen)
  rClr::clrSet (rcpp_cpp_gen, 'OpaquePointers', TRUE)
  rClr::clrSet (rcpp_cpp_gen, 'OpaquePointerClassName', 'opaque_pointer_handle')
  rClr::clrSet (rcpp_cpp_gen, 'CallGetMethod', '->get()')
  rClr::clrSet (rcpp_cpp_gen, 'AddRcppExport', TRUE)
  rClr::clrSet (rcpp_cpp_gen, 'DeclarationOnly', FALSE)
  rClr::clrSet (rcpp_cpp_gen, 'FunctionNamePostfix', '_Rcpp')
  rClr::clrSet (rcpp_cpp_gen, 'PrependOutputFile', prepend_header)
  
  rcpp_cpp_gen <- configure_cpp_typemap(rcpp_cpp_gen, convert_numerics=TRUE)
  set_wrapper_type_map(rcpp_cpp_gen, 'MarshaledDateTime', 'Rcpp::Datetime')
  set_wrapper_type_converter(rcpp_cpp_gen, 'MarshaledDateTime', '_datetime',
    'MarshaledDateTime C_ARGNAME = toDateTimeStruct(RCPP_ARGNAME);', 
    '// C_ARGNAME - no cleanup needed')  
  set_wrapper_type_map(rcpp_cpp_gen, 'date_time_to_second', 'Rcpp::Datetime')
  set_wrapper_type_converter(rcpp_cpp_gen, 'date_time_to_second', '_datetime', 
    'date_time_to_second C_ARGNAME = to_date_time_to_second<Rcpp::Datetime>(RCPP_ARGNAME);', 
    '// C_ARGNAME - no cleanup needed')  

# Converters for datatypes (uchronia)
  set_wrapper_type_map(rcpp_cpp_gen, 'regular_time_series_geometry*', 'const Rcpp::S4&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'regular_time_series_geometry*', '_tsgeom', 
    'regular_time_series_geometry* C_ARGNAME = cinterop::timeseries::to_regular_time_series_geometry_ptr(RCPP_ARGNAME);', 
    'delete C_ARGNAME;')

  set_wrapper_type_map(rcpp_cpp_gen, 'TS_GEOMETRY_PTR', 'const Rcpp::S4&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'TS_GEOMETRY_PTR', '_tsgeom', 
    'regular_time_series_geometry* C_ARGNAME = cinterop::timeseries::to_regular_time_series_geometry_ptr(RCPP_ARGNAME);',
    'delete C_ARGNAME;')

  set_wrapper_type_map(rcpp_cpp_gen, 'multi_statistic_definition*', 'const Rcpp::List&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'multi_statistic_definition*', '_mstatdef', 
    'multi_statistic_definition* C_ARGNAME = cinterop::statistics::to_multi_statistic_definition_ptr(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<multi_statistic_definition>(C_ARGNAME);')

  set_wrapper_type_map(rcpp_cpp_gen, 'named_values_vector*', 'const NumericVector&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'named_values_vector*', '_nvv', 
    'named_values_vector* C_ARGNAME = cinterop::utils::to_named_values_vector_ptr(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<named_values_vector>(C_ARGNAME);')

  set_wrapper_type_map(rcpp_cpp_gen, 'values_vector*', 'const NumericVector&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'values_vector*', '_vv', 
    'values_vector* C_ARGNAME = cinterop::utils::to_values_vector_ptr(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<values_vector>(C_ARGNAME);')

  set_wrapper_type_map(rcpp_cpp_gen, 'character_vector*', 'const CharacterVector&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'character_vector*', '_cv', 
    'character_vector* C_ARGNAME = cinterop::utils::to_character_vector_ptr(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<character_vector>(C_ARGNAME);')

  set_wrapper_type_map(rcpp_cpp_gen, 'string_string_map*', 'const CharacterVector&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'string_string_map*', '_dict', 
    'string_string_map* C_ARGNAME = cinterop::utils::to_string_string_map_ptr(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<string_string_map>(C_ARGNAME);')

  # I am really not sure we should have const pointer allowed. Does that not lead to memory leaks? why was this here?? 
  # Coimment out and see if still needed. 
  # Yes it is, because of datatypes SetItemEnsembleTimeSeriesAsStructure. May make sense.
  set_wrapper_type_map(rcpp_cpp_gen, 'const multi_regular_time_series_data*', 'const Rcpp::S4&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'const multi_regular_time_series_data*', '_tsd_ptr', 
    'auto C_ARGNAME_x = cinterop::timeseries::to_multi_regular_time_series_data(RCPP_ARGNAME); multi_regular_time_series_data* C_ARGNAME = &C_ARGNAME_x;',
    'cinterop::disposal::dispose_of<multi_regular_time_series_data>(C_ARGNAME_x);')

  # Note that '&' is a not valid C concept... so the following should not be used. Legacy to be replaced with ptrs
  set_wrapper_type_map(rcpp_cpp_gen, 'const multi_regular_time_series_data&', 'const Rcpp::S4&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'const multi_regular_time_series_data&', '_tsd', 
    'multi_regular_time_series_data C_ARGNAME = cinterop::timeseries::to_multi_regular_time_series_data(RCPP_ARGNAME);',
    'cinterop::disposal::dispose_of<multi_regular_time_series_data>(C_ARGNAME);')

  set_wrapper_type_map(rcpp_cpp_gen, 'const regular_time_series_geometry&', 'const Rcpp::S4&')
  set_wrapper_type_converter(rcpp_cpp_gen, 'const regular_time_series_geometry&', '_tsd', 
    'regular_time_series_geometry C_ARGNAME = cinterop::timeseries::to_regular_time_series_geometry(RCPP_ARGNAME);', 
    '// C_ARGNAME - no cleanup needed')  

  set_wrapper_type_map(rcpp_cpp_gen, 'SceParameters', 'Rcpp::NumericVector')
  set_wrapper_type_converter(rcpp_cpp_gen, 'SceParameters', '_sceparams', 'SceParameters C_ARGNAME = mhcpp::interop::r::ToSceParameters<NumericVector>(RCPP_ARGNAME);', '// C_ARGNAME - no cleanup needed')


# structures used for interop on hypercubes
  # set_wrapper_type_map(rcpp_cpp_gen, 'hypercube_parameter_set*', 'Rcpp::DataFrame')
  # set_wrapper_type_converter(rcpp_cpp_gen, 'hypercube_parameter_set*', '_hc', 'hypercube_parameter_set* C_ARGNAME = to_date_time_to_second<Rcpp::DataFrame>(RCPP_ARGNAME);', '// C_ARGNAME - no cleanup needed')  

  return(rcpp_cpp_gen)

}

#' generate_swift_rcpp_glue
#' 
#' generate C++ wrappers for the swift R package, derived from the SWIFT C API. 
#'  
#' @param ` the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @export
generate_swift_rcpp_glue <- function(swiftSrcPath='', prepend_header=default_rcppgen_header_prepend()) {
  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()

  rcpp_cpp_gen <- create_rcpp_generator_base(prepend_header)
  
  api_filter <- create_swift_api_filter()
  gen <- create_wrapper_generator(rcpp_cpp_gen, api_filter)

  swiftr_include_dir <- file.path( pkg_path_swiftr(swiftSrcPath), 'inst/include')
  if(!dir.exists(swiftr_include_dir)) stop(paste0(swiftr_include_dir, ' not found'))
  
  outfile <- file.path( pkg_path_swiftr(swiftSrcPath), 'src/rcpp_generated.cpp')

  api_header_file <- extern_c_api_header_file_swiftr(swiftSrcPath)

  generate_wrapper_code(gen, api_header_file, outfile)
}

configure_cpp_typemap <- function(converter, convert_numerics=FALSE) {
  set_wrapper_type_converter(converter, 'char**'  , '_charpp', 'char** C_ARGNAME = to_ansi_char_array(RCPP_ARGNAME);', 'free_ansi_char_array(C_ARGNAME, RCPP_ARGNAME.size());')

  if(convert_numerics) {
    # TODO REFACTOR see \swift\bindings\cpp\swift_cpp_typeconverters.cpp
    # set_wrapper_type_converter(converter, 'double**', '_doublepp', 'double** C_ARGNAME = createJaggedDoubleArray(RCPP_ARGNAME);', 'freeJaggedDoubleArray(C_ARGNAME, RCPP_ARGNAME.size());')
    # set_wrapper_type_converter(converter, 'double*', '_doublep', 'double* C_ARGNAME = createDoubleArray(RCPP_ARGNAME);', 'freeDoubleArray(C_ARGNAME, RCPP_ARGNAME.size());')
    # set_wrapper_type_converter(converter, 'double**', '_doublepp', 'double** C_ARGNAME = to_double_ptr_array(RCPP_ARGNAME);', 'free_double_ptr_array(C_ARGNAME, RCPP_ARGNAME.size());')
    # set_wrapper_type_converter(converter, 'double*', '_doublep', 'double* C_ARGNAME = createDoubleArray(RCPP_ARGNAME);', 'freeDoubleArray(C_ARGNAME, RCPP_ARGNAME.size());')
    
    # Used by SWIFT - may need revision in refactor for other needs.
    set_wrapper_type_converter(converter, 'double**', '_doublepp', 'double** C_ARGNAME = as_double_ptr_array(RCPP_ARGNAME);', 'delete[] C_ARGNAME;')
  }
  rClr::clrCall(converter, 'ClearCustomWrappers')
  converter <- add_common_custom_wrappers(converter)
  return(converter)
}

#' generate_swift_rcpp_glue
#' 
#' generate C++ wrappers for the swift R package, derived from the SWIFT C API. 
#'  
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_impl the text to prepend to the generated C++ code implementation (i.e. .cpp file), e.g. includes and helper data marshalling functions declarations.
#' @param prepend_header the text to prepend to the generated C++ code header (i.e. .h file), e.g. includes and helper data marshalling functions implementations.
#' @import rClr
#' @export
generate_swift_cpp_api <- function(swiftSrcPath='', prepend_impl = default_cppgen_prepend(), prepend_header=default_cppgen_header_prepend()) {

  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()

  cppGen <- rClr::clrNew('ApiWrapperGenerator.CppApiWrapperGenerator')
  # clrGetProperties(cppGen)
  # rClr::clrSet (cppGen, 'OpaquePointers', TRUE)
  rClr::clrSet (cppGen, 'DeclarationOnly', FALSE)
  rClr::clrSet (cppGen, 'FunctionNamePostfix', '_cpp')
  rClr::clrSet (cppGen, 'PrependOutputFile', prepend_impl)
  
  cppGen <- configure_cpp_typemap(cppGen)

  api_filter <- create_swift_api_filter()
  gen <- create_wrapper_generator(cppGen, api_filter)

  swiftr_include_dir <- file.path( pkg_path_swiftr(swiftSrcPath), 'inst/include')
  if(!dir.exists(swiftr_include_dir)) stop(paste0(swiftr_include_dir, ' not found'))
  
  out_root_dir <- file.path( swiftSrcPath, 'bindings/cpp')
  
  outfile_cpp <- file.path(out_root_dir, 'swift_cpp_api_generated.cpp')
  outfile_h <- file.path(out_root_dir, 'swift_cpp_api_generated.h')

  infile <- extern_c_api_header_file_swiftr(swiftSrcPath)
  apifile <- file.path( swiftr_include_dir, 'extern_c_api.h')
  if(!file.exists(infile)) stop(paste0(infile, ' not found'))
  
  generate_wrapper_code(gen, infile, outfile_cpp)

  rClr::clrSet (cppGen, 'DeclarationOnly', TRUE)
  rClr::clrSet (cppGen, 'PrependOutputFile', prepend_header)
  generate_wrapper_code(gen, infile, outfile_h)
}

#' generate_swift_csharp_api
#' 
#' generate C# wrappers, derived from the SWIFT C API. 
#'  
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_impl the text to prepend to the generated C# code implementation, e.g. includes and helper data marshalling functions declarations.
#' @import rClr
#' @export
generate_swift_csharp_api <- function(swiftSrcPath='', prepend_impl = default_cppgen_prepend(), prepend_header=default_cppgen_header_prepend()) {

  if(swiftSrcPath=='') swiftSrcPath <- find_env_SwiftSrcPath()

  csConv <- rClr::clrNew('ApiWrapperGenerator.CsharpApiWrapperGenerator')
  # clrGetProperties(csConv)
  # rClr::clrSet (csConv, 'OpaquePointers', TRUE)
  rClr::clrSet (csConv, 'FunctionNamePostfix', '_cs')
  rClr::clrSet (csConv, 'PrependOutputFile', prepend_impl)
  
  csConv <- configure_cpp_typemap(csConv)  
  api_filter <- create_swift_api_filter()
  gen <- create_wrapper_generator(csConv, api_filter)
  
  out_root_dir <- file.path( swiftSrcPath, 'bindings/csharp')
  
  outfile_cs <- file.path(out_root_dir, 'swift_csharp_api_generated.cs')

  infile <- extern_c_api_header_file_swiftr(swiftSrcPath)
  if(!file.exists(infile)) stop(paste0(infile, ' not found'))
  
  generate_wrapper_code(gen, infile, outfile_cs)
}

create_xptrwrap_generator <- function(prepend_header=default_xptr_wrapper_prepend()) {
  conv <- rClr::clrNew('ApiWrapperGenerator.RXptrWrapperGenerator')
  # clrGetProperties(conv)
  rClr::clrSet (conv, 'FunctionNamePostfix', '_R') 
  rClr::clrSet (conv, 'ApiCallPostfix', '_Rcpp') 
  rClr::clrSet (conv, 'GenerateRoxygenDoc', TRUE) 
  # rClr::clrSet (conv, 'RoxygenDocPostamble', "#' @export") 
  rClr::clrSet (conv, 'PrependOutputFile', prepend_header) 
  
  rClr::clrCall(conv, 'SetTransientArgConversion', '.*_PTR'  , '_xptr', 'C_ARGNAME <- cinterop::getExternalXptr(RCPP_ARGNAME)', '')  
  rClr::clrCall(conv, 'ClearCustomWrappers')
  return(conv)
}

create_matlabwrap_generator <- function(prepend_header=default_matlab_wrapper_prepend()) {
  conv <- rClr::clrNew('ApiWrapperGenerator.MatlabApiWrapperGenerator')
  # clrGetProperties(conv)
  rClr::clrSet (conv, 'FunctionNamePostfix', '_m') 
  rClr::clrSet (conv, 'ApiCallPostfix', '') 
  # rClr::clrSet (conv, 'GenerateRoxygenDoc', TRUE) 
  # rClr::clrSet (conv, 'RoxygenDocPostamble', "#' @export") 
  rClr::clrSet (conv, 'PrependOutputFile', prepend_header) 
  # rClr::clrCall(conv, 'SetTransientArgConversion', '.*_PTR'  , '_xptr', 'C_ARGNAME <- cinterop::getExternalXptr(RCPP_ARGNAME)', '')  
  rClr::clrCall(conv, 'ClearCustomWrappers')
  return(conv)
}

#' generate R code to wrap external pointers to domain objects.
#' 
#' generate R code to wrap external pointers to domain objects.
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_header the text to prepend to the generated R code.
#' @import rClr
#' @export
generate_xptr_wrappers <- function(swiftSrcPath='', prepend_header=default_xptr_wrapper_prepend(),
  infile=extern_c_api_header_file_swiftr(swiftSrcPath),
  outfile=outfile_xptr_wrappers_swiftr(swiftSrcPath),
  api_filter=create_swift_api_filter()
) {
  conv <- create_xptrwrap_generator(prepend_header);
  conv <- add_common_custom_wrappers(conv)
  gen <- create_wrapper_generator(conv, api_filter)

  generate_wrapper_code(gen, infile, outfile)
  
}

#' generate R code to wrap external pointers to domain objects.
#' 
#' generate R code to wrap external pointers to domain objects.
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_header the text to prepend to the generated R code.
#' @import rClr
#' @export
generate_matlab_wrappers <- function(swiftSrcPath='', prepend_header=default_matlab_wrapper_prepend(),
  infile=extern_c_api_header_file_swiftr(swiftSrcPath),
  outfolder=outfolder_matlab_wrappers_swiftr(swiftSrcPath),
  api_filter=create_swift_api_filter(), 
  libraryName='mylibname'
) {
  conv <- create_matlabwrap_generator(prepend_header);
  returnscharptrptr <- rClr::clrCall(conv, 'ReturnsCharPtrPtrWrapper')
  rClr::clrCall(conv, 'AddCustomWrapper', returnscharptrptr)
  rClr::clrSet(conv, 'NativeLibraryNameNoext', libraryName)

  # returnsdoubleptr <- rClr::clrCall(conv, 'ReturnsDoublePtrWrapper')
  # rClr::clrCall(conv, 'AddCustomWrapper', returnsdoubleptr)
  
  gen <- create_wrapper_generator(conv, api_filter)
  rClr::clrSet(gen, 'FileExt', "m");
  if(!dir.exists(outfolder)) dir.create(outfolder, recursive=TRUE);
  rClr::clrCall(gen, 'CreateWrapperOneFunctionPerFile', infile, outfolder);
  invisible(NULL)
}


#' generate R code to wrap external pointers to SWIFT objects.
#' 
#' generate R code to wrap external pointers to SWIFT objects.
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @param prepend_header the text to prepend to the generated R code.
#' @import rClr
#' @export
generate_xptr_wrappers_from_rcppfunc <- function(swiftSrcPath='', prepend_header=default_xptr_wrapper_prepend(),
  infile  =file.path( pkg_path_swiftr(swiftSrcPath), 'src/rcpp_swift.cpp'),
  outfile =file.path( pkg_path_swiftr(swiftSrcPath), 'R/swift-pkg-wrap-generated.r')
) {
  conv <- create_xptrwrap_generator(prepend_header);
  rClr::clrSet (conv, 'ApiCallPostfix', '') 
  # rClr::clrSet (conv, 'RoxygenDocPostamble', "#' @export") 
  rClr::clrCall(conv, 'SetTransientArgConversion', 'XPtr<OpaquePointer>', '_xptr', 'C_ARGNAME <- cinterop::getExternalXptr(RCPP_ARGNAME)', '')  
    
  api_filter <- create_rcpp_exported_func()
  gen <- create_wrapper_generator(conv, api_filter)

  generate_wrapper_code(gen, infile, outfile)
  
}

#' Perform Rcpp::compileAttributes on SWIFT R
#' 
#' Perform Rcpp::compileAttributes on SWIFT R
#' 
#' @param swiftSrcPath the root of swift src codebase. If empty string, find_env_SwiftSrcPath() is used.
#' @import Rcpp
#' @export
swiftr_compileAttributes <- function(swiftSrcPath='') {
  capihelp::swiftr_compileAttributes(swiftSrcPath)
}

#' generate_rpp_rcpp_glue
#' 
#' generate C++ wrappers for the rpp R package, derived from the SWIFT C API. 
#'  
#' @param rppSrcPath the root of rpp src codebase. 
#' @param prepend_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @export
generate_rpp_rcpp_glue <- function(rppSrcPath, prepend_header=default_rpp_cppgen_prepend()) {

  cppConv <- create_rcpp_generator_base(prepend_header)
  set_wrapper_type_map(cppConv, 'RPP_THETA_PARAMETERS', 'Rcpp::NumericVector')
  set_wrapper_type_converter(cppConv, 'RPP_THETA_PARAMETERS', '_theta', 'Theta  C_ARGNAME = toThetaParameters(RCPP_ARGNAME);', '// C_ARGNAME - no cleanup needed')

  set_wrapper_type_map(cppConv, 'RPP_VECTOR_OF_THETA_PARAMETERS_PTR', 'const Rcpp::DataFrame&')
  set_wrapper_type_converter(cppConv, 'RPP_VECTOR_OF_THETA_PARAMETERS_PTR', '_theta', 
    'RPP_VECTOR_OF_THETA_PARAMETERS_PTR C_ARGNAME = toLeadTimeBjpParameters(RCPP_ARGNAME);', 'disposeLeadTimeBjpParameters(C_ARGNAME);')
  
  api_filter <- create_rpp_api_filter()
  gen <- create_wrapper_generator(cppConv, api_filter)
  
  outfile <- file.path( pkg_path_rppr(rppSrcPath), 'src/rcpp_generated.cpp')

  # C:\src\csiro\stash\per202\rpp-cpp\include\rpp
  api_header_file <- file.path( rppSrcPath, 'include/rpp/rpp_c_interop.h')

  generate_wrapper_code(gen, api_header_file, outfile)
}

#' generate_qpp_rcpp_glue
#' 
#' generate C++ wrappers for the qpp R package, derived from the SWIFT C API. 
#'  
#' @param qppSrcPath the root of qpp src codebase. 
#' @param prepend_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @export
generate_qpp_rcpp_glue <- function(qppSrcPath, prepend_header=default_qpp_cppgen_prepend()) {

  cppConv <- create_rcpp_generator_base(prepend_header)
  set_wrapper_type_map(cppConv, 'QppMarshaledDateTime', 'Rcpp::Datetime')
  set_wrapper_type_converter(cppConv, 'QppMarshaledDateTime', '_datetime',
    'QppMarshaledDateTime C_ARGNAME = toDateTimeStruct(RCPP_ARGNAME);', 
    '// C_ARGNAME - no cleanup needed')  
  # set_wrapper_type_map(cppConv, 'RPP_THETA_PARAMETERS', 'Rcpp::NumericVector')
  # set_wrapper_type_converter(cppConv, 'RPP_THETA_PARAMETERS', '_theta', 'Theta  C_ARGNAME = toThetaParameters(RCPP_ARGNAME);', '// C_ARGNAME - no cleanup needed')

  # set_wrapper_type_map(cppConv, 'RPP_VECTOR_OF_THETA_PARAMETERS_PTR', 'const Rcpp::DataFrame&')
  # set_wrapper_type_converter(cppConv, 'RPP_VECTOR_OF_THETA_PARAMETERS_PTR', '_theta', 
  #   'RPP_VECTOR_OF_THETA_PARAMETERS_PTR C_ARGNAME = toLeadTimeBjpParameters(RCPP_ARGNAME);', 'disposeLeadTimeBjpParameters(C_ARGNAME);')
  
  api_filter <- create_qpp_api_filter()
  gen <- create_wrapper_generator(cppConv, api_filter)
  
  outfile <- file.path( pkg_path_qppr(qppSrcPath), 'src/rcpp_generated.cpp')

  # C:\src\csiro\stash\per202\qpp-cpp\include\qpp
  api_header_file <- file.path( qppSrcPath, 'libqpp/include/qpp/qpp_extern_c_api.h')

  generate_wrapper_code(gen, api_header_file, outfile)
}

#' generate_uchronia_rcpp_glue
#' 
#' generate C++ wrappers for the uchronia R package, derived from the SWIFT C API. 
#'  
#' @param uchroniaSrcPath the root of uchronia src codebase. 
#' @param prepend_header the text to prepend to the generated C++ code, e.g. includes and helper data marshalling functions.
#' @export
generate_uchronia_rcpp_glue <- function(uchroniaSrcPath, prepend_header=default_uchronia_cppgen_prepend()) {

  cppConv <- create_rcpp_generator_base(prepend_header)
  # set_wrapper_type_map(cppConv, 'RPP_THETA_PARAMETERS', 'Rcpp::NumericVector')
  # set_wrapper_type_converter(cppConv, 'RPP_THETA_PARAMETERS', '_theta', 'Theta  C_ARGNAME = toThetaParameters(RCPP_ARGNAME);', '// C_ARGNAME - no cleanup needed')
  
  api_filter <- create_uchronia_api_filter()
  gen <- create_wrapper_generator(cppConv, api_filter)
  
  outfile <- file.path( pkg_path_uchronia_r(uchroniaSrcPath), 'src/rcpp_generated.cpp')

  # C:\src\csiro\stash\per202\rpp-cpp\include\rpp
  api_header_file <- file.path( uchroniaSrcPath, 'include/datatypes/extern_c_api.h')

  generate_wrapper_code(gen, api_header_file, outfile)
}

#' @export
rppr_compileAttributes <- function(rppSrcPath='') {
  capihelp::rppr_compileAttributes(rppSrcPath)
}

#' @export
qppr_compileAttributes <- function(qppSrcPath='') {
  capihelp::qppr_compileAttributes(qppSrcPath)
}

#' @export
uchronia_r_compileAttributes <- function(uchroniaSrcPath='') {
  capihelp::uchronia_r_compileAttributes(uchroniaSrcPath)
}

generate_wrapper_code <- function(generator, infile, outfile) {
  rClr::clrCall(generator, 'CreateWrapperHeader', infile, outfile)
  invisible(NULL)
}

#' @export
apply_c_preprocessor <- function(include_dirs, api_importer_file, outfile, execute=TRUE) {
  capihelp::apply_c_preprocessor(include_dirs, api_importer_file, outfile, execute)
}

#' @export
create_cffi_cdefs <- function(preprocessed_cpp_file, outdir, pattern_start_structs, 
  extern_c_start_match='extern .C. \\{', extern_c_end_match = '^\\}') {
  capihelp::create_cffi_cdefs(preprocessed_cpp_file, outdir, pattern_start_structs, 
    extern_c_start_match, extern_c_end_match)
}

#' generate Python code to wrap external pointers to domain objects.
#' 
#' generate Python code to wrap external pointers to domain objects.
#' 
#' @param prepend_header the text to prepend to the generated R code.
#' @import rClr
#' @export
generate_py_cffi_wrappers <- function(
  prepend_header=default_py_cffi_wrapper_prepend(),
  infile=extern_c_api_header_file_swiftr(swiftSrcPath),
  outfile=outfile_py_cffi_wrappers_swiftr(swiftSrcPath),
  api_filter=create_swift_api_filter(),
  cffi_obj_name='nativelib'
) {

  # for now let's see what can be done with cffi, without filtering out:
  rClr::clrSet(api_filter, 'ContainsNone', c('#define', '#define'))

  conv <- create_py_cffi_wrap_generator(prepend_header, cffi_obj_name=cffi_obj_name);
  conv <- add_common_custom_wrappers(conv)
  gen <- create_wrapper_generator(conv, api_filter)

  generate_wrapper_code(gen, infile, outfile)
  
}

#' @export
create_py_cffi_generator_base <- function(prepend_header=default_py_cffi_wrapper_prepend(), cffi_obj_name='nativelib') {
  conv <- rClr::clrNew('ApiWrapperGenerator.PythonCffiWrapperGenerator')
  # clrGetProperties(conv)
  rClr::clrSet (conv, 'FunctionNamePostfix', '_py') 
  rClr::clrSet (conv, 'ApiCallPrefix', paste0(cffi_obj_name, ".")) 
  rClr::clrSet (conv, 'ApiCallPostfix', '') 
  rClr::clrSet (conv, 'GeneratePyDocstringDoc', TRUE) 
  # rClr::clrSet (conv, 'RoxygenDocPostamble', "#' @export") 
  rClr::clrSet (conv, 'PrependOutputFile', prepend_header) 

  rClr::clrSet (conv, 'CreateXptrObjRefFunction', 'custom_wrap_cffi_native_handle')
  rClr::clrSet (conv, 'GetXptrFromObjRefFunction', 'unwrap_cffi_native_handle')


  # HACK
  prepend_header_custom <- paste0(prepend_header, '
def custom_wrap_cffi_native_handle(obj, type_id="", release_native = None):
    if release_native is None:
        release_native = DisposeSharedPointer_py
    return wrap_cffi_native_handle(obj, type_id, release_native)

'
  )
  rClr::clrSet (conv, 'PrependOutputFile', prepend_header_custom) 
  
  rClr::clrCall(conv, 'SetTransientArgConversion', '.*_PTR'  , '_xptr', 'C_ARGNAME = unwrap_cffi_native_handle(RCPP_ARGNAME)', '')  
  rClr::clrCall(conv, 'ClearCustomWrappers')

  set_wrapper_type_map(conv, 'MarshaledDateTime', 'Rcpp::Datetime')
  set_wrapper_type_converter(conv, 'MarshaledDateTime', '_datetime',
    'C_ARGNAME = to_date_time_to_second(RCPP_ARGNAME)', 
    '# C_ARGNAME - no cleanup needed')  
  set_wrapper_type_map(conv, 'date_time_to_second', 'Rcpp::Datetime')
  set_wrapper_type_converter(conv, 'date_time_to_second', '_datetime', 
    'C_ARGNAME = to_date_time_to_second(RCPP_ARGNAME)', 
    '# C_ARGNAME - no cleanup needed')  

# Converters for datatypes (uchronia)
  set_wrapper_type_map(conv, 'regular_time_series_geometry*', 'const Rcpp::S4&')
  set_wrapper_type_converter(conv, 'regular_time_series_geometry*', '_tsgeom', 
    'C_ARGNAME = cinterop.timeseries.to_regular_time_series_geometry_ptr(RCPP_ARGNAME)', 
    '# delete C_ARGNAME')

  set_wrapper_type_map(conv, 'TS_GEOMETRY_PTR', 'const Rcpp::S4&')
  set_wrapper_type_converter(conv, 'TS_GEOMETRY_PTR', '_tsgeom', 
    'C_ARGNAME = cinterop.timeseries.to_regular_time_series_geometry_ptr(RCPP_ARGNAME)',
    '# delete C_ARGNAME')


  set_wrapper_type_map(conv, 'const multi_regular_time_series_data*', 'const Rcpp::S4&')
  set_wrapper_type_converter(conv, 'const multi_regular_time_series_data*', '_tsd_ptr', 
    'C_ARGNAME = cinterop.timeseries.to_multi_regular_time_series_data(RCPP_ARGNAME)',
    '# cinterop::disposal::dispose_of<multi_regular_time_series_data>(C_ARGNAME_x)')

  # Note that '&' is a not valid C concept... so the following should not be used. Legacy to be replaced with ptrs
  set_wrapper_type_map(conv, 'const multi_regular_time_series_data&', 'const Rcpp::S4&')
  set_wrapper_type_converter(conv, 'const multi_regular_time_series_data&', '_tsd', 
    'C_ARGNAME = cinterop.timeseries.to_multi_regular_time_series_data(RCPP_ARGNAME)',
    '# cinterop::disposal::dispose_of<multi_regular_time_series_data>(C_ARGNAME)')

  set_wrapper_type_map(conv, 'const regular_time_series_geometry&', 'const Rcpp::S4&')
  set_wrapper_type_converter(conv, 'const regular_time_series_geometry&', '_tsd', 
    'C_ARGNAME = cinterop.timeseries.to_regular_time_series_geometry(RCPP_ARGNAME)', 
    '# C_ARGNAME - no cleanup needed')  

  set_wrapper_type_map(conv, 'SceParameters', 'Rcpp::NumericVector')
  set_wrapper_type_converter(conv, 'SceParameters', '_sceparams', 'SceParameters C_ARGNAME = mhcpp::interop::r::ToSceParameters<NumericVector>(RCPP_ARGNAME)', '// C_ARGNAME - no cleanup needed')

  return(conv)
}


#' @export
create_py_cffi_wrap_generator <- function(prepend_header=default_py_cffi_wrapper_prepend(), cffi_obj_name='nativelib') {
  conv <- create_py_cffi_generator_base(prepend_header, cffi_obj_name)
  return(conv)
}

