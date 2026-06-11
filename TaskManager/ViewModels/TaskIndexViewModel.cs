// 日付型（DateTime）などの標準的なシステム機能を使用するための名前空間をインポート
using System;
// プロパティに [StringLength] や [Required] などの入力制限ルール（検証用属性）を付けられるようにするシステム共通のパッケージです
using System.ComponentModel.DataAnnotations;

// タスクのソート順（SortOrders）などの定数定義が格納されている場所をインポート
using TaskManager.Data;
// タスクのデータ構造（TaskItem）が定義されている場所をインポート
using TaskManager.Models;

// 画面表示用の入れ物（ビューモデル層）であることを示す名前空間の定義
namespace TaskManager.ViewModels
{
	// タスク一覧画面（Index.cshtml）専用の、検索条件と表示データを一括管理するためのViewModelクラスの定義
	public class TaskIndexViewModel
    {
		// 画面からの入力文字数を30文字までに制限し、超過した場合は指定のエラーメッセージを出す設定
		[StringLength(30, ErrorMessage = "検索ワードは{1}文字以内で入力してください。")]
		// ユーザーが検索窓に打ち込んだ「キーワード（検索文字列）」を保持するプロパティ（未入力時はNull）
		public string? SearchString { get; set; }

		// ユーザーが絞り込みドロップダウンで選択した「カテゴリ名」を保持するプロパティ（未選択時はNull）
		public string? Category { get; set; }

		// タスクの一覧をどのような順番で並べるかの設定を保持するプロパティ（初期値として「昇順」の定数を設定）
		public string SortOrder { get; set; } = SortOrders.Ascending;

		// ユーザーが絞り込みドロップダウンで選択した「ステータス（進行状況）」を保持するプロパティ（未選択時はNull）
		public string? Status { get; set; }

		// ユーザーが絞り込みドロップダウンで選択した「優先度」を保持するプロパティ（未選択時はNull）
		public string? Priority { get; set; }

		// ユーザーがカレンダー等で指定した「この日付より前のタスク（期限の検索条件）」を保持するプロパティ（未選択時はNull）
		public DateTime? DueDateBefore { get; set; }


		// 画面上の「完了済みを非表示にする」というチェックボックスのON/OFF状態（真偽値）を保持するプロパティ
		public bool HideCompleted { get; set; }

		// 「期限が迫っているタスクのみを表示する」というチェックボックスのON/OFF状態を保持するプロパティ
		public bool FilterUpcomingDeadline { get; set; }

		// 期限切れ間近アラートの表示フラグ
		public bool ShowExpiryAlert { get; set; }


		// 画面最上部の検索用ドロップダウンの選択肢として並べるための、現在存在する重複のないカテゴリ名の一覧リスト（初期値は空のリスト）
		public List<string> Categories { get; set; } = new();

		// データベースから検索・絞り込みで抽出された、最終的に画面のテーブル等に表示するタスクデータのリスト（初期値は空のリスト）
		public List<TaskManager.Models.TaskItem> Tasks { get; set; } = new();

		
	}
}
