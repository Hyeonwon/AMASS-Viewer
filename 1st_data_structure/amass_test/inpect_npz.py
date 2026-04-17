import numpy as np

PATH = r"C:\Users\onede\OneDrive\바탕 화면\바탕화면에 있던 것\hyeongwon\1st\amass_test\01_05_poses.npz"


def inspect_npz(path: str):
    print(f"\n{'='*60}")
    print(f"  NPZ Inspector: {path}")
    print(f"{'='*60}")

    data = np.load(path, allow_pickle=True)
    keys = list(data.keys())
    print(f"\n[키 목록] 총 {len(keys)}개: {keys}\n")

    for k in keys:
        v = data[k]

        if v.dtype == object:
            inner = v.item() if v.ndim == 0 else v
            print(f"[{k}]")
            print(f"  dtype  : object (dict or nested)")
            if isinstance(inner, dict):
                print(f"  내부 키: {list(inner.keys())}")
            else:
                print(f"  sample : {str(inner)[:80]}")
            print()
            continue

        print(f"[{k}]")
        print(f"  shape  : {v.shape}")
        print(f"  dtype  : {v.dtype}")

        if np.issubdtype(v.dtype, np.floating) or np.issubdtype(v.dtype, np.integer):
            print(f"  range  : [{v.min():.4f}, {v.max():.4f}]")
            print(f"  mean   : {v.mean():.4f}  std: {v.std():.4f}")
            print(f"  sample : {v.flat[:6].tolist()}")

            last = v.shape[-1] if v.ndim >= 1 else None
            hint = guess_rotation(v, last)
            if hint:
                print(f"  [추론]  : {hint}")

        print()


def guess_rotation(v, last_dim):
    hints = []

    if last_dim == 3:
        if abs(v.min()) <= 3.2 and abs(v.max()) <= 3.2:
            hints.append("axis-angle 또는 euler (범위가 -π~π 근처)")
        else:
            hints.append("3D position/translation 가능성 높음")

    elif last_dim == 4:
        try:
            flat = v.reshape(-1, 4).astype(np.float64)
            norms = np.linalg.norm(flat, axis=-1)
            if 0.98 < norms.mean() < 1.02:
                hints.append(f"quaternion (norm≈{norms.mean():.3f}) — (w,x,y,z) 또는 (x,y,z,w) 확인 필요")
            else:
                hints.append(f"4-dim 벡터 (norm={norms.mean():.3f}, quaternion 아닐 수 있음)")
        except Exception:
            pass

    elif last_dim == 6:
        hints.append("6D rotation 표현 가능성 높음 (Zhou et al. 2019)")

    elif last_dim == 9:
        hints.append("rotation matrix (3×3 flattened) 가능성")

    elif v.ndim >= 3 and v.shape[-2:] == (3, 3):
        try:
            flat = v.reshape(-1, 3, 3).astype(np.float64)
            dets = np.linalg.det(flat)
            hints.append(f"rotation matrix 3×3 (det≈{dets.mean():.3f})")
        except Exception:
            pass

    if last_dim == 3 and v.ndim >= 2 and v.shape[-2] > 10:
        col_means = np.abs(v.reshape(-1, 3)).mean(axis=0)
        up_axis = int(np.argmax(col_means))
        axis_name = ['X', 'Y', 'Z'][up_axis]
        hints.append(f"값 분산 기준 우세 축: {axis_name} (Y-up=1, Z-up=2 참고)")

    return " / ".join(hints) if hints else None


inspect_npz(PATH)