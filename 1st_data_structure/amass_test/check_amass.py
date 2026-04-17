import numpy as np

data = np.load(r"C:\Users\onede\OneDrive\바탕 화면\바탕화면에 있던 것\hyeongwon\1st\amass_test\test.npz", allow_pickle=True)
print(data.files)

for key in data.files:
    print(f"{key}: {data[key].shape}, data type: {data[key].dtype}")

print(data['mocap_framerate'])