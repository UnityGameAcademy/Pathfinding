using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// priority queue implemented using generic list with min binary heap
public class PriorityQueue<T> where T : IComparable<T>
{
    // List of items in our queue
    List<T> data;

    // number of items currently in queue
    public int Count { get { return data.Count; }}

    // constructor
    public PriorityQueue()
    {
        this.data = new List<T>();
    }

    // add an item to the queue and sort using a min binary heap
    public void Enqueue(T item)
    {
        // add the item to the end of the data List
        data.Add(item);

        // start at the last position in the heap
        int childindex = data.Count - 1;

        // sort using a min binary heap
        while (childindex > 0)
        {
            // find the parent position in the heap
            int parentindex = (childindex - 1) / 2;

            // if parent and child are already sorted, stop sorting
            if (data[childindex].CompareTo(data[parentindex]) >= 0)
            {
                break;
            }

            // ... otherwise, swap parent and child
            T tmp = data[childindex];
            data[childindex] = data[parentindex];
            data[parentindex] = tmp;

            // move up one level in the heap and repeat until sorted
            childindex = parentindex;

        }
    }

    // remove an item from queue and keep it sorted using a min binary heap
    public T Dequeue()
    {
        // get the index for the last item
        int lastindex = data.Count - 1;

        // store the first item in the List in a variable
        T frontItem = data[0];

        // replace the first item with the last item
        data[0] = data[lastindex];

        // shorten the queue and remove the last position 
        data.RemoveAt(lastindex);

        // decrement our item count
        lastindex--;

        // start at the beginning of the queue to sort the binary heap
        int parentindex = 0;

        // sort using min binary heap
        while (true)
        {
            // choose the left child
            int childindex = parentindex * 2 + 1;

            // if there is no left child, stop sorting
            if (childindex > lastindex)
            {
                break;
            }

            // the right child
            int rightchild = childindex + 1;

            // if the value of the right child is less than the left child, switch to the right branch of the heap
            if (rightchild <= lastindex && data[rightchild].CompareTo(data[childindex]) < 0)
            {
                childindex = rightchild;
            }

            // if the parent and child are already sorted, then stop sorting
            if (data[parentindex].CompareTo(data[childindex]) <= 0)
            {
                break;
            }

            // if not, then swap the parent and child
            T tmp = data[parentindex];
            data[parentindex] = data[childindex];
            data[childindex] = tmp;

            // move down the heap onto the child's level and repeat until sorted
            parentindex = childindex;

        }

        // return the original first item
        return frontItem;
    }

    // look at the first item without dequeuing 
    public T Peek()
    {
        T frontItem = data[0];
        return frontItem;
    }

    // is the item contained in the data List?
    public bool Contains(T item)
    {
        return data.Contains(item);
    }

    // return the data as a generic List
    public List<T> ToList()
    {
        return data;
    }

}
