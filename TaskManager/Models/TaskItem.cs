// データベースの主キー（[Key]）や入力制限（[Required]）を設定するための名前空間をインポート
using System.ComponentModel.DataAnnotations;
// データベースのテーブル名や列名（[Column]）を直接指定するための名前空間をインポート
using System.ComponentModel.DataAnnotations.Schema;
// Entity Framework Coreのデータベース操作に関する基本機能をインポート
using Microsoft.EntityFrameworkCore;
// タスクのステータスや優先度の定数（TaskStatuses等）が定義されている場所をインポート
using TaskManager.Data;

// データを管理する仕組み（モデル層）であることを示す名前空間の定義
namespace TaskManager.Models
{
	public class TaskItem
	{
		// このプロパティが、データベース内でデータを1件ずつ識別するための「主キー（プライマリキー）」であることを指定
		[Key]
		// データベース側で新しくレコードが追加された際、1, 2, 3... と自動的に番号を増やす（自動採番）設定
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		// データベース内の実際のテーブルの列名（カラム名）を「Id」という名前に指定
		[Column("Id")]
		// タスクの固有識別番号を保持するプロパティ（初期値として0を設定）
		public int Id { get; set; } = 0;

		// 画面からの入力およびデータベースへの保存において、値が空っぽであることを禁止する制限
		[Required(ErrorMessage = "必須項目です")]
		// 画面（HTML）でラベルを表示する際、プロパティ名（Title）ではなく「タイトル」という日本語で表示させる設定
		[Display(Name = "タイトル")]
		// データベースの最大文字数を30文字に制限し、超えた場合は指定のエラーメッセージを出す設定
		[StringLength(30, ErrorMessage = "タイトルは{1}文字以内で入力してください")]
		// タスクの名前（件名）を保持するプロパティ（Nullエラー防止のため初期値に空文字を設定）
		public string Title { get; set; } = string.Empty;

		// 画面からの入力およびデータベースへの保存において、カテゴリ未入力を禁止する制限
		[Required(ErrorMessage = "必須項目です")]
		// 画面のラベル表示において「カテゴリ」という日本語名を使用する設定
		[Display(Name = "カテゴリ")]
		// データベースの最大文字数を15文字に制限し、超えた場合は指定のエラーメッセージを出す設定
		[StringLength(15, ErrorMessage = "カテゴリは{1}文字以内で入力してください")]
		// タスクのカテゴリ名を保存する項目です。一覧画面でのカテゴリ絞り込み検索の基準データとして使用されます。
		public string Category { get; set; } = string.Empty;

		// 画面からの入力およびデータベースへの保存において、期限日が未入力になるのを禁止する制限
		[Required(ErrorMessage = "必須項目です")]
		// 画面のラベル表示において「期限」という日本語名を使用する設定
		[Display(Name = "期限")]
		// 画面（HTML）で入力ボックスを作る際、時刻なしの「日付入力専用（カレンダー選択）」のUIにするよう指定
		[DataType(DataType.Date)]
		// タスクの完了期限となる年月日を保持するプロパティ
		public DateTime DueDate { get; set; }

		// 画面のラベル表示において「状態」という日本語名を使用する設定
		[Display(Name = "状態")]
		// タスクの進行状況を保持するプロパティ（新規作成時の初期値として「未着手」の文字列を設定）
		public string Status { get; set; } = TaskStatuses.NotStarted;

		// タスクの進行状況を保持するプロパティ（新規作成時の初期値として「未着手」の文字列を設定）
		[Display(Name = "優先度")]
		// タスクの重要度・緊急度を保持するプロパティ（新規作成時の初期値として「中」の文字列を設定）
		public string Priority { get; set; } = TaskPriority.Medium;

		// 画面のラベル表示において「詳細」という日本語名を使用する設定
		[Display(Name = "詳細")]
		// データベースの最大文字数を1000文字に制限し、超えた場合は指定のエラーメッセージを出す設定
		[StringLength(1000, ErrorMessage = "詳細は{1}文字以内で入力してください")]
		// タスクの具体的なメモや説明文を保持するプロパティ（末尾の ? により、未入力（Null）であっても許可する）
		public string? Detail { get; set; }

		// システム側でのデータ管理や、他人のタスクを非表示にするセキュリティ追跡用の列
		// タスクがデータベースに最初に登録された日時を保持するプロパティ（初期値として、生成された瞬間の現在時刻を設定）
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		// このタスクを作成したユーザーの固有ID（IdentityUserのID）を紐付けるプロパティ（Nullを許容）
		public string? CreatedBy { get; set; }
		// タスクの内容が上書き変更された最終日時を保持するプロパティ（初期値として、生成された瞬間の現在時刻を設定）
		public DateTime UpdatedAt { get; set; } = DateTime.Now;

	}
}