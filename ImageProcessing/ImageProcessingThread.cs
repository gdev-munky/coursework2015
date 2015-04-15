using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ImageProcessing
{
    public class ImageProcessingThread
    {
        public ImageProcessingThread()
        {
            Tasks = new Queue<Rectangle>();
        }
        protected Thread MyThread { get; set; }
        protected Queue<Rectangle> Tasks { get; private set; }
        protected object QueueLock = new object();
        public int TasksCount { get; protected set; }
        public int CompletedTasksCount { protected get; set; }
        public int TasksLeft { get { return TasksCount - CompletedTasksCount; } }
        public bool IsRunning { get; private set; }
        public void PushTask(Rectangle block)
        {
            lock (QueueLock)
            {
                Tasks.Enqueue(block);
                TasksCount++;
            }
        }
        public Rectangle PopTask()
        {
            Rectangle p;
            lock (QueueLock)
            {
                p = Tasks.Dequeue();
            }
            return p;
        }

        public void ResetCounters()
        {
            if (IsRunning)
                throw new Exception("ResetCounters works only while thread is not working");
            TasksCount = Tasks.Count;
            CompletedTasksCount = 0;
        }

        public bool Finished { get { return TasksLeft < 1; } }

        public void Start(ImageProcessingThreadContext processingContext)
        {
            Wait();
            MyThread = new Thread(o =>
            {
                IsRunning = true;
                var c = (ImageProcessingThreadContext) o;
                while (!Finished)
                {
                    var block = PopTask();
                    for (var y = block.Top; y < block.Bottom; ++y)
                        for (var x = block.Left; x < block.Right; ++x)
                            c.Processor(x, y, c);
                    CompletedTasksCount++;
                }
                IsRunning = false;
            });
            MyThread.Start(processingContext);
        }

        public void Wait()
        {
            if (MyThread != null && IsRunning)
                MyThread.Join();
        }
    }

    public delegate void DProcessPixel(int x, int y, ImageProcessingThreadContext context);
}
