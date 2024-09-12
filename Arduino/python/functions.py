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
