// アプリ全体で使う「決まった言葉（文字）」をまとめて用意しておく仕組み（モデル層）
namespace TaskManager.Models
{
    // リテラル値を定数(文字)で定義
    // 言葉を定数文字にして入れることで間違いが減ったり、変更が簡単になる

    // 状態を管理する定数
    public static class TaskStatuses
    {
        public const string NotStarted = "未着手";
        public const string InProgress = "進行中";
        public const string Completed = "完了";
    }

    // 優先度を管理する定数
    public static class TaskPriority
    {
        public const string High = "高";
        public const string Medium = "中";
        public const string Low = "低";
    }

    // ソート用の定数
	public static class SortOrders
	{
		// 昇順（近い順）の定数
		public const string Ascending = "asc";

		// 降順（遠い順）の定数
		public const string Descending = "desc";
	}
}
