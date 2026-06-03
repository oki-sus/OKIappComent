// ログイン機能（Identity）を組み込んだデータベース管理機能を利用するための名前空間をインポート
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// データベース操作の基本機能（DbSetなど）を提供する Entity Framework Core の名前空間をインポート
using Microsoft.EntityFrameworkCore;
// データベースにテーブルとして登録したいタスクのデータ構造（TaskItem）がある場所をインポート
using TaskManager.Models;

// データベース関連のファイルをまとめるデータ層（Data）に属していることを示す名前空間の定義
namespace TaskManager.Data
{
	// アプリとデータベースを繋ぐ心臓部（コンテキストクラス）の定義
	// 「IdentityDbContext」を継承することで、自作のタスクデータだけでなく、
	// ユーザー管理用のテーブル（ユーザー名やパスワード等）も自動的に一緒に管理できるようになります
	// (DbContextOptions<ApplicationDbContext> options) は「プライマリコンストラクタ」というC#の書き方で、
	// Program.csから渡された「SQLiteを使う」といった設定情報（options）を直接受け取って後ろのベース機能に引き渡しています
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
		// C#の「TaskItemクラス」を、データベース上の「TaskItems」という名前のテーブルとして扱うための宣言
		// この1行があるおかげで、プログラム側から「_context.TaskItems」と書くだけで、
		// 実際のデータベースのテーブルに対してデータの保存や検索、削除が自由に行えるようになります
		public DbSet<TaskItem> TaskItems { get; set; }

	}

}
