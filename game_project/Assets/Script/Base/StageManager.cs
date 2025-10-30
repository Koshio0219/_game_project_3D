using System.Collections;
using UnityEngine;

namespace Game.Base
{
    public enum StageStates
    {
        GetReady,
        MapBlockCreateStart,
        MapBlockCreateEnd,
        NavMeshBuildStart,
        NavMeshBuildEnd,
        CurtainInputStart,
        CurtainInputEnd,
        EnemyBuildStart,
        EnemyBuildEnd,
        BattleStarted,
        BattleClear,//win
        GameOver//lose
    }

    public class StageManager : MonoBehaviour
    {

    }
}