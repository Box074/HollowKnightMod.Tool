
namespace System;

public delegate void Action<T, T1, T2, T3, T4, T5, T6, T7, T8>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9);
public delegate void Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10);
public delegate void Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11);
public delegate void Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11, T11 arg12);
public delegate void Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11, T11 arg12, T12 arg13);


public delegate TR Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TR>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9);
public delegate TR Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10);
public delegate TR Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11);
public delegate TR Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TR>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11, T11 arg12);
public delegate TR Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TR>(T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11, T11 arg12, T12 arg13);