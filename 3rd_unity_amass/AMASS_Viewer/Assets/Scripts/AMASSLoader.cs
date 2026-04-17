using UnityEngine;
using System.IO;
using NumSharp;

public class AMASSLoader : MonoBehaviour
{
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "01_05_poses.npz");

        if (!File.Exists(path))
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + path);
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);
        var stream = new MemoryStream(bytes);
        var npz = new NpzDictionary(stream, false);

        foreach (var key in npz.Keys)
        {
            Debug.Log("key: " + key);
        }

        NDArray poses = npz["poses.npy"];
        Debug.Log("poses shape: " + poses.shape);
        Debug.Log("poses dtype: " + poses.dtype);

        NDArray trans = npz["trans.npy"];
        Debug.Log("trans shape: " + trans.shape);

        double[] frame0 = poses[0].ToArray<double>();
        for (int i = 0; i < 10; i++)
        {
            int joint = i / 3;
            int axis = i % 3;
            Debug.Log($"joint[{joint}] axis[{axis}] = {frame0[i]:F4}");
        }
    }
}