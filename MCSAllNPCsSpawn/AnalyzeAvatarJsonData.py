# Just a simple script to analyze AvatarJsonData to find cultivation levels
import json
from collections import OrderedDict
import statistics

with open('./BaseGameData/Data/AvatarJsonData.json') as avatar_json:
    json_data = json.load(avatar_json)
    levels = {}
    for item in json_data.items():
        level = item[1]["Level"]
        if level not in levels:
            levels[level] = 1
        else:
            levels[level] += 1
    levels = dict(OrderedDict(sorted(levels.items(), key=lambda t: t[0])))
    levelsSorted = dict(sorted(levels.items(), key=lambda item: item[1]))
    print("Levels:", levels)
    print("Levels Sorted:", levelsSorted)
    # Let's get the average, median and mode
    levelsList = []
    for key, value in levels.items():
        levelsList += value * [key]
    average = statistics.mean(levelsList)
    median = statistics.median(levelsList)
    mode = statistics.mode(levelsList)
    print(f"Average: {average}\nMedian: {median}\nMode: {mode}")
