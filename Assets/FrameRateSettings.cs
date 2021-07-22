using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
public class FrameRateSettings : MonoBehaviour
{
   void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
    }
 
    private void OnVuforiaStarted()
    {
        // 查询Vuforia推荐的帧速率和团结
        int targetFps = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.NONE);
 
        //默认情况下，我们使用Application.targetFrameRate设置建议的帧频。
        // Google Cardboard不使用vsync，OVR明确禁用了它。 如果开发人员
        //在其质量设置中使用vsync，他们还应该设置其QualitySettings.vSyncCount
        //根据上面返回的值。
        //例如：如果targetFPS> 50-> vSyncCount = 1; 否则vSyncCount = 2;
        if (Application.targetFrameRate != targetFps)
        {
            Debug.Log("Setting frame rate to " + targetFps + "fps");
            Application.targetFrameRate = targetFps;
        }
        
        //开启自动对焦模式
        CameraDevice.Instance.SetFocusMode(
            CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
            
    }
 
    private void OnPaused(bool paused)
    {
        if (!paused)
        {
            /// /恢复
            /// /设置了自动对焦模式应用时恢复
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }
}
