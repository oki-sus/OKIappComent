// Entity Framework Coreのデータベース操作に関する共通機能をインポート
using Microsoft.EntityFrameworkCore;
// 画面での入力必須（[Required]）や文字数制限（[StringLength]）を設定するための名前空間をインポート
using System.ComponentModel.DataAnnotations;
// タスクのステータスや優先度の定数（TaskStatuses等）が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータモデル（TaskItem）が定義されている場所をインポート
using TaskManager.Models;

// 画面表示用の入れ物（ビューモデル層）であることを示す名前空間の定義
namespace TaskManager.ViewModels
{
	// 各タスク画面（作成・編集・詳細・削除）の共通土台となる、親クラス（ベースViewModel）の定義
	// 余計なものを渡さないために共通のものだけ
	public class TaskBaseViewModel
    {
		// タスクを一意に識別するための固有の管理番号（ID）を保持するプロパティ
		public int Id { get; set; }

		// 画面からの入力において、タイトルが空っぽのまま保存ボタンが押されるのを防ぐ必須項目設定
		[Required(ErrorMessage = "必須項目です")]
		// 画面からの入力文字数を30文字までに制限し、超過した場合は指定のエラーメッセージを出す設定
		[StringLength(30, ErrorMessage = "タイトルは{1}文字以内で入力してください")]
		// タスクの名前（件名）を保持するプロパティ（初期値として空文字を設定し、Nullエラーを防ぐ）
		public string Title { get; set; } = string.Empty;

		// 画面からの入力において、カテゴリが未記入のまま送信されるのを防ぐ必須項目設定
		[Required(ErrorMessage = "必須項目です")]
		// 画面からの入力文字数を15文字までに制限し、超過した場合は指定のエラーメッセージを出す設定
		[StringLength(15, ErrorMessage = "カテゴリは{1}文字以内で入力してください")]
		// タスクの分類（仕事、プライベート等）を保持するプロパティ（初期値として空文字を設定）
		public string Category { get; set; } = string.Empty;

		// 画面からの入力において、期限日が未入力のまま送信されるのを防ぐ必須項目設定
		[Required(ErrorMessage = "必須項目です")]
		// ブラウザ（HTML）で入力欄を表示する際、時刻の入らないカレンダー選択専用のUIにするよう指定
		[DataType(DataType.Date)]
		// タスクの完了期限となる年月日を保持するプロパティ（初期値として画面を開いた日の現在日時を設定）
		public DateTime DueDate { get; set; } = DateTime.Now;

		// タスクの現在の進行状況を保持するプロパティ（特に指定がない場合の初期値として「未着手」の定数を設定）
		public string Status { get; set; } = TaskStatuses.NotStarted;

		// タスクの重要度・緊急度を保持するプロパティ（特に指定がない場合の初期値として「中」の定数を設定）
		public string Priority { get; set; } = TaskPriority.Medium;

		// 画面からの入力文字数を1000文字までに制限し、超過した場合は指定のエラーメッセージを出す設定
		[StringLength(1000, ErrorMessage = "詳細は{1}文字以内で入力してください")]
		// 型の後ろに ? をつけることで、詳細なメモ書きが「未入力（Null）」の状態であっても許可するプロパティ
		public string? Detail { get; set; }


		// データベースから取得した生のタスクデータ（entity）を、このViewModelのプロパティへ移し替える共通メソッド
		// virtualを付与することで、子クラス（詳細画面用など）がこのメソッドを独自の項目でカスタマイズ（オーバーライド）できるようにする
		public virtual void MapFromEntity(TaskItem entity)
        {
			// DBのタスクIDを、ViewModelのIDプロパティへコピー
			this.Id = entity.Id;
			// DBのタイトルを、ViewModelのタイトルプロパティへコピー
			this.Title = entity.Title;
			// DBのカテゴリを、ViewModelのカテゴリプロパティへコピー
			this.Category = entity.Category;
			// DBの期限日を、ViewModelの期限日プロパティへコピー
			this.DueDate = entity.DueDate;
			// DBのステータス（状態）を、ViewModelのステータスプロパティへコピー
			this.Status = entity.Status;
			// DBの優先度を、ViewModelの優先度プロパティへコピー
			this.Priority = entity.Priority;
			// DBの詳細メモを、ViewModelの詳細メモプロパティへコピー
			this.Detail = entity.Detail;
        }

    }
}
