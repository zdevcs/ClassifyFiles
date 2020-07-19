﻿using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using System;
using FzLib.Basic;
using FzLib.Extension;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassifyFiles.Util;
using System.Diagnostics;
using ModernWpf.Controls;
using ClassifyFiles.UI.Component;
using static ClassifyFiles.Util.FileClassUtility;
using System.Collections;
using System.Collections.Concurrent;
using ListView = System.Windows.Controls.ListView;
using System.Collections.Specialized;
using System.Windows.Data;
using FI = System.IO.FileInfo;
using ClassifyFiles.UI.Util;
using ClassifyFiles.Enum;
using ClassifyFiles.UI.Event;

namespace ClassifyFiles.UI.Panel
{
    /// <summary>
    /// FIlesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class FilesViewer : UserControl, INotifyPropertyChanged
    {
        public FilesViewer()
        {
            DataContext = this;
            InitializeComponent();
            SetGroupEnable(Configs.GroupByDir);
            new DragDropFilesHelper(FindResource("lvwFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("grdFiles") as ListBox).Regist();
            new DragDropFilesHelper(FindResource("lvwDetailFiles") as ListBox).Regist();
            var btn = grdAppBar.Children.OfType<AppBarToggleButton>()
                .FirstOrDefault(p => int.Parse(p.Tag as string) == Configs.LastViewType);
            if (btn != null)
            {
                ViewTypeButton_Click(btn, new RoutedEventArgs());
            }

            FileIcon.Tasks.ProcessStatusChanged += TaskQueue_ProcessStatusChanged;
        }

        private void TaskQueue_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            progress.IsActive = e.IsRunning;
        }

        private ItemsControl filesContent;
        public ItemsControl FilesContent
        {
            get => filesContent;
            set
            {
                filesContent = value;
                this.Notify(nameof(FilesContent));
            }
        }
        protected ProgressDialog GetProgress()
        {
            return (Window.GetWindow(this) as MainWindow).Progress;
        }
        private Project project;
        public virtual Project Project
        {
            get => project;
            set
            {
                project = value;
                this.Notify(nameof(Project));
            }
        }
        private ObservableCollection<UIFile> files;
        public ObservableCollection<UIFile> Files
        {
            get => files;
            set
            {
                files = value;
                this.Notify(nameof(Files), nameof(FileTree));
            }
        }
        /// <summary>
        /// 供树状图使用的文件树
        /// </summary>
        public List<UIFile> FileTree => Files == null ? null : new List<UIFile>(
            FileUtility.GetFileTree<UIFile>(Project, Files, p => new UIFile(p), p => p.File.Dir, p => p.SubUIFiles)
            .SubUIFiles);

        private double iconSize = 64;
        public double IconSize
        {
            get => iconSize;
            set
            {
                iconSize = value;
                this.Notify(nameof(IconSize));
            }
        }
        public const int pagingItemsCount = 120;

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task SetFilesAsync(IEnumerable<File> files)
        {
            if (files == null || !files.Any())
            {
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                List<UIFile> filesWithIcon = new List<UIFile>();
                await Task.Run(() =>
               {
                   foreach (var file in files)
                   {
                       UIFile uiFile = new UIFile(file);
                       filesWithIcon.Add(uiFile);
                   }
               });
                Files = new ObservableCollection<UIFile>(filesWithIcon);
                await Task.Delay(100);//不延迟大概率会一直转圈
                //await RealtimeRefresh(Files.Take(100));
            }
            if(Configs.SortType!=0)
            {
                await SortAsync((SortType)Configs.SortType);
            }
        }
        public async Task SetFilesAsync(IEnumerable<UIFile> files)
        {
            if (files == null || !files.Any())
            {
                Files = new ObservableCollection<UIFile>();
            }
            else
            {
                Files = new ObservableCollection<UIFile>(files);
                await Task.Delay(100);//不延迟大概率会一直转圈
                //await RealtimeRefresh(Files.Take(100));
            }
        }
        public async Task AddFilesAsync(IEnumerable<File> files, bool tags = true)
        {
            List<UIFile> filesWithIcon = new List<UIFile>();
            await Task.Run(() =>
           {
               foreach (var file in files)
               {
                   UIFile uiFile = new UIFile(file);
                   //await uiFile.LoadTagsAsync(Project);
                   filesWithIcon.Add(uiFile);
               }
           });
            foreach (var file in filesWithIcon)
            {
                Files.Add(file);
                await file.LoadAsync();
            }

            this.Notify(nameof(Files));
        }

        private UIFile GetSelectedFile()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItem as UIFile,
                TreeView t => t.SelectedItem as UIFile,
                _ => null,
            };
        }
        private IReadOnlyList<UIFile> GetSelectedFiles()
        {
            return FilesContent switch
            {
                ListBox lvw => lvw.SelectedItems.Cast<UIFile>().ToList().AsReadOnly(),
                TreeView t => new List<UIFile>() { t.SelectedItem as UIFile }.AsReadOnly(),
                _ => new List<UIFile>().AsReadOnly(),
            };
        }

        private async void lvwFiles_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                File file = GetSelectedFile()?.File;
                if (file != null)
                {
                    if (file.IsFolder && CurrentFileView == FileView.Tree)//是目录
                    {
                        return;
                    }
                    string path = file.GetAbsolutePath();

                    if (!file.IsFolder && !System.IO.File.Exists(path))
                    {
                        await new ErrorDialog().ShowAsync("文件不存在", "打开失败");
                        e.Handled = true;
                        return;
                    }
                    else if (file.IsFolder && !System.IO.Directory.Exists(path))
                    {
                        await new ErrorDialog().ShowAsync("文件夹不存在", "打开失败");
                        e.Handled = true;
                        return;
                    }
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    p.Start();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                await new ErrorDialog().ShowAsync(ex, "打开失败");
            }
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && CurrentFileView != FileView.Detail)
            {
                UIFileSize.DefaultIconSize += e.Delta / 30;
                Configs.IconSize = UIFileSize.DefaultIconSize;
                //Files.ForEach(p => p.Size.UpdateIconSize());
                e.Handled = true;
            }
        }

        private void treeFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }

            static TreeViewItem VisualUpwardSearch(DependencyObject source)
            {
                while (source != null && !(source is TreeViewItem))
                    source = VisualTreeHelper.GetParent(source);

                return source as TreeViewItem;
            }
        }

        private void OpenDirMernuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedFile() != null)
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select, \"{GetSelectedFile().File.GetAbsolutePath(false)}\"",
                    UseShellExecute = true
                };
                p.Start();
            }
        }

        public async Task RefreshAsync()
        {
            FileIcon.ClearCaches();
            var files = Files;
            Files = null;
            await SetFilesAsync(files);
        }

        public event EventHandler ViewTypeChanged;
        private void ViewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            int type = int.Parse((sender as FrameworkElement).Tag as string);
            grdAppBar.Children.OfType<AppBarToggleButton>().ForEach(p => p.IsChecked = false);
            (sender as AppBarToggleButton).IsChecked = true;
            CurrentFileView = (FileView)type;
            RefreshFileView();
        }

        public void SetGroupEnable(bool enable)
        {
            foreach (var list in Resources.Values.OfType<ListBox>())
            {
                if (enable)
                {
                    list.SetBinding(ListBox.ItemsSourceProperty, new Binding() { Source = FindResource("listDetailItemsSource") as CollectionViewSource });
                }
                else
                {
                    list.SetBinding(ListBox.ItemsSourceProperty, nameof(Files));
                }
            }
        }

        private void RefreshFileView()
        {
            var selectedFile = GetSelectedFile();
            if (CurrentFileView == FileView.List)
            {
                FilesContent = FindResource("lvwFiles") as ListBox;

            }
            else if (CurrentFileView == FileView.Icon || CurrentFileView == FileView.Tile)
            {
                FilesContent = FindResource("grdFiles") as ListBox;
                FilesContent.ItemTemplate = FindResource(CurrentFileView == FileView.Icon ? "grdIconView" : "grdTileView") as DataTemplate;
            }
            else if (CurrentFileView == FileView.Tree)
            {
                FilesContent = FindResource("treeFiles") as TreeView;
            }
            else if (CurrentFileView == FileView.Detail)
            {
                FilesContent = FindResource("lvwDetailFiles") as ListView;
            }
            if (selectedFile != null && FilesContent is ListBox list)
            {
                list.SelectedItem = selectedFile;
                list.ScrollIntoView(selectedFile);
            }
            Configs.LastViewType = (int)CurrentFileView;
            ViewTypeChanged?.Invoke(this, new EventArgs());
        }

        public void SelectFileByDir(string dir)
        {
            UIFile file = null;
            switch (CurrentFileView)
            {
                case FileView.List:
                case FileView.Icon:
                case FileView.Tile:
                case FileView.Detail:
                    file = Files.FirstOrDefault(p => p.File.Dir == dir);
                    break;
                default:
                    break;
            }
            SelectFile(file);
        }
        public void SelectFile(UIFile file)
        {
            if (FilesContent is ListBox lbx)
            {
                lbx.SelectedItem = file;
                lbx.ScrollIntoView(file);
            }
            else if (filesContent is ModernWpf.Controls.ListView lvw)
            {
                lvw.SelectedItem = file;
                lvw.ScrollIntoView(file);
            }
        }
        public FileView CurrentFileView { get; private set; } = FileView.List;
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void Tags_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Class c = (e.Source as ContentPresenter).Content as Class;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ClickTag?.Invoke(this, new ClickTagEventArgs(c));
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                TagGroup tg = sender as TagGroup;

                await RemoveFilesFromClass(new File[] { tg.File.File }, c);
                tg.File.Classes.Remove(c);
            }
            e.Handled = true;
        }
        public event EventHandler<ClickTagEventArgs> ClickTag;

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

            ContextMenu menu = FindResource("menu") as ContextMenu;
            menu.Items.Clear();
            var files = GetSelectedFiles();
            if (files == null || files.Count == 0)
            {
                menu.IsOpen = false;
                return;
            }
            if (files.Count == 1)
            {
                MenuItem menuOpenFolder = new MenuItem() { Header = "打开目录" };
                menuOpenFolder.Click += OpenDirMernuItem_Click;
                menu.Items.Add(menuOpenFolder);

            }
            MenuItem menuCopy = new MenuItem() { Header = "复制" };
            menuCopy.Click += MenuCopy_Click; ;
            menu.Items.Add(menuCopy);
            if ((!files.Any(p => p.File.IsFolder) || CurrentFileView != FileView.Tree) && Project.Classes != null)
            {
                menu.Items.Add(new Separator());

                foreach (var tag in Project.Classes)
                {
                    bool? isChecked = null;
                    if (!files.Any(p => p.Classes == null))
                    {
                        if (files.Any(p => p.Classes.Any(q => q.ID == tag.ID)))
                        {
                            if (files.All(p => p.Classes.Any(q => q.ID == tag.ID)))
                            {
                                isChecked = true;
                            }
                            //这里else  isChecked = null;
                        }
                        else
                        {
                            isChecked = false;
                        }
                    }
                    CheckBox chk = new CheckBox()
                    {
                        Content = tag.Name,
                        IsChecked = isChecked
                    };
                    chk.Click += async (p1, p2) =>
                     {
                         GetProgress().Show(false);
                         if (chk.IsChecked == true)
                         {
                             await AddFilesToClassAsync(files.Select(p => p.File), tag);
                             foreach (var file in files)
                             {
                                 var newC = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                                 if (newC == null)
                                 {
                                     file.Classes.Add(tag);
                                 }
                             }
                         }
                         else
                         {
                             await RemoveFilesFromClass(files.Select(p => p.File), tag);
                             foreach (var file in files)
                             {
                                 var c = file.Classes.FirstOrDefault(p => p.ID == tag.ID);
                                 if (c != null)
                                 {
                                     file.Classes.Remove(c);
                                 }
                             }
                         }

                         GetProgress().Close();
                     };
                    menu.Items.Add(chk);
                }
            }

        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            var files = new StringCollection();
            if (CurrentFileView != FileView.Tree)
            {
                files.AddRange(GetSelectedFiles().Select(p => p.File.GetAbsolutePath()).ToArray());
            }
            else
            {
                files.Add(GetSelectedFile().File.GetAbsolutePath());

            }
            Clipboard.SetFileDropList(files);
        }

        private void SearchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string txt = sender.Text.ToLower();
                var suggestions = Files == null ? new List<UIFile>() :
                    Files.Where(p => (p.File.IsFolder ? p.File.Dir : p.File.Name).ToLower().Contains(txt)).ToList();

                sender.ItemsSource = suggestions.Count > 0 ?
                    suggestions : new string[] { "结果为空" } as IEnumerable;
            }
        }

        private void SearchTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            UIFile file = args.ChosenSuggestion as UIFile;
            SelectFile(file);
        }

        private void SearchTextBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
        }
        public async Task SortAsync(SortType type)
        {
            IEnumerable<UIFile> files = null;
            await Task.Run(() =>
            {
                switch (type)
                {
                    case SortType.Default:
                        files = Files
                            .OrderBy(p => p.File.Dir)
                            .ThenBy(p => p.File.Name);
                        break;
                    case SortType.NameUp:
                        files = Files
                            .OrderBy(p => p.File.Name)
                            .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.NameDown:
                        files = Files
                           .OrderByDescending(p => p.File.Name)
                           .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.LengthUp:
                        files = Files
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.Length)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.LengthDown:
                        files = Files
                          .OrderByDescending(p => GetFileInfoValue(p,nameof(FI.Length)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.LastWriteTimeUp:
                        files = Files
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.LastWriteTime)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.LastWriteTimeDown:
                        files = Files
                          .OrderByDescending(p => GetFileInfoValue(p, nameof(FI.LastWriteTime)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                    case SortType.CreationTimeUp:
                        files = Files
                          .OrderBy(p => GetFileInfoValue(p, nameof(FI.CreationTime)))
                          .ThenBy(p => p.File.Name)
                          .ThenBy(p => p.File.Dir);
                        break;
                    case SortType.CreationTimeDown:
                        files = Files
                          .OrderByDescending(p => GetFileInfoValue(p, nameof(FI.CreationTime)))
                          .ThenByDescending(p => p.File.Name)
                          .ThenByDescending(p => p.File.Dir);
                        break;
                }
                files = files.ToArray();
            });
            Files = new ObservableCollection<UIFile>(files);
            SetGroupEnable(false);
            //this.Notify(nameof(Files));

            long GetFileInfoValue(UIFile file, string name)
            {
                try
                {
                    return name switch
                    {
                        nameof(FI.Length) => file.FileInfo.Length,
                        nameof(FI.LastWriteTime) => file.FileInfo.LastWriteTime.Ticks,
                        nameof(FI.CreationTime) => file.FileInfo.CreationTime.Ticks,
                        _ => throw new NotImplementedException(),
                    };
                }
                catch
                {
                    return 0;
                }
            }
        }
    }

}
