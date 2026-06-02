using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    // Edit用のViewModel
    public class TaskEditViewModel : TaskBaseViewModel
    {
		// TaskBaseViewModelでは足りないEdit(編集)で表示するための作成日を保持
		public DateTime CreatedAt { get; set; } = DateTime.Now;

        

    }
}
