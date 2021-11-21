using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKDebug.Menu;
using UnityEngine;

namespace HKDebug
{
    static class Tool
    {
        public static ButtonGroup group = new ButtonGroup();
        public static ButtonGroup gates = new ButtonGroup();
        public static void EnterSceneFormGate()
        {
            
            gates.buttons.Clear();
            var gs = UnityEngine.Object.FindObjectsOfType<TransitionPoint>(true);
            if (gs.Length == 1)
            {
                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
                {
                    EntryGateName = gs[0].name,
                    SceneName = gs[0].gameObject.scene.name
                });
                return;
            }
            foreach(var v in gs)
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
                label = "工具",
                submit = (_) => MenuManager.EnterGroup(group)
            });
            group.AddButton(new ButtonInfo()
            {
                label = "重新加载当前场景",
                submit = (_) => ReloadScene()
            });
            group.AddButton(new ButtonInfo()
            {
                label= "Enter Scene From Gate",
                submit = (_) => EnterSceneFormGate()
            });
            group.AddButton(new ButtonInfo()
            {
                label = "Hazard Respawn",
                submit = (_) => HeroController.instance.StartCoroutine(HeroController.instance.HazardRespawn())
            });
            group.AddButton(new ButtonInfo()
            {
                label = "允许暂停",
                submit = (_) => PlayerData.instance.disablePause = false
            });
            group.AddButton(new ButtonInfo()
            {
                label = "结束当前GG",
                submit = (_) => BossSceneController.Instance.EndBossScene()
            });
            group.AddButton(new ButtonInfo()
            {
                label = "允许移动",
                submit = (_) =>
                {
                    HeroController.instance.hero_state = GlobalEnums.ActorStates.idle;
                    HeroController.instance.AcceptInput();
                }
            });
            group.AddButton(new ButtonInfo()
            {
                label = "启用无敌",
                submit = (but) =>
                {
                    canDamage = !canDamage;
                    but.label = (!canDamage ? "禁用" : "启用") + "无敌";
                }
            });
            group.AddButton(new ButtonInfo()
            {
                label = "返回菜单",
                submit = (_) => UnityEngine.SceneManagement.SceneManager.LoadScene("Quit_To_Menu",
                UnityEngine.SceneManagement.LoadSceneMode.Single)
            });
            group.AddButton(new ButtonInfo()
            {
                label = "Fade Out",
                submit = (_) => PlayMakerFSM.BroadcastEvent("FADE OUT")
            });
            group.AddButton(new ButtonInfo()
            {
                label = "增加33灵魂",
                submit = (_) => HeroController.instance.AddMPCharge(33)
            });
            On.HeroController.TakeDamage += HeroController_TakeDamage;
        }
        static bool canDamage = true;
        private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go,
            GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
        {
            if (canDamage)
            {
                orig(self, go, damageSide, damageAmount, hazardType);
            }
        }

        static void ReloadScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
