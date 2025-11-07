

// 最小堆实现

//IComparable<T> 的核心作用是提供对象之间的比较规则，这是最小堆（MinHeap<T>）正常工作的前提；
//MinHeap<T> 通过约束 T : IComparable<T>，确保堆中的元素可以比较，从而维护 “最小元素在顶部” 的特性；
//StateWithPriority 实现 IComparable<StateWithPriority>，是为了让自己能被放入最小堆，
//作为 A* 算法的优先队列，按优先级顺序处理搜索状态。

using System;
using System.Collections.Generic;

public class MinHeap<T> where T : IComparable<T>
{
    private List<T> heap;

    public int Count => heap.Count;

    public MinHeap()
    {
        heap = new List<T>();   
    }

    public MinHeap(int capacity)
    {
        heap = new List<T>(capacity); 
    }


    public void Push(T item)
    {
        heap.Add(item);
        HeapifyUp(heap.Count - 1);
    }

    public T Pop()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("堆为空");
        }
        T min = heap[0];    
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];  // 把最后一个拿出来垫一垫第一个位置。
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
        {
            HeapifyDown(0);  // 向下调整
        }

        return min;
    }
    
    // 向上调整堆（用于插入）
    private void HeapifyUp(int index)
    {
        // 不断和父节点比，向堆顶调整
        while (index > 0)
        {
            //这是堆的树形结构在数组中的存储规则，左子节点位置 = 2父节点 + 1，右子节点 = 2父节点 + 2
            int parentIndex = (index - 1) / 2;

            if (heap[index].CompareTo(heap[parentIndex]) < 0)
            {
                Swap(index, parentIndex);   
                index = parentIndex;
            }
            else
            {
                break;  
            }
        }
    }
    
    // 向下调整堆（用于删除）
    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            // 找出父节点和两个子节点中最小的
            if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
            {
                smallest = leftChild;   
            }

            if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
            {
                smallest = rightChild;  
            }
            
            if (smallest != index)
            {
                Swap(index, smallest);
                index = smallest;
            }
            else
            {
                break;
            }
        }
    }
    private void Swap(int i, int j)
    {
        T temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}

// 包装了一层TKGameState，带优先级，用于A*算法。
public class StateWithPriority : IComparable<StateWithPriority>
{
    public TKGameState State { get; set; }
    public float Priority { get; set; }
    public int Sequence { get; set; }

    // 比较的规则：用优先级比较
    public int CompareTo(StateWithPriority other)
    {
        if (other == null) return 1;
        int result = Priority.CompareTo(other.Priority);  // 优先级一样返回0；
        if(result == 0) return Sequence.CompareTo(other.Sequence);  
        return result;
    }
}

