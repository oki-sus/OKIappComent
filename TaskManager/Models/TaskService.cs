// Entity Framework Coreを使って非同期でのデータ取得（ToListAsyncなど）を行うための名前空間をインポート
using Microsoft.EntityFrameworkCore;
// データベース接続クラス（ApplicationDbContext）が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータモデル（TaskItem）や定数クラスが定義されている場所をインポート
using TaskManager.Models;
// 画面表示用の箱（TaskBaseViewModelなど）が定義されている場所をインポート
using TaskManager.ViewModels;

// データを処理する仕組み（モデル層）であることを示す名前空間の定義
namespace TaskManager.Models
{
	// コントローラーの代わりに、データの加工や検索ロジックなどの頭脳部分を一手に引き受けるサービスクラスの定義
	public class TaskService
    {
		// データベースと通信するための窓口をクラス内で保持するための非公開変数。readonlyで安全性を確保
		private readonly ApplicationDbContext _context;
		// コントローラーなどから、現在動いているDB接続窓口（context）を受け取るためのコンストラクタ
		public TaskService(ApplicationDbContext context)
        {
			// 受け取った接続窓口をクラスの変数にセットして、クラス内のすべてのメソッドから使えるようにする
			_context = context;
        }

		// 指定されたタスクIDのデータを安全に取得し、各画面（編集、詳細、削除）用のViewModelに中身を移し替える共通メソッド
		public async Task<bool> LoadTaskIntoViewModelAsync(TaskBaseViewModel vm, int? id, string? userId)
        {
			// ガード節。URLのタスクIDが空、または現在ログインしているユーザーのIDが取れない場合は即座に処理を中断（失敗を返す）
			if (id == null || string.IsNullOrEmpty(userId)) return false;

			// 指定されたタスクIDであり、かつ「現在ログイン中のユーザーが作ったタスク」であるレコードをDBから非同期で1件検索
			var foundTask = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

			// 該当するタスクがデータベース内に見つからなかった（または他人のタスクだった）場合の処理
			if (foundTask == null)
            {
				// 呼び出し元のコントローラーに対して、データがなかったことを示す「false（失敗）」を返す
				return false;
            }

			// 土台となる共通項目（ID、タイトル、カテゴリ、期限、状態、優先度、詳細メモ）をViewModelに一括コピーする
			vm.MapFromEntity(foundTask);

			// パターンマッチング。もし今処理しているViewModelの正体が「詳細画面用（TaskDetailsViewModel）」だった場合の処理
			if (vm is TaskDetailsViewModel detailsVm)
            {
				// 共通項目に加えて、詳細画面に必要な「作成日時」を詰め替える
				detailsVm.CreatedAt = foundTask.CreatedAt;
				// 共通項目に加えて、詳細画面に必要な「最終更新日時」を詰め替える
				detailsVm.UpdatedAt = foundTask.UpdatedAt;
            }
			// もしViewModelの正体が詳細画面用ではなく「編集画面用（TaskEditViewModel）」だった場合の処理
			else if (vm is TaskEditViewModel editVm)
            {
				// 編集時に作成日時が消えてしまうのを防ぐため、追加で「作成日時」を詰め替える
				editVm.CreatedAt = foundTask.CreatedAt;
            }

			// すべてのデータの詰め替えが安全かつ正常に完了したため、true（成功）を返す
			return true;
        }

