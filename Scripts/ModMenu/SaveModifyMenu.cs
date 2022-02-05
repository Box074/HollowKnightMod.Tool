
namespace HKTool.ModMenu;

class SaveModifyMenu : CustomMenu
{
    public static readonly MethodInfo MGetSaveSlotPath = typeof(DesktopPlatform).GetMethod("GetSaveSlotPath",
        BindingFlags.NonPublic | BindingFlags.Instance);
    public static SaveModifyMenu instance = null;
    public SaveModifyMenu(CustomMenu returnScreen) : base(returnScreen, "HKTool.Menu.ModifySaveTitle".Get())
    {
        SaveModifyCoreMenu.instance = new(this);
    }
    public static SaveGameData LoadSaveGameData(int slot)
    {
        if (GameManager.instance.profileID == slot && HeroController.UnsafeInstance != null)
        {
            return new SaveGameData(PlayerData.instance, SceneData.instance);
        }
        string sp = (string)MGetSaveSlotPath.FastInvoke(Platform.Current, slot, 0);
        if (!File.Exists(sp))
        {
            return null;
        }

        byte[] b = File.ReadAllBytes(sp);
        SaveGameData saveGameData = null;
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream serializationStream = new MemoryStream(b);
        var text = Encryption.Decrypt((string)binaryFormatter.Deserialize(serializationStream));
        saveGameData = JsonConvert.DeserializeObject<SaveGameData>(text, new JsonSerializerSettings
        {
            ContractResolver = ShouldSerializeContractResolver.Instance,
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = Modding.JsonConverterTypes.ConverterTypes
        });
        return saveGameData;
    }
    public static void SaveSaveGameData(int slot, SaveGameData data)
    {
        string sp = (string)MGetSaveSlotPath.FastInvoke(Platform.Current, slot, 0);

        string text2 = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = ShouldSerializeContractResolver.Instance,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = Modding.JsonConverterTypes.ConverterTypes
        });
        string graph = Encryption.Encrypt(text2);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, graph);
            byte[] binary = memoryStream.ToArray();
            File.WriteAllBytes(sp, binary);
        }
    }
    protected override void Build(ContentArea contentArea)
    {
        for (int i = 1; i <= 4; i++)
        {
            int slotId = i;
            AddButton(string.Format("HKTool.Menu.ModifySaveSlot".Get(), i.ToString()), "",
                () =>
                {
                    var sd = LoadSaveGameData(slotId);
                    if (sd != null)
                    {
                        SaveModifyCoreMenu.currentData = sd;
                        SaveModifyCoreMenu.slotId = slotId;
                        GoToMenu(SaveModifyCoreMenu.instance);
                    }
                });
        }
    }
}

class SaveModifyCoreMenu : CustomMenu
{
    abstract class PlayerDataModifyBase : CustomMenu
    {
        public PlayerDataModifyBase(CustomMenu rs) : base(rs, "HKTool.Menu.ModifySaveTitle".Get())
        {

        }
        protected void AddBoolOption(string label, Action<bool> onChange, Func<bool> onRefresh)
        {
            AddOption(label, "", new string[] { "HKTool.Menu.Bool.False".Get(), "HKTool.Menu.Bool.True".Get() },
                (id) => { if (pd != null) onChange(id == 1); }, () => pd == null ? 0 : (onRefresh() ? 1 : 0));
        }
        protected void AddIntOption(string label, int minValue, int maxValue,
         Action<int> onChange, Func<int> onRefresh)
        {
            string[] s = new string[maxValue - minValue];
            for (int i = 0; i < maxValue - minValue; i++)
            {
                s[i] = (i + minValue).ToString();
            }
            AddOption(label, "", s, (id) => { if (pd != null) onChange(id + minValue); },
                 () => pd != null ? (onRefresh() - minValue) : 0);
        }
    }
    class AllPlayerDataModify : PlayerDataModifyBase
    {
        public static readonly List<string> boolOptions = new();

