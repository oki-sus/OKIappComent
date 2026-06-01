using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Models
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LoadTaskIntoViewModelAsync(TaskBaseViewModel vm, int? id, string? userId)
        {
            if (id == null || string.IsNullOrEmpty(userId)) return false;

            var foundTask = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id && m.CreatedBy == userId);

            if (foundTask == null) return false;

            vm.MapFromEntity(foundTask);

            // もし詳細画面用（TaskDetailsViewModel）なら、追加で日時も詰め替える
            if (vm is TaskDetailsViewModel detailsVm)
            {
                detailsVm.CreatedAt = foundTask.CreatedAt;
                detailsVm.UpdatedAt = foundTask.UpdatedAt;
            }
            // もし編集画面用（TaskEditViewModel）なら、追加で作成日時も詰め替える
            else if (vm is TaskEditViewModel editVm)
            {
                editVm.CreatedAt = foundTask.CreatedAt;
            }

            return true;
        }

        // 一覧画面用のデータをViewModelにロードするメソッド
        public async Task LoadTaskIndexDataAsync(TaskIndexViewModel vm, string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            // カテゴリ一覧の取得
            vm.Categories = await _context.TaskItems
                .Where(t => t.CreatedBy == userId && !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .ToListAsync();

            // タスク一覧を取得するクエリの組み立て
            var query = _context.TaskItems
                .Where(t => t.CreatedBy == userId)
                .AsQueryable();

            // 検索フィルタの適用
            // キーワード
            if (!string.IsNullOrEmpty(vm.SearchString))
            {
                query = query.Where(t => t.Title.Contains(vm.SearchString));
            }

            // カテゴリ
            if (!string.IsNullOrEmpty(vm.Category))
            {
                query = query.Where(t => t.Category == vm.Category);
            }

            // 状態
            if (!string.IsNullOrEmpty(vm.Status))
            {
                query = query.Where(t => t.Status == vm.Status);
            }

            // 優先度
            if (!string.IsNullOrEmpty(vm.Priority))
            {
                query = query.Where(t => t.Priority == vm.Priority);
            }

            // 期限（以前）
            if (vm.DueDateBefore.HasValue)
            {
                query = query.Where(t => t.DueDate <= vm.DueDateBefore.Value);
            }

            // 完了済みを非表示にするフラグ
            if (vm.HideCompleted)
            {
                query = query.Where(t => t.Status != TaskStatuses.Completed);
            }

            // 並び替え
            query = vm.SortOrder == SortOrders.Descending
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate);

            // 結果をViewModelのTasksに詰め込む
            vm.Tasks = await query.ToListAsync();
        }

    }
}
