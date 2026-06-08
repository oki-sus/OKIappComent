// ログイン中のユーザー識別情報（クッキー等から抽出する情報）を扱うための名前空間をインポート
using System.Security.Claims;
// 画面や処理に対して「ログイン必須」などのセキュリティ制限をかけるための機能をインポート
using Microsoft.AspNetCore.Authorization;
// MVCパターンのコントローラーや、画面遷移（ActionResult）を扱うための基本機能をインポート
using Microsoft.AspNetCore.Mvc;
// データベースに対して非同期処理（FirstOrDefaultAsyncなど）を行うための拡張機能をインポート
using Microsoft.EntityFrameworkCore;
// データベース接続クラス（ApplicationDbContext）が定義されている場所をインポート
using TaskManager.Data;
// タスクのデータ構造（TaskItem）が定義されている場所をインポート
using TaskManager.Models;
// 画面表示用の箱（TaskIndexViewModelなど）が定義されている場所をインポート
using TaskManager.ViewModels;

// 開発中のタスク管理アプリのコントローラー層であることを示す名前空間の定義
namespace TaskManager.Controllers
{
	// このコントローラー内のすべての処理に対して、事前にログインしていることを必須条件にするセキュリティ属性
	[Authorize]
	// ブラウザからのリクエストを受け取り、画面の表示やデータの保存を割り振る司令塔クラスの定義
	public class TaskItemsController : Controller
	{
		// データベースと通信するための窓口。readonlyにより、コンストラクタ以外での予期せぬ上書きを防止
		private readonly ApplicationDbContext _context;
		// データの検索や共通の詰め替え処理など、具体的なビジネスロジックを肩代わりしてくれるクラスの変数
		private readonly TaskService _taskService;

		// アプリ起動時に、外部（Program.cs等）から自動的にDBの接続窓口を注入してもらうコンストラクタ
		public TaskItemsController(ApplicationDbContext context)
		{
			// 受け取ったDBの接続窓口を、クラス内の非公開変数にセットしていつでも使えるようにする
			_context = context;
			// データの詰め替えや検索ロジックを行うTaskServiceを、同じDB窓口を共有する形でインスタンス化
			_taskService = new TaskService(context);
        }

		// 一覧画面を表示するための処理（ブラウザから通常アクセスされた場合）
		[HttpGet]
		// 検索条件を受け取り、非同期でデータを取得して一覧画面（View）を返すアクションメソッド
		public async Task<IActionResult> Index(TaskIndexViewModel vm)
        {
			// 現在ログインしているユーザーの固有識別ID（NameIdentifier）を認証クッキーから取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// 万が一、何らかの理由でユーザーIDが正常に取得できなかった（ログインしていない）場合の安全策
			if (string.IsNullOrEmpty(userId))
            {
				// セキュリティが破られないよう、標準のIdentityログイン画面へ強制的に転送（リダイレクト）させる
				return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

			// TaskServiceに「このユーザーの検索条件に合わせて、データをDBから取ってきて箱（vm）に詰めて」と依頼
			await _taskService.LoadTaskIndexDataAsync(vm, userId);

			// 残り3日以内の未完了タスクがあるかどうかをチェックする
			// あるならTrue、ないならFalseを、画面行きの箱（vm.ShowExpiryAlert）にセットして引き渡す
			vm.ShowExpiryAlert = await _taskService.HasUpcomingDeadlineTasksAsync(userId);

			// 検索結果のタスク一覧や検索条件が詰まったViewModelを、一覧画面（Index.cshtml）に渡して描画
			return View(vm);
        }

		// 新規作成画面を表示するための処理（「新規登録」ボタンを押したとき）
		[HttpGet]
		// ユーザーに文字を入力してもらうための、空っぽの登録フォーム画面を返す
		public IActionResult Create()
		{
			// 何もデータを乗せずに、そのまま新規作成画面（Create.cshtml）を表示する
			return View();
		}

		// 新規作成画面で「登録」ボタンが押されたときに、入力データをDBへ保存する処理
		[HttpPost]
		// セキュリティ対策。他サイトから勝手にデータを送りつけられる不正リクエスト（CSRF）を検知してブロックする
		[ValidateAntiForgeryToken]
		// 画面の入力欄から組み立てられたタスクのデータ（taskItem）を引数で受け取る
		public async Task<IActionResult> Create(TaskItem taskItem)
		{
			// ガード節 モデルに設定したバリデーション（必須入力や文字数制限）に違反していないかをチェック
			if (!ModelState.IsValid)
			{
				// 入力不備（タイトル未入力など）があれば、エラーメッセージを添えて元の入力画面へ突き返す
				return View(taskItem);
			}

			// このタスクの所有者を確定させるため、現在ログインしているユーザーのIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// ユーザーIDが空なら、認証エラーと判断してもう一度ログインし直すための画面（Challenge）を呼び出す
			if (string.IsNullOrEmpty(userId)) return Challenge();


            taskItem.CreatedBy = userId;            // 誰が作ったか
            taskItem.CreatedAt = DateTime.Now;      // 作った日時
            taskItem.UpdatedAt = DateTime.Now;      // 更新日時(新規作成だから作成日時と同じ)

			// Entity Frameworkの変更追跡機能に対して、「新しくこのタスクを追加してね」と登録
			_context.Add(taskItem);
			// 追跡に登録された追加コマンドを、実際のSQLiteデータベースに対して非同期で安全に保存（書き込みを実行）
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));     // 成功したらIndexへ画面切り替え
		}

		// タスク一覧画面で「完了」ボタンが押されたときに動く処理
		[HttpPost]
		// セキュリティ対策。URLやリクエストを偽造した不正なデータ書き換え命令をブロックする
		[ValidateAntiForgeryToken]
		// 完了状態にしたいタスクの固有番号（id）を引数としてピンポイントで受け取る
		public async Task<IActionResult> Complete(int id)
		{
			// 悪意あるユーザーが「他人のタスクID」を勝手に指定して完了にできないよう、自分のユーザーIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// 指定されたタスクIDであり、かつ「自分が作ったタスク（CreatedBy == userId）」であるレコードをDBから1件探す
			var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

			// もし指定されたIDのタスクが存在しない、あるいは「他人のタスク」だった場合のガード処理
			if (taskItem == null)
			{
				// 情報漏洩を防ぐため、一律で「そんなページは見つかりません（404エラー）」を画面に返す
				return NotFound();
			}

			// タスクのステータス管理プロパティを、完了状態を表す文字列（Completed）に書き換える
			taskItem.Status = TaskStatuses.Completed;
			// データを書き換えたので、更新日時を今この瞬間の現在時刻に上書きする
			taskItem.UpdatedAt = DateTime.Now;

			// 変更したタスクの情報をデータベースに反映（Updateコマンドとして準備）
			_context.Update(taskItem);
			// 更新されたステータスと日時を、実際のSQLiteデータベースへ非同期でしっかりと保存する
			await _context.SaveChangesAsync();

			// 完了処理がすべて終わったら、最新の状態を反映させるためにタスク一覧画面（Index）へ戻す
			return RedirectToAction(nameof(Index));
		}

		// 編集画面を表示するための処理（タスクの「編集」ボタンを押したとき）
		[HttpGet]
		// どのタスクを編集するかを特定するため、URLからタスクID（Nullの可能性あり）を受け取る
		public async Task<IActionResult> Edit(int? id)
        {
			// 自分のタスクだけを編集できるようにするため、現在ログイン中のユーザーIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// 編集画面に必要なデータを格納するための、専用の空の箱（ViewModel）を用意する
			var vm = new TaskEditViewModel();

			// TaskServiceにIDとユーザーIDを渡し、安全な所有権チェックと、箱（vm）へのデータ詰め替えを依頼
			var isLoaded = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

			// データが見つからなかった、または他人のタスクで詰め替えに失敗（isLoadedがfalse）した場合の処理
			if (!isLoaded)
            {
				// 不正アクセス防止のために「ページが見つかりません（404エラー）」を出して処理を遮断する
				return NotFound();
            }

			// 指定されたIDのデータが自分のものだと証明され、データが詰まったViewModelを編集画面（Edit.cshtml）に渡して表示
			return View(vm);
        }

		// 編集画面で「保存」ボタンが押されたときに、上書きされたデータをDBへ反映する処理
		[HttpPost]
		// セキュリティ対策。他サイトからの不正ななりすまし更新リクエストを検知して弾く
		[ValidateAntiForgeryToken]
		// URLに含まれるIDと、画面の入力フォームから送られてきたViewModel（vm）の両方を受け取る
		public async Task<IActionResult> Edit(int id, TaskEditViewModel vm)
        {
			// URLのIDと、画面から送られてきたデータの内部IDが一致しているかを検証する整合性チェック
			if (id != vm.Id)
            {
				// IDが書き換えられているなどの不整合があれば、404エラーを出して処理を中断
				return NotFound();
            }

			// 入力エラー（文字数超過など）が起きていないかを判定
			if (!ModelState.IsValid)
            {
				// 不備があれば、入力された内容を保持したままエラーメッセージを添えて編集画面に突き返す
				return View(vm);
            }

			// ハッキング対策。他人のタスクを勝手に上書き保存させないために、現在のログインユーザーIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// ユーザーIDの取得に失敗した場合は処理を継続できないため、弾く
			if (string.IsNullOrEmpty(userId))
            {
				// 再ログインを要求する処理へ飛ばす
				return Challenge();
            }

			// 保存を実行する直前に、もう一度「本当にこのIDは、このユーザーが作ったタスクか？」をDBから直接探して確認
			var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);
			// もしタスクが消えていた、あるいは他人のタスクだった場合の処理
			if (taskItem == null)
            {
				// 情報守秘のために404エラーを返す
				return NotFound();
            }

			// 例外が起きる可能性のあるDB更新処理を安全に行うためのtryブロックを開始
			try
			{
				// 画面から送られてきた最新のタイトルを、DBから取ってきた元データに上書き
				taskItem.Title = vm.Title;
				// 画面から送られてきた最新のカテゴリを、元データに上書き
				taskItem.Category = vm.Category;
				// 画面から送られてきた最新の期限日を、元データに上書き
				taskItem.DueDate = vm.DueDate;
				// 画面から送られてきた最新のステータス（進行状況）を、元データに上書き
				taskItem.Status = vm.Status;
				// 画面から送られてきた最新の優先度を、元データに上書き
				taskItem.Priority = vm.Priority;
				// 画面から送られてきた最新の詳細メモを、元データに上書き
				taskItem.Detail = vm.Detail;
				// 編集を行ったので、最終更新日時を今この瞬間の現在時刻に上書き
				taskItem.UpdatedAt = DateTime.Now;

				// 変更されたすべての項目をDBの追跡機能に反映
				_context.Update(taskItem);
				// 上書きされた最新のタスクデータを、実際のSQLiteデータベースへ非同期で上書き保存する
				await _context.SaveChangesAsync();
            }

			// 同時実行エラー（自分が編集している間に、他のブラウザ等でデータが削除されたケース）をキャッチ
			catch (DbUpdateConcurrencyException)
            {
				// エラーの原因が「タスクが物理的に消えていたから」なのかを確認するため、IDが存在するかチェック
				if (!await _context.TaskItems.AnyAsync(e => e.Id == vm.Id))
                {
					// すでにデータが消えていたなら、お探しのデータはないということで404エラーを返す
					return NotFound();
                }
				// 削除が原因ではない、その他の重大なDBエラーなら、あえてそのままシステムエラーとして上に投げる（throw）
				else throw;
            }
			// すべての上書き保存が安全に成功したら、タスク一覧画面（Index）へ画面を切り替える
			return RedirectToAction(nameof(Index));
        }

		// タスクの詳しい中身（メモなど）を見る詳細画面の表示処理
		[HttpGet]
		// どのタスクの詳細を見るかを特定するため、URLからタスクIDを受け取る
		public async Task<IActionResult> Details(int? id)
        {
			// 自分のタスクだけを閲覧可能にするため、ログイン中のユーザーIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// 詳細画面に必要な項目（作成日時等も含む）を格納する、専用のViewModelを用意する
			var vm = new TaskDetailsViewModel();

			// TaskServiceに処理を依頼し、安全にデータを取得してViewModelに詰め替えてもらう（仕組みはEditのGETと同じ）
			var isLoad = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

			// データが見つからなかった、または他人のタスクだった（isLoadがfalse）場合のガード処理
			if (!isLoad)
            {
				// セキュリティとプライバシー保護のため、存在しない扱い（404エラー）にして追い出す
				return NotFound();
            }
			// データの詰め替えが無事成功し、中身が揃ったViewModelを詳細画面（Details.cshtml）に渡して表示する
			return View(vm);
        }


		// 削除の「最終確認画面」を表示するための処理（タスクの「削除」ボタンを押したとき）
		[HttpGet]
		// どのタスクを消そうとしているかを特定するため、URLからタスクIDを受け取る
		public async Task<IActionResult> Delete(int? id)
        {
			// 他人のタスクを勝手に消させない防衛策として、ログイン中のユーザーIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// 削除確認画面（「本当に消しますか？」画面）に表示する項目を乗せるためのViewModelを用意
			var vm = new TaskDeleteViewModel();

			// TaskServiceに依頼して、削除対象のタスクが自分のものなら安全にデータを箱（vm）に詰め替えてもらう
			var isLoaded = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

			// データが存在しない、もしくは他人のデータで詰め替えに失敗した場合の処理
			if (!isLoaded)
            {
				// 不正な削除を防ぐため、一律で404エラーを返してアクセスを拒否する
				return NotFound();
            }
			// 安全性が確認され、消そうとしているタスクの情報が載ったViewModelを削除確認画面（Delete.cshtml）に渡して表示
			return View(vm);
        }

		// 削除確認画面で「はい、削除します」が押されたときに、実際にDBからデータを抹消する処理
		[HttpPost, ActionName("Delete")]
		// セキュリティ対策。他サイトから意図しない削除命令リクエストを無理やり送りつけられるハッキングをブロック
		[ValidateAntiForgeryToken]
		// 実際に削除を実行するタスクの固有IDを引数として受け取る
		public async Task<IActionResult> DeleteConfirmed(int id)
        {
			// 最終実行の直前チェックとして、現在ログインしているユーザーのIDを取得
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// 削除しようとしているデータが、本当に「指定のID」かつ「自分が作ったタスク」であるかをDBから直接取得して確認
			var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

			// 確認した結果、ちゃんと自分のタスクデータが存在していた（nullではない）場合の処理
			if (taskItem != null)
            {
				// Entity Frameworkの変更追跡機能に対して、「このタスクデータをDBから抹消してね」と命令
				_context.TaskItems .Remove(taskItem);
				// 追跡された削除コマンドを、実際のSQLiteデータベースに対して非同期で実行し、物理的にデータを消去する
				await _context.SaveChangesAsync();
            }

			// 削除処理がすべて無事に完了したら、タスク一覧画面（Indexアクション）へ画面を切り替えて戻る
			return RedirectToAction(nameof(Index));
        }

    }
}