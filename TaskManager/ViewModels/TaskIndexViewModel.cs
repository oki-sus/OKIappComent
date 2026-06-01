using System;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskIndexViewModel
    {
		public string? SearchString { get; set; }
		public string? Category { get; set; }
		public string SortOrder { get; set; } = SortOrders.Ascending;

		public string? Status { get; set; }
		public string? Priority { get; set; }
		public DateTime? DueDateBefore { get; set; }

        // 完了済みを非表示にするフラグ
        public bool HideCompleted { get; set; }

        public List<string> Categories { get; set; } = new();
		public List<TaskManager.Models.TaskItem> Tasks { get; set; } = new();

		
	}
}
