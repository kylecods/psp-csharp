cmake_minimum_required(VERSION 3.20)
project(pspdna C)

# Invoke with: cmake -DCMAKE_TOOLCHAIN_FILE=$PSPDEV/psp/share/pspdev.cmake ..

set(SOURCES
    # Core DNA runtime
    CLIFile.c
    Delegate.c
    Finalizer.c
    Generics.c
    Heap.c
    InternalCall.c
    JIT.c
    JIT_Execute.c
    MetaData.c
    MetaData_Fill.c
    MetaData_Search.c
    MethodState.c
    RVA.c
    Sys.c
    Thread.c
    Type.c

    # System type bindings
    System.Array.c
    System.Char.c
    System.Console.c
    System.DateTime.c
    System.Diagnostics.Debugger.c
    System.Environment.c
    System.Enum.c
    System.GC.c
    System.IO.FileInternal.c
    System.Math.c
    System.Net.Dns.c
    System.Net.Sockets.Socket.c
    System.Object.c
    System.Runtime.CompilerServices.RuntimeHelpers.c
    System.RuntimeType.c
    System.String.c
    System.Type.c
    System.Threading.Interlocked.c
    System.Threading.Monitor.c
    System.Threading.Thread.c
    System.ValueType.c
    System.WeakReference.c

    # PSP hardware bindings
    controls.c
    callback.c
    graph.c
    Psp.Debug.c
    Psp.Controls.c
    Psp.Display.c
    Psp.Kernel.c
    Psp.BasicGraphics.c
    Psp.BasicGraphics2.c
    # Psp.Audio.c
    # Psp.Gu.c
    # Psp.Gum.c

    # Entry point
    dna.c

    # Stubs for symbols pulled in by libSDL2/libfreetype but never called
    # at runtime (EGL/OpenGL ES path, POSIX symbols missing from PSP newlib)
    psp_stubs.c
)

add_executable(${PROJECT_NAME} ${SOURCES})


target_compile_options(${PROJECT_NAME} PRIVATE -O2 -G0 -Wall -ffast-math)
target_compile_definitions(${PROJECT_NAME} PRIVATE __PSP__)

target_include_directories(${PROJECT_NAME} PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}
    $ENV{PSPSDK}/include
    $ENV{PSPDEV}/psp/include
)

target_link_libraries(${PROJECT_NAME}
    SDL2 SDL2_ttf harfbuzz freetype png bz2 z
    pspvram psphprm
    pspgu pspgum pspge pspdisplay pspctrl
    pspaudiolib pspaudio
    pspuser pspsdk pspdebug psputility
    m c
)

create_pbp_file(
    TARGET ${PROJECT_NAME}
    ICON_PATH NULL
    BACKGROUND_PATH NULL
    PREVIEW_PATH NULL
    TITLE ${PROJECT_NAME}
    VERSION 01.00
)

# # ── Managed assemblies (C# → corlib.dll + Dna.AppMenu.exe) ───────────────────
# # Requires the standalone Roslyn 'csc' tool on PATH:
# #   dotnet tool install --global Microsoft.Net.Sdk.Compilers
# #
# # dna.c looks for "Dna.AppMenu.exe" on the filesystem at startup, so
# # Cube3D is compiled under that name so the runtime picks it up automatically.

# find_program(CSC csc)

# if(CSC)
#     set(CSC_FLAGS -nostdlib -unsafe -optimize+ -langversion:7.3 -warn:3 -noconfig -nologo)

#     # Collect corlib sources, excluding MSBuild intermediates
#     file(GLOB_RECURSE CORLIB_SOURCES
#         ${CMAKE_CURRENT_SOURCE_DIR}/../corlib/*.cs
#     )
#     list(FILTER CORLIB_SOURCES EXCLUDE REGEX ".*/obj/.*")
#     list(FILTER CORLIB_SOURCES EXCLUDE REGEX ".*/bin/.*")

#     set(CORLIB_OUT    ${CMAKE_CURRENT_BINARY_DIR}/corlib.dll)
#     set(CUBE3D_OUT    ${CMAKE_CURRENT_BINARY_DIR}/Dna.AppMenu.exe)
#     set(CUBE3D_SRC    ${CMAKE_CURRENT_SOURCE_DIR}/../samples/Cube3D/Program.cs)

#     add_custom_command(
#         OUTPUT  ${CORLIB_OUT}
#         COMMAND ${CSC} ${CSC_FLAGS} -target:library -out:${CORLIB_OUT} ${CORLIB_SOURCES}
#         DEPENDS ${CORLIB_SOURCES}
#         COMMENT "Compiling corlib.dll (${CMAKE_CURRENT_SOURCE_DIR}/../corlib)"
#     )

#     add_custom_command(
#         OUTPUT  ${CUBE3D_OUT}
#         COMMAND ${CSC} ${CSC_FLAGS} -target:exe -r:${CORLIB_OUT}
#                 -out:${CUBE3D_OUT} ${CUBE3D_SRC}
#         DEPENDS ${CUBE3D_SRC} ${CORLIB_OUT}
#         COMMENT "Compiling Cube3D → Dna.AppMenu.exe"
#     )

#     add_custom_target(managed ALL DEPENDS ${CORLIB_OUT} ${CUBE3D_OUT})
#     message(STATUS "csc found at ${CSC} — managed assemblies will be built")
# else()
#     message(WARNING "csc not found — skipping managed build.\n"
#         "Install with: dotnet tool install --global Microsoft.Net.Sdk.Compilers")
# endif()
