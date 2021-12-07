using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HKTool.DebugTools;
using HKTool;

namespace HKDebug.Menu
{
    class MenuShow : DebugViewBase
    {

        public override string GetViewName() => "Default Debug Tools";

        

        public override void OnDebugDraw()
        {
            if (!MenuManager.HasButton) return;
            List<ButtonInfo> buttons = MenuManager.Buttons;
            if (buttons == null)
            {
                MenuManager.LeaveGroup();
                return;
            }
            if (buttons.Count == 0)
            {
                MenuManager.LeaveGroup();
                return;
            }

            if (MenuManager.groups.Count != 0)
            {
                if (GUILayout.Button("HKTool.DebugMenu.Back".Get()))
                {
                    MenuManager.LeaveGroup();
                }
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                if (GUILayout.Button(buttons[i].label))
                {
                    MenuManager.Submit(buttons[i]);
                }
            }
        }
    }
}
