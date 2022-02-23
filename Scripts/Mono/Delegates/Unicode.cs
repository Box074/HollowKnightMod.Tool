
namespace HKTool.Mono;

#region StdCall
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<T>();
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<TA0, T>(TA0 arg0);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<TA0, TA1, T>(TA0 arg0, TA1 arg1);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<TA0, TA1, TA2, T>(TA0 arg0, TA1 arg1, TA2 arg2);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<TA0, TA1, TA2, TA3, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate T StdCallUnicodeFunc<TA0, TA1, TA2, TA3, TA4, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);

[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction();
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<T>(T arg);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<TA0, T>(TA0 arg0, T arg1);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<TA0, TA1, T>(TA0 arg0, TA1 arg1, T arg2);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<TA0, TA1, TA2, T>(TA0 arg0, TA1 arg1, TA2 arg2, T arg3);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<TA0, TA1, TA2, TA3, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, T arg4);
[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
public delegate void StdCallUnicodeAction<TA0, TA1, TA2, TA3, TA4, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, T arg5);
#endregion
#region Cdecl
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<T>();
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<TA0, T>(TA0 arg0);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<TA0, TA1, T>(TA0 arg0, TA1 arg1);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<TA0, TA1, TA2, T>(TA0 arg0, TA1 arg1, TA2 arg2);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<TA0, TA1, TA2, TA3, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate T CdeclUnicodeFunc<TA0, TA1, TA2, TA3, TA4, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);

[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction();
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<T>(T arg);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<TA0, T>(TA0 arg0, T arg1);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<TA0, TA1, T>(TA0 arg0, TA1 arg1, T arg2);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<TA0, TA1, TA2, T>(TA0 arg0, TA1 arg1, TA2 arg2, T arg3);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<TA0, TA1, TA2, TA3, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, T arg4);
[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public delegate void CdeclUnicodeAction<TA0, TA1, TA2, TA3, TA4, T>(TA0 arg0, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, T arg5);
#endregion
