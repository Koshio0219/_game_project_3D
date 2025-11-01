using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Hud
{
    [RequireComponent(typeof(HudView))]
    public abstract class HudCtrl<T>:MonoBehaviour where T : HudView
    {
        private T view = null;
        protected  T View{
            get
            {
                if(view  == null)
                {
                    view = GetComponent<T>();
                }
                return view;
            }
        }
    }
}
