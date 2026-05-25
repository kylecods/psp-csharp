/*
 * psp_stubs.c
 *
 * Stub implementations for symbols that libSDL2.a references but that are
 * only needed when using the OpenGL ES / EGL renderer path.  The DNA runtime
 * uses SDL2's PSP GU (sceGu) renderer, so these functions are never called at
 * runtime.  They exist purely to satisfy the static linker.
 */

/* ── EGL stubs (from pspgl, not installed in standard pspdev) ────────────── */
typedef void *EGLDisplay;
typedef void *EGLSurface;
typedef void *EGLContext;
typedef void *EGLConfig;
typedef int   EGLint;
typedef unsigned int EGLenum;
typedef unsigned int EGLBoolean;

#define EGL_FALSE 0
#define EGL_TRUE  1

void   *eglGetProcAddress(const char *name)                              { (void)name; return 0; }
EGLBoolean eglTerminate(EGLDisplay dpy)                                  { (void)dpy; return EGL_FALSE; }
EGLDisplay eglGetDisplay(void *display_id)                               { (void)display_id; return 0; }
EGLint  eglGetError(void)                                                { return 0x3000; /* EGL_SUCCESS */ }
EGLBoolean eglInitialize(EGLDisplay dpy, EGLint *maj, EGLint *min)       { (void)dpy;(void)maj;(void)min; return EGL_FALSE; }
EGLBoolean eglChooseConfig(EGLDisplay dpy, const EGLint *attribs,
                            EGLConfig *configs, EGLint size, EGLint *num) { (void)dpy;(void)attribs;(void)configs;(void)size;(void)num; return EGL_FALSE; }
EGLBoolean eglGetConfigAttrib(EGLDisplay dpy, EGLConfig cfg,
                               EGLint attr, EGLint *val)                  { (void)dpy;(void)cfg;(void)attr;(void)val; return EGL_FALSE; }
EGLContext eglCreateContext(EGLDisplay dpy, EGLConfig cfg,
                             EGLContext share, const EGLint *attribs)     { (void)dpy;(void)cfg;(void)share;(void)attribs; return 0; }
EGLSurface eglCreateWindowSurface(EGLDisplay dpy, EGLConfig cfg,
                                   void *win, const EGLint *attribs)      { (void)dpy;(void)cfg;(void)win;(void)attribs; return 0; }
EGLBoolean eglMakeCurrent(EGLDisplay dpy, EGLSurface draw,
                            EGLSurface read, EGLContext ctx)              { (void)dpy;(void)draw;(void)read;(void)ctx; return EGL_FALSE; }
EGLBoolean eglSwapInterval(EGLDisplay dpy, EGLint interval)              { (void)dpy;(void)interval; return EGL_FALSE; }
EGLBoolean eglSwapBuffers(EGLDisplay dpy, EGLSurface surface)            { (void)dpy;(void)surface; return EGL_FALSE; }
EGLBoolean eglDestroyContext(EGLDisplay dpy, EGLContext ctx)             { (void)dpy;(void)ctx; return EGL_FALSE; }

/* ── POSIX stubs (newlib glob.c pulls these in transitively) ─────────────── */
int   issetugid(void)  { return 0; }
char *getlogin(void)   { return "psp"; }
