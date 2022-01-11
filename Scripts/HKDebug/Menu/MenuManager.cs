using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HKDebug.Menu
{
    public delegate void ButtonSubmit(ButtonInfo button);
    public class ButtonInfo
    {
        public string label = "";
        public ButtonSubmit submit = null;
    }
    public class ButtonGroup
    {
        public List<ButtonInfo> buttons = new List<ButtonInfo>();
        public void AddButton(ButtonInfo but)
        {
            buttons.Add(but);
        }
    }
    public static class MenuManager
    {
        public static List<ButtonInfo> root = new List<ButtonInfo>();
        public static Stack<ButtonGroup> groups = new Stack<ButtonGroup>();
        public static bool HasButton => root.Count != 0;
        public static List<ButtonInfo> Buttons => groups.Count == 0 ? root : groups.Peek().buttons;
        public static void EnterGroup(ButtonGroup bg)
        {
            groups.Push(bg);
        }
        public static void LeaveGroup()
        {
            groups.Pop();
        }

        public static void AddButton(ButtonInfo but)
        {
            root.Add(but);
        }
        public static void Submit(ButtonInfo but)
        {
            try
            {
                but.submit?.Invoke(but);
            }catch(Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }
    }
}
