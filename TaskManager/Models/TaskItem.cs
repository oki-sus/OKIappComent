using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.Models
{
	public class TaskItem
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; } = 0;

        [Required(ErrorMessage = "必須項目です")]
		[Display(Name = "タイトル")]
        [StringLength(100, ErrorMessage = "タイトルは100文字以内で入力してください")]
        public string Title { get; set; } = string.Empty;

		[Required(ErrorMessage = "必須項目です")]
		[Display(Name = "カテゴリ")]
        [StringLength(50, ErrorMessage = "カテゴリは50文字以内で入力してください")]
        public string Category { get; set; } = string.Empty;

		[Required(ErrorMessage = "必須項目です")]
		[Display(Name = "期限")]
		[DataType(DataType.Date)]
		public DateTime DueDate { get; set; }

		[Display(Name = "状態")]
		public string Status { get; set; } = TaskStatuses.NotStarted;

		[Display(Name = "優先度")]
		public string Priority { get; set; } = TaskPriority.Medium;

		[Display(Name = "詳細")]
        [StringLength(1000, ErrorMessage = "詳細は1000文字以内で入力してください")]
        public string? Detail { get; set; }

		// 管理・追跡用カラム
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public string? CreatedBy { get; set; }
		public DateTime UpdatedAt { get; set; } = DateTime.Now;


		

	}
}