using NumSharp;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class AMASSLoader : MonoBehaviour
{
    public MotionList motionList;

    NDArray poses;
    NDArray trans;
    GameObject[] spheres;

    int totalFrames;
    float mocapFramerate = 120f;
    float timer = 0f;
    int currentFrame = 0;
    int currentMotionIndex = 0;
    bool isReady = false;

    void Start()
    {
        // sphere 미리 생성
        spheres = new GameObject[52];
        for (int j = 0; j < 52; j++)
        {
            spheres[j] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[j].transform.localScale = Vector3.one * 0.05f;
            spheres[j].name = $"Joint_{j}";
        }

        LoadMotion(0);
    }

    void LoadMotion(int index)
    {
        // 엣지 케이스 - 목록 비어있음
        if (motionList == null || motionList.motions.Count == 0)
        {
            Debug.LogError("MotionList가 비어있습니다.");
            return;
        }

        // 엣지 케이스 - 인덱스 범위 초과
        if (index < 0 || index >= motionList.motions.Count)
        {
            Debug.LogError($"잘못된 모션 인덱스: {index}");
            return;
        }

        MotionData data = motionList.motions[index];
        string path = Path.Combine(Application.streamingAssetsPath, "Motions", data.fileName + ".npz");

        // 엣지 케이스 - 파일 없음
        if (!File.Exists(path))
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + path);
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);

        // 엣지 케이스 - 빈 파일
        if (bytes.Length == 0)
        {
            Debug.LogError("파일이 비어있습니다: " + data.fileName);
            return;
        }

        var stream = new MemoryStream(bytes);
        var npz = new NpzDictionary(stream, false);

        poses = npz["poses.npy"];
        trans = npz["trans.npy"];

        // 엣지 케이스 - 프레임 수 0
        if (poses.shape[0] == 0)
        {
            Debug.LogError("프레임 수가 0입니다: " + data.fileName);
            return;
        }

        // FK 상태 리셋
        currentFrame = 0;
        timer = 0f;
        totalFrames = poses.shape[0];
        mocapFramerate = data.frameRate;

        Debug.Log($"[{data.motionName}] 로드 완료 - 총 {totalFrames}프레임 / {mocapFramerate}fps");

        UpdateFrame(0);
        isReady = true;
    }

    void Update()
    {
        if (!isReady) return;

        // 키보드 전환
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentMotionIndex = (currentMotionIndex + 1) % motionList.motions.Count;
            LoadMotion(currentMotionIndex);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentMotionIndex = (currentMotionIndex - 1 + motionList.motions.Count) % motionList.motions.Count;
            LoadMotion(currentMotionIndex);
        }

        // 시간 기반 프레임 진행
        timer += Time.deltaTime;
        float frameDuration = 1f / mocapFramerate;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;

            if (currentFrame >= totalFrames)
                currentFrame = 0;

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

    int[] GetParentIndex()
    {
        return new int[]
        {
            -1,  0,  0,  0,  1,  2,  3,  4,  5,  6,
             7,  8,  9,  9,  9, 12, 13, 14, 16, 17,
            18, 19, 20, 22, 23, 20, 25, 26, 20, 28,
            29, 20, 31, 32, 20, 34, 35, 21, 37, 38,
            21, 40, 41, 21, 43, 44, 21, 46, 47, 21,
            49, 50
        };
    }

    Vector3[] GetTPoseOffsets()
    {
        return new Vector3[]
        {
            new Vector3( 0.0000f,  0.0000f,  0.0000f),
            new Vector3(-0.0572f, -0.2357f,  0.0000f),
            new Vector3( 0.0572f, -0.2357f,  0.0000f),
            new Vector3( 0.0000f,  0.1000f,  0.0000f),
            new Vector3( 0.0000f, -0.3700f,  0.0000f),
            new Vector3( 0.0000f, -0.3700f,  0.0000f),
            new Vector3( 0.0000f,  0.1000f,  0.0000f),
            new Vector3( 0.0000f, -0.3800f,  0.0000f),
            new Vector3( 0.0000f, -0.3800f,  0.0000f),
            new Vector3( 0.0000f,  0.1000f,  0.0000f),
            new Vector3( 0.0000f, -0.0700f,  0.0000f),
            new Vector3( 0.0000f, -0.0700f,  0.0000f),
            new Vector3( 0.0000f,  0.1900f,  0.0000f),
            new Vector3(-0.0400f,  0.1600f,  0.0000f),
            new Vector3( 0.0400f,  0.1600f,  0.0000f),
            new Vector3( 0.0000f,  0.0800f,  0.0000f),
            new Vector3(-0.1500f,  0.0000f,  0.0000f),
            new Vector3( 0.1500f,  0.0000f,  0.0000f),
            new Vector3(-0.2650f,  0.0000f,  0.0000f),
            new Vector3( 0.2650f,  0.0000f,  0.0000f),
            new Vector3(-0.2650f,  0.0000f,  0.0000f),
            new Vector3( 0.2650f,  0.0000f,  0.0000f),
            new Vector3(-0.0500f,  0.0000f,  0.0200f),
            new Vector3(-0.0350f,  0.0000f,  0.0000f),
            new Vector3(-0.0250f,  0.0000f,  0.0000f),
            new Vector3(-0.0500f,  0.0000f,  0.0066f),
            new Vector3(-0.0380f,  0.0000f,  0.0000f),
            new Vector3(-0.0250f,  0.0000f,  0.0000f),
            new Vector3(-0.0500f,  0.0000f, -0.0200f),
            new Vector3(-0.0300f,  0.0000f,  0.0000f),
            new Vector3(-0.0200f,  0.0000f,  0.0000f),
            new Vector3(-0.0500f,  0.0000f, -0.0130f),
            new Vector3(-0.0350f,  0.0000f,  0.0000f),
            new Vector3(-0.0250f,  0.0000f,  0.0000f),
            new Vector3(-0.0200f,  0.0100f,  0.0200f),
            new Vector3(-0.0300f,  0.0000f,  0.0000f),
            new Vector3(-0.0200f,  0.0000f,  0.0000f),
            new Vector3( 0.0500f,  0.0000f,  0.0200f),
            new Vector3( 0.0350f,  0.0000f,  0.0000f),
            new Vector3( 0.0250f,  0.0000f,  0.0000f),
            new Vector3( 0.0500f,  0.0000f,  0.0066f),
            new Vector3( 0.0380f,  0.0000f,  0.0000f),
            new Vector3( 0.0250f,  0.0000f,  0.0000f),
            new Vector3( 0.0500f,  0.0000f, -0.0200f),
            new Vector3( 0.0300f,  0.0000f,  0.0000f),
            new Vector3( 0.0200f,  0.0000f,  0.0000f),
            new Vector3( 0.0500f,  0.0000f, -0.0130f),
            new Vector3( 0.0350f,  0.0000f,  0.0000f),
            new Vector3( 0.0250f,  0.0000f,  0.0000f),
            new Vector3( 0.0200f,  0.0100f,  0.0200f),
            new Vector3( 0.0300f,  0.0000f,  0.0000f),
            new Vector3( 0.0200f,  0.0000f,  0.0000f),
        };
    }

    Matrix4x4 AxisAngleToMatrix(float x, float y, float z)
    {
        float angle = Mathf.Sqrt(x * x + y * y + z * z);
        if (angle < 1e-6f) return Matrix4x4.identity;

        float ax = x / angle, ay = y / angle, az = z / angle;
        float c = Mathf.Cos(angle), s = Mathf.Sin(angle), t = 1f - c;

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