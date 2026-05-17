using UnityEngine;
using Mushus.CaptureTools;

public class InspectCameras
{
    public static void Execute()
    {
        var settings = Object.FindFirstObjectByType<AvatarCaptureSettings>();
        if (settings == null)
        {
            Debug.LogError("AvatarCaptureSettings not found.");
            return;
        }

        PrintCam("Front", settings.FrontCamera);
        PrintCam("Back", settings.BackCamera);
        PrintCam("Side", settings.SideCamera);
        PrintCam("Face", settings.FaceCamera);
    }

    private static void PrintCam(string label, Camera cam)
    {
        if (cam == null)
        {
            Debug.Log($"{label}: null");
            return;
        }
        Debug.Log($"{label}: {cam.name}, Pos: {cam.transform.position}, Rot: {cam.transform.eulerAngles}, LocalPos: {cam.transform.localPosition}, LocalRot: {cam.transform.localEulerAngles}");
    }
}
