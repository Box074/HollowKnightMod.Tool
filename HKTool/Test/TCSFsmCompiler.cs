using HKTool.FSM.CSFsmEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKTool.Test
{
    internal class TCSFsmCompiler : CSFsmHost
    {
        [FsmVar]
        public FsmGameObject Test1 = new();
        [FsmVar]
        public FsmGameObject Test2 = new();
        [FsmVar]
        public FsmGameObject Test3 = new();
        [FsmVar]
        public FsmGameObject Test41 = new();
        [FsmVar]
        public FsmGameObject Test6 = new();
        [FsmVar]
        public FsmInt Te1 = new();
        [FsmVar]
        public FsmInt Te2 = new();
        [FsmVar]
        public FsmInt Te3 = new();

        [Binding]
        public GameObject self = null!;

        [Binding]
        public MeshRenderer renderer= null!;

        [Binding("Test")]
        public SpriteRenderer spriteRenderer = null!;

        [Binding("Test/Test2")]
        public SpriteRenderer spriteRenderer2 = null!;

        [FsmState]
        private IEnumerator Test_State((AddAnimationClip, RemoveAllComponents, 
            SetMeshRenderer, SetMeshRendererChildren, GetChild, GetChildCount, ActivateAllChildren,
            ActivateGameObject?, SetGameObject, SetFsmBool?, SetFsmInt) actions)
        {
            actions.Item1.Name = "A";
            actions.Item11.Name = "A";
            (actions.Item1, actions.Item3).Item1.Name = "B";
            yield return null;
        }

        [FsmState]
        private IEnumerator Test_State2(SetFsmBool a5, (AddComponent?, AddComponent) action1, 
            (SetMeshRenderer, SetMeshRenderer?) action2, SetFsmGameObject a3, SetFsmInt? a4)
        {
            yield return null;
        }

        [FsmState]
        private IEnumerator Test_State3(
            CSFsmStateMetadata token,
            [FsmTransition("Test_State")] FsmEvent ev1,
            [FsmTransition("Test_State2")] FsmEvent ev2,
            [GetOrAdd] SpawnFromPoolV2 a_spawn,
            FindChild[] a_findChilds
            )
        {
            a_spawn.speedMax = 0;
            
            yield return token;
        }

        [FsmState]
        private IEnumerator Test_Start1(
            [FsmTransition(nameof(Test_2))] FsmEvent r0,
            [FsmTransition(nameof(Test_3), "FINISHED")] FsmEvent r1
            )
        {
            yield return null;
            HKToolMod2.logger.Log("I'm Test_Start1!");
        }

        [FsmState]
        private IEnumerator Test_2(
            CSFsmStateMetadata token,
            [FsmTransition(nameof(Test_Start1))] FsmEvent r0,
            [FsmTransition(nameof(Test_3))] FsmEvent r1
            )
        {
            yield return token;
            HKToolMod2.logger.Log("I'm Test_2!");

            yield return r1;
        }

        [FsmState]
        private IEnumerator Test_3(
            CSFsmStateMetadata token,
            [FsmTransition(nameof(Test_Start1))] FsmEvent r0,
            [FsmTransition(nameof(Test_3))] FsmEvent r1
            )
        {
            int count = 0;
            yield return null;
            HKToolMod2.logger.Log($"I'm Test_3! {count++}");

            yield return r0;
        }
    }
}
