# CMakeList.txt : Top-level CMake project file, do global configuration
# and include sub-projects here.
#
cmake_minimum_required (VERSION 3.8)

project ("GameInternals")

# Include sub-projects.
#add_subdirectory ("GameInternals")

set(CMAKE_CXX_STANDARD 17)
add_compile_definitions(DLL_EXPORTS)
add_library(GameInternals SHARED GameInternals/GameInternals.cpp GameInternals/GameInternals.h)


