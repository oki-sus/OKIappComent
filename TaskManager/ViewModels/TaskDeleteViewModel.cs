// Entity Framework Coreのデータベース操作に関する共通機能をインポート
using Microsoft.EntityFrameworkCore;
// タスクのステータスや優先度の定数が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータモデル（TaskItem）が定義されている場所をインポート
using TaskManager.Models;

// 画面表示用の入れ物（ビューモデル層）であることを示す名前空間の定義
namespace TaskManager.ViewModels
{
	// 削除確認画面（Delete.cshtml）専用の入れ物（ViewModel）クラスの定義
	// 後ろに「: TaskBaseViewModel」をつけることで、親クラスの共通項目（IdやTitleなど）をすべて自動で引き継いでいる（継承）
	public class TaskDeleteViewModel : TaskBaseViewModel
    {
		// 削除確認画面で表示したい項目（タイトル、期限、状態など）は、
		// すべて親クラス（TaskBaseViewModel）に書かれているため、このクラスの中身は空っぽです。
		// 
		// 「中身が同じなら親クラスをそのまま画面に使えばいいのでは？」と思うかもしれませんが、
		// 画面ごとに専用のクラス（型）を分けておくことで、将来「削除画面のときだけ、本当に消していいかのチェックボックスを追加したい」
		// といった画面独自の仕様変更（拡張）が起きた際に、他の画面に迷惑をかけずにこのファイルの中だけで安全に改造できるようになります。

	}
}
