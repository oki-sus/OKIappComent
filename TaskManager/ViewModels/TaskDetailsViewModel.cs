// Entity Framework Coreのデータベース操作に関する共通機能をインポート
using Microsoft.EntityFrameworkCore;
// タスクのステータスや優先度の定数が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータモデル（TaskItem）が定義されている場所をインポート
using TaskManager.Models;

// 画面表示用の入れ物（ビューモデル層）であることを示す名前空間の定義
namespace TaskManager.ViewModels
{
	// タスク詳細画面（Details.cshtml）専用の入れ物（ViewModel）クラスの定義
	// 親クラス（TaskBaseViewModel）を継承することで、タイトルや期限などの基本項目をすべて自動で引き継いでいる
	public class TaskDetailsViewModel : TaskBaseViewModel
    {
		// 親クラス（TaskBaseViewModel）の項目だけでは足りない、詳細画面専用の「作成日時」を保持するプロパティ（初期値は現在時刻）
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		// 親クラス（TaskBaseViewModel）の項目だけでは足りない、詳細画面専用の「最終更新日時」を保持するプロパティ（初期値は現在時刻）
		public DateTime UpdatedAt { get; set; } = DateTime.Now;


    }
}
