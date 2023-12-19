using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Rendering.DebugUI.MessageBox;
using UnityEngine.UIElements;

public class PathfindingGrid : MonoBehaviour
{
    //Singleton Pattern
    private static PathfindingGrid _instance;
    public static PathfindingGrid Instance { get { return _instance; } }

    //TODO: 
    // - Add hack for fixing bottlnecks around obstacles


    //Grid Settings
    public float cellSize = 0.5f;
    public uint cellCountX = 10;
    public uint cellCountY = 10;

    public Vector3 GridOrigin;
    public float BestCostMax = 1000;
    public GridCell[,] cells;

    public bool UseEikonal = true;

    // Debug and Visulaization Controls
    public bool ShowDebugDisplay = true;
    public bool ShowGrid = false;
    public bool ShowImpassible = false;
    public bool ShowIntegrationField = false;
    public bool ShowWeightField = false;
    public bool ShowHeatMap = false;
    public bool ShowArrows = false;


    //Static method to build a Flow Field
    public static void GenFlowField(Vector3 goalPosition)
    {
        //Generate the flow field
        Instance.BuildFlowFeild(goalPosition);
    }

    public static Vector2 GetDirectionAtPosition(Vector3 position)
    {
        return Instance.GetCellAtWorldPos(position).bestDir;
    }

    private void Awake()
    {
        //Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        //Initializae the Grid
        InitPathfindingGrid();

        //Build the Cost field
        GenerateGridCosts();

    }

    public void InitPathfindingGrid()
    {
        cells = new GridCell[cellCountX, cellCountY];

        for (int x = 0; x < cellCountX; x++)
        {
            for (int y = 0; y < cellCountY; y++)
            {
                //Get the cells position
                Vector3 pos = new Vector3(GridOrigin.x + (x * cellSize), GridOrigin.y, GridOrigin.z + (y * cellSize));

                //Create cell
                cells[x, y] = new GridCell(pos, new Vector2Int(x, y), 1);
            }
        }

    }

    //Generates Grid Cost Field
    public void GenerateGridCosts()
    {
        //Ran once to generate the cost/obstuction grid globally

        //Iterate throug every cell and do a collision check
        Vector3 colliderExtents = Vector3.one * cellSize / 2;
        LayerMask mask = LayerMask.GetMask("Static Obst");


        for (int x = 0; x < cellCountX; x++)
        {
            for (int y = 0; y < cellCountY; y++)
            {
                Vector3 position = new Vector3(cells[x, y].worldPosition.x, cells[x, y].worldPosition.y + (cellSize / 2), cells[x, y].worldPosition.z);
                Collider[] hits = Physics.OverlapBox(position, colliderExtents, Quaternion.identity, mask);
                //Debug.Log(hits.Length);
                if (hits.Length > 0)
                {
                    //Mark cell as impassible
                    cells[x, y].weight = 255;
                }
            }
        }
    }
   

    //Integration using a Dikstra like method(returns discrete values
    public void DijkstraIntegration(GridCell goalCell)
    {
        if (goalCell.weight == 255)
        {
            Debug.Log("Goal is Impassible");
        }

        //Basic system based on dijkstra, doing this differently would be slower but would result in nicer paths
        goalCell.bestCost = 1;

        foreach (GridCell c in cells)
        {
            if (c == goalCell) { continue; }
            c.bestCost = BestCostMax;
        }



        Queue<GridCell> fringe = new Queue<GridCell>();

        fringe.Enqueue(goalCell);

        //While fringe is not empty
        while (fringe.Count > 0)
        {
            GridCell currentCell = fringe.Dequeue();


            //Get the neighbors
            GridCell[] nbrs = GetNeighbors(currentCell.index);

            foreach (GridCell nbr in nbrs)
            {
                if (nbr == null) { continue; }
                if (nbr.weight == 255) { continue; }

                if (nbr.weight + currentCell.bestCost < nbr.bestCost)
                {
                    nbr.bestCost = (ushort)(nbr.weight + currentCell.bestCost);
                    fringe.Enqueue(nbr);
                }

            }
        }



    }

