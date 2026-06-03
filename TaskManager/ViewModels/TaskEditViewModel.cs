// Entity Framework Coreのデータベース操作に関する共通機能をインポート
using Microsoft.EntityFrameworkCore;
// タスクのステータスや優先度の定数が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータモデル（TaskItem）が定義されている場所をインポート
using TaskManager.Models;

// 画面表示用の入れ物（ビューモデル層）であることを示す名前空間の定義
namespace TaskManager.ViewModels
{
	// タスク編集画面（Edit.cshtml）専用の入れ物（ViewModel）クラスの定義
	// 親クラス（TaskBaseViewModel）を継承することで、タイトルや期限などの基本項目をすべて自動で引き継いでいる
	public class TaskEditViewModel : TaskBaseViewModel
    {
		// 親クラス（TaskBaseViewModel）の基本項目だけでは足りない、編集画面専用の「作成日時」を保持するプロパティ（初期値は現在時刻）
		// タスクの編集時であっても、データベース上の最初の「作成日」が消えたり上書きされたりしないよう、画面側で一時保管しておくために必要となります
		public DateTime CreatedAt { get; set; } = DateTime.Now;

        

    }
}
