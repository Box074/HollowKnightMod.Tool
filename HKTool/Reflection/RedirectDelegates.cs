﻿
namespace HKTool.Reflection;
public delegate object RD_CallMethod(object? @this, object?[] args, Type[]? genTypes);
public delegate object RD_GetField(object? @this);
public delegate void RD_SetField(object? @this, object? val);
public delegate object RD_CreateInstance(object[] args);
public delegate IntPtr RT_GetFieldPtr(object? inst);