    //Fast Iterative Method Eikonal solver
    public void FIMIntegration(GridCell goalCell)
    {
        if (goalCell.weight == 255)
        {
            Debug.Log("Goal is Impassible");
        }

        //Active list
        List<GridCell> active = new List<GridCell>();

        //Set intial costs 
        goalCell.bestCost = 1;
        foreach (GridCell x in cells)
        {
            if (x != goalCell)
            {
                x.bestCost = BestCostMax;
            }
        }

        //Add neighbors of of the goal to the active list
        GridCell[] nbrs = GetNeighbors(goalCell.index);

        foreach (GridCell nbr in nbrs)
        {
            active.Add(nbr);
        }


        float v = U(active[0]);

        //Update the points in the active list
        while (active.Count > 0)
        {
            for (int n = 0; n < active.Count; n++)
            {
                GridCell x = active[n];


                float p = x.bestCost;
                float q = U(x);
                x.bestCost = q;

                if (Mathf.Abs(p - q) < float.Epsilon)
                {
                    foreach (GridCell nbr in GetNeighbors(x.index))
                    {
                        if (nbr == null) { continue; }
                        if (active.Contains(nbr) == false)
                        {
                            p = nbr.bestCost;
                            q = U(nbr);
                            if (p > q)
                            {
                                nbr.bestCost = q;
                                active.Add(nbr);
                            }
                        }
                    }

                    //Remove x from active
                    active.Remove(x);
                    n--;
                }
            }

        }

    }
    public float U(GridCell cell)
    {
        //Solve quadratic equation

        float f = cell.weight; //math.abs(cell.weight - 255);
        float cellSize = 1.0f;


        if (f == 255)
        {
            f = 0;
        }


        GridCell[] nbrs = GetNeighbors(cell.index);
        float Ux;
        float Uy;
        if (nbrs[0] == null)
        {
            Uy = nbrs[2].bestCost;
        }
        else if (nbrs[2] == null)
        {
            Uy = nbrs[0].bestCost;
        }
        else
        {
            Uy = Mathf.Min(nbrs[0].bestCost, nbrs[2].bestCost);
        }

        if (nbrs[3] == null)
        {
            Ux = nbrs[1].bestCost;
        }
        else if (nbrs[1] == null)
        {
            Ux = nbrs[3].bestCost;
        }
        else
        {
            Ux = Mathf.Min(nbrs[1].bestCost, nbrs[3].bestCost);
        }


        if (Mathf.Abs(Ux - Uy) >= cellSize / f)
        {
            //Lower dimensional solution
            var result = math.min(Ux + 1 / f, Uy + 1 / f);
            return result;
        }

        float U = (Ux + Uy + math.sqrt((Ux + Uy) * (Ux + Uy) - 2 * (Ux * Ux + Uy * Uy - cellSize / (f * f)))) * 0.5f;

        return U;


    }

    public void BuildFlowFeild(Vector3 goalPosition)
    {
        GridCell goalCell = GetCellAtWorldPos(goalPosition);

        if (UseEikonal == true)
        {
            //Use Fast Iterative Eikonal
            FIMIntegration(goalCell);
        }
        else
        {
            //Dijksras
            DijkstraIntegration(goalCell);
        }

        //Turn the integration field into a flow/vector field.
        foreach (GridCell c in cells)
        {
            c.bestDir = -genVector(c).normalized;
        }


    }
    public Vector2 genVector(GridCell c)
    {
        //Calculate the gradient 

        GridCell[] nbrs = Get8Neighbors(c.index);


        float[] costs = new float[nbrs.Length];
        bool hasOBST = false;

        for (int i= 0; i < nbrs.Length; i++)
        {
            if (nbrs[i] == null)
            {
                //Set max cost and flag a wall
                costs[i] = BestCostMax;
                hasOBST = true;
            }
            else
            {
                //Get cost
                costs[i] = nbrs[i].bestCost;
                if (costs[i] == BestCostMax)
                {
                    hasOBST = true;
                }
            }
        }


        //Check if the nhbrs contain a wall
        if (hasOBST)
        {
            //Use the min method
            GridCell minCell = null;
            for (int x=1; x< nbrs.Length; x++)
            {
                if (nbrs[x] == null) {  continue; }
                if (minCell == null || minCell.bestCost > nbrs[x].bestCost)
                {
                    minCell = nbrs[x];
                }
            }

            //Calculate the direction
            Vector2 offset = c.index - minCell.index;
            //Debug.Log(offset);
            return offset.normalized;
        }
        else
        {
            //Calculate gradient
            Vector3 y = (costs[0] - costs[4]) * (Vector2.up);
            Vector3 x = (costs[2] - costs[6]) * (Vector2.right);
            Vector3 d1 = (costs[1] - costs[5]) * (Vector2.up + Vector2.right);
            Vector3 d2 = (costs[3] - costs[7]) * (Vector2.down + Vector2.right);
            return (x + y + d1 + d2) / 4;

        }
    }



    //Return the cell at a world position
    public GridCell GetCellAtWorldPos(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - GridOrigin - (cellSize/2 * Vector3.back) - (cellSize/2 * Vector3.left);

        int x = Mathf.FloorToInt(relativePos.x / cellSize);
        int y = Mathf.FloorToInt(relativePos.z / cellSize);

        int indexX = (int)Mathf.Clamp(x, 0, cellCountX - 1);
        int indexY = (int)Mathf.Clamp(y, 0, cellCountY - 1);

        return cells[indexX, indexY];
    }

    //Return the cardinal neighbors
    public GridCell[] GetNeighbors(Vector2Int index)
    {
        GridCell[] nbrs = new GridCell[4];

        //Get cardinal neighbors
        //North
        Vector2Int pos = index + Vector2Int.up;
        if (pos.y < cellCountY)
        {
            nbrs[0] = cells[pos.x, pos.y];
        }

        //East 
        pos = index + Vector2Int.right;
        if (pos.x < cellCountX)
        {
            nbrs[1] = cells[pos.x, pos.y];
        }

        //South
        pos = index + Vector2Int.down;
        if (pos.y >= 0)
        {
            nbrs[2] = cells[pos.x, pos.y];
        }

        //West
        pos = index + Vector2Int.left;
        if (pos.x >= 0)
        {
            nbrs[3] = cells[pos.x, pos.y];
        }

        //Return neighbors
        return nbrs;

    }


    //Return all 8 directions
    public GridCell[] Get8Neighbors(Vector2Int index)
    {
        GridCell[] nbrs = new GridCell[8];

        //Get cardinal neighbors
        //North
        Vector2Int pos = index + Vector2Int.up;
        if (pos.y < cellCountY)
        {
            nbrs[0] = cells[pos.x, pos.y];
        }

        //NE
        pos = index + Vector2Int.up + Vector2Int.right;
        if (pos.x < cellCountX && pos.y < cellCountY)
        {
            nbrs[1] = cells[pos.x, pos.y];
        }

        //East 
        pos = index + Vector2Int.right;
        if (pos.x < cellCountX)
        {
            nbrs[2] = cells[pos.x, pos.y];
        }

        //SE
        pos = index + Vector2Int.down + Vector2Int.right;
        if (pos.x < cellCountX && pos.y >= 0)
        {
            nbrs[3] = cells[pos.x, pos.y];
        }


        //South
        pos = index + Vector2Int.down;
        if (pos.y >= 0)
        {
            nbrs[4] = cells[pos.x, pos.y];
        }

        //SW
        pos = index + Vector2Int.down + Vector2Int.left;
        if (pos.x >= 0 && pos.y >= 0)
        {
            nbrs[5] = cells[pos.x, pos.y];
        }

        //West
        pos = index + Vector2Int.left;
        if (pos.x >= 0)
        {
            nbrs[6] = cells[pos.x, pos.y];
        }

        //NW
        pos = index + Vector2Int.up + Vector2Int.left;
        if (pos.x >= 0 && pos.y < cellCountY)
        {
            nbrs[7] = cells[pos.x, pos.y];
        }


        //Return neighbors
        return nbrs;

    }


    //Draw debug 
#if UNITY_EDITOR

    public void OnDrawGizmos()
    {
        if (ShowDebugDisplay && Application.isPlaying)
        {
            //Draw the grid 
            for (int x = 0; x < cellCountX; x++)
            {
                for (int y = 0; y < cellCountY; y++)
                {
                    DrawDebugCell(cells[x, y].worldPosition, cells[x, y].weight, cells[x, y].bestCost, cells[x, y].bestDir);
                }
            }
        }

    }
    private void DrawDebugCell(Vector3 postion, byte weight, float bestCost, Vector2 dir)
    {
        //Cell outlines in yellow
        if (ShowGrid)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(postion, new Vector3(cellSize, 0.0f, cellSize));
            Gizmos.DrawCube(postion, new Vector3(0.05f, 0.05f, 0.05f));
        }

        //Indicate Impassible
        if (ShowImpassible && weight == 255)
        {
            Gizmos.color = new Color(255, 0, 0, 0.5f);
            Gizmos.DrawCube(postion, Vector3.one * cellSize);

        }

        //Draw Direcitonal Arrows
        if (ShowArrows)
        {
            Gizmos.color = Color.blue;
            Vector3 arrowDir = new Vector3(dir.x, 0.0f, dir.y);
            if (arrowDir != Vector3.zero)
            {
                DrawArrow.ForGizmo(postion + -(arrowDir * 0.5f) + (Vector3.up * 0.5f), arrowDir); ;
            }

        }

        //Draw a heatmap
        if (ShowHeatMap)
        {
            //Get value
            float v = (bestCost / 100);

            //Make color in HSV
            Gizmos.color = Color.HSVToRGB(v, 1.0f, 1.0f);

            //Draw cube
            Gizmos.DrawCube(postion, new Vector3(cellSize, 0.01f, cellSize));
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        //Draw Labels for Integration Field
        if (ShowIntegrationField)
        {
            Handles.Label(postion, bestCost.ToString(),style);
        }
        else if (ShowWeightField)
        {
            //Draw Labels for the Weights Field
            Handles.Label(postion, weight.ToString(), style);
        }


    }
#endif

}



//Represents a cell
public class GridCell
{
    public Vector3 worldPosition;
    public Vector2Int index;
    public byte weight; //Maxcost is 255

    public float bestCost;

    public Vector2 bestDir;


    public GridCell(Vector3 worldPosition, Vector2Int index, byte weight)
    {
        this.worldPosition = worldPosition;
        this.index = index;
        this.weight = weight;
        this.bestCost = float.MaxValue;
        this.bestDir = Vector2.zero;
    }
}

  
public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
}




/*
public Vector2 genVector(GridCell c)
{
    //Calculate the gradient 

    GridCell[] nbrs = Get8Neighbors(c.index);


    float c1 = c.bestCost;
    float c2 = BestCostMax;
    float c3 = BestCostMax;
    float c4 = BestCostMax;
    float c5 = BestCostMax;
    float c6 = BestCostMax;
    float c7 = BestCostMax;
    float c8 = BestCostMax;
    float c9 = BestCostMax;

    //Calculate each direction and sum

    if (nbrs[0] != null)
    {
        c2 = nbrs[0].bestCost;
    }
    if (nbrs[1] != null)
    {
        c3 = nbrs[1].bestCost;
    }
    if (nbrs[2] != null)
    {
        c4 = nbrs[2].bestCost;
    }
    if (nbrs[3] != null)
    {
        c5 = nbrs[3].bestCost;
    }
    if (nbrs[4] != null)
    {
        c6 = nbrs[4].bestCost;
    }
    if (nbrs[5] != null)
    {
        c7 = nbrs[5].bestCost;
    }
    if (nbrs[6] != null)
    {
        c8 = nbrs[6].bestCost;
    }
    if (nbrs[7] != null)
    {
        c9 = nbrs[7].bestCost;
    }

    //Calculate gradients
    Vector2 grad1 = computeCrossGradient(c9, c2, c8, c1);
    Vector2 grad2 = computeCrossGradient(c2, c3, c1, c4);
    Vector2 grad3 = computeCrossGradient(c8, c1, c7, c6);
    Vector2 grad4 = computeCrossGradient(c1, c4, c6, c5);

    return (grad1 + grad2 + grad3 + grad4) / 4;

}
*/
/*
public Vector2 computeCrossGradient(float c1, float c2, float c3, float c4)
{
    //Compute x
    return new Vector2((c4-c1)/1.414213f, (c2 - c3) / 1.414213f);
}
*/
/*
public Vector2 genVector(GridCell c)
{
    GridCell[] nbrs = Get8Neighbors(c.index);

    Vector2 dir = Vector2.zero;

    //Calculate each direction and sum

    if (nbrs[0] != null)
    {
        dir += (Vector2.up) * (1 / nbrs[0].bestCost);
    }

    if (nbrs[1] != null)
    {
        dir += (Vector2.up + Vector2.right).normalized * (1 / nbrs[1].bestCost);
    }

    if (nbrs[2] != null)
    {
        dir += (Vector2.right) * 1 / nbrs[2].bestCost;
    }

    if (nbrs[3] != null)
    {
        dir += (Vector2.down + Vector2.right).normalized * (1 / nbrs[3].bestCost);
    }

    if (nbrs[4] != null)
    {
        dir += (Vector2.down) * (1 / nbrs[4].bestCost);
    }

    if (nbrs[5] != null)
    {
        dir += (Vector2.down + Vector2.left).normalized * (1 / nbrs[5].bestCost);
    }

    if (nbrs[6] != null)
    {
        dir += (Vector2.left) * (1 / nbrs[6].bestCost);
    }

    if (nbrs[7] != null)
    {
        dir += (Vector2.left + Vector2.up).normalized * (1 / nbrs[7].bestCost);
    }

    return dir;
}


*/

/*
GridCell[] nbrs = GetNeighbors(c.index);

float minX;
float minY;

if (nbrs[0] == null)
{
    minY = nbrs[2].bestCost;
}
else if (nbrs[2] == null)
{
    minY = nbrs[0].bestCost;
}
else
{
    minY = Mathf.Min(nbrs[0].bestCost, nbrs[2].bestCost);
}

if (nbrs[3] == null)
{
    minX = nbrs[1].bestCost;
}
else if (nbrs[1] == null)
{
    minX = nbrs[3].bestCost;
}
else
{
    minX = Mathf.Min(nbrs[1].bestCost, nbrs[3].bestCost);
}

var vec = new Vector2(minX - c.bestCost, minY - c.bestCost);

if (nbrs[3] != null && nbrs[1] != null && Mathf.Abs(nbrs[3].bestCost) > math.abs(nbrs[1].bestCost))
    vec.x *= -1;
if (nbrs[2] != null && nbrs[0] != null && math.abs(nbrs[2].bestCost) > math.abs(nbrs[0].bestCost))
    vec.y *= -1;

c.bestDir = vec.normalized;
*/
/*
GridCell[] nbrs = GetNeighbors(c.index);

bool t = false;
foreach (GridCell nbr in nbrs)
{
    if (nbr == null) { t = true; }
}

if (t) { continue; }

Vector2 dir = new Vector2(nbrs[1].bestCost - nbrs[3].bestCost, nbrs[0].bestCost- nbrs[2].bestCost).normalized;
c.bestDir = -dir;

*/


// Try 1/x? something more complex?

//Or try 
//Vector.x = left_tile.distance - right_tile.distance
//Vector.y = up_tile.distance - down_tile.distance