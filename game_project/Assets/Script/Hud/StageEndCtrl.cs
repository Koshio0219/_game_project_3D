using Game.Base;
using Game.Framework;

namespace Game.Hud
{
    public class StageEndCtrl : HudCtrl<StageEndView>
    {
        private void Awake()
        {
            EventQueueSystem.AddListener<StageStatesEvent>(StageStatesEventHandler);

        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<StageStatesEvent>(StageStatesEventHandler);
        }

        private void StageStatesEventHandler(StageStatesEvent e)
        {
            switch (e.to)
            {
                case StageStates.BattleClear:
                    View.Win();
                    break;
                case StageStates.GameOver:
                    View.Lose();
                    break;
            }
        }

        private void Start()
        {
            View.endPage.Hide();
        }
    }
}

