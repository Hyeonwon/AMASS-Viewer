using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "AMASS/MotionList")]
public class MotionList : ScriptableObject
{
    public List<MotionData> motions = new List<MotionData>();
}