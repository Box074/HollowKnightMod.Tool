global using System;
global using System.IO;
global using System.IO.Compression;
global using System.Collections;
global using System.Diagnostics;
global using System.Collections.Generic;
global using System.Reflection;
global using System.Reflection.Emit;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Linq;
global using UnityEngine;
global using UnityEngine.UI;
global using UnityEngine.Audio;
global using UnityEngine.SceneManagement;
global using Unity.Collections;
global using Unity.Collections.LowLevel;
global using Unity.Collections.LowLevel.Unsafe;
global using Debug = UnityEngine.Debug;
global using USceneManager = UnityEngine.SceneManagement.SceneManager;

global using Newtonsoft.Json;
global using Newtonsoft.Json.Converters;

global using Modding;
global using Modding.Patches;
global using Modding.Menu;
global using Modding.Menu.Config;
global using Modding.Menu.Components;
global using GlobalEnums;
global using Language;

global using HKTool;
global using HKTool.FSM;
global using HKTool.FSM.CSFsm;
global using HKTool.Reflection;
global using HKTool.Menu;
global using HKTool.SpecialBehaviour;
global using HKTool.Utils;
global using HKTool.Utils.Compile;
global using static HKTool.Utils.Compile.ReflectionHelperEx;
global using HKTool.Unity;
global using HKTool.Attributes;
global using HKTool.Runtime;
global using HKTool.MAPI;
global using HKTool.MAPI.Loader;
global using HKTool.Patcher;
global using HKTool.Unsafe;

global using Mono.Cecil;
global using Mono.Cecil.Cil;
global using MonoMod.Cil;
global using MonoMod.RuntimeDetour.HookGen;
global using MonoMod.RuntimeDetour;
global using MonoMod.Utils;
global using MOpCodes = Mono.Cecil.Cil.OpCodes;
global using OpCodes = System.Reflection.Emit.OpCodes;
global using OpCode = System.Reflection.Emit.OpCode;

global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;

global using UObject = UnityEngine.Object;
global using HReflectionHelper = HKTool.Reflection.ReflectionHelper;

global using PreloadObject = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>>;
global using PreloadAsset = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, UnityEngine.Object>>;
global using MethodAttributes = System.Reflection.MethodAttributes;


global using Type = System.Type;
