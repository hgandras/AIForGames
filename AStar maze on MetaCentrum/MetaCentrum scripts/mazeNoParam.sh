#!/bin/bash
#PBS -l select=1:ncpus=1:mem=4gb:scratch_local=1gb
#PBS -l walltime=1:00:00
#PBS -e TODO: set to the location where you want logs to be saved
#PBS -o TODO: set to the location where you want logs to be saved

DATADIR=TODO: set accordingly
RESULTDIR=TODO: set accordingly
ROOT=TODO: set accordingly

echo "$PBS_JOBID is running on node `hostname -f` in a scratch directory $SCRATCHDIR" >> $DATADIR/jobs_info.txt

module add python/python-3.7.7-intel-19.0.4-mgiwa7z || { echo >&2 "Error in module add python"; exit 2; }

# test if scratch directory is set
# if scratch directory is not set, issue error message and exit
test -n "$SCRATCHDIR" || { echo >&2 "Variable SCRATCHDIR is not set!"; exit 1; }

cp -R $DATADIR/maze_astar $SCRATCHDIR
cd $SCRATCHDIR/maze_astar

python maze_astar_no_param.py

cp -a results/. $RESULTDIR

clean_scratch
