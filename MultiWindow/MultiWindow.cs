using ojo3Utility.Collections;
using ojo3Utility.Common;
using ojo3Utility.Common.Data;
using ojo3Utility.Controls.ContextMenu;
using ojo3Utility.Controls.ContextMenu.MenuItem;
using ojo3Utility.Controls.Forms;
using ojo3Utility.Controls.Mouse;
using ojo3Utility.Controls.TimerControls;
using ojo3Utility.Controls.Utility;
using ojo3Utility.Global;
using ojo3Utility.Pictures;
using ojo3Utility.Pictures.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotViewer
{
    public partial class MultiWindow : Ojo3Form
    {
        public MultiWindow(object obj = null, Point? location = null, bool isAdjustSizeForAspectRatio = false)
        {
            Add(this);

            _backgroundTileImage = GetTileImage();

            FileReceiver = async (args) => await SendFiles(args);

            Initialize_WindowItem(obj, (Point)location, isAdjustSizeForAspectRatio);

            InitializeComponent();

            InitializeComponent_Local();
        }

        public void Initialize_WindowItem(object obj, Point location, bool isAdjustSizeForAspectRatio = false)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            var path = string.Empty;

            _isAdjustSizeForAspectRatio = isAdjustSizeForAspectRatio;

            switch (obj)
            {
                case Bitmap bmp:
                    {
                        _clipBoardImage = bmp;
                        break;
                    }
                case string str:
                    {
                        path = str;
                        break;
                    }
                case WindowItem wi:
                    {
                        LocalWindowItem = wi;
                        _baseSize = LocalWindowItem.Size;
                        path = LocalWindowItem.FilePath;
                        location = LocalWindowItem.Location;
                        TopMost = LocalWindowItem.TopMost;
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            //$"{IsParent}) {location}, {LocalWindowItem.Location}".test();

            Initialize_WindowItem(path, location);
        }

        private void Initialize_WindowItem(string path, Point location)
        {
            // Focusを取っているウィンドウのindex
            //_currentFocusIndex = Index;

            LocalWindowItem.FilePath = path;

            if (location != Point.Empty)
            {
                LocalWindowItem.Location = location;
            }

            GeneratePathinfo(path);
        }

        /****************************************************/

        private void InitializeComponent_Local()
        {
            Name = $"{nameof(MultiWindow)}_{Index}";
            TopMost = true;
            //Styling = _style;
            FadeDuration = 200;
            //_isfade = FadeDuration > 0;

            IsParent = _isParent;
            _isParent = false;

            Index = _windowIndex;

            if (IsParent)
            {
                InitializeComponent_Parent();
            }
            else
            {
                InitializeComponent_Child();
                Initialize_Controls();
            }

            //contextMenuStrip = new Ojo3ContextMenuStrip(Styling)
            //{
            //    Font = _baseFont,
            //};
        }

        private void InitializeComponent_Parent()
        {
            // Ojo3Formの基本実装で設定を保存
            IsSaveSettings = true;

            // キャプションを表示しない
            FormBorderStyle = FormBorderStyle.FixedSingle;
            //FormBorderStyle = FormBorderStyle.None;
            //FormBorderStyle.test();
            Padding = new Padding(0);
            MaximizeBox = false;

            // リサイズハンドル非表示
            statusStrip1.SizingGrip = false;

            systemInfoLabel1.Text = string.Empty;
            systemInfoLabel1.InfoType = SystemInfoType.Memory;
            systemInfoLabel1.TextAlign = ContentAlignment.MiddleLeft;
            systemInfoLabel1.Start();
        }

        private int _pensize = 4;
        private void InitializeComponent_Child()
        {
            // 明示的にセットすることでHandleを保持
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            // リサイズ用の感知領域...?
            Padding = new Padding(_pensize);

            _pbx.SetDoubleBuffer(true);
            _pbx.SizeMode = PictureBoxSizeMode.Normal;
            _pbx.BackColor = Color.Transparent;
            _pbx.TabStop = true;

            //statusStrip1.Font.test();
            Controls.Remove(statusStrip1);
        }

        private void Initialize_Controls()
        {
            if (IsParent) throw new Exception("親では呼ばないでください.");

            var visible = false;

            var width = (int)(_pbx.Width * 0.3);

            _lbl_ColorPreview = new Label
            {
                BackColor = Color.Red,
                Size = new Size(width, width),
                Visible = false,
                BorderStyle = BorderStyle.None,
            };
            _pbx.Controls.Add(_lbl_ColorPreview);
            _lbl_ColorPreview.BringToFront();
            _lbl_ColorPreview.Paint += (s, e) =>
            {
                var rect = _lbl_ColorPreview.ClientRectangle;
                using (var pen = new Pen(Color.Red)
                {
                    Width = 4,
                })
                {
                    rect.Inflate(-_pensize, -_pensize);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            lblinfo_image = new Label()
            {
                Text = "[ZoomScale: -%] / [X: -, Y: -] / [X: -, Y: -]",
                BackColor = Color.FromArgb(150, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font(_baseFont.FontFamily, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Visible = visible,
                Dock = DockStyle.Bottom,
                Padding = new Padding(2, 2, 2, 2),
                Height = 32,
            };
            _pbx.Controls.Add(lblinfo_image);
            lblinfo_image.Width = _pbx.Width - 4;
            lblinfo_image.BringToFront();

            lblinfo_filterValue = new Label()
            {
                Text = "100%",
                BackColor = Color.Transparent,
                Font = new Font(_baseFont.FontFamily, _lblDefaultFontSize),
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Margin = new Padding(10),
                Visible = false,
            };
            lblinfo_filterValue.SetDoubleBuffer(true);
            _pbx.Controls.Add(lblinfo_filterValue);

            _pbxRank = new PictureBox
            {
                Size = new Size(_icoStar.Width, _icoStar.Width),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(5),
                Visible = visible,
            };

            //Initialize_BookMark();
            Initialize_Rank();

            _pbx.Controls.Add(_pbxRank);
            _pbxRank.SetDoubleBuffer(true);
        }

        /****************************************************/

        private void Initialize_BookMark()
        {
            //LocalWindowItem.Bookmark.test();
            DrawBookMark(LocalWindowItem);

            _pbxRank.MouseClick += (s, e) =>
            {
                LocalWindowItem.Bookmark = !LocalWindowItem.Bookmark;
                DrawBookMark(LocalWindowItem);
            };
        }
        private void DrawBookMark(WindowItem item)
        {
            _pbxRank.Image?.Dispose();
            _pbxRank.Image = null;
            _pbxRank.Width = _icoStar.Width;
            _pbxRank.Image = new Bitmap(_pbxRank.Width, _pbxRank.Height);

            using (var icon = new Bitmap(_icoStar))
            using (var g = Graphics.FromImage(_pbxRank.Image))
            {
                if (!item.Bookmark)
                {
                    icon.Decolor(ImageFilters.GrayScale);
                }
                g.Clear(_pbxRank.BackColor);
                g.DrawImage(icon, new Rectangle(0, 0, _icoStar.Width, _icoStar.Width));
            }
        }

        private int _iconWidth_star = 32;
        private void Initialize_Rank()
        {
            DrawRank(LocalWindowItem);

            _pbxRank.MouseMove += (s, e) =>
            {
                var item = new WindowItem()
                {
                    Rank = (Rank)(e.Location.X / _iconWidth_star + 1)
                };
                DrawRank(item);
            };
            _pbxRank.MouseClick += (s, e) =>
            {
                LocalWindowItem.Rank = (Rank)(e.Location.X / _iconWidth_star + 1);
                DrawRank(LocalWindowItem);
            };
            _pbxRank.MouseLeave += (s, e) => DrawRank(LocalWindowItem);
        }
        private void DrawRank(WindowItem item)
        {
            _pbxRank.Image?.Dispose();
            _pbxRank.Image = null;

            _pbxRank.Width = _iconWidth_star * 5;
            _pbxRank.Height = _iconWidth_star;
            _pbxRank.Image = new Bitmap(_pbxRank.Width, _pbxRank.Height);

            using (var g = Graphics.FromImage(_pbxRank.Image))
            {
                var rank = (int)item.Rank;
                var diff = 5 - rank;
                var width = _iconWidth_star;
                var rect = _pbxRank.ClientRectangle;

                g.Clear(Color.Transparent);

                using (var icon = new Bitmap(_icoStar))
                {
                    rect = RenderingImage(g, icon, 0, rank, rect, width);

                    if (diff <= 0) return;

                    icon.Decolor(ImageFilters.GrayScale);

                    RenderingImage(g, icon, 0, diff, rect, width);
                }
            }
        }

        private Rectangle RenderingImage(Graphics g, Bitmap icon, int start, int end, Rectangle rect, int width)
        {
            for (var i = 0; i < end; i++)
            {
                g.DrawImage(icon, new Rectangle(rect.X, rect.Y, width, width));
                rect.X += width;
            }

            return rect;
        }

        /****************************************************/

        private void Add(MultiWindow mw)
        {
            _windowList.Add(mw);
            ++_windowIndex;
        }
        private void Remove(MultiWindow mw)
        {
            _windowList.Remove(mw);
            _windowIndex = _windowList.Count - 1;
        }

        /****************************************************/

        private async Task SendFiles(string[] files)
        {
            if (files.Length == 0) return;

            files = IO.GetImageFiles(files);

            var count = files.Length;

            if (count == 0)
            {
                Ojo3MessageBox.Show("対応ファイルはありません");
                return;
            }

            _isLoadComplete = false;

            if (count >= _sendMax)
            {
                if (_isCommandLineLaunch)
                {

                }
                else
                {
                    if (Ojo3MessageBox.Show($"ファイル数が{_sendMax}枚を超えています.\n処理を続けますか？\n\n" +
                        $"ファイル数 => {count}個 (およそ{count * 23.75}Mb 消費)",
                        MessageBoxButtons.YesNo) == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                // タイリングする場合はソート
                files = files.OrderBy(a => a).ToArray();

                _isTiling = true;

                _isfade = count < 10;
            }

            // 親ウィンドウかつUI操作の場合は最小化
            //if (IsParent && !_isCommandLineLaunch)
            if (IsParent)
            {
                WindowState = FormWindowState.Minimized;
            }

            await ForeachTask(files);

            // ファイルが多い場合は並べる
            if (_isTiling)
                Tiling();

            // オフセットをリセット
            _offset = _defaultOffset;

            CreateAccessHistory(files);
        }
        private void FilesDialog()
        {
            var file = IO.FilesDialog(Extention.ImageExtentions);
            FileReceiver(file);
        }

        /****************************************************/

        private async void CreateAccessHistory(string[] files)
        {
            // すべての子ウィンドウのContextMenuStripを初期化
            _accessFileHistory.AddRange(files.Select(f => new AccessHistoryItem(f, null)));
            _accessFileHistory = _accessFileHistory.Distinct().ToList();
            await _windowList.ForEachAsync((w, i) => w.CreateContextMenu(true));
        }
        private async void CreateContextMenu(bool reset = false)
        {
            if (reset)
            {
                ContextMenuStrip = null;
            }

            if (ContextMenuStrip != null)
            {
                //"return ContextMenuStrip != null".test();
                return;
            }

            await CreateAccessHistory();

            if (_accessFileHistoryMenu == null)
            {
                //"return _accessFileHistoryMenu == null".test();
                return;
            }

            //$"CreateContextMenu: {reset}".test();

            var list = IsParent ? _parentMenu : _childMenu;

            var idx = list.FindIndex(m => m.Type == ContextMenuItems.History);

            // 履歴が追加済の場合は、削除
            if (idx != -1)
            {
                list.RemoveAt(idx);
            }
            else
            {
                idx = 0;
            }

            // 履歴メニューの可否
            _accessFileHistoryMenu.Enable = _accessFileHistory.Count > 0;

            // 履歴をidxに追加
            list.Insert(idx, _accessFileHistoryMenu);

            if (contextMenuStrip == null)
            {
                contextMenuStrip = new Ojo3ContextMenuStrip(Styling)
                {
                    Font = _baseFont,
                };
            }

            ContextMenuStrip = contextMenuStrip.CreateMenuItems(IsParent ? _parentMenu : _childMenu, ContextMenu_Click, (item, tsmi) => FilterMenu(list));

            SetEnable_MenuItems(ContextMenuItems.Paste, false, Clipboard.ContainsImage());
            //(ontextMenuStrip != null).test();
        }
        private async Task CreateAccessHistory()
        {
            _accessFileHistoryMenu = null;

            await Task.Run(async () =>
            {
                // 子ウィンドウを読み込むまで待機
                while (!_isLoadComplete)
                {
                    await Task.Delay(200);
                    continue;
                }

                if (_accessFileHistory.Count == 0)
                {
                    _accessFileHistory = (await Log.ReadLines(_logAccessFileHistory)
                        .WhereAsync(h => File.Exists(h))
                        .SelectAsync(h => new AccessHistoryItem(h, null)))
                        .ToList();
                    //_accessFileHistory.Count.test();
                }

                var historyItem = new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.History, "履歴");

                if (_accessFileHistory.Count == 0)
                {
                    _accessFileHistoryMenu = historyItem;
                    return;
                }

                var list = new List<AccessHistoryItem>(_accessFileHistory);

                historyItem.Children = list.ReverseList()
                    .Take(_accessFileHistoryMax)
                    .Select((ai, i) =>
                    {
                        if (ai.Image == null)
                            ai.Image = Ojo3Image.Resize(Ojo3Image.ReadImage(ai.FilePath), _thumbnailWidth);
                        return new Ojo3MenuItem<ContextMenuItems>(ContextMenuItems.History, ai.FilePath, ai.Image);
                    }).ToList();

                _accessFileHistoryMenu = historyItem;
            });
        }
        private void ContextMenuStripNull() => ContextMenuStrip = null;
        private bool FilterMenu(List<Ojo3MenuItem<ContextMenuItems>> list)
        {
            bool isContainImage = false;

            try
            {
                var isSta = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
                //isSta.test();
                isContainImage = Clipboard.ContainsImage();
            }
            catch (Exception)
            {
            }

            foreach (var m in list)
            {
                m.Checked = false;

                if (m.Type == ContextMenuItems.Paste)
                {
                    m.Enable = isContainImage;
                    m.HotKey = Const.HotKeys.Paste;
                }
                else if (m.Type == ContextMenuItems.TopMost)
                {
                    m.Checked = TopMost;
                }
                else if (m.Type == ContextMenuItems.ShowInTaskbar)
                {
                    m.Checked = !ShowInTaskbar;
                }
                else if (m.Type == ContextMenuItems.KeepRatio)
                {
                    m.Checked = LocalWindowItem.KeepRatio;
                    m.Enable = ImageType == ImageType.Image;
                }

                //m.Type.test();

                if (m.HasChild)
                {
                    foreach (var c in m.Children)
                    {
                        // チェックを初期化
                        c.Checked = false;

                        switch (c.Type)
                        {
                            case ContextMenuItems.GrayScale:
                                c.Checked = LocalWindowItem.ImageFilterItem.Filter == ImageFilters.GrayScale;
                                break;
                            case ContextMenuItems.Binary:
                                c.Checked = LocalWindowItem.ImageFilterItem.Filter == ImageFilters.Binary;
                                break;
                            case ContextMenuItems.Sepia:
                                c.Checked = LocalWindowItem.ImageFilterItem.Filter == ImageFilters.Sepia;
                                break;
                            case ContextMenuItems.Night:
                                c.Checked = LocalWindowItem.ImageFilterItem.Filter == ImageFilters.Night;
                                break;
                            case ContextMenuItems.Invert:
                                c.Checked = LocalWindowItem.ImageFilterItem.Invert;
                                break;
                        }
                    }
                }
            }

            return true;
        }

        /****************************************************/

        private void LoadImage()
        {
            _processedImage?.Dispose();
            _processedImage = null;
            _currentImage?.Dispose();
            _currentImage = null;
            _imageItem?.Dispose();
            _imageItem = null;

            if (IsParent)
            {
                _pbx.SizeMode = PictureBoxSizeMode.StretchImage;
                //_currentImage = GetInitializeImage(_pbx.Size);
                return;
            }

            if (_clipBoardImage != null)
            {
                _imageItem = new ImageItem(_clipBoardImage, false);
            }
            else
            {
                _imageItem = Ojo3Image.Read(LocalWindowItem.FilePath);
            }

            if (_imageItem.Image != null)
            {
                _currentImage = _imageItem.Image;
                _bmpSize = _currentImage.Size;
                _bmpResolution = _currentImage.HorizontalResolution;
                _isAnime = _imageItem.IsAnimatedImage;
                LocalWindowItem.Orientation = _imageItem.Orientation;
                LocalWindowItem.ImageRatio = Ojo3Image.GetRatioOfWidthToHeight(_bmpSize);

                // WindowItemの親がLandscape以外かつ
                // 自身がLandscapeの場合に、高さを基準に幅を決定
                _isAdjustSizeForAspectRatio &= LocalWindowItem.Orientation == ImageOrientation.Landscape;
            }

            //_imageItem.test();
        }
        private void CreateProcessedImage()
        {
            if (_currentImage == null) return;
            if (_isAnime) return;

            _processedImage?.Dispose();
            _processedImage = null;

            if (LocalWindowItem.ImageFilterItem.IsChanged)
            {
                _processedImage = new Bitmap(_currentImage);
                _processedImage.Decolor(LocalWindowItem.ImageFilterItem);
            }
        }

        /****************************************************/

        private void InitializeWindow(bool sizeReset = true, [CallerMemberName] string caller = null)
        {
            _isResize = true;

            var size = GetInitialSize(sizeReset);
            //var size = Size;
            ;

            // 初期ウィンドウ
            if (IsParent)
            {
                InitializeWindow_Parent(size);
            }
            else
            {
                //_currentImage = null;
                if (_currentImage == null)
                {
                    Error();
                    return;
                }

                InitializeWindow_Child(sizeReset, size);
                AdjustLabel_FilterValue();
            }


            _isResize = false;
        }
        private Size GetInitialSize(bool sizeReset) => IsParent ? _defaultSize : (sizeReset ? _baseSize : Size);
        private void InitializeWindow_Parent(Size size)
        {
            MaximumSize = size;
            MinimumSize = size;
            Size = size;
        }
        /// <summary>
        /// 比率を保持してサイズ決定を行う通知を初期化します。
        /// </summary>
        private void ResetWindowRetio()
        {
            _windowRatio = 1.0f;
        }
        private void InitializeWindow_Child(bool sizeReset, Size size)
        {
            // アスペクト比が初期の場合、
            // もしくはリセットされた場合
            // 比率を保持してサイズ決定
            if (_windowRatio == 1.0f)
            {
                size = AdjustSizeForAspectRatio(sizeReset, size);
            }

            if (WindowState == FormWindowState.Normal)
            {
                SetWindowSize(size);
                AdjustRankLocation();
            }

            AdjustWindowLocation();
        }
        private Size AdjustSizeForAspectRatio(bool sizeReset, Size size)
        {
            //$"sizeReset: {sizeReset}, _isAdjustSizeForAspectRatio: {_isAdjustSizeForAspectRatio}".test();

            if (LocalWindowItem.KeepRatio)
            {
                // ■比率を保持してサイズ決定
                // WindowItemの親がLandscape以外かつ
                // 自身がLandscapeの場合に、高さを基準に幅を決定
                if (sizeReset && _isAdjustSizeForAspectRatio)
                {
                    "幅を調整しました.".test();
                    var ratio = (float)_bmpSize.Width / _bmpSize.Height;
                    size.Height = size.Width;
                    size.Width = (int)(size.Width * ratio);
                }
                else
                {
                    size = Rect.GetSizeFixedWidth(_bmpSize, size.Width);
                }
            }

            // Sizeをセットする前に(幅を高さで割る)
            _windowRatio = Ojo3Image.GetRatioOfWidthToHeight(size);

            return size;
        }
        private void SetWindowSize(Size size)
        {
            //_isResize = true;

            // MinimumSizeを先にセット
            if (MinimumSize == Size.Empty)
            {
                if (LocalWindowItem.KeepRatio)
                {
                    MinimumSize = CalculateMinimumSize(_defaultSize.Width, LocalWindowItem.Orientation, _bmpSize);
                }
                else
                {
                    MinimumSize = _defaultSize;
                }
                //MinimumSize.test();
            }

            //SuspendLayout();
            Size = size;
            //ResumeLayout();
            //PerformLayout();

            LocalWindowItem.Size = size;

            //_isResize = false;
        }
        private void AdjustRankLocation()
        {
            if (_pbxRank != null)
            {
                _pbxRank.Location = new Point(_pbx.Width - _pbxRank.Width - _pbxRank.Margin.Right, _pbxRank.Margin.Top);
            }
        }
        private Size CalculateMinimumSize(int defaultWidth, ImageOrientation orientation, Size bmpSize)
        {
            // 幅を高さにセットして、幅を比率に準じて再計算
            if (orientation == ImageOrientation.Landscape)
            {
                var ratio = Ojo3Image.GetRatioOfWidthToHeight(bmpSize);
                return new Size((int)(defaultWidth * ratio), defaultWidth);
            }
            else
            {
                var ratio = Ojo3Image.GetRatioOfHeightToWidth(bmpSize);
                return new Size(defaultWidth, (int)(defaultWidth * ratio));
            }
        }
        private void AdjustWindowLocation()
        {
            if (_isTiling || !_enableFocus) return;

            // タイリングしていない場合に、ウィンドウの位置を調整
            // X軸Y軸からはみ出たらセンタリングに補正
            LocalWindowItem.Location = Location = Rect.GetAllScreenBounds().Contains(LocalWindowItem.Location) ? LocalWindowItem.Location : Rect.GetCenter_Screen(this);
        }
        private bool Error()
        {
            if (_currentImage != null) return false;

            _isLoadComplete = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            lblinfo_filterValue.BlueScreen();
            this.Draggable(true, lblinfo_filterValue);
            ShowLabel($"読み込みエラー.\n\n----------\n{LocalWindowItem}");
            return true;
        }

        /****************************************************/

        private void ReDraw(bool reset = true, bool filerReset = true)
        {
            if (_currentImage == null)
            {
                return;
            }

            if (IsParent)
            {
                _pbx.Image = _currentImage;
                return;
            }

            if (reset)
            {
                ResetResources();
            }

            if (_isAnime)
            {
                lblinfo_image.Visible = true;
                InitializeAnimeIfNeeded();
                if (filerReset)
                {
                    ApplyAnimeSettings();
                }
                _anime.Play();
            }
            else
            {
                InitializePictureIfNeeded(reset);
                _picture.Draw(_processedImage ?? _currentImage, _pbx.Width, _pbx.Height);
                SetCaption();
                //BookMark(LocalWindowItem);
            }
        }
        private void ResetResources()
        {
            _anime?.Dispose();
            _anime = null;
            _picture?.Dispose();
            _picture = null;
        }
        private void InitializeAnimeIfNeeded()
        {
            if (_anime == null)
            {
                _anime = new Anime(ref _pbx, _currentImage);
                _anime.ProgressInvoker.ProgressChanged += (e) => Update_lblinfo(e.Message);
            }
        }
        private void ApplyAnimeSettings()
        {
            if (_isAnime)
            {
                _anime.ImageFilterItem = LocalWindowItem.ImageFilterItem;
            }
        }
        private void InitializePictureIfNeeded(bool reset = false)
        {
            if (_picture == null)
            {
                _picture = new Picture(ref _pbx);
            }
            if (reset)
            {
                _picture.MatrixReset();
            }
        }

        /****************************************************/

        // すべてのプロパティをリセットする
        private void ImageFilterItemReset_All()
        {
            //LocalWindowItem.ImageRatio = 1.0f;
            ResetWindowRetio();
            LocalWindowItem.ImageFilterItem.Reset();
        }
        // 個々のプロパティをリセットする
        private void ImageFilterItemReset()
        {
            _isRedraw = _isShift || _isCtrl || _isAlt;

            if (_isShift)
            {
                LocalWindowItem.ImageFilterItem.Reset(true, false, false, false);
            }
            else if (_isCtrl)
            {
                LocalWindowItem.ImageFilterItem.Reset(false, true, false, false);
            }
            else if (_isAlt)
            {
                LocalWindowItem.ImageFilterItem.Reset(false, false, true, false);
            }
        }
        private void SetImageFilters(ImageFilters filters)
        {
            LocalWindowItem.ImageFilterItem.Filter = LocalWindowItem.ImageFilterItem.Filter != filters ? filters : ImageFilters.None;
        }
        private Bitmap MakeOutputImage(bool rawImage = false)
        {
            var bmp = new Bitmap(rawImage ? _currentImage : _pbx.Image);

            if (LocalWindowItem.ImageFilterItem.IsChanged)
            {
                bmp.Decolor(LocalWindowItem.ImageFilterItem);
            }

            using (var pic = new Picture(bmp.Width, bmp.Height))
            {
                pic.Draw(bmp, bmp.Width, bmp.Height);
                return new Bitmap(pic.DrawnImage);
            }
        }

        /****************************************************/

        private void InitializeEvents()
        {
            this.Draggable(true, _pbx);
            //this.DragAndDrop((args) => SendFiles(args));
            this.DragAndDrop(FileReceiver);

            _pbx.MouseDown += Pbx_MouseDown;
            _pbx.MouseUp += Pbx_MouseUp;

            if (_currentImage != null)
            {
                if (!IsParent)
                {
                    _pbx.MouseDoubleClick += Pbx_MouseDoubleClick;
                    _pbx.MouseWheel += Pbx_MouseWheel;
                    _pbx.MouseMove += Pbx_MouseMove;
                }
            }
        }

        /****************************************************/

        private void WindowClose(bool allClose = false)
        {
            if (allClose)
            {
                CloseAll();
                return;
            }

            Close();
        }
        private void CloseAll()
        {
            _isTiling = false;
            // 子フォームの削除
            _windowList.Where(w => !w.IsParent).ForEach((w, i) => w.Close());
            Reset();
        }
        private void Reset()
        {
            if (IsParent || _windowList.Count != 1) return;

            _isTiling = false;

            _windowList.RemoveAll(w => !w.IsParent);

            // 親ウィンドウ
            var mw = _windowList[0];

            _baseSize = mw.Size;
            _windowIndex = mw.Index;
            _isfade = mw.FadeDuration > 0;

            mw.WindowState = FormWindowState.Normal;
            mw.Activate();
        }

        /****************************************************/

        private void GenerateWindow(string path)
        {
            if (path.Length > 0)
            {
                GenerateWindow_Core(path, ImageType.Image, !_isCommandLineLaunch);
            }
        }
        private void GenerateWindow(Bitmap bmp, ImageType imageType = ImageType.Image)
        {
            if (bmp != null && bmp.Width != 0 && bmp.Height != 0)
            {
                GenerateWindow_Core(bmp, imageType, true);
            }
        }
        private void GenerateWindow(WindowItem wi)
        {
            if (!string.IsNullOrEmpty(wi.FilePath))
                GenerateWindow_Core(wi, ImageType.Image, false);
        }
        private void GenerateWindow_Core<T>(T obj, ImageType imageType, bool checkOrientation)
        {
            var location = GetLocationSetSize(imageType);

            // 親がLandscapeではなく、自身がLandscapeの場合に
            // 高さ基準で幅を決定
            var isAdjustSizeForAspectRatio = checkOrientation & LocalWindowItem.Orientation != ImageOrientation.Landscape;

            _isfade.test(true);
            var mw = new MultiWindow(obj, location, isAdjustSizeForAspectRatio)
            {
                FadeDuration = _isfade ? 200 : 0,
                ImageType = imageType
            };

            // 読み込み済みの場合は、KeepRatioをセット
            if (_isLoadComplete)
            {
                mw.LocalWindowItem.KeepRatio = LocalWindowItem.KeepRatio;
            }
            else if (imageType == ImageType.Palette)
            {
                mw.LocalWindowItem.KeepRatio = false;
            }

            mw.Show();

            // OpenerをActiveにすることで、
            // 子が開いたときに、子がFocusを取れる
            Activate();
        }
        private async Task ForeachTask<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (item is string path)
                    GenerateWindow(path);
                else if (item is WindowItem wi)
                    GenerateWindow(wi);
                await Task.Delay(_isfade ? 200 : 0);
            }
            //await items.ForEachAsync(async (item) =>
            //{
            //    this.Invoking(() => GenerateWindow(item));
            //    await Task.Delay(_isfade ? 200 : 0);
            //});
        }

        private Point GetLocationSetSize(ImageType imageType)
        {
            var location = Location;

            //var location = Point.Empty;

            // コマンドライン起動の場合
            // 親ウィンドウのサイズを基準とする
            if (_isCommandLineLaunch)
            {
                _baseSize = _defaultSize;
                location = Rect.GetCenter_Screen(this);
            }
            // パレットの場合
            else if (imageType == ImageType.Palette)
            {
                _baseSize = new Size(360, 360);
            }
            // 元ウィンドウにドラッグアンドドロップした場合
            else if (WindowState == FormWindowState.Normal)
            {
                _baseSize = Size;
            }

            location.X += _offset;
            location.Y += _offset;
            _offset += 32;

            //$"{_baseSize}, {location}".test();

            return location;
        }
        private void GeneratePathinfo(string path)
        {
            if (path.Length > 0)
            {
                _pathInfo = new PathInfo(path);
                Caption = _pathInfo.Name;
            }
        }

        /****************************************************/

        private async void Tiling()
        {
            var list = Children.Where(w => w.WindowState == FormWindowState.Normal);

            if (!_isTiling)
            {
                await list.ForEachAsync((mw, i) => mw.Location = mw.LocalWindowItem.Location);
                return;
            }

            var screen = Screen.FromPoint(Location);

            var screenRectangle = screen.WorkingArea;

            var taskbarHeight = screen.Bounds.Height - screen.WorkingArea.Height;

            var locations = GetTilingPoints(list.Select(w => w.Size).ToList(), screenRectangle, 10, taskbarHeight + Padding.Top + Padding.Bottom);

            await list.OrderBy(mw => mw.LocalWindowItem.Orientation).ForEachAsync((mw, i) => mw.Location = locations[i]);
        }
        public static List<Point> GetTilingPoints(List<Size> list, Rectangle rectangle, int marginSize = 0, int additionalMargin = 0)
        {
            var x = rectangle.Left + marginSize;
            var y = rectangle.Top + marginSize;
            var marginX = marginSize;
            var marginY = marginSize;
            var points = new List<Point>();
            var columnHeight = new Dictionary<int, Size>();
            var enableArea = new Rectangle(x, y, 0, 0);
            var column = 0;
            var rows = 0;

            for (var i = 0; i < list.Count; i++)
            {
                var size = list[i];

                // 右にはみ出たらカラムを戻して、ロウを上げる
                if (x + size.Width >= rectangle.Right)
                {
                    column = 0;
                    rows++;
                    marginY = marginSize;
                    x = rectangle.Left + marginX;
                }

                // 先頭列なら
                if (column == 0)
                {
                    enableArea.Height += size.Height + marginY;
                }

                // １行目だけ幅を加算
                if (rows == 0)
                {
                    enableArea.Width += size.Width + marginX;
                }

                // スクリーンの下部を超えそうになったら抜ける
                if (enableArea.Height >= rectangle.Bottom)
                {
                    break;
                }

                // 2行目以降は列を参照
                var find = columnHeight.ContainsKey(column);

                if (find)
                {
                    y = columnHeight[column].Height;

                    if (columnHeight[column].Width < size.Width)
                    {
                        //$"はみ出てます => [{rows}, {column}]".test();
                    }
                }

                points.Add(new Point(x, y));

                // _baseSizeとSizeの差分を足し込んで位置を調整！！
                var mx = size.Width + marginX;
                var newsize = new Size(mx, size.Height + marginY + additionalMargin);
                x += mx;

                if (find)
                {
                    columnHeight[column] += newsize;
                }
                else
                {
                    columnHeight.Add(column, newsize);
                }

                //$"[{rows}, {column}], {columnHeight[column]}".test();

                column++;
            }

            //"-----------------------------------".test();

            return points;
        }
        private async void ShowAll()
        {
            if (_windowList.Count == 1) return;

            await Children.ForEachAsync((w, i) =>
            {
                w.Show();

                if (w.WindowState == FormWindowState.Minimized)
                {
                    w.WindowState = FormWindowState.Normal;
                }

                if (!w.TopMost)
                {
                    w.BringTopMost();
                }
            });
        }
        private async void HideAll()
        {
            if (_windowList.Count == 1) return;
            await Children.ForEachAsync((w, i) => w.Hide());
            _windowList[0].WindowState = FormWindowState.Minimized;
        }
        private void SetTopMost()
        {
            TopMost = !TopMost;
            LocalWindowItem.TopMost = TopMost;
            CreateContextMenu(true);
        }
        private void SetCaption()
        {
            if (_anime == null && _picture == null) return;
            if (_currentImage == null) return;
            Text = $"[{Index}] {Caption} [{_bmpSize.Width} x {_bmpSize.Height} / {_bmpResolution}dpi ] ({(_anime != null ? _anime.Scale : _picture.Scale):F2}%)";
        }

        /****************************************************/
        private Label _lbl_ColorPreview;

        private void AdjustLabel_FilterValue()
        {
            lblinfo_filterValue.Visible = true;
            var emSize = WindowState == FormWindowState.Maximized ? (float)(Width * 0.1) : _lblDefaultFontSize;
            lblinfo_filterValue.Font = new Font(Font.FontFamily, emSize, FontStyle.Bold);
            lblinfo_filterValue.ForeColor = Color.Magenta;
            lblinfo_filterValue.TextAlign = ContentAlignment.MiddleCenter;
            lblinfo_filterValue.Location = Rect.GetCenter_Screen(_pbx.Bounds, lblinfo_filterValue.Size);
            lblinfo_filterValue.Visible = false;
        }
        private void AdjustLabel_Infomation()
        {
            lblinfo_image.Width = _pbx.Width;
            lblinfo_image.Location = new Point(0, _pbx.Height - lblinfo_image.Height);
        }
        private void ShowLabel(string text)
        {
            lblinfo_filterValue.Visible = true;
            lblinfo_filterValue.Text = text;
            lblinfo_filterValue.Location = Rect.GetCenter_Screen(_pbx.Bounds, lblinfo_filterValue.Size);
        }
        /// <summary>
        /// _isShift, _isCtrl, _isRightButtonを初期化
        /// </summary>
        private void HideLabel()
        {
            if (IsParent) return;

            _isShift = false;
            _isCtrl = false;
            _isAlt = false;
            _isRightButton = false;
            _isLeftButton = false;

            if (_currentImage != null)
            {
                lblinfo_filterValue.Visible = false;
            }
        }
        private void Update_lblinfo(string text)
        {
            if (IsParent) return;

            if (lblinfo_image.Visible)
            {
                lblinfo_image.Text = text;
            }
        }
        private void Update_lblinfo(Point location)
        {
            if (IsParent) return;
            if (!lblinfo_image.Visible && !_lbl_ColorPreview.Visible) return;

            var obj = _anime ?? _picture;

            if (obj == null) return;

            var matrix = obj.Matrix.Clone();

            matrix.Invert();

            var pf = new PointF[] { location };

            // ピクチャボックス座標を画像座標に変換する
            matrix.TransformPoints(pf);

            var point = pf[0];

            var x = location.X;
            var y = location.Y;

            // 画像の座標
            Update_lblinfo($"{Index}) [{Focused}] [Scale: {obj.Scale:F2}%] / [X: {x}, Y: {y}] / [X: {point.X:F2}, Y: {point.Y:F2}]");
        }

        private void GetPixel(Point location)
        {
            var pt = new Point(location.X + 10, location.Y + 10);
            if (pt.X < 0 || pt.X >= _pbx.Width || pt.Y < 0 || pt.Y >= _pbx.Height) return;

            _lbl_ColorPreview.Location = pt;

            var bmp = (_anime ?? _picture).DrawnImage as Bitmap;

            if (bmp == null) return;

            var x = (pt.X * bmp.Width / _pbx.ClientSize.Width) - Padding.Left;
            var y = (pt.Y * bmp.Height / _pbx.ClientSize.Height) - Padding.Left;

            //new Point(x, y).test(false);

            if (x < 0 || y < 0) return;

            var color = bmp.GetPixel(x, y);

            _lbl_ColorPreview.BackColor = color;

            Update_lblinfo($"{color}");
        }

        public void IsException(bool condition, string message = null, [CallerMemberName] string member = null, [CallerLineNumber] int number = -1)
        {
            if (!condition) return;
            message = $"{message ?? "例外: "} [ {member} ({number}) ]";
            throw new Exception(message);
        }

        /****************************************************/

        public override void FileReceive(string[] args)
        {
            // コマンドライン
            _isCommandLineLaunch = true;
            base.FileReceive(args);
        }

        /****************************************************/

        private void TestRectangle(Rectangle rect)
        {
            rect.test();

            var p = new Form()
            {
                Text = "HitArea",
                FormBorderStyle = FormBorderStyle.None,
                ClientSize = new Size(rect.Width, rect.Height),
                BackColor = Color.Red,
                Opacity = 0.3,
            };
            p.Show();
            p.Draggable(true);
            p.Location = new Point(rect.X, rect.Y);
            p.PreviewKeyDown += (s, e) => p.Close();
            p.TopMost = true;
        }

        /****************************************************/
        private bool IsParent { get; set; }
        private int Index { get; set; }
        private string Caption { get; set; } = $"{Guid.NewGuid()}";
        private ImageType ImageType { get; set; }
        private WindowItem LocalWindowItem { get; set; } = new WindowItem();
        private List<MultiWindow> Children => _windowList.Skip(1).ToList();

        private float _windowRatio = 1.0f;
    }
}
