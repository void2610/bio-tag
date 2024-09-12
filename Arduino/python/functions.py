import numpy as np


def gaussian_filter(data, sigma):
    kernel_size = int(6 * sigma + 1)
    kernel_size = kernel_size + 1 if kernel_size % 2 == 0 else kernel_size
    kernel = np.exp(-0.5 * (np.arange(kernel_size) - kernel_size // 2) ** 2 / sigma**2)
    kernel /= kernel.sum()

    filtered_data = np.convolve(data, kernel, mode="valid")
    return filtered_data


def moving_average(data, window_size):
    return np.convolve(data, np.ones(window_size) / window_size, mode="valid")


def diff_filter(data):
    diff_data = np.diff(data)
    return diff_data


# 興奮状態の判定
def is_excited(data: np.ndarray, th: float) -> bool:
    global excited_carry

    if abs(data[-1]) > th:
        return True
    else:
        if abs(data[-2]) > th:
            # start = len(data) - 2
            # end = 0
            # 閾値を下回る部分まで遡る
            for i in range(2, len(data)):
                if abs(data[-i]) < th:
                    excited_carry = i
                    break
    return False