		// 一覧画面（Index）に表示するための、検索ドロップダウン用カテゴリリストと、絞り込み済みのタスク一覧を読み込むメソッド
		public async Task LoadTaskIndexDataAsync(TaskIndexViewModel vm, string userId)
        {
			// ユーザーIDが空っぽだった場合は、処理を継続できないため何もせずメソッドを即座に終了する
			if (string.IsNullOrEmpty(userId))
            {
                return;
            }

			// 一覧画面の上部にある「カテゴリ絞り込み用ドロップダウン」に表示するための、選択肢リストをDBから作成
			vm.Categories = await _context.TaskItems
				// 「自分が作ったタスク」であり、かつ「カテゴリ名が空ではない」データだけを対象に絞り込む
				.Where(t => t.CreatedBy == userId && !string.IsNullOrEmpty(t.Category))
				// タスク全体ではなく、カテゴリ名の文字列だけを切り抜く
				.Select(t => t.Category)
				// 重複している同じカテゴリ名（例：「仕事」が何個もある状態）を1つにまとめる
				.Distinct()
				// 条件に合う重複なしのカテゴリ名リストを、非同期で実際にデータベースから取得してViewModelに格納する
				.ToListAsync();

			// 遅延実行の準備。まだデータベースに命令は送らず、C#の内部で「SQLの土台（条件文）」を組み立て始める
			// まずは「自分が作ったタスクであること」という絶対条件を設定し、後から条件を追加できる形（AsQueryable）にする
			var query = _context.TaskItems
                .Where(t => t.CreatedBy == userId)
                .AsQueryable();

			// ここからのif文は、画面の検索フォームでユーザーが条件を入力した時だけ、条件文を連結していく
			// ユーザーが検索窓にキーワードを打ち込んでいた場合の処理
			if (!string.IsNullOrEmpty(vm.SearchString))
            {
				// 「タイトルにそのキーワードを部分一致（Contains）で含んでいること」という条件をクエリに付け足す
				query = query.Where(t => t.Title.Contains(vm.SearchString));
            }

			// ユーザーが検索ドロップダウンで特定のカテゴリを選択していた場合の処理
			if (!string.IsNullOrEmpty(vm.Category))
            {
				// 「カテゴリ名が完全に一致していること」という条件をクエリに付け足す
				query = query.Where(t => t.Category == vm.Category);
            }

			// ユーザーが検索ドロップダウンでステータス（未着手・進行中など）を選択していた場合の処理
			if (!string.IsNullOrEmpty(vm.Status))
            {
				// 「進行状態が完全に一致していること」という条件をクエリに付け足す
				query = query.Where(t => t.Status == vm.Status);
            }

			// ユーザーが検索ドロップダウンで優先度（高・中・低）を選択していた場合の処理
			if (!string.IsNullOrEmpty(vm.Priority))
            {
				// 「優先度の設定が完全に一致していること」という条件をクエリに付け足す
				query = query.Where(t => t.Priority == vm.Priority);
            }

			// ユーザーがカレンダー等で期限日の条件（〇〇日以前）を指定していた場合の処理
			if (vm.DueDateBefore.HasValue)
            {
				// 「タスクの期限日が、指定された日付以下（それより前）であること」という条件をクエリに付け足す
				query = query.Where(t => t.DueDate <= vm.DueDateBefore.Value);
            }

			// ユーザーが画面の「完了済みを非表示にする」チェックボックスにチェックを入れていた場合の処理
			if (vm.HideCompleted)
            {
				// 「ステータスが完了（Completed）ではないもの」という条件をクエリに付け足す
				query = query.Where(t => t.Status != TaskStatuses.Completed);
            }

			// 画面側から届いたViewModel（vm）の「FilterUpcomingDeadline」フラグが真（true）であるか判定する。
			// チェックが外れている（false）場合は、内部のクエリ改変処理を行わずに、そのままこのifブロック全体を安全にスルーさせる
			if (vm.FilterUpcomingDeadline)
			{
				// 今日の日付を取得（時間部分を00:00:00にリセットして日付のみで比較できるようにする）
				var today = DateTime.Today;

				// 警告アラートの発生条件と同じく、「3日後の終わり」を基準として算出
				var targetDate = today.AddDays(3);

				// 組み立て途中のSQL（query）に対して、「自分のデータ」という大前提に加えて、さらに
				// 1. 状態が「完了（Completed）」ではない未完了タスクであること
				// 2. 期限が過去に切れたものではなく、今日以降であること
				// 3. 期限が今日から数えて3日以内（targetDate以下）であること
				// という3つの条件式を、論理演算子で繋いでデータベースへの要求（Where）に上書き連結する。
				query = query.Where(t =>
					t.Status != TaskStatuses.Completed &&
					t.DueDate.Date >= today &&
					t.DueDate.Date <= targetDate
				);
			}

			// 三項演算子を使って並び替え順を判定。並び替えの設定が「降順（Descending）」だった場合の処理
			query = vm.SortOrder == SortOrders.Descending
				// 期限日が遅い順（カレンダーの未来の日付順）に並べ替える指示をクエリに付け足す
				? query.OrderByDescending(t => t.DueDate)
				// 降順ではない（昇順：Ascending）なら、期限日が近い順（今日明日のタスクが上に来る順）に並べ替える指示を足す
				: query.OrderBy(t => t.DueDate);

			// ここで初めて本物のSQLが自動生成され、一度だけDBへリクエストが飛ぶ
			// 今まで組み立ててきたすべての条件に合致するタスク一覧を非同期で一括取得し、一覧画面用の箱（vm.Tasks）に詰め込む
			vm.Tasks = await query.ToListAsync();
        }

		// 現在ログインしているユーザーのタスクの中から、期限まで「残り3日以内」かつ「未完了（Completedではない）」のものが1件でもあるか判定する
		public async Task<bool> HasUpcomingDeadlineTasksAsync(string userId)
		{
			// ユーザーIDが正常に引き渡されていない場合は、安全のために即座に「存在しない（false）」として処理を終了する
			if (string.IsNullOrEmpty(userId)) return false;

			// システムの現在の今日の日付を取得する
			var today = DateTime.Today;
			// アラート対象となる「3日後の期限日」を計算してターゲット日付を決める
			var targetDate = today.AddDays(3);

			// データベースのTaskItemsテーブルに対して、3日以内かつ未完了、でデータが存在するかをチェックする
			return await _context.TaskItems.AnyAsync(t =>
				t.CreatedBy == userId &&						// 自分の作ったタスクであること
				t.Status != TaskStatuses.Completed &&			// 状態が「完了」ではないこと（未完了）
				t.DueDate.Date >= today &&						// 期限が過去に切れたものではなく、今日以降であること
				t.DueDate.Date <= targetDate					// 期限が「今日から数えて3日以内」であること
			);
		}
	}
}
