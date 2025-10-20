# AlgoViz
This project is a work in progress generalized algorithm visualizer. We are currently focusing on dynamic programming (DP) algorithms, and finding the best ways to visualize them intuitively for pedagogical use. We start by looking at a DP solution to subset sum, a problem that asks: given an array of nonnegative integers A and a goal nonnegative integer B, does there exist a subset of A such that the sum of all the elements is B?

## DP Matrix Representation
Watch as the algorithm first iterates through the base cases, then through the recurrence. Hovering over an entry reveals how a retrieval algorithm could recollect a valid subset. Clicking on an entry shows the subproblems that that entry relied on.

https://github.com/user-attachments/assets/bc6aaf83-ddb3-43f3-ad52-f51321cac860


## Decision Tree Representation
One paradigm of dynamic programming algorithms is to look at the nth element in an array, and make some sort of choice about that element, often whether or not it is a member of a subset. We encapsulate this idea into a decision tree, where users can see all paths, whether the element contributes to their subset or not. The tree also highlights the exponentation time complexity of a brute force approach, as opposed to the pseudopolynomial DP solution.

https://github.com/user-attachments/assets/08064f42-55c5-4ca7-a838-f1dcb1fa7ba6
