using UnityEngine;
using System.IO;
using NumSharp;

public class AMASSLoader : MonoBehaviour
{
    NDArray poses;
    NDArray trans;
    GameObject[] spheres;

    int totalFrames;
    float mocapFramerate = 120f;
    float timer = 0f;
    int currentFrame = 0;
    bool isReady = false;

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

        poses = npz["poses.npy"];
        trans = npz["trans.npy"];
        totalFrames = poses.shape[0];

        Debug.Log($"총 프레임 수: {totalFrames}, framerate: {mocapFramerate}");

        // sphere 미리 생성
        spheres = new GameObject[52];
        for (int j = 0; j < 52; j++)
        {
            spheres[j] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[j].transform.localScale = Vector3.one * 0.05f;
            spheres[j].name = $"Joint_{j}";
        }

        // 0번 프레임으로 초기화
        UpdateFrame(0);
        isReady = true;
    }

    void Update()
    {
        if (!isReady) return; 

        // 시간 기반으로 프레임 진행
        timer += Time.deltaTime;
        float frameDuration = 1f / mocapFramerate;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;

            if (currentFrame >= totalFrames)
                currentFrame = 0;  // 루프

            UpdateFrame(currentFrame);
        }
    }

    void UpdateFrame(int frameIdx)
    {
        double[] framePose = poses[frameIdx].ToArray<double>();
        double[] frameTrans = trans[frameIdx].ToArray<double>();

        Vector3 root = new Vector3(
            (float)frameTrans[0],
            (float)frameTrans[1],
            (float)frameTrans[2]
        );

        Vector3[] jointPositions = ComputeFK(framePose, root);

        for (int j = 0; j < jointPositions.Length; j++)
        {
            Vector3 pos = jointPositions[j];
            pos = new Vector3(-pos.x, pos.z, pos.y);
            spheres[j].transform.position = pos;
        }
    }

    // 1단계 - SMPL-H Joint Hierarchy
    int[] GetParentIndex()
    {
        return new int[]
        {
            -1,  // 0  root (골반)
             0,  // 1  left_hip
             0,  // 2  right_hip
             0,  // 3  spine1
             1,  // 4  left_knee
             2,  // 5  right_knee
             3,  // 6  spine2
             4,  // 7  left_ankle
             5,  // 8  right_ankle
             6,  // 9  spine3
             7,  // 10 left_foot
             8,  // 11 right_foot
             9,  // 12 neck
             9,  // 13 left_collar
             9,  // 14 right_collar
            12,  // 15 head
            13,  // 16 left_shoulder
            14,  // 17 right_shoulder
            16,  // 18 left_elbow
            17,  // 19 right_elbow
            18,  // 20 left_wrist
            19,  // 21 right_wrist
            20,  // 22 left_index1
            22,  // 23 left_index2
            23,  // 24 left_index3
            20,  // 25 left_middle1
            25,  // 26 left_middle2
            26,  // 27 left_middle3
            20,  // 28 left_pinky1
            28,  // 29 left_pinky2
            29,  // 30 left_pinky3
            20,  // 31 left_ring1
            31,  // 32 left_ring2
            32,  // 33 left_ring3
            20,  // 34 left_thumb1
            34,  // 35 left_thumb2
            35,  // 36 left_thumb3
            21,  // 37 right_index1
            37,  // 38 right_index2
            38,  // 39 right_index3
            21,  // 40 right_middle1
            40,  // 41 right_middle2
            41,  // 42 right_middle3
            21,  // 43 right_pinky1
            43,  // 44 right_pinky2
            44,  // 45 right_pinky3
            21,  // 46 right_ring1
            46,  // 47 right_ring2
            47,  // 48 right_ring3
            21,  // 49 right_thumb1
            49,  // 50 right_thumb2
            50,  // 51 right_thumb3
        };
    }

    // 2단계 - SMPL-H T-pose Joint Offset
    Vector3[] GetTPoseOffsets()
    {
        return new Vector3[]
        {
            new Vector3( 0.0000f,  0.0000f,  0.0000f),  // 0  root
            new Vector3(-0.0572f, -0.2357f,  0.0000f),  // 1  left_hip
            new Vector3( 0.0572f, -0.2357f,  0.0000f),  // 2  right_hip
            new Vector3( 0.0000f,  0.1000f,  0.0000f),  // 3  spine1
            new Vector3( 0.0000f, -0.3700f,  0.0000f),  // 4  left_knee
            new Vector3( 0.0000f, -0.3700f,  0.0000f),  // 5  right_knee
            new Vector3( 0.0000f,  0.1000f,  0.0000f),  // 6  spine2
            new Vector3( 0.0000f, -0.3800f,  0.0000f),  // 7  left_ankle
            new Vector3( 0.0000f, -0.3800f,  0.0000f),  // 8  right_ankle
            new Vector3( 0.0000f,  0.1000f,  0.0000f),  // 9  spine3
            new Vector3( 0.0000f, -0.0700f,  0.0000f),  // 10 left_foot
            new Vector3( 0.0000f, -0.0700f,  0.0000f),  // 11 right_foot
            new Vector3( 0.0000f,  0.1900f,  0.0000f),  // 12 neck
            new Vector3(-0.0400f,  0.1600f,  0.0000f),  // 13 left_collar
            new Vector3( 0.0400f,  0.1600f,  0.0000f),  // 14 right_collar
            new Vector3( 0.0000f,  0.0800f,  0.0000f),  // 15 head
            new Vector3(-0.1500f,  0.0000f,  0.0000f),  // 16 left_shoulder
            new Vector3( 0.1500f,  0.0000f,  0.0000f),  // 17 right_shoulder
            new Vector3(-0.2650f,  0.0000f,  0.0000f),  // 18 left_elbow
            new Vector3( 0.2650f,  0.0000f,  0.0000f),  // 19 right_elbow
            new Vector3(-0.2650f,  0.0000f,  0.0000f),  // 20 left_wrist
            new Vector3( 0.2650f,  0.0000f,  0.0000f),  // 21 right_wrist
            new Vector3(-0.0500f,  0.0000f,  0.0200f),  // 22 left_index1
            new Vector3(-0.0350f,  0.0000f,  0.0000f),  // 23 left_index2
            new Vector3(-0.0250f,  0.0000f,  0.0000f),  // 24 left_index3
            new Vector3(-0.0500f,  0.0000f,  0.0066f),  // 25 left_middle1
            new Vector3(-0.0380f,  0.0000f,  0.0000f),  // 26 left_middle2
            new Vector3(-0.0250f,  0.0000f,  0.0000f),  // 27 left_middle3
            new Vector3(-0.0500f,  0.0000f, -0.0200f),  // 28 left_pinky1
            new Vector3(-0.0300f,  0.0000f,  0.0000f),  // 29 left_pinky2
            new Vector3(-0.0200f,  0.0000f,  0.0000f),  // 30 left_pinky3
            new Vector3(-0.0500f,  0.0000f, -0.0130f),  // 31 left_ring1
            new Vector3(-0.0350f,  0.0000f,  0.0000f),  // 32 left_ring2
            new Vector3(-0.0250f,  0.0000f,  0.0000f),  // 33 left_ring3
            new Vector3(-0.0200f,  0.0100f,  0.0200f),  // 34 left_thumb1
            new Vector3(-0.0300f,  0.0000f,  0.0000f),  // 35 left_thumb2
            new Vector3(-0.0200f,  0.0000f,  0.0000f),  // 36 left_thumb3
            new Vector3( 0.0500f,  0.0000f,  0.0200f),  // 37 right_index1
            new Vector3( 0.0350f,  0.0000f,  0.0000f),  // 38 right_index2
            new Vector3( 0.0250f,  0.0000f,  0.0000f),  // 39 right_index3
            new Vector3( 0.0500f,  0.0000f,  0.0066f),  // 40 right_middle1
            new Vector3( 0.0380f,  0.0000f,  0.0000f),  // 41 right_middle2
            new Vector3( 0.0250f,  0.0000f,  0.0000f),  // 42 right_middle3
            new Vector3( 0.0500f,  0.0000f, -0.0200f),  // 43 right_pinky1
            new Vector3( 0.0300f,  0.0000f,  0.0000f),  // 44 right_pinky2
            new Vector3( 0.0200f,  0.0000f,  0.0000f),  // 45 right_pinky3
            new Vector3( 0.0500f,  0.0000f, -0.0130f),  // 46 right_ring1
            new Vector3( 0.0350f,  0.0000f,  0.0000f),  // 47 right_ring2
            new Vector3( 0.0250f,  0.0000f,  0.0000f),  // 48 right_ring3
            new Vector3( 0.0200f,  0.0100f,  0.0200f),  // 49 right_thumb1
            new Vector3( 0.0300f,  0.0000f,  0.0000f),  // 50 right_thumb2
            new Vector3( 0.0200f,  0.0000f,  0.0000f),  // 51 right_thumb3
        };
    }

    // 3단계 - axis-angle → 회전 행렬 (Rodrigues)
    Matrix4x4 AxisAngleToMatrix(float x, float y, float z)
    {
        float angle = Mathf.Sqrt(x * x + y * y + z * z);

        if (angle < 1e-6f)
            return Matrix4x4.identity;

        float ax = x / angle;
        float ay = y / angle;
        float az = z / angle;

        float c = Mathf.Cos(angle);
        float s = Mathf.Sin(angle);
        float t = 1f - c;

        Matrix4x4 m = Matrix4x4.identity;
        m[0, 0] = t * ax * ax + c;
        m[0, 1] = t * ax * ay - s * az;
        m[0, 2] = t * ax * az + s * ay;
        m[1, 0] = t * ax * ay + s * az;
        m[1, 1] = t * ay * ay + c;
        m[1, 2] = t * ay * az - s * ax;
        m[2, 0] = t * ax * az - s * ay;
        m[2, 1] = t * ay * az + s * ax;
        m[2, 2] = t * az * az + c;

        return m;
    }

    // 4단계 - FK 계산
    Vector3[] ComputeFK(double[] pose, Vector3 rootPos)
    {
        int jointCount = 52;
        int[] parents = GetParentIndex();
        Vector3[] offsets = GetTPoseOffsets();

        Vector3[] positions = new Vector3[jointCount];
        Matrix4x4[] rotations = new Matrix4x4[jointCount];

        for (int j = 0; j < jointCount; j++)
        {
            float x = (float)pose[j * 3 + 0];
            float y = (float)pose[j * 3 + 1];
            float z = (float)pose[j * 3 + 2];

            Matrix4x4 localRot = AxisAngleToMatrix(x, y, z);

            if (j == 0)
            {
                positions[j] = rootPos;
                rotations[j] = localRot;
            }
            else
            {
                int p = parents[j];
                Vector3 worldOffset = rotations[p].MultiplyVector(offsets[j]);
                positions[j] = positions[p] + worldOffset;
                rotations[j] = rotations[p] * localRot;
            }
        }
        return positions;
    }
}