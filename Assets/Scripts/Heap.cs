//Heap used to improve efficiency by adding or removing the next candidate in the AStar algorithm

using System;

public class Heap<T> where T : IHeapItem<T>
{
    private readonly T[] _items;

    // Constructor that takes the maximum heap size
    public Heap(int maxHeapSize) 
    {
        _items = new T[maxHeapSize];
    }
    
    // Add an item to the heap
    public void Add(T item) 
    {
        item.HeapIndex = Count;
        _items[Count] = item;
        SortUp(item);
        Count++;
    }

    // Remove the first item from the heap
    public T RemoveFirst() 
    {
        T firstItem = _items[0];
        Count--;
        _items[0] = _items[Count];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }

    // Update the position of an item in the heap
    public void UpdateItem(T item) 
    {
        SortUp(item);
    }

    // Get the number of items in the heap
    public int Count { get; private set; }

    // Check if the heap contains an item
    public bool Contains(T item) 
    {
        return Equals(_items[item.HeapIndex], item);
    }

    // Sort an item down the heap until it's in the correct position
    private void SortDown(T item) 
    {
        while (true) 
        {
            var childIndexLeft = item.HeapIndex * 2 + 1;
            var childIndexRight = item.HeapIndex * 2 + 2;

            if (childIndexLeft < Count) 
            {
                // If the right child is greater than the left child, swap with them
                var swapIndex = childIndexLeft;

                if (childIndexRight < Count) 
                {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0) 
                    {
                        swapIndex = childIndexRight;
                    }
                }

                // If the child is greater than the parent, swap them
                if (item.CompareTo(_items[swapIndex]) < 0) 
                {
                    Swap (item,_items[swapIndex]);
                }
                else 
                {
                    return;
                }
            }
            else 
            {
                return;
            }
        }
    }

    // Sort an item up the heap until it's in the correct position
    private void SortUp(T item) 
    {
        var parentIndex = (item.HeapIndex-1)/2;
		
        while (true) 
        {
            T parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0) 
            {
                Swap (item,parentItem);
            }
            else 
            {
                break;
            }
            parentIndex = (item.HeapIndex-1)/2;
        }
    }

    // Swap two items in the heap
    private void Swap(T itemA, T itemB) {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
        (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
    }
}

// Interface for items that can be added to a heap
public interface IHeapItem<in T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}