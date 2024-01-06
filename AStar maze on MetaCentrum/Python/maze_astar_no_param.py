import os
import time

from pyamaze_lib import maze
from queue import PriorityQueue


def h(cell1, cell2):
    x1, y1 = cell1
    x2, y2 = cell2
    return abs(x1 - x2) + abs(y1 - y2)


def astar(m):
    start = (m.rows, m.cols, 0)
    costs = {cell: float('inf') for cell in m.grid}
    costs[(start[0], start[1])] = float(h((start[0], start[1]), (1, 1)))

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

if __name__ == '__main__':
    maze = maze(100, 100)
    maze.CreateMaze()

    startTime = time.time()
    _ = astar(maze)
    endTime = time.time()

    timeElapsed = endTime - startTime
    print(timeElapsed)

    file_name = "results" + os.sep + "results.txt"
    os.makedirs(os.path.dirname(file_name), exist_ok=True)

    with open(file_name, 'w') as file:
        file.write(str(timeElapsed))
