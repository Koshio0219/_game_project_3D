using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Hud
{
    public class StagePreCtrl : HudCtrl<StagePreView>
    {
        private void Awake()
        {
            EventQueueSystem.AddListener<StageStatesEvent>(StageStatesHandler);
        }

        private void StageStatesHandler(StageStatesEvent e)
        {
            //switch (e.to)
            //{
            //    case StageStates.CurtainInputEnd:
            //        View.FadeOut();
            //        break;
            //}
        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<StageStatesEvent>(StageStatesHandler);
        }
    }
}

