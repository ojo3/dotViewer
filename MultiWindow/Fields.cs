using ojo3Utility.Common;
using ojo3Utility.Controls.ContextMenu;
using ojo3Utility.Controls.ContextMenu.MenuItem;
using ojo3Utility.Pictures;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static ojo3Utility.Global.Const;

namespace dotViewer
{
    public partial class MultiWindow
    {
        // TotalSize
        private static readonly List<MultiWindow> _windowList = new List<MultiWindow>();
        //private static readonly Style _style = Style.Dark;
        private static readonly string _iniFile = AppSettings.GetCurrentPath("data.bin");
        private static readonly string _imageSaveDirectoryName = "save";
        private static readonly string _imageSaveDirectory = AppSettings.GetCurrentPath(_imageSaveDirectoryName);

        // UI
        // Controls
        private static readonly Font _baseFont = new Font("Yu Gothic UI", 12.26f, FontStyle.Regular);
        private static readonly float _lblDefaultFontSize = 32;
        private bool _isShift;
        private bool _isCtrl;
        private bool _isAlt;
        private bool _isLeftButton;
        private bool _isRightButton;

        /****************************************************/

        // Application
        // コマンドラインからの起動を通知
        private static bool _isCommandLineLaunch;
        private static bool _isTiling;
        /// <summary>
        /// ロード済みを通知
        /// </summary>
        private static bool _isLoadComplete;
        private static bool _enableFocus = true;
        private static readonly int _sendMax = 10;
        private static readonly int _defaultOffset = 32;
        // ウィンドウ位置のオフセット初期値
        private static int _offset = _defaultOffset;
        private static int _currentFocusIndex = -1;

        // Item
        private static int _windowIndex = -1;
        private static bool _isParent = true;
        private static bool _isfade = true;
        private bool _isAdjustSizeForAspectRatio;
        private bool _isRedraw;
        private bool _isResize;
        private bool _isAnime;
        private float _bmpResolution;
        private int _currentFileIndex;
        // 基底となるサイズ
        private static readonly Size _defaultSize = new Size(256, 274);
        private static Size _baseSize = _defaultSize;
        private Size _bmpSize;
        // マウスダウンした位置情報
        private Point _mouseDownPoint;
        private Bitmap _currentImage;
        private Bitmap _processedImage;
        private Bitmap _clipBoardImage;
        private Anime _anime;
        private Picture _picture;
        private ImageItem _imageItem;
        private PathInfo _pathInfo;
        private readonly List<string> _files = new List<string>();

        /****************************************************/

        private Ojo3ContextMenuStrip contextMenuStrip;
        private Label lblinfo_filterValue;
        private Label lblinfo_image;

        private readonly List<Ojo3MenuItem<ContextMenuItems>> _parentMenu = new List<Ojo3MenuItem<ContextMenuItems>>()
        {
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Open, "開く", hotkeys: HotKeys.Open),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Paste, "貼り付け", hotkeys: HotKeys.Paste),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.TopMost, "トップに表示", hotkeys: (HotKeys)Keys.T),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Close, "終了", hotkeys: HotKeys.Close),
        };

        private readonly List<Ojo3MenuItem<ContextMenuItems>> _childMenu = new List<Ojo3MenuItem<ContextMenuItems>>()
        {
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Copy, "コピー", hotkeys: HotKeys.Copy),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Paste, "貼り付け", hotkeys: HotKeys.Paste),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Duplication, "複製", hotkeys: (HotKeys)(Keys.Control | Keys.D)),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Open, "ファイルを開く", hotkeys: HotKeys.Open),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Save, "保存", hotkeys: HotKeys.Save),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Print, "印刷", hotkeys: HotKeys.Print),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.None, "画像処理")
            {
                Children = new List<Ojo3MenuItem<ContextMenuItems>>()
                {
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.GrayScale, "グレー", hotkeys: (HotKeys)Keys.F9),
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Binary, "バイナリ", hotkeys: (HotKeys)Keys.F10),
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Sepia, "セピア", hotkeys: (HotKeys)Keys.F11),
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Night, "ナイト", hotkeys : (HotKeys)Keys.F12),
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Invert, "色反転", hotkeys : (HotKeys)(Keys.Control | Keys.I)),
                    new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Palette, "カラーパレットを生成", hotkeys : (HotKeys)(Keys.Control| Keys.Shift | Keys.P)),
                }
            },
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.KeepRatio, "アスペクト比をキープ", hotkeys: (HotKeys)Keys.K),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.ShowInTaskbar, "タスクバーに表示しない"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.TopMost, "トップに表示", hotkeys: (HotKeys)Keys.T),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Separator, "-"),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Minimized, "最小化", hotkeys: (HotKeys)Keys.F4),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.AllClose, "すべて閉じる", hotkeys: HotKeys.Quit),
            new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.Close, "閉じる", hotkeys: HotKeys.Close),
        };

        private PictureBox _pbxRank;
        private Bitmap _icoStar = Icons.GetIcon(Icons.SystemIcons.Star);
        private static int _thumbnailWidth = 50;
        private static int _accessFileHistoryMax = 15;
        private static string _logAccessFileHistory = AppSettings.GetCurrentPath("history.txt");
        private static List<AccessHistoryItem> _accessFileHistory = new List<AccessHistoryItem>();
        /// <summary>
        /// ファイル履歴を格納
        /// </summary>
        private static Ojo3MenuItem<ContextMenuItems> _accessFileHistoryMenu;
    }
}
