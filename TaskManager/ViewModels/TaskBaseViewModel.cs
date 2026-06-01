using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskBaseViewModel
    {
        // 各画面で使うタスクの入れ物
        public int Id { get; set; }

        [Required(ErrorMessage = "必須項目です")]
        [StringLength(100, ErrorMessage = "タイトルは100文字以内で入力してください")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "必須項目です")]
        [StringLength(50, ErrorMessage = "カテゴリは50文字以内で入力してください")]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "必須項目です")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = TaskStatuses.NotStarted;

        public string Priority { get; set; } = TaskPriority.Medium;

        [StringLength(1000, ErrorMessage = "詳細は1000文字以内で入力してください")]
        public string? Detail { get; set; }

        public virtual void MapFromEntity(TaskItem entity)
        {
            this.Id = entity.Id;
            this.Title = entity.Title;
            this.Category = entity.Category;
            this.DueDate = entity.DueDate;
            this.Status = entity.Status;
            this.Priority = entity.Priority;
            this.Detail = entity.Detail;
        }

    }
}