        static AllPlayerDataModify()
        {
            var fs = typeof(PlayerData).GetRuntimeFields();
            foreach (var v in fs)
            {
                if (v.FieldType == typeof(bool))
                {
                    boolOptions.Add(v.Name);
                }
            }
        }
        public AllPlayerDataModify(CustomMenu rs) : base(rs)
        {

        }
        protected override void Build(ContentArea contentArea)
        {
            foreach (var v in boolOptions)
            {
                AddBoolOption(v, (b) => pd.SetBoolInternal(v, b), () => pd.GetBoolInternal(v));
            }
        }
    }
    class SkillModify : PlayerDataModifyBase
    {
        public SkillModify(CustomMenu rs) : base(rs)
        {

        }
        protected override void Build(ContentArea contentArea)
        {
            AddBoolOption("HKTool.Menu.ModifySave.HasDash".Get(),
                (b) => pd.hasDash = pd.canDash = b, () => pd.hasDash && pd.canDash);
            AddBoolOption("HKTool.Menu.ModifySave.HasBlackDash".Get(),
                (b) => pd.canShadowDash = pd.hasShadowDash = b, () => pd.hasShadowDash && pd.canShadowDash);
            AddBoolOption("HKTool.Menu.ModifySave.HasDashSlash".Get(),
                (b) => pd.hasDashSlash = b, () => pd.hasDashSlash);
            AddBoolOption("HKTool.Menu.ModifySave.HasGreatSlash".Get(),
                (b) => pd.hasNailArt = b, () => pd.hasNailArt);
            AddBoolOption("HKTool.Menu.ModifySave.HasCycloneSlash".Get(),
                (b) => pd.hasCyclone = b, () => pd.hasCyclone);

            AddBoolOption("HKTool.Menu.ModifySave.HasDoubleJump".Get(),
                (b) => pd.hasDoubleJump = b, () => pd.hasDoubleJump);
            AddBoolOption("HKTool.Menu.ModifySave.HasInvJump".Get(),
                (b) => pd.infiniteAirJump = b, () => pd.infiniteAirJump);
            AddBoolOption("HKTool.Menu.ModifySave.HasWallJump".Get(),
                (b) => pd.hasWalljump = b, () => pd.hasWalljump);
            AddBoolOption("HKTool.Menu.ModifySave.HasSuperDash".Get(),
                (b) => pd.canSuperDash = pd.hasSuperDash = b, () => pd.canSuperDash && pd.hasSuperDash);
            AddBoolOption("HKTool.Menu.ModifySave.HasDreamNail".Get(),
                (b) => pd.hasDreamNail = b, () => pd.hasDreamNail);

            AddBoolOption("HKTool.Menu.ModifySave.HasDreamNailP".Get(),
                (b) => pd.dreamNailUpgraded = b, () => pd.dreamNailUpgraded);
            AddBoolOption("HKTool.Menu.ModifySave.HasDreamGate".Get(),
                (b) => pd.hasDreamGate = b, () => pd.hasDreamGate);
            AddBoolOption("HKTool.Menu.ModifySave.HasSwimming".Get(),
                (b) => pd.hasAcidArmour = b, () => pd.hasAcidArmour);
            AddBoolOption("HKTool.Menu.ModifySave.HasLantern".Get(),
                (b) => pd.hasLantern = b, () => pd.hasLantern);
            AddIntOption("HKTool.Menu.ModifySave.FireballLevel".Get(),
                0, 2,
                (b) => pd.fireballLevel = b, () => pd.fireballLevel);

            AddIntOption("HKTool.Menu.ModifySave.QuakeLevel".Get(),
                0, 2,
                (b) => pd.quakeLevel = b, () => pd.quakeLevel);
            AddIntOption("HKTool.Menu.ModifySave.ScreamLevel".Get(),
                0, 2,
                (b) => pd.screamLevel = b, () => pd.screamLevel);
        }
    }
    class CharmModify : PlayerDataModifyBase
    {
        public CharmModify(CustomMenu rs) : base(rs)
        {

        }
        protected override void Build(ContentArea contentArea)
        {
            AddOption("HKTool.Menu.ModifySave.GrimmChildLevel".Get(), "",
                new string[] { "0", "1", "2", "3", "4", "5" },
                (id) =>
                {
                    if (pd == null) return;
                    pd.grimmChildLevel = id;
                    pd.destroyedNightmareLantern = id > 4;
                },
                () =>
                {
                    if (pd == null) return 0;
                    return pd.grimmChildLevel;
                });
            AddOption("HKTool.Menu.ModifySave.KingsoulState".Get(), "",
                new string[] { "HKTool.Menu.ModifySave.KS.L0".Get(),
                    "HKTool.Menu.ModifySave.KS.L1".Get(),
                    "HKTool.Menu.ModifySave.KS.L2".Get(),
                    "HKTool.Menu.ModifySave.KS.L3".Get() },
                (id) =>
                {
                    pd.royalCharmState = id + 1;
                    pd.gotShadeCharm = id == 3;
                },
                () =>
                {
                    if (pd == null) return 0;
                    return pd.royalCharmState - 1;
                });
            for (int i = 1; i <= 40; i++)
            {
                int charmId = i;
                string charmName;
                if (charmId != 36)
                {
                    charmName = global::Language.Language.Get($"CHARM_NAME_{charmId}", "UI");
                }
                else
                {
                    charmName = global::Language.Language.Get("CHARM_NAME_36_B", "UI");
                }
                AddBoolOption(
                    string.Format("HKTool.Menu.ModifySave.CharmGot".Get(),
                        charmId.ToString(), charmName),
                    (id) =>
                    {
                        pd.SetBoolInternal($"gotCharm_{charmId}", id);
                    },
                    () =>
                    {
                        return pd.GetBoolInternal($"gotCharm_{charmId}");
                    }
                    );
                AddBoolOption(
                    string.Format("HKTool.Menu.ModifySave.CharmEquipped".Get(),
                        charmId.ToString(), charmName),
                    (id) =>
                    {
                        pd.SetBoolInternal($"equippedCharm_{charmId}", id);
                    },
                    () =>
                    {
                        return pd.GetBoolInternal($"equippedCharm_{charmId}");
                    }
                    );
                AddBoolOption(
                    string.Format("HKTool.Menu.ModifySave.CharmNew".Get(),
                        charmId.ToString(), charmName),
                    (id) =>
                    {
                        pd.SetBoolInternal($"newCharm_{charmId}", id);
                    },
                    () =>
                    {
                        return pd.GetBoolInternal($"newCharm_{charmId}");
                    }
                    );
                if (charmId >= 23 && charmId <= 25)
                {
                    string scn;
                    switch (charmId)
                    {
                        case 23:
                            scn = "Heart";
                            break;
                        case 24:
                            scn = "Greed";
                            break;
                        case 25:
                        default:
                            scn = "Strength";
                            break;
                    }
                    AddOption(
                        string.Format("HKTool.Menu.ModifySave.SpecailCharmState".Get(),
                        charmId.ToString(), charmName),
                        "",
                        new string[] {"HKTool.Menu.ModifySave.SCS.N".Get(),
                            "HKTool.Menu.ModifySave.SCS.B".Get(),
                            "HKTool.Menu.ModifySave.SCS.S".Get()},
                        (id) =>
                        {
                            if (pd == null) return;
                            pd.SetBoolInternal($"fragile{scn}_unbreakable", id == 2);
                            pd.SetBoolInternal($"brokenCharm_{charmId}", id == 1);
                        },
                        () =>
                        {
                            if (pd == null) return 0;
                            if (pd.GetBoolInternal($"brokenCharm_{charmId}")) return 1;
                            if (pd.GetBoolInternal($"fragile{scn}_unbreakable")) return 2;
                            return 0;
                        });
                }

            }
        }
    }
    class MiscModify : PlayerDataModifyBase
    {
        public MiscModify(CustomMenu rs) : base(rs)
        {

        }
        protected override void Build(ContentArea contentArea)
        {
            AddOption("HKTool.Menu.GameMode".Get(), "",
                (id) =>
                {
                    if(pd == null) return;
                    pd.bossRushMode = id == 3;
                    pd.permadeathMode = id == 3 ? 0 : pd.permadeathMode;

                },
                () =>
                {
                    if(pd == null) return 0;
                    return pd.bossRushMode ? 3 : pd.permadeathMode;
                }, "HKTool.Menu.GM.0".Get(),
                    "HKTool.Menu.GM.1".Get(),
                    "HKTool.Menu.GM.2".Get(),
                    "HKTool.Menu.GM.3".Get());
        }
    }
    private static CharmModify charmMenu;
    private static SkillModify skillModify;
    private static AllPlayerDataModify allOptions;
    private static MiscModify misc;
    public SaveModifyCoreMenu(CustomMenu rsa) : base(rsa, "HKTool.Menu.ModifySaveTitle".Get())
    {
        charmMenu = new(this);
        skillModify = new(this);
        misc = new(this);
    }
    private static PlayerData pd => currentData?.playerData;
    public static SaveModifyCoreMenu instance = null;
    public static SaveGameData currentData = null;
    public static int slotId = -1;
    protected override void Back()
    {
        if (HasGameSaveData())
        {
            SaveModifyMenu.SaveSaveGameData(slotId, currentData);
        }
        currentData = null;
        slotId = -1;
        base.Back();
    }
    public static bool HasGameSaveData() => currentData != null && slotId != -1;
    protected override void Build(ContentArea contentArea)
    {
        AddButton("HKTool.Menu.ModifySave.AllOptions".Get(), "",
            () =>
            {
                if (allOptions == null)
                {
                    allOptions = new(this);
                }
                else
                {
                    allOptions.Refresh();
                }
                GoToMenu(allOptions);
            });
        AddButton("HKTool.Menu.ModifySave.Charms".Get(), "",
            () =>
            {
                charmMenu.Refresh();
                GoToMenu(charmMenu);
            });
        AddButton("HKTool.Menu.ModifySave.Skills".Get(), "",
            () =>
            {
                skillModify.Refresh();
                GoToMenu(skillModify);
            });
        AddButton("HKTool.Menu.ModifySave.Misc".Get(), "",
            () => 
            {
                misc.Refresh();
                GoToMenu(misc);
            });
    }     
}
