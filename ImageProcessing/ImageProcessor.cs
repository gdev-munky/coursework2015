using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageProcessing
{
    public abstract class TemplateImageProcessor
    {
        protected abstract IEnumerable<ImageProcessingTask> GetTasks();

        public virtual bool Run(out TimeSpan totalTimeSpent)
        {
            var timeStart = DateTime.UtcNow;
            var context = new ImageProcessingContext();
            var result = GetTasks().All(task => task.Run(context));
            totalTimeSpent = DateTime.UtcNow - timeStart;
            return result;
        }
    }
    public class CustomImageProcessor : TemplateImageProcessor
    {
        public CustomImageProcessor()
        {
            TaskQueue = new Queue<ImageProcessingTask>();
        }
        public Queue<ImageProcessingTask> TaskQueue { get; set; }
        public bool Run(IEnumerable<ImageProcessingTask> tasks, out TimeSpan totalTimeSpent)
        {
            var timeStart = DateTime.UtcNow;
            var context = new ImageProcessingContext();
            var result = tasks.All(task => task.Run(context));
            totalTimeSpent = DateTime.UtcNow - timeStart;
            return result;
        }

        protected override IEnumerable<ImageProcessingTask> GetTasks()
        {
            return TaskQueue;
        }
    }
}
 