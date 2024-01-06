#!/bin/bash

for pcw in $(seq -f "%0.2f" 1 0.1 2)
do
  for hw in $(seq -f "%0.2f" 1 0.1 2)
  do
    echo $pcw $hw
    qsub -v pathCostWeight=$pcw,heuristicWeight=$hw script.sh
  done
done
