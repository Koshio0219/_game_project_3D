using UnityEngine;
using UnityEngine.UI;

namespace Game.Hud
{
    [RequireComponent(typeof(CanvasScaler))]
    public class FixCanvasTool : MonoBehaviour
    {
        void Awake()
        {
            FixResolution();
        }

        public void FixResolution()
        {
            CanvasScaler scaler = GetComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            float sWToH = scaler.referenceResolution.x * 1.0f / scaler.referenceResolution.y;
            float vWToH = Screen.width * 1.0f / Screen.height;
            if (sWToH > vWToH)
            {
                //fix width
                scaler.matchWidthOrHeight = 0;
            }
            else
            {
                //fix height
                scaler.matchWidthOrHeight = 1;
            }
        }
    }
}
