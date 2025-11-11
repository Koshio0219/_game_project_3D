using Game.Framework;

namespace Game.Hud
{
    public class StageShowCtrl : HudCtrl<StageShowView>
    {
        private void Awake()
        {
            EventQueueSystem.AddListener<PlayerHpChangeEvent>(PlayerHpChangeHandler);
            EventQueueSystem.AddListener<SwordPointChangeEvent>(SwordPointChangeHandler);
            EventQueueSystem.AddListener<PlayerPropChangedEvent>(PlayerPropChangedHandler);
            EventQueueSystem.AddListener<PlayerEnterLevelEvent>(PlayerEnterLevelHandler);
            EventQueueSystem.AddListener<PlayerDeadEvent>(PlayerDeadHnadler);
        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<PlayerHpChangeEvent>(PlayerHpChangeHandler);
            EventQueueSystem.RemoveListener<SwordPointChangeEvent>(SwordPointChangeHandler);
            EventQueueSystem.RemoveListener<PlayerPropChangedEvent>(PlayerPropChangedHandler);
            EventQueueSystem.RemoveListener<PlayerEnterLevelEvent>(PlayerEnterLevelHandler);
            EventQueueSystem.RemoveListener<PlayerDeadEvent>(PlayerDeadHnadler);
        }

        private void PlayerPropChangedHandler(PlayerPropChangedEvent e)
        {
            View.UpdateStats(e.playerData);
        }

        private void PlayerEnterLevelHandler(PlayerEnterLevelEvent e)
        {
            View.InitHpbar(e.playerData.HP,e.playerData.MaxHP);
            View.InitSwordPointBar(e.playerData.SwordPoint,e.playerData.MaxSwordPoint);
            View.UpdateStats(e.playerData);
        }

        private void PlayerDeadHnadler(PlayerDeadEvent e)
        {
            View.FadeOut();
        }

        private void SwordPointChangeHandler(SwordPointChangeEvent e)
        {
            View.UpdateSwordPointBar(e.lastP, e.nowP, e.maxP);
        }

        private void PlayerHpChangeHandler(PlayerHpChangeEvent e)
        {
            View.UpdateHpbar(e.lastHp, e.nowHp,e.maxHp);
        }
    }
}