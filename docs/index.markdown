---
layout: default
---
<div id="HeaderPics">

 <img src="./assets/img/heatmap.jpg" alt=""> 
  <img src="./assets/img/FlowField.png" alt=""> 

</div>

# Group Pathfinding With Flow Fields - Caleb Wiebolt

Below is the written report for my Final Project for CSCI 4611 Animation and Planning in Games class. For the project I implemented a Flow Field pathfinding solutions for groups of agents. The Flow Field is built on a grid representation and the integration field is calculated with the Eikonal equation, providing a smoother vector field when compared to methods based on a form of Dijkstra's algorithm. To look at the source code or a pre-built executable click the button below. 

<a href="{{ site.github.repository_url }}" class="btn btn-dark">Go to the Code</a>



## Features Attempted
### Presentation Video
{% include video1.html %}

### Results Video
{% include video2.html %}


## My project
For my project, inspured by the RTS games that I grew up plauing, I wanted to implement a pathfinding solution for large groups of agents. From previous reading I was aware of the idea of using flow fields for pathfinding instead of more traditional solutions like A* and decided that this would be both an interesting challenge and a fun looking demo.

## The Algorithm

### Overview
There are many ways to go about pathfinding large groups. The default for most people would probably be an applicaiton of A* or one of its variants. The issue with A* is that for large groups of agents that are going to the same place, an individual path needs to be calcualted for each. This creates a great deal of repatative and overlapping computation. One solution to this is pathfinding using Flow Fields, which involves calculating a single vector field that provides a path from every location towards the goal. Each agent then simply references their position on the Flow Field to recive the local direction in which they need to travel to reach the goal.

The Flow Field algorithm works like this. First we calculate an obstruction map. A map of values for each cell giving how difficult it is to travel through that cell. In my implementation each cell keeps a byte indicating its weight, where 0 is unubstructed and 255 is fully impassible. Given that obstruction map we can then calculate and integration map. This is a map of values that repsresents the distance that would need to be traveled in order to get to this cell. Given the integration map we then calculate a vector field where the vectors in each cell points towards the direction the gradient is decending, ultimatly leading towards the goal point.

The integration field can be cacluated in many different ways. One of the computationally fastest ways to do this is with a version of Dijkstra's algorithm to step through each node going out from the goal position. The issue with this methods was because it is inherently disccrete, the calculated integration field had noticable sqaure or dimond shape artifacting. This caused agents to take odd paths favoring the cardinal axis. After attemtpting this method I switched to calculating the integration field using the Eikonal equation, which is a partial differential equation used to model wave propogation amongst other things. While more computationally expesniev, the field provided is much smoother and more realistic, similar to if a wave in water originalted form the goal position and radiated outwards. There are several ways to perform these calcualtions, for instance the Fast Martching Method. For my implementation I used the Fast Iterative Method, described in the paper linked below.

To calculate the vector field I initially simply calculate the gradient at each cell. This caused issues however in very narrow bottlnecks. I observed that cells that were next to walls or other obstructions were so weighted by the high integration value that the vecotor almost always pointed directly away from the obstalce, causing agents to get stuck. To solve this I implemented a solution that checks if a cells is ajacent to a obstructed cell and if so, instead of calcualting the actual gradient, the vector is simply pointed towards the cell with the lowest value. This effectivley solved the issue.

### Bottlenecks and Improvements
One of the main bottlenecks with Flow Field pathfining is that, while the computation of the field does not scale with the number of units like an A* solution, it does scale with the map size. Large maps very quickly become computationally untennable. Flow Fields also start to loose some of their advantages when many agents are pathing to many different goals. One improvment that could be implemented to lesson the effective of map size is a chunking solution. This is a method that was used in several papers that I reviewed. They described a hybrid solution that ran A* on a coarse map and then used Flow Fields locally. The project could also be improved by implementing local collision avoidance for the agents or perhaps some sort of flocking behavior in addition to the Flow Field algorithm.


## Connection To Course Material
This project has connections to many topics we covered in our course. The most obvious is the connections to our path planning section, talking about various search methods and A*. There is also an interesting overlap between the use of flow fields for pathfinding and the use of flow fields/vector fields for eularian fluid simulation. I also found that our section on partial differential equations, vector fields, and gradients, helped lay the foundation for my understanding of this method.


## Connection To the State of the Art
Flow Fields, while an old technology are used in modern games. One of the most recent examples of this is A Pluage Tale which used a flow field system to control the movmeent of its swarms of rats that harry the charecter. In terms of pathfinding for large groups, most games use a system which utalizes a hybrid system nuilt on variants of A* and local steering algorithms. Recent papers have sugested different methods of weighting A* to incentivse group like behavior. One paper I ready described the use of a global direction map that would be learned as agents moved around the world. The idea was to weight A* to favor cells if the vector in those cells pointed in the same direction as the desired movment. This would essentially increntivse paths to follow the directions of previous paths. Another paper I read sugested a similar method, this time based on ant pheremone trails, leavign markers that would influence A* to follow. These methods not only had interesting effects on group movement behavior but also performance as each A* path searched less and less of the graph as the weighting got higher in the direction of the paths of their group memebers. It is clear from my readings that real modern soltuions for group movment in games involve a series of layered algorithms depending on the group behaviors and performance budget that the developers desire.


## Progress Dicussion and Images
For the most part my project progressed as expected. Sadly I didn't get to implement some of my stretch goals like multi level terrain or dynamic obstacles. That being said i am very happy with what I was able to get done. My initial sketch and a screenshot of my progress part way through the process are both below.

### Initial Sketch
<img src="./assets/img/sketch1.png" alt="" style="max-width: 800px"> 

### Progress Screenshot
<img src="./assets/img/progressScreenshot.png" alt="" style="max-width: 800px"> 


## Feedback and Response
While much of my feedback was just general posative and ecouraging sentiments, one bit of feedback really pushed me to put in the work to implement the Eiknol equation to get smoother looking paths. Initially i had a version working using Dijkstras algorhtm and was dicussing the artifacitng present at one of our progress sessions. I was unsure if I could comprehend the math well enought to implement a better solution. The feedback was the push I needed to attempt it and after many hours and a rather dense paper I got the system working.

## Limitations and Further Work
One of the main limiations of the overall project is the steering behavior of the aggents following the flow Field. They can occasionally behaves somewhat arraticly and not turn smoothly. Also there are some occasional bugs with how the agents responds to unitys physics system. A lot of this stems from a lack of local collision avoidance and a lack of proper arival detection. Adding both would be great extensions to this project. It would also be interesting to see a chunked/hybrid version of this system working with a A* search done first on a coarse grid and then a smaller flow field built only along that path.

## Tools and Libraries Used
*   Unity 2022.3.9f1 and Visual Studio

## Papers and Resources
* <a href="https://en.wikipedia.org/wiki/Eikonal_equation"> Eikonal Equation Wiki Page </a>
* <a href="https://leifnode.com/2013/12/flow-field-pathfinding/"> Flow Field Pathfinding Tutorial by LeifNode </a>
* <a href="https://gamedev.stackexchange.com/questions/153000/fix-my-flow-field-pathfinding"> A Very Helpful StackOverflow Thread about Calculating the Integration Field</a>
* Continuum Crowds by Adrien Treuille, Seth Cooper, and Zoran Popovic
* A Fast Iterative Method for Eikonal Equations by Won-Ki Jeong and Ross T. Whitaker
* <a href="http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter23_Crowd_Pathfinding_and_Steering_Using_Flow_Field_Tiles.pdf"> GameAIPro Chapter 23 Crowd Apthfinding and Steering Using Flow Field Tiles</a>
 