// Webアプリの画面遷移やリクエストを処理するための基本機能を取り込む
using Microsoft.AspNetCore.Mvc;
// Razorページ（画面とC#がペアになる仕組み）の基本クラスを取り込む
using Microsoft.AspNetCore.Mvc.RazorPages;
// ユーザー登録やログインなど、認証機能（Identity）の部品を取り込む
using Microsoft.AspNetCore.Identity;
// 入力チェック（必須入力やメールアドレス形式の検証など）の機能を取り込む
using System.ComponentModel.DataAnnotations;

// プロジェクト内の「ID管理（ユーザー登録）用」のフォルダ空間を定義
namespace TaskManager.Areas.Identity.Pages.Account
{
	// ユーザー登録画面の裏側の処理を担当するデータモデルクラスを定義
	public class RegisterModel : PageModel
	{
		// ログイン状態を管理するためのシステム専用の部品を入れる変数
		private readonly SignInManager<IdentityUser> _signInManager;
		// ユーザーデータの登録や削除を管理するためのシステム専用の部品を入れる変数
		private readonly UserManager<IdentityUser> _userManager;

		// クラスが呼び出されたときに、システムから必要な2つの部品（Manager）を受け取ってセットする
		// コンストラクタでも、システムからの部品が万が一Nullだったらその場で例外を投げてクラッシュを防ぐ
		public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
		{
			// ユーザー管理部品がNullでないかチェックし、安全に変数を保存
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			// ログイン管理部品がNullでないかチェックし、安全に変数を保存
			_signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
		}

		// 画面の入力フォームとC#の変数をガッチリ結びつけるための設定
		[BindProperty]
		// ユーザーが入力したメールアドレスとパスワードを受け取るための「箱」を準備
		public InputModel Input { get; set; } = new InputModel();

		// 画面側の「Model.ReturnUrl」という指定を受け止めるための変数を定義
		public string? ReturnUrl { get; set; }

		// 画面からの入力データを受け取るためだけの専用の構造（クラス）を定義
		public class InputModel
		{
			// この項目は空っぽでの送信を禁止する（必須チェック）
			[Required]
			// 入力された文字が正しいメールアドレスの形式（@があるか等）かチェックする
			[EmailAddress]
			// ユーザーが入力したメールアドレスを保存する変数
			public string Email { get; set; } = string.Empty;

			// この項目は空っぽでの送信を禁止する（必須チェック）
			[Required]
			// このデータがパスワード（画面では黒丸で隠す文字）であることを指定
			[DataType(DataType.Password)]
			// ユーザーが入力したパスワードを保存する変数
			public string Password { get; set; } = string.Empty;

			// 画面側の確認用パスワードの指定（Input.ConfirmPassword）を受け止めるための箱
			[Required]
			// このデータもパスワード形式として扱う
			[DataType(DataType.Password)]
			// 1回目に入力された「Password」プロパティと同じ文字が入っているか厳しくチェックする
			[Compare("Password", ErrorMessage = "パスワードと確認用パスワードが一致しません。")]
			// ユーザーが入力した確認用のパスワードを保存する変数
			public string ConfirmPassword { get; set; } = string.Empty;
		}

		// ユーザーが最初に登録画面にアクセスした（URLを開いた）ときに動く処理
		public void OnGet(string? returnUrl = null)
		{
			// もし戻り先URL（returnUrl）が空っぽ（Null）だったら、安全のためにルート（/）を代わりに入れておく
			returnUrl ??= Url.Content("~/");

			// 安全性が保証された戻り先URLを、クラス全体の変数（ReturnUrl）にしっかりと保存しておく
			ReturnUrl = returnUrl;
		}

		// ユーザーが画面で「登録ボタン」を押してデータが送られてきたときに動く処理
		public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
		{
			// もし送信された戻り先URLが空っぽ（Null）だったら、安全のためにルート（/）を代わりにセットする
			returnUrl ??= Url.Content("~/");
			// 画面が再表示されたときのために、安全な戻り先URLを現在の変数に上書きセットしておく
			ReturnUrl = returnUrl;

			// メールアドレスの形式や必須チェックなど、画面の入力ルールが1つでも違反していれば、これ以上先の重たい処理に進ませずに即座に現在の画面に追い返す
			if (!ModelState.IsValid)
			{
				// データベース処理を一切行わない安全な状態のまま、エラーメッセージを乗せて現在の画面を再表示する
				return Page();
			}

			// ここから下は、入力データが100%安全であると「証明された通信」だけが通過できる

			// データベースに保存するために、入力されたメールアドレスを持った新しいユーザー枠を作成
			var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
			// 枠（user）とパスワード（Input.Password）をシステムに渡し、データベースへの登録を実行する
			var result = await _userManager.CreateAsync(user, Input.Password);

			// データベースへのユーザー登録が、見事に成功した場合の処理
			if (result.Succeeded)
			{
				// 本来ここに書いてあった「自動ログイン処理（_signInManager.SignInAsync）」をあえて書かずに完全に無視します。

				// 自動ログインをさせないまま、即座にログイン画面（Loginページ）へと強制的にジャンプさせる
				return RedirectToPage("Login");
			}

			// もし「パスワードが短すぎる」などの理由でデータベース登録に失敗した場合の処理
			foreach (var error in result.Errors)
			{
				// 発生したエラーメッセージを画面（赤い文字のエリア）に表示するように登録する
				ModelState.AddModelError(string.Empty, error.Description);
			}

			// 入力チェックやデータベース登録でエラーがあった場合は、現在の登録画面をもう一度表示する
			return Page();
		}
	}
}