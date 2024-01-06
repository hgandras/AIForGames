import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

from matplotlib import rcParams
rcParams.update({'figure.autolayout': True})

RESULTS_FILE = 'results/results.txt'

results = pd.read_csv(RESULTS_FILE)
results = results.round(3)
results = results.pivot(index='pathCostWeight', columns='heuristicWeight', values='timeElapsed')

heatmap = sns.heatmap(results, annot=True, fmt="g", cmap='viridis')
plt.show()

fig = heatmap.get_figure()
fig.savefig(RESULTS_FILE[:-4] + '.png', format='png', bbox_inches="tight")
