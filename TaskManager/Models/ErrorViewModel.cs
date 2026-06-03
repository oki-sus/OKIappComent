// 設定から自動生成されたファイルで、今回使ってはいません

// アプリ内で発生したエラー情報を管理するためのモデル層（Models）に属していることを示す名前空間の定義
namespace TaskManager.Models
{
	// エラー画面（Error.cshtml）にデータを引き渡すための専用の箱（ViewModel）クラスの定義
	public class ErrorViewModel
    {
		// エラーが発生した原因特定（ログの照合）に使うための、リクエストごとの一意の識別ID（Nullの可能性あり）
		public string? RequestId { get; set; }

		// 画面にリクエストIDを表示すべきかを判定するフラグ（RequestIdが空でない場合は自動的にtrueになる）
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
