import os
import sys
import time

from pyamaze_lib import maze
from queue import PriorityQueue


def h(cell1, cell2):
    x1, y1 = cell1
    x2, y2 = cell2
    return abs(x1 - x2) + abs(y1 - y2)


def astar(m, pathCostWeight, heuristicWeight):
    start = (m.rows, m.cols, 0)
    costs = {cell: float('inf') for cell in m.grid}
    costs[(start[0], start[1])] = float(0 * pathCostWeight + h((start[0], start[1]), (1, 1)) * heuristicWeight)

    opened = PriorityQueue()
    opened.put((costs[(start[0], start[1])], start))
    astarPath = {}

    while not opened.empty():
        currCell = opened.get()[1]
        if currCell[0] == 1 and currCell[1] == 1:
            break
        for d in 'ESNW':
            if m.maze_map[(currCell[0], currCell[1])][d]:
                moveX = 0
                moveY = 0
                if d == 'E':
                    moveY = 1
                if d == 'W':
                    moveY = -1
                if d == 'N':
                    moveX = -1
                if d == 'S':
                    moveX = 1

                childCellX = currCell[0] + moveX
                childCellY = currCell[1] + moveY

                pathCost = currCell[2] + 1 # depth of parent + 1
                heuristic = h((childCellX, childCellY), (1, 1)) # Manhattan distance from goal

                pathCost *= pathCostWeight
                heuristic *= heuristicWeight

                childCellCost = float(pathCost + heuristic)
                childCell = (childCellX, childCellY, currCell[2] + 1)

                if childCellCost < costs[(childCellX, childCellY)]:
                    costs[(childCellX, childCellY)] = childCellCost
                    opened.put((childCellCost, childCell))
                    astarPath[(childCellX, childCellY)] = (currCell[0], currCell[1])

    fwdPath = {}
    cell = (1, 1)
    while cell != (start[0], start[1]):
        fwdPath[astarPath[cell]] = cell
        cell = astarPath[cell]
    return fwdPath


def test_local(maze):
    weights = [1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2.0]
    weightCombinations = [(pw, hw) for pw in weights for hw in weights]

    file_name = "results" + os.sep + "results.txt"
    os.makedirs(os.path.dirname(file_name), exist_ok=True)

    with open(file_name, 'w') as file:
        file.write("pathCostWeight,heuristicWeight,timeElapsed" + '\n')

    with open(file_name, 'a') as file:
        for weights in weightCombinations:
            startTime = time.time()
            _ = astar(maze, weights[0], weights[1])
            endTime = time.time()

            timeElapsed = endTime - startTime
            file.write(str(weights[0]) + ',' + str(weights[1]) + ',' + str(timeElapsed) + '\n')


def test_metacentrum(maze):
    pathCostWeight = sys.argv[1]
    heuristicWeight = sys.argv[2]

    file_name = "results" + os.sep + "pcw-" + pathCostWeight + "-hw-" + heuristicWeight + ".txt"
    os.makedirs(os.path.dirname(file_name), exist_ok=True)

    with open(file_name, 'w') as file:
        file.write("pathCostWeight,heuristicWeight,timeElapsed" + '\n')

    with open(file_name, 'a') as file:
        startTime = time.time()
        _ = astar(maze, float(pathCostWeight), float(heuristicWeight))
        endTime = time.time()

        timeElapsed = endTime - startTime
        file.write(pathCostWeight + ',' + heuristicWeight + ',' + str(timeElapsed) + '\n')


if __name__ == '__main__':
    maze = maze(100, 100)
    maze.CreateMaze()

    # test_local(maze)
    test_metacentrum(maze)
