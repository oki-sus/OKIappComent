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
		private readonly ApplicationDbContext _context;
        private readonly TaskService _taskService;

        public TaskItemsController(ApplicationDbContext context)
		{
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
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(TaskItem taskItem)
		{
			if (!ModelState.IsValid)
			{
                return View(taskItem);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            taskItem.CreatedBy = userId;
            taskItem.CreatedAt = DateTime.Now;
            taskItem.UpdatedAt = DateTime.Now;

            _context.Add(taskItem);
            await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		// POST: TaskItems/Complete
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Complete(int id)
		{
			// 自分のタスクかチェック（セキュリティ対策）
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			
            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

            if (taskItem == null)
			{
				return NotFound();
			}

            taskItem.Status = TaskStatuses.Completed;
            taskItem.UpdatedAt = DateTime.Now;

            _context.Update(taskItem);
            await _context.SaveChangesAsync();

			// 一覧画面に戻る
			return RedirectToAction(nameof(Index));
		}

        // 編集画面を表示 (GET: TaskItems/Edit)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         
            var vm = new TaskEditViewModel();

            var isLoaded = await _taskService.LoadTaskIntoViewModelAsync(vm, id, userId);

            if (!isLoaded)
            {
                return NotFound();
            }

            return View(vm);
        }

        // 編集内容を保存 (POST: TaskItems/Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

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

            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);
            if (taskItem == null)
            {
                return NotFound();
            }

            try
            {
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
            {
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
        {
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

            if (taskItem != null)
            {
                _context.TaskItems .Remove(taskItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}