using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfindingLearn : MonoBehaviour
{
    public static List<NodeBase> FindPath(NodeBase startNode,NodeBase targetNode)
    {
        var toSearch = new List<NodeBase>() { startNode };  // 主要通过最底下一句来一边找一边更新
        var processed = new List<NodeBase>();

        while (toSearch.Any()) // 等价于 while (toSearch.Count > 0) 
        {
            var current = toSearch[0];
            foreach (var t in toSearch)
            {
                // 在总代价相同的节点中，优先选择离目标更近（H 值更小）的节点
                if (t.F < current.F || t.F == current.F && t.H < current.H)
                    current = t;
            }
            processed.Add(current); // 表示当前的节点已经被处理过了
            toSearch.Remove(current);

            // 检查结果
            if(current == targetNode)
            {
                var currentPathTitle = targetNode;
                var path = new List<NodeBase>();
                while (currentPathTitle != startNode)
                {
                    path.Add(currentPathTitle);
                    currentPathTitle = currentPathTitle.Connection;
                }
                return path;
            }

            foreach(var neighbor in current.Neighbors.Where(t => t.Walkable && !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbor);

                // 通过当前current更新出来的neighbor的G值
                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G)  // 更新后的G值小于原来的，需要更新
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current); //帮我们完成后重新追踪路劲

                    if (!inSearch) // 如果这个neighbor不在toSearch中，就设置下H（之前可能没有设置，）
                    {
                        neighbor.SetH(neighbor.GetDistance(targetNode)); 
                        toSearch.Add(neighbor); 
                    }
                }
            }
        }
        return null;
    }
}
