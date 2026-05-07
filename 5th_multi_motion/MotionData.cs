using UnityEngine;

[CreateAssetMenu(menuName = "AMASS/MotionData")]
public class MotionData : ScriptableObject
{
    public string motionName;
    public string fileName;   // 확장자 없이 "01_05_poses"
    public float frameRate = 120f;
}