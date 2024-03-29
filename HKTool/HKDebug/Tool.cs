﻿
using HKDebug.Menu;

namespace HKDebug;
static class Tool
{
    public static ButtonGroup group = new ButtonGroup();
    public static ButtonGroup gates = new ButtonGroup();
    public static void EnterSceneFormGate()
    {

        gates.buttons.Clear();
        var gs = UObject.FindObjectsOfType<TransitionPoint>(true);
        if (gs.Length == 1)
        {
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
            {
                EntryGateName = gs[0].name,
                SceneName = gs[0].gameObject.scene.name
            });
            return;
        }
        foreach (var v in gs)
        {
            gates.AddButton(
                new ButtonInfo()
                {
                    label = v.name,
                    submit = (_) =>
                    {
                        MenuManager.LeaveGroup();
                        if (v != null)
                        {
                            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
                            {
                                EntryGateName = v.name,
                                SceneName = v.gameObject.scene.name
                            });
                        }
                    }
                }
                );
        }
        MenuManager.EnterGroup(gates);
    }
    public static void Init()
    {
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "重新加载当前场景",
            submit = (_) => ReloadScene()
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "从Gate进入当前场景",
            submit = (_) => EnterSceneFormGate()
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "Hazard Respawn",
            submit = (_) => HeroController.instance.StartCoroutine(HeroController.instance.HazardRespawn())
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "允许暂停",
            submit = (_) => PlayerData.instance.disablePause = false
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "结束当前GG",
            submit = (_) => BossSceneController.Instance.EndBossScene()
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "允许移动",
            submit = (_) =>
            {
                HeroController.instance.hero_state = ActorStates.idle;
                HeroController.instance.AcceptInput();
            }
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "启用无敌",
            submit = (but) =>
            {
                canDamage = !canDamage;
                but.label = (!canDamage ? "禁用" : "启用") + "无敌";
            }
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "返回菜单",
            submit = (_) => USceneManager.LoadScene("Quit_To_Menu",
            LoadSceneMode.Single)
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "Fade Out",
            submit = (_) => PlayMakerFSM.BroadcastEvent("FADE OUT")
        });
        MenuManager.AddButton(new ButtonInfo()
        {
            label = "增加33灵魂",
            submit = (_) => HeroController.instance.AddMPCharge(33)
        });
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        
    }
    static bool canDamage = true;
    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go,
        CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (canDamage)
        {
            orig(self, go, damageSide, damageAmount, hazardType);
        }
    }

    static void ReloadScene()
    {
        USceneManager.LoadScene(USceneManager.GetActiveScene().name,
            LoadSceneMode.Single);
    }
}

