global using System;
global using System.IO;
global using System.IO.Compression;
global using System.Collections;
global using System.Collections.Generic;
global using System.Reflection;
global using System.Reflection.Emit;
global using System.Runtime.Serialization.Formatters.Binary;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Linq;
global using UnityEngine;
global using UnityEngine.UI;
global using UnityEngine.SceneManagement;

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
global using HKTool.DebugTools;
global using HKTool.FSM;
global using HKTool.FSM.CSFsm;
global using HKTool.Reflection;
global using HKTool.Menu;
global using HKTool.ModMenu;
global using HKTool.SpecialBehaviour;
global using HKTool.Utils;
global using HKTool.Unity;
global using HKTool.Attributes;
global using HKTool.Runtime;
global using HKTool.MAPI;
global using HKTool.MAPI.Loader;
global using HKTool.Modules;

global using MonoMod.Cil;
global using MonoMod.RuntimeDetour.HookGen;
global using MonoMod.Utils;
global using MOpCodes = Mono.Cecil.Cil.OpCodes;

global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;

global using UObject = UnityEngine.Object;
global using HReflectionHelper = HKTool.Reflection.ReflectionHelper;

global using PreloadObject = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>>;
