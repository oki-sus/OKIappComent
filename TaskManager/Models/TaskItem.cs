using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.Models
{
	public class TaskItem
	{
        [Key]		// 主キー
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]		// 自動採番(番号割り当て)
        [Column("Id")]		// DBのカラム(列)を指定
        public int Id { get; set; } = 0;

        [Required(ErrorMessage = "必須項目です")]		// 必須入力にする
		[Display(Name = "タイトル")]		// 画面表示
        [StringLength(100, ErrorMessage = "タイトルは100文字以内で入力してください")]		// 文字数制限
        public string Title { get; set; } = string.Empty;

		[Required(ErrorMessage = "必須項目です")]     // 必須入力にする
		[Display(Name = "カテゴリ")]        // 画面表示
		[StringLength(50, ErrorMessage = "カテゴリは50文字以内で入力してください")]			// 文字数制限
		public string Category { get; set; } = string.Empty;

		[Required(ErrorMessage = "必須項目です")]     // 必須入力にする
		[Display(Name = "期限")]      // 画面表示
		[DataType(DataType.Date)]
		public DateTime DueDate { get; set; }

		[Display(Name = "状態")]      // 画面表示
		public string Status { get; set; } = TaskStatuses.NotStarted;

		[Display(Name = "優先度")]     // 画面表示
		public string Priority { get; set; } = TaskPriority.Medium;

		[Display(Name = "詳細")]      // 画面表示
		[StringLength(1000, ErrorMessage = "詳細は1000文字以内で入力してください")]			// 文字数制限
		public string? Detail { get; set; }

		// 管理・追跡用カラム
		public DateTime CreatedAt { get; set; } = DateTime.Now;		// いつ作ったか
		public string? CreatedBy { get; set; }						// 誰が作ったか
		public DateTime UpdatedAt { get; set; } = DateTime.Now;		// 最終更新日時

	}
}