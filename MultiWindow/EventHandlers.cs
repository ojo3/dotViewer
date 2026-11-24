using ojo3Utility.Collections;
using ojo3Utility.Common;
using ojo3Utility.Common.Data;
using ojo3Utility.Controls.ColorDesign;
using ojo3Utility.Controls.Forms;
using ojo3Utility.Controls.Mouse;
using ojo3Utility.Controls.Utility;
using ojo3Utility.Global;
using ojo3Utility.Pictures;
using ojo3Utility.Pictures.Processing;
using ojo3Utility.Tasks;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        private async void ContextMenu_Click(object sender, EventArgs e)
        {
            var tsm = (ToolStripMenuItem)sender;
            var cmi = (ContextMenuItems)tsm.Tag;

            switch (cmi)
            {
                case ContextMenuItems.None:
                    break;
                case ContextMenuItems.Duplication:
                    {
                        // オフセットをリセット
                        _offset = _defaultOffset;

                        if (_clipBoardImage != null)
                        {
                            GenerateWindow(_clipBoardImage);
                        }
                        else
                        {
                            GenerateWindow(LocalWindowItem.FilePath);
                        }
                        return;
                    }
                case ContextMenuItems.TopMost:
                    {
                        SetTopMost();
                        break;
                    }
                case ContextMenuItems.GrayScale:
                    {
                        SetImageFilters(ImageFilters.GrayScale);
                        break;
                    }
                case ContextMenuItems.Binary:
                    {
                        SetImageFilters(ImageFilters.Binary);
                        break;
                    }
                case ContextMenuItems.Sepia:
                    {
                        SetImageFilters(ImageFilters.Sepia);
                        break;
                    }
                case ContextMenuItems.Night:
                    {
                        SetImageFilters(ImageFilters.Night);
                        break;
                    }
                case ContextMenuItems.Invert:
                    {
                        LocalWindowItem.ImageFilterItem.Invert = !LocalWindowItem.ImageFilterItem.Invert;
                        break;
                    }
                case ContextMenuItems.Copy:
                    {
                        CreateContextMenu(true);
                        Clipboard.SetImage(MakeOutputImage());
                        return;
                    }
                case ContextMenuItems.Paste:
                    {
                        using (var clip = Ojo3Image.GetClipBoardImage())
                        {
                            GenerateWindow(clip);
                        }

                        break;
                    }
                case ContextMenuItems.ShowInTaskbar:
                    {
                        CreateContextMenu(true);
                        LocalWindowItem.ShowInTaskbar = !LocalWindowItem.ShowInTaskbar;
                        ShowInTaskbar = LocalWindowItem.ShowInTaskbar;
                        break;
                    }
                case ContextMenuItems.Print:
                    {
                        using (var bmp = MakeOutputImage(true))
                        {
                            Ojo3Image.Print(bmp);
                        }

                        break;
                    }
                case ContextMenuItems.Palette:
                    {
                        using (var bmp = new Bitmap(_currentImage))
                        {
                            bmp.Decolor(LocalWindowItem.ImageFilterItem);
                            var palette = await Ojo3Image.CreatePalette(bmp, 64, 400);
                            GenerateWindow(palette, ImageType.Palette);
                        }
                        break;
                    }
                case ContextMenuItems.Close:
                    {
                        WindowClose();
                        break;
                    }
                case ContextMenuItems.AllFront:
                    break;
                case ContextMenuItems.AllClose:
                    {
                        if (_windowList.Count > 1)
                        {
                            // すべて閉じる
                            if (Ojo3MessageBox.Show("すべてのサムネイルを閉じますか？",
                                MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                            {
                                WindowClose(true);
                                return;
                            }
                        }
                        break;
                    }
                case ContextMenuItems.Open:
                    {
                        FilesDialog();
                        return;
                    }
                case ContextMenuItems.Save:
                    {
                        ImageSave();
                        break;
                    }
                case ContextMenuItems.History:
                    {
                        GenerateWindow(tsm.Text);
                        break;
                    }
                case ContextMenuItems.Minimized:
                    {
                        WindowState = FormWindowState.Minimized;
                        break;
                    }
                case ContextMenuItems.Separator:
                    break;
                case ContextMenuItems.KeepRatio:
                    {
                        SetKeepRatio();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            CreateProcessedImage();

            switch (cmi)
            {
                case ContextMenuItems.GrayScale:
                case ContextMenuItems.Binary:
                case ContextMenuItems.Sepia:
                case ContextMenuItems.Night:
                case ContextMenuItems.Invert:
                    {
                        CreateContextMenu(true);
                        ReDraw(false, true);
                        break;
                    }
            }
        }

        private void SetKeepRatio()
        {
            LocalWindowItem.KeepRatio = !LocalWindowItem.KeepRatio;
            ResetWindowRetio();
            ResetMimimumSize();
            InitializeWindow(false);
            ReDraw();
            CreateContextMenu(true);
        }

        private void ResetMimimumSize()
        {
            MinimumSize = Size.Empty;
        }

        private void ImageSave()
        {
            if (_currentImage != null)
            {
                var filepath = Path.Combine(_imageSaveDirectory, $"{(LocalWindowItem.FilePath.Length == 0 ? $"{Caption}.png" : LocalWindowItem.FilePath)}");

                if (!Directory.Exists(_imageSaveDirectory))
                {
                    _ = Directory.CreateDirectory(_imageSaveDirectory);
                }

                if (File.Exists(filepath))
                {
                    if (Ojo3MessageBox.Show("すでに保存されたファイルがあります.\n上書きしますか？", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                using (var bmp = new Bitmap(_currentImage))
                {
                    bmp.Decolor(LocalWindowItem.ImageFilterItem);
                    bmp.Save(filepath, ImageFormat.Png);
                }

                // クリップボードから貼り付けた場合は、FilePathをセット
                if (_clipBoardImage != null)
                {
                    LocalWindowItem.FilePath = filepath;
                }

                ImageType = ImageType.Image;
            }
        }

        /****************************************************/

        // KeyEventが処理されたあとにContextMenuのショートカットが処理される
        //protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //e.KeyCode.test();
            base.OnKeyDown(e);

            _isRedraw = false;
            _isShift = e.Shift;
            _isCtrl = e.Control;
            _isAlt = e.Alt;

            if (IsParent)
            {
                KeyDownParent(e);
                return;
            }

            if (_isAlt && !_isCtrl && !_isShift)
            {
                GetPixel(Cursor.Position);
                _lbl_ColorPreview.Visible = true;
                return;
            }

            _lbl_ColorPreview.Visible = false;

            CaseCommonKey(e);

            CaseFunctionKey(e);

            if (_isRedraw)
            {
                if (_isShift)
                {
                    // 画像フィルタをすべてリセット
                    ImageFilterItemReset_All();
                }

                if (_currentImage == null)
                {
                    LoadImage();
                }

                InitializeWindow(false);
                CreateProcessedImage();
                ReDraw(true, true);
            }
        }

        private void KeyDownParent(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F6:
                    {
                        HideAll();
                        break;
                    }
                case Keys.S:
                    {
                        ApplyStyle();
                        break;
                    }
            }
        }

        private void CaseCommonKey(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Down:
                case Keys.Up:
                    {
                        if (_isCtrl)
                        {
                            if ((e.KeyCode == Keys.Left && Location.X == 0) || (e.KeyCode == Keys.Up && Location.Y == 0))
                            {
                                return;
                            }

                            var AddValueX = e.KeyCode == Keys.Right || e.KeyCode == Keys.Down ? 1 : -1;

                            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
                            {
                                LocalWindowItem.Location = Location = new Point(Location.X + AddValueX, Location.Y);
                            }

                            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                            {
                                LocalWindowItem.Location = Location = new Point(Location.X, Location.Y + AddValueX);
                            }

                            return;
                        }
                        break;
                    }
            }

            switch (e.KeyCode)
            {
                case Keys.B:
                    {
                        _isRedraw = true;
                        SetImageFilters(ImageFilters.Blue);
                        break;
                    }
                case Keys.G:
                    {
                        _isRedraw = true;
                        SetImageFilters(ImageFilters.Green);
                        break;
                    }
                case Keys.H:
                    {
                        if (_isShift)
                        {
                            (_anime ?? _picture)?.FlipY(_processedImage ?? _currentImage, Point.Empty);
                        }
                        else
                        {
                            (_anime ?? _picture)?.FlipX(_processedImage ?? _currentImage, Point.Empty);
                        }
                        break;
                    }
                case Keys.K:
                    {
                        SetKeepRatio();
                        Tiling();
                        break;
                    }
                case Keys.O:
                    {
                        if (!_isCtrl)
                        {
                            if (_isShift)
                            {
                                IO.Open(LocalWindowItem.FilePath, FileType.File, true);
                            }
                            else if (_isAlt)
                            {
                                IO.Open(_imageSaveDirectory, FileType.Directory, false);
                            }
                        }
                        break;
                    }
                case Keys.Q:
                    {
                        if (!_isShift && !_isCtrl && !IsParent)
                        {
                            lblinfo_image.Visible = !lblinfo_image.Visible;
                            _pbxRank.Visible = !_pbxRank.Visible;
                        }
                        break;
                    }
                case Keys.R:
                    {
                        _isRedraw = true;
                        SetImageFilters(ImageFilters.Red);
                        break;
                    }
                case Keys.S:
                    {
                        ApplyStyle();
                        return;
                    }
                case Keys.T:
                    {
                        SetTopMost();
                        return;
                    }
                case Keys.Add:
                    {
                        if (_isAnime)
                        {
                            if (_isCtrl)
                            {
                                _anime?.SetInterval(10);
                            }
                            else
                            {
                                _anime.Zoom(_currentImage, 1, Point.Empty, _zoomScale);
                            }
                        }
                        else
                        {
                            _picture.Zoom(_currentImage, 1, Point.Empty, _zoomScale);
                        }
                        break;
                    }
                case Keys.Subtract:
                    {
                        if (_isAnime)
                        {
                            if (_isCtrl)
                            {
                                _anime?.SetInterval(-10);
                            }
                            else
                            {
                                _anime.Zoom(_currentImage, -1, Point.Empty, _zoomScale);
                            }
                        }
                        else
                        {
                            _picture.Zoom(_currentImage, -1, Point.Empty, _zoomScale);
                        }
                        break;
                    }
                case Keys.NumPad5:
                    {
                        break;
                    }
                case Keys.Space:
                    {
                        if (_isAnime)
                        {
                            _anime?.Pause();
                        }
                        else
                        {
                        }
                        break;
                    }
                case Keys.Tab:
                    {
                        var idx = _windowList.GetIndex(_currentFocusIndex, 1);

                        if (idx != -1)
                        {
                            idx = idx == 0 ? ++idx : idx;
                            _currentFocusIndex = idx;
                            _windowList[_currentFocusIndex].Activate();
                        }

                        break;
                    }
                case Keys.Enter:
                    {
                        if (WindowState != FormWindowState.Maximized)
                        {
                            WindowState = FormWindowState.Maximized;
                        }
                        break;
                    }
                case Keys.Escape:
                    {
                        WindowState = FormWindowState.Normal;
                        break;
                    }
                case Keys.Right:
                case Keys.Left:
                    {
                        // Shift + 方向キーでファイル移動
                        if (_isShift)
                        {
                            if (LocalWindowItem.FilePath.Length != 0)
                            {
                                if (_files.Count == 0)
                                {
                                    // 同じ階層のファイルを取得
                                    var tmp = IO.GetImageFiles(new string[1] { _pathInfo.DirectoryFullName }).OrderByNatural(f => f);

                                    if (tmp.Count() > 0)
                                    {
                                        _files.AddRange(tmp);
                                    }
                                }

                                if (_files.Count > 0)
                                {
                                    var direction = e.KeyCode == Keys.Right ? 1 : -1;

                                    _currentFileIndex = _files.GetIndex(_currentFileIndex, direction, true, LocalWindowItem.FilePath);

                                    LocalWindowItem.FilePath = _files[_currentFileIndex];

                                    ResetWindowRetio();

                                    GeneratePathinfo(LocalWindowItem.FilePath);

                                    LoadImage();

                                    CreateAccessHistory(new string[] { LocalWindowItem.FilePath });

                                    _isRedraw = true;
                                }
                            }
                        }
                        else
                        {
                            if (_isAnime)
                            {
                                if (e.KeyCode == Keys.Right)
                                {
                                    _anime?.Next();
                                }
                                else
                                {
                                    _anime?.Back();
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private static bool _proccessing;
        private async void ApplyStyle()
        {
            if (_isShift || _isCtrl || _isAlt)
                return;

            ApplyFontChildren = false;
            Styling = Styling == Style.Dark ? Style.Bright : Style.Dark;

            _proccessing = true;
            await _windowList.ForEachAsync(async (w, i) =>
            {
                w.Styling = Styling;
                w.ApplyFontChildren = ApplyFontChildren;

                if (w.IsParent)
                {
                    w._pbx.Image?.Dispose();
                    w._pbx.Image = null;
                    if (w.WindowState != FormWindowState.Minimized)
                    {
                        w._pbx.Image = GetInitializeImage(_pbx.Size);
                        w._pbx.Refresh();
                    }
                }
                else
                {
                    w.DrawBorder();
                    w.SetBackgroundImage(false);
                }

                await Task.Delay(100);
            });
            _proccessing = false;
        }

        private async void CaseFunctionKey(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    {
                        IO.Open("key.html", FileType.File);
                        break;
                    }
                case Keys.F2:
                    {
                        _isTiling = !_isTiling;
                        Tiling();
                        break;
                    }
                case Keys.F3:
                    {
                        _baseSize = Size;

                        _enableFocus = false;

                        // すべての子Windowのsizeを揃える
                        await _windowList.Skip(1).ForEachAsync(async (mw, i) =>
                        {
                            mw.ResetSize();
                            await Task.Delay(100);
                        });

                        _enableFocus = true;

                        // 起点となるWindowにフォーカス
                        Activate();

                        if (_isTiling)
                            Tiling();
                        break;
                    }
                case Keys.F4:
                    {
                        break;
                    }
                case Keys.F5:
                    {
                        if (WindowState != FormWindowState.Minimized)
                        {
                            _isRedraw = true;
                        }
                        break;
                    }
                case Keys.F6:
                    {
                        HideAll();
                        break;
                    }
                case Keys.F7:
                    {
                        break;
                    }
                case Keys.F8:
                    {
                        break;
                    }
            }
        }

        private void ResetSize()
        {
            // 基準となるWindow以外は、KeepRatioを無効にする
            //if (mw.Index != Index)
            if (!Focused)
            {
                LocalWindowItem.KeepRatio = false;
            }

            // サイズ比率をリセットして
            // AdjustSizeForAspectRatioでの
            // 比率を保持してサイズ決定を呼ばないようにする
            ResetWindowRetio();
            ResetMimimumSize();
            InitializeWindow();
            ReDraw();
        }

        private async void Shown_Parent()
        {
            //Size.test();
            //MinimumSize.test();
            if (!Theme.TestBrightness(BackColor))
            {
                (_pbx.Image as Bitmap).Invert();
            }

            var items = (await Xml.ReadListFromXml<WindowItem>(Log.Read(_iniFile)).WhereAsync(w => File.Exists(w.FilePath))).ToList();
            //var items = Xml.ReadListFromXml<WindowItem>(Log.Read(_iniFile)).Where(w => File.Exists(w.FilePath)).ToList();
            var count = items.Count;

            if (_isCommandLineLaunch || count > 0)
            //if (count > 0)
            {
                // 先にフォーカスを取る！！
                // そのあとのFormでGotFocusが発火する.
                //Activate();
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                //Activate();
            }

            if (count > 0)
            {
                _isfade = count < 10;

                await ForeachTask(items);
            }
            else
            {
                // サムネイルが0の場合は、
                // ロード済みを通知
                _isLoadComplete = true;
            }
        }

        private async void Shown_Child()
        {
            ShowInTaskbar = LocalWindowItem.ShowInTaskbar;

            SetBackgroundImage();

            // すべての読み込みが完了したら、アクティブにする
            // ⇒ 最後のウィンドウがアクティブになる
            if (_isLoadComplete)
            {
                // 読み込みが完了したらコマンド起動通知を下げる
                _isCommandLineLaunch = false;
                //Activate();
                Focus();
            }

            if (LocalWindowItem.ImageFilterItem.IsChanged)
            {
                // フィルタを適用
                using (var task = Task.Run(() =>
                {
                    CreateProcessedImage();
                    this.TimerAction(() => ReDraw(!_isAnime, true), 200);
                }))
                {
                    await task;
                }
            }
        }

        /****************************************************/

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (IsParent) return;
            HideLabel();
            _lbl_ColorPreview.Visible = false;
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            _isResize = true;
            // 現在のサイズをキャッシュ
            _baseSize = Size;
            _anime?.Stop();
            DrawBorder(false);
            base.OnResizeBegin(e);
            //$"OnResizeBegin: {_isResize}".test();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!IsParent) return;
            if (_currentImage != null) return;
            if (_pbx.Image != null) return;
            _currentImage = GetInitializeImage(_pbx.Size);
            _pbx.Image = new Bitmap(_currentImage);
        }

        private Bitmap GetInitializeImage(Size size)
        {
            //$"{size}, {BackColor}".test();
            if (size.Width == 0 || size.Height == 0) return null;
            using (var bmp = Ojo3Image.GetInitializeImage("Drag&Drop  Here!!", size, !Theme.TestBrightness(BackColor)))
                return new Bitmap(bmp);
        }

        // WindowStateが変更されたら呼ばれる
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (_currentImage == null) return;
            if (!_isLoadComplete) return;

            if (IsParent)
            {
                if (WindowState == FormWindowState.Normal)
                {
                    if (_isLoadComplete)
                    {
                        // 親ウィンドウにフォーカスしたら、子ウィンドウをアクティブにする！
                        ShowAll();
                    }
                }
            }
            else
            {
                // 最小化の場合は、タスクバーを一時的に表示
                switch (WindowState)
                {
                    case FormWindowState.Minimized:
                        {
                            LocalWindowItem.ShowInTaskbar = ShowInTaskbar;
                            ShowInTaskbar = true;
                            break;
                        }
                    case FormWindowState.Normal:
                        {
                            ShowInTaskbar = LocalWindowItem.ShowInTaskbar;
                            break;
                        }
                }

                //if (!_isAnime && WindowState != FormWindowState.Minimized && !_isResize)
                if (WindowState != FormWindowState.Minimized)
                {
                    if (_isAnime)
                    {

                    }
                    else
                    {
                        // WindowStateの変更時のみに実行
                        if (!_isResize)
                        {
                            ReDraw();
                            DrawBorder(WindowState == FormWindowState.Normal);
                        }
                    }
                }
            }
        }

        // キャプションを掴んでFormを動かした場合も呼ばれる
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            _isResize = false;

            if (IsParent)
            {
                LocalWindowItem.Location = Location;
                //LocalSettings_DotViewer.Data.Position = Location;
            }
            // 子ウィンドウが対象
            else
            {
                if (WindowState == FormWindowState.Normal)
                {
                    // サイズ変更が行われた場合のみ
                    if (_baseSize != Size)
                    {
                        ResetWindowRetio();
                        InitializeWindow(false);
                        ReDraw();
                        Tiling();
                    }

                    DrawBorder(WindowState == FormWindowState.Normal);
                }
            }
        }

        /****************************************************/

        private IntPtr _oldHandle;
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UDraggable.SetHandle(Handle, _oldHandle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            _oldHandle = Handle;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadImage();

            // ウィンドウサイズを決定
            InitializeWindow();//, _isAdjustSizeForAspectRatio);

            // 画像描画
            ReDraw(!_isAnime);

            InitializeEvents();

            if (IsParent)
            {

            }
            else
            {
                AdjustLabel_Infomation();
                Update_lblinfo(MousePosition);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // すべてのWindowItemを読み込み完了を通知
            _isLoadComplete = _windowList.Count > 1 && _windowIndex + 1 == _windowList.Count;
            //$"{_windowIndex}) {_isLoadComplete} ({_windowList.Count})".test();
            if (Location.X < 0 || Location.Y < 0 || _windowList.Count == 1)
            {
                Location = Rect.GetCenter_Screen(this);
            }

            // 親ウィンドウの場合は、ファイルリストを読み込み後、最小化
            if (IsParent)
            {
                Shown_Parent();
            }
            else
            {
                Shown_Child();
            }

            //_windowList.Count.test(true);

            CreateContextMenu(true);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isfade = _windowList.Count < _sendMax;
            FadeDuration = _isfade ? 200 : 0;

            ImageDispose();

            if (IsParent)
            {
                // サムネイルリストを保存
                Log.Write(_iniFile, Xml.WriteList(Children.Select(w => w.LocalWindowItem).ToList()));

                // アクセス履歴を保存
                if (_accessFileHistory.Count > 0)
                {
                    Log.Write(_logAccessFileHistory, string.Join(Const.CRLF, _accessFileHistory.Select(h => h.FilePath).Distinct().Take(_accessFileHistoryMax)));
                }

                _isTiling = false;
                _windowList.Skip(1).ForEach(w => w.Close());
                _accessFileHistory.ForEach(h => h.Dispose());
                base.OnFormClosing(e);
                return;
            }

            Remove(this);

            Reset();

            Tiling();

            base.OnFormClosing(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            DrawBorder(WindowState == FormWindowState.Normal);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DrawBorder(false);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            _backgroundTileImage = GetTileImage();
        }

        /*****************************************************************************/

        private bool _drawBorder = false;
        private void DrawBorder(bool selected = false, [CallerMemberName] string caller = null)
        {
            if (IsParent || !_isLoadComplete || _proccessing)
                return;

            //$"{_windowIndex}) {_isLoadComplete} ({_windowList.Count})".test();

            if (_drawBorder)
            {
                //$"Index: {Index} / Focused: {Focused} / selected: {selected} / _isLoadComplete: {_isLoadComplete}, caller: {caller}".test();

                IsException(ColorTheme == null, $"{nameof(ColorTheme)} がNULLです. => {caller}");

                using (var g = CreateGraphics())
                using (var p = new Pen(selected ? ColorTheme.SelectionBackColor : Color.Transparent, _pensize))
                {
                    var rect = ClientRectangle;
                    rect.Inflate(-_pensize, -_pensize);
                    _pensize = (_pensize / 2) > 1 ? _pensize / 2 : 1;
                    g.Clear(BackColor);
                    g.DrawRectangle(p, rect);
                }
            }
            else
            {
                SetBackgroundImage(selected);
            }
        }

        private Bitmap _backgroundTileImage;
        private void SetBackgroundImage(bool selected = false)
        {
            IsException(_backgroundTileImage == null);

            BackgroundImage?.Dispose();
            BackgroundImage = null;

            var tile = new Bitmap(_backgroundTileImage);

            if (!selected)
            {
                BackgroundImage = tile;
                return;
            }

            var bg = new Bitmap(tile.Width, tile.Height);
            var color = Color.FromArgb(127, ColorTheme.SelectionBackColor);

            using (var g = Graphics.FromImage(bg))
            using (var b = new SolidBrush(color))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(tile, ClientRectangle);
                g.FillRectangle(b, ClientRectangle);
            }

            BackgroundImage = bg;
        }

        private Bitmap GetTileImage()
        {
            var tile = Ojo3Image.GetTileImage();

            // 背景色が暗い場合は、画像を反転
            if (!Theme.TestBrightness(BackColor))
            {
                tile.Invert();
            }

            return tile;
        }

        private void ImageDispose()
        {
            // PictureBox.Imageの参照を解除しないと
            // _currentImageをDisposeできない
            _pbx.ImageDispose();
            BackgroundImage?.Dispose();
            BackgroundImage = null;

            _imageItem?.Dispose();
            _imageItem = null;
            _anime?.Dispose();
            _anime = null;
            _picture?.Dispose();
            _picture = null;

            _currentImage?.Dispose();
            _currentImage = null;
            _processedImage?.Dispose();
            _processedImage = null;
            _clipBoardImage?.Dispose();
            _clipBoardImage = null;
        }

        private void SetEnable_MenuItems(ContextMenuItems menuItem, bool @checked, bool enabled)
        {
            var find = ContextMenuStrip?.Items.Cast<ToolStripItem>()
                .Where(m => (m.Tag is ContextMenuItems items) && items == menuItem).FirstOrDefault().ToObject<ToolStripMenuItem>();

            if (find != null)
            {
                find.Checked = @checked;
                find.Enabled = enabled;
            }
        }

        /****************************************************/

        private void Pbx_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // ローカルリセット
                ImageFilterItemReset();

                InitializeWindow(false);
                CreateProcessedImage();
                ReDraw(true, true);
                return;
            }

            _isRightButton = e.Button == MouseButtons.Right;
            _isLeftButton = e.Button == MouseButtons.Left;
            _mouseDownPoint = e.Location;

            if (_isRightButton)
            {
                CreateContextMenu();
                SetEnable_MenuItems(ContextMenuItems.Paste, false, Clipboard.ContainsImage());
            }
        }
        private void Pbx_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isAnime && !_isRightButton)
            {
                if (_isAlt)
                {
                    GetPixel(e.Location);
                    return;
                }
                else
                {
                    Update_lblinfo(e.Location);
                    return;
                }
            }

            if (_isRightButton)
            {
                if (_mouseDownPoint.X != e.X || _mouseDownPoint.Y != e.Y)
                {
                    ContextMenuStripNull();

                    _pbx.Cursor = Cursors.NoMove2D;

                    // 静止画もしくは、アニメが停止中の場合
                    if (!_isAnime || (_isAnime && !_anime.IsPlay))
                    {
                        Update_lblinfo(e.Location);
                    }

                    if (_isAnime)
                    {
                        _anime?.Move(_currentImage, _pbx.Width, _pbx.Height, e.Location, _mouseDownPoint);
                    }
                    else
                    {
                        _picture?.Move(_processedImage ?? _currentImage, _pbx.Width, _pbx.Height, e.Location, _mouseDownPoint);
                    }

                    _mouseDownPoint = e.Location;
                }
            }
        }
        private void Pbx_MouseUp(object sender, MouseEventArgs e)
        {
            // オフセットをリセット
            //_offset = _defaultOffset;
            _pbx.Cursor = Cursors.Default;
            LocalWindowItem.Location = Location;
            HideLabel();
            CreateContextMenu(true);
        }
        private void Pbx_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (IsParent)
                {

                }
                else
                {
                    WindowState = (WindowState == FormWindowState.Maximized) ? FormWindowState.Normal : FormWindowState.Maximized;

                    if (_isAnime)
                    {
                        if (WindowState != FormWindowState.Minimized)
                        {
                            ReDraw();
                            AdjustLabel_FilterValue();
                        }
                    }
                }
            }
        }
        private void Pbx_MouseWheel(object sender, MouseEventArgs e) => MouseWheel_Core(e);

        private float _zoomScale = 1.1f;
        private int _wheelCount;
        private CancellationTokenSource _cts;
        private async void MouseWheel_Core(MouseEventArgs e)
        {
            if (IsParent || (_picture == null && _anime == null)) return;

            // 拡大縮小
            if (_isRightButton)
            {
                ContextMenuStripNull();

                if (_isAnime)
                {
                    if (!_anime.IsPlay) Update_lblinfo(e.Location);
                    _anime.Zoom(_currentImage, e.Delta / 120, e.Location, _zoomScale);
                }
                else
                {
                    _picture.Zoom(_processedImage ?? _currentImage, e.Delta / 120, e.Location, _zoomScale);
                    Update_lblinfo(e.Location);
                }
                return;
            }

            _wheelCount++;

            if (_wheelCount > 2)
            {
                _wheelCount = 0;
                return;
            }

            var isWheelDown = UControl.CheckDelta(e) == WheelDirection.Down;
            var redraw = false;
            var labelString = string.Empty;

            //if (_isShift && _isCtrl)
            //{

            //}

            // Form透明度の変更
            if (_isCtrl && _isAlt)
            {
                if (isWheelDown)
                {
                    if (Opacity - 0.1 > 0.09)
                    {
                        Opacity -= 0.1;
                    }
                }
                else
                {
                    if (Opacity + 0.1 <= 1.0)
                    {
                        Opacity += 0.1;
                    }
                }

                labelString = $"{Opacity * 100:00}%";
            }

            // BinaryThresholdの変更
            else if (_isShift)
            {
                if (LocalWindowItem.ImageFilterItem.Filter == ImageFilters.Binary)
                {
                    if (isWheelDown)
                    {
                        if (LocalWindowItem.ImageFilterItem.BinaryThreshold - 1 > 0)
                        {
                            redraw = true;
                            LocalWindowItem.ImageFilterItem.BinaryThreshold -= 1;
                        }
                    }
                    else
                    {
                        if (LocalWindowItem.ImageFilterItem.BinaryThreshold + 1 <= 255)
                        {
                            redraw = true;
                            LocalWindowItem.ImageFilterItem.BinaryThreshold += 1;
                        }
                    }

                    labelString = $"{LocalWindowItem.ImageFilterItem.BinaryThreshold}";
                }
            }

            // Form透明度変更
            // Brightnessの変更
            else if (_isCtrl)
            {
                redraw = true;
                LocalWindowItem.ImageFilterItem.Brightness += isWheelDown ? -0.01 : 0.01;
                labelString = $"{LocalWindowItem.ImageFilterItem.Brightness * 10:F2}";
            }

            else if (_isAlt)
            {
                redraw = true;
                LocalWindowItem.ImageFilterItem.Saturation += isWheelDown ? -0.01 : 0.01;
                labelString = $"{LocalWindowItem.ImageFilterItem.Saturation * 10:F2}";
            }

            ShowLabel(labelString);

            if (redraw)
            {
                if (_cts != null)
                {
                    "処理中".test();
                    _cts?.Cancel();
                    return;
                }

                _cts = new CancellationTokenSource();

                await this.TaskRun(() =>
                {
                    CreateProcessedImage();
                    ReDraw(false, true);
                }, _cts);

                _cts?.Dispose();
                _cts = null;
            }
        }
    }
}
