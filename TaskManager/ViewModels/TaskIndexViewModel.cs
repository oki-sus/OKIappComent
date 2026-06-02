using System;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
	// タスク一覧画面のためのデータの入れ物:検索条件と表示データ

	// Index用のViewModel
    public class TaskIndexViewModel
    {
		public string? SearchString { get; set; }		// キーワード

		public string? Category { get; set; }			// カテゴリ

		public string SortOrder { get; set; } = SortOrders.Ascending;		// 並び変え順(昇順降順)


		public string? Status { get; set; }			// ステータス

		public string? Priority { get; set; }		// 優先度

		public DateTime? DueDateBefore { get; set; }	// 期限日


        // 完了済みを非表示にするフラグ
        public bool HideCompleted { get; set; }


		// 検索ドロップダウンに表示するための重複なしカテゴリ一覧リスト
		public List<string> Categories { get; set; } = new();

		// 画面に表示する絞り込み済みのタスク一覧データ
		public List<TaskManager.Models.TaskItem> Tasks { get; set; } = new();

		
	}
}
