using UnityEngine;
using System.Collections.Generic; // Required for Queue<T>


public class QuePlayOrder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Declare a Queue to store strings (you can use any type)
    private Queue<string> messageQueue = new Queue<string>();

    void Start()
    {
        // Enqueue (add) elements to the end of the queue
        EnqueueMessage("First message arrived.");
        EnqueueMessage("Second message arrived.");
        EnqueueMessage("Third message arrived.");

        // Dequeue (remove) elements from the front of the queue
        //ProcessNextMessage();
        //ProcessNextMessage();
        //ProcessNextMessage();
        //ProcessNextMessage(); // Attempt to dequeue from an empty queue
    }

    // Method to add a message to the queue
    void EnqueueMessage(string message)
    {
        messageQueue.Enqueue(message);
        Debug.Log($"Enqueued: {message}. Queue count: {messageQueue.Count}");
    }

    // Method to process the next message in the queue (FIFO)
    void ProcessNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            // Dequeue removes and returns the element at the front
            string nextMessage = messageQueue.Dequeue();
            Debug.Log($"Dequeued and processed: {nextMessage}. Queue count: {messageQueue.Count}");
        }
        else
        {
            Debug.LogWarning("Message queue is empty. No messages to process.");
        }
    }

    // You can also peek at the next item without removing it
    void PeekNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            string nextMessage = messageQueue.Peek();
            Debug.Log($"Next message to be processed (peeked): {nextMessage}. Queue count: {messageQueue.Count}");
        }
        else
        {
            Debug.LogWarning("Message queue is empty. No messages to peek.");
        }
    }

    
}
