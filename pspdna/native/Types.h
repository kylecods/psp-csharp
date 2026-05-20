/* native/Types.h
 * DNA internal call type aliases.
 *
 * DNA's evaluation stack operates on 32-bit (I32/U32/F32) and 64-bit
 * (I64/U64) slots.  All managed types — bool, byte, short, int, enum —
 * are widened to 32 bits when passed to internal calls.
 *
 * HEAP_PTR is a 32-bit offset into the DNA heap, used for all managed
 * object references (class instances, arrays, strings).
 *
 * PTR is a raw 32-bit machine address used for unsafe void* parameters.
 *
 * Internal call functions receive arguments in C declaration order,
 * exactly as the CLR would push them onto the evaluation stack.
 * Static methods receive no implicit "this" parameter.
 * Instance methods receive HEAP_PTR pThis_ as the first argument.
 *
 * Return values follow standard C calling convention:
 *   void  → no return
 *   bool  → U8  (0 = false, non-zero = true)
 *   int   → I32
 *   uint  → U32
 *   float → F32
 */

#pragma once

#include <stdint.h>

typedef int8_t   I8;
typedef uint8_t  U8;
typedef int16_t  I16;
typedef uint16_t U16;
typedef int32_t  I32;
typedef uint32_t U32;
typedef int64_t  I64;
typedef uint64_t U64;
typedef float    F32;
typedef double   F64;

/* Managed object pointer — offset into the DNA GC heap */
typedef U32 HEAP_PTR;

/* Raw machine address — used for unsafe void* parameters */
typedef U32 PTR;

/* NULL pointer constant for managed objects */
#define HEAP_NULL ((HEAP_PTR)0)
