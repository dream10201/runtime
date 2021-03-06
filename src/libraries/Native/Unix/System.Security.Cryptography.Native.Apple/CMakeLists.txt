project(System.Security.Cryptography.Native.Apple C)

include(${CMAKE_CURRENT_LIST_DIR}/extra_libs.cmake)

set(NATIVE_LIBS_EXTRA)
append_extra_cryptography_apple_libs(NATIVE_LIBS_EXTRA)

set(NATIVECRYPTO_SOURCES
    pal_digest.c
    pal_ecc.c
    pal_hmac.c
    pal_keyagree.c
    pal_keychain.c
    pal_random.c
    pal_rsa.c
    pal_sec.c
    pal_seckey.c
    pal_signverify.c
    pal_ssl.c
    pal_symmetric.c
    pal_trust.c
    pal_x509.c
    pal_x509chain.c
)

if (CLR_CMAKE_TARGET_IOS)
    add_definitions(-DTARGET_IOS)
endif()

if (CLR_CMAKE_TARGET_TVOS)
    add_definitions(-DTARGET_TVOS)
endif()

if (GEN_SHARED_LIB)
    add_library(System.Security.Cryptography.Native.Apple
        SHARED
        ${NATIVECRYPTO_SOURCES}
        ${VERSION_FILE_PATH}
    )
endif()

if (NOT GEN_SHARED_LIB AND NOT CLR_CMAKE_TARGET_IOS AND NOT CLR_CMAKE_TARGET_TVOS AND NOT CLR_CMAKE_TARGET_ANDROID AND NOT CLR_CMAKE_TARGET_BROWSER)
    set(NATIVECRYPTO_SOURCES ${NATIVECRYPTO_SOURCES} entrypoints.c)
endif()

add_library(System.Security.Cryptography.Native.Apple-Static
    STATIC
    ${NATIVECRYPTO_SOURCES}
)

set_target_properties(System.Security.Cryptography.Native.Apple-Static PROPERTIES OUTPUT_NAME System.Security.Cryptography.Native.Apple CLEAN_DIRECT_OUTPUT 1)

if (GEN_SHARED_LIB)
    target_link_libraries(System.Security.Cryptography.Native.Apple
        ${NATIVE_LIBS_EXTRA}
    )
endif()

if (GEN_SHARED_LIB)
    install_with_stripped_symbols (System.Security.Cryptography.Native.Apple PROGRAMS .)
endif()

install (TARGETS System.Security.Cryptography.Native.Apple-Static DESTINATION ${STATIC_LIB_DESTINATION})
