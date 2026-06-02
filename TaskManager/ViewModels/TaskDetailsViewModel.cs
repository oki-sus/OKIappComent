using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    // Details用のViewModel
    public class TaskDetailsViewModel : TaskBaseViewModel
    {
        // TaskBaseViewModelでは足りないDetails(詳細)で表示するための作成日と更新日を保持
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}
