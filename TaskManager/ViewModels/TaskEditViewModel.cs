using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskEditViewModel : TaskBaseViewModel
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        

    }
}
