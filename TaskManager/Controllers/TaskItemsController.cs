using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
	[Authorize] // ログイン必須
	public class TaskItemsController : Controller
	{
		private readonly ApplicationDbContext _context;     // DBの窓口　　readonlyで読み取り専用
        private readonly TaskService _taskService;          // 処理をするクラスをController内で使うために保管しておく変数

        public TaskItemsController(ApplicationDbContext context)
		{
            // DBを接続
			_context = context;
            _taskService = new TaskService(context);
        }

        // 一覧画面
        [HttpGet]
        public async Task<IActionResult> Index(TaskIndexViewModel vm)
        {
            // ユーザーIDの取得
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ログインしていなければログイン画面へ
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // TaskServiceにお願いしてViewModelにデータを読み込んでもらう
            await _taskService.LoadTaskIndexDataAsync(vm, userId);

            return View(vm);
        }

        // 新規作成画面（GET）
        [HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		// 新規作成処理（POST）
		[HttpPost]
		[ValidateAntiForgeryToken]      // セキュリティ対策トークン
		public async Task<IActionResult> Create(TaskItem taskItem)
		{
            // ガード節     エラーがないか
			if (!ModelState.IsValid)
			{
                return View(taskItem);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);    // ユーザーID取得
            if (string.IsNullOrEmpty(userId)) return Challenge();           // 出来なければログインし直し

            taskItem.CreatedBy = userId;            // 誰が作ったか
            taskItem.CreatedAt = DateTime.Now;      // 作った日時
            taskItem.UpdatedAt = DateTime.Now;      // 更新日時(新規作成だから作成日時と同じ)

            _context.Add(taskItem);                 // タスクデータを追加
            await _context.SaveChangesAsync();      // DBへ保存

			return RedirectToAction(nameof(Index));     // 成功したらIndexへ画面切り替え
		}

		// POST: TaskItems/Complete
		[HttpPost]
		[ValidateAntiForgeryToken]      // セキュリティ対策トークン
		public async Task<IActionResult> Complete(int id)
		{
			// 自分のタスクかチェック（セキュリティ対策）
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);    // ユーザーID取得
            
            //Idが一致しているデータをデータベースから持ってくる
            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

            if (taskItem == null)
			{                           
				return NotFound();      // 存在しないなら404エラーを出す
			}

            taskItem.Status = TaskStatuses.Completed;       // タスクのステータスを完了に書き換え
            taskItem.UpdatedAt = DateTime.Now;              // 更新日時を更新

            _context.Update(taskItem);
            await _context.SaveChangesAsync();              // データベースに保存

			// 一覧画面に戻る
			return RedirectToAction(nameof(Index));
		}

        // 編集画面を表示 (GET: TaskItems/Edit)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            // 自分のタスクかチェック
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         
            var vm = new TaskEditViewModel();

            
            var isLoaded = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

            if (!isLoaded)
            {
                return NotFound();
            }

            return View(vm);        // 指定したIdのタスクがこのユーザーのものならデータを詰める
        }

        // 編集内容を保存 (POST: TaskItems/Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]      // セキュリティ対策トークン
		public async Task<IActionResult> Edit(int id, TaskEditViewModel vm)
        {
            // Idが一致してるか
            if (id != vm.Id)
            {
                return NotFound();
            }

            // 入力エラーがないか
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // セキュリティチェック：他人のタスクを書き換えられないようにする
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // タスクがユーザーのものか再度確認
            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);
            if (taskItem == null)
            {
                return NotFound();
            }

            try
            {   // 画面から送られてきた最新の入力情報を元データに上書きし保存
                taskItem.Title = vm.Title;
                taskItem.Category = vm.Category;
                taskItem.DueDate = vm.DueDate;
                taskItem.Status = vm.Status;
                taskItem.Priority = vm.Priority;
                taskItem.Detail = vm.Detail;
                taskItem.UpdatedAt = DateTime.Now;

                _context.Update(taskItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {   // データ書き換え中にデータが消えたり消されたりするのを検知
                if (!await _context.TaskItems.AnyAsync(e => e.Id == vm.Id))
                {
                    return NotFound();
                }
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskItems/Details
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {   // タスクの中身をみる　仕組みはEdit(GET)と同じ
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = new TaskDetailsViewModel();

            var isLoad = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

            if (!isLoad)
            {
                return NotFound(); // 存在しない、または自分のものでない場合は404
            }
            
            return View(vm);
        }


        //  削除確認画面を表示 (GET: TaskItems/Delete)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            // 自分のタスクかチェック
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = new TaskDeleteViewModel();

            var isLoaded = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

            if (!isLoaded)
            {
                return NotFound();
            }

            return View(vm);
        }

        //  削除を実行 (POST: TaskItems/Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]      // セキュリティ対策トークン
		public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Id取得
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // データを持ってくる
            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

            if (taskItem != null)   // データがあれば
            {
                _context.TaskItems .Remove(taskItem);   // 削除
                await _context.SaveChangesAsync();      // 保存
            }

			// 一覧画面に戻る
			return RedirectToAction(nameof(Index));
        }

    }
}