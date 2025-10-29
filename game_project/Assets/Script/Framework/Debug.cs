using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public enum DebugType
    {
        ALL,
        ErrorAndWarning,
        OnlyError,
        CLOSE
    }

    public struct DebugData
    {
        public DebugType debugType;
        public string log;

        public DebugData(DebugType debugType, string log)
        {
            this.debugType = debugType;
            this.log = log;
        }
    }

    public class Debug
    {
        private static DebugType debugType = DebugType.ALL;
        internal static DebugType DebugType       
        {
            get => debugType;
            set
            {
                if (debugType == value) return;
                debugType = value;
            }
        }

        //public List<DebugData> debugDatas = new List<DebugData>();

        public static void Log(string log)
        {
            if (debugType != DebugType.ALL) return;                      
            UnityEngine.Debug.Log(log);
        }

        public static void LogWarning(string log) 
        {
            if (debugType == DebugType.OnlyError || debugType == DebugType.CLOSE) return;
            UnityEngine.Debug.LogWarning(log);
        }

        public static void LogError(string log) 
        {
            if (debugType == DebugType.CLOSE) return;
            UnityEngine.Debug.LogError(log);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            UnityEngine.Debug.DrawLine(start, end, color,10);
        }
    }
}
