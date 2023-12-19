---
layout: default
---
<div id="HeaderPics">

 <img src="./assets/img/heatmap.jpg" alt=""> 
  <img src="./assets/img/FlowField.PNG" alt=""> 

</div>

# Group Pathfinding With Flow Fields - Caleb Wiebolt

Below is the written report for my Final Project for CSCI 4611 Animation and Planning in Games class. For the project, I implemented a Flow Field pathfinding solution for groups of agents. The Flow Field is built on a grid representation and the integration field is calculated with the Eikonal equation, providing a smoother vector field when compared to methods based on a form of Dijkstra's algorithm. To look at the source code or a pre-built executable click the button below. 

<a href="{{ site.github.repository_url }}" class="btn btn-dark">Go to the Code</a>



## Features Attempted
### Presentation Video
{% include video1.html %}

### Results Video
{% include video2.html %}


## My project
For my project, inspired by the RTS games that I grew up playing, I wanted to implement a pathfinding solution for large groups of agents. From previous reading, I was aware of the idea of using flow fields for pathfinding instead of more traditional solutions like A* and decided that this would be both an interesting challenge and a fun-looking demo.

## The Algorithm

### Overview
There are many ways to go about pathfinding large groups. The default for most people would probably be an application of A* or one of its variants. The issue with A* is that for large groups of agents that are going to the same place, an individual path needs to be calculated for each. This creates a great deal of repetitive and overlapping computation. One solution to this is pathfinding using Flow Fields, which involves calculating a single vector field that provides a path from every location toward the goal. Each agent then simply references their position on the Flow Field to receive the local direction in which they need to travel to reach the goal.

The Flow Field algorithm works like this. First, we calculate an obstruction map. A map of values for each cell giving how difficult it is to travel through that cell. In my implementation, each cell keeps a byte indicating its weight, where 0 is unobstructed and 255 is fully impassible. Given that obstruction map, we can then calculate an integration map. This is a map of values that represents the distance that would need to be traveled in order to get to the goal cell. Given the integration map, we then calculate a vector field where the vectors in each cell point towards the direction the gradient is descending, ultimately leading towards the goal point.

The integration field can be calculated in many different ways. One of the computationally fastest ways to do this is with a version of Dijkstra's algorithm to step through each node going out from the goal position. The issue with this method is that because it is inherently discrete, the calculated integration field had noticeable square or diamond shape artifacting. This caused agents to take odd paths favoring the cardinal axis. After attempting this method I switched to calculating the integration field using the Eikonal equation, which is a partial differential equation used to model wave propagation amongst other things. While more computationally expensive, the field provided is much smoother and more realistic, similar to if a wave in water originates from the goal position and radiated outwards. There are several ways to perform these calculations, for instance, the Fast Marching Method. For my implementation, I used the Fast Iterative Method, described in the paper linked below.

To calculate the vector field I initially simply calculate the gradient at each cell. This caused issues however in very narrow bottlenecks. I observed that cells that were next to walls or other obstructions were so weighted by the high integration value that the vector almost always pointed directly away from the obstacle, causing agents to get stuck. To solve this I implemented a solution that checks if a cell is adjacent to an obstructed cell and if so, instead of calculating the actual gradient, the vector is simply pointed towards the cell with the lowest value. This effectively solved the issue.

### Bottlenecks and Improvements
One of the main bottlenecks with Flow Field pathfinding is that, while the computation of the field does not scale with the number of units like an A* solution, it does scale with the map size. Large maps very quickly become computationally untenable. Flow Fields also start to lose some of their advantages when many agents are pathing to many different goals. One improvement that could be implemented to lessen the effects of map size is a chunking solution. This is a method that was used in several papers that I reviewed. They described a hybrid solution that ran A* on a coarse map and then used Flow Fields locally. The project could also be improved by implementing local collision avoidance for the agents or perhaps some sort of flocking behavior in addition to the Flow Field algorithm.


## Connection To Course Material
This project has connections to many topics we covered in our course. The most obvious is the connections to our path planning section, talking about various search methods and A*. There is also an interesting overlap between the use of flow fields for pathfinding and the use of flow fields/vector fields for Eulerian Fluid Simulation. I also found that our section on partial differential equations, vector fields, and gradients, helped lay the foundation for my understanding of this method.


## Connection To the State of the Art
Flow Fields, while an old technology are used in modern games. One of the most recent examples of this is A Plague Tale which used a flow field system to control the movement of its swarms of rats that harry the character. In terms of pathfinding for large groups, most games utilize a hybrid system built on variants of A* and local steering algorithms. Recent papers have suggested different methods of weighting A* to incentivize group-like behavior. One paper I read described the use of a global direction map that would be learned as agents moved around the world. The idea was to weight A* to favor cells if the vector in those cells pointed in the same direction as the desired movement. This would essentially encourage paths to follow the directions of previous paths. Another paper I read suggested a similar method, this time based on ant pheromone trails, leaving markers that would influence A* to follow. These methods not only had interesting effects on group movement behavior but also performance as each A* path searched less and less of the graph as the weighting got higher in the direction of the paths of their group members. It is clear from my readings that real modern solutions for group movement in games involve a series of layered algorithms depending on the group behaviors and performance budget that the developers desire.


## Progress Discussion and Images
For the most part, my project progressed as expected. Sadly I didn't get to implement some of my stretch goals like multi-level terrain or dynamic obstacles. That being said I am very happy with what I was able to get done. My initial sketch and a screenshot of my progress part way through the process are both below.

### Initial Sketch
<img src="./assets/img/sketch1.png" alt="" style="max-width: 800px"> 

### Progress Screenshot
<img src="./assets/img/progressScreenshot.png" alt="" style="max-width: 800px"> 


## Feedback and Response
While much of my feedback was just generally positive and encouraging sentiments, one bit of feedback pushed me to put in the work to implement the Eiknol equation to get smoother-looking paths. Initially, I had a version working using Dijkstra's algorithm and was discussing the artifacting present at one of our progress sessions. I was unsure if I could comprehend the math well enough to implement a better solution. The feedback was the push I needed to attempt it and after many hours and a rather dense paper, I got the system working.

## Limitations and Further Work
One of the main limitations of the overall project is the steering behavior of the agents following the flow Field. They can occasionally behave somewhat erratically and not turn smoothly. Also, there are some occasional bugs with how the agents respond to Unity's physics system. A lot of this stems from a lack of local collision avoidance and a lack of proper arrival detection. Adding both would be great extensions to this project. It would also be interesting to see a chunked/hybrid version of this system working with an A* search done first on a coarse grid and then a smaller flow field built only along that path.

## Tools and Libraries Used
*   Unity 2022.3.9f1 and Visual Studio

## Papers and Resources
* <a href="https://en.wikipedia.org/wiki/Eikonal_equation"> Eikonal Equation Wiki Page </a>
* <a href="https://leifnode.com/2013/12/flow-field-pathfinding/"> Flow Field Pathfinding Tutorial by LeifNode </a>
* <a href="https://gamedev.stackexchange.com/questions/153000/fix-my-flow-field-pathfinding"> A Very Helpful StackOverflow Thread about Calculating the Integration Field</a>
* Continuum Crowds by Adrien Treuille, Seth Cooper, and Zoran Popovic
* A Fast Iterative Method for Eikonal Equations by Won-Ki Jeong and Ross T. Whitaker
* <a href="http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter23_Crowd_Pathfinding_and_Steering_Using_Flow_Field_Tiles.pdf"> GameAIPro Chapter 23 Crowd Apthfinding and Steering Using Flow Field Tiles</a>
 
