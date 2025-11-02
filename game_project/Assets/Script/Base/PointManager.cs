using Game.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Base
{
    public enum GetPointItem
    {
        KillSlime,
        KillGuard,
        KillGhost,
        GetGem,
    }

    public class PointManager
    {
        public PointManager()
        {
            currentPoint = 0;
            mapItemToPoint.Clear();
            mapItemToPoint.AddOrSet(GetPointItem.KillSlime, 0);
            mapItemToPoint.AddOrSet(GetPointItem.KillGuard, 0);
            mapItemToPoint.AddOrSet(GetPointItem.KillGhost, 0);
            mapItemToPoint.AddOrSet(GetPointItem.GetGem, 0);
        }

        private int currentPoint;
        public int CurrentPoint
        {
            get
            {
                return currentPoint;
            }
            set
            {
                if (value == currentPoint) return;
                var last = currentPoint;
                currentPoint = value;

                EventQueueSystem.QueueEvent(new PointChangeEvent(last, currentPoint));      
            }
        }

        //private int? goalPoint = null;
        //public int GoalPoint
        //{
        //    get
        //    {
        //        goalPoint ??= GameData.Instance.LevelConfig.levelDatas[GameManager.Instance.LevelIdx].goalPoint;
        //        return goalPoint.Value;
        //    }
        //}

        //public bool ReachGoal => CurrentPoint >= GoalPoint;

        public Dictionary<GetPointItem, int> mapItemToPoint = new(5);

        public int AddPoint(GetPointItem item, int point)
        {
            mapItemToPoint[item] += point;
            CurrentPoint += point;
            return CurrentPoint;
        }
    }
}
