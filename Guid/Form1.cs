using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Guid
{
    enum Numbers
    {
        Four = 4,
        Twelve = 12,
        Thirty = 13
    }

    public partial class Form1 : Form
    {
        string[] strDrives;
        string pathForTree;
        string pathForList;

        TreeNode root;
        ImageList images;

        ImageList smallIcon;
        ImageList largeIcon;

        ColumnHeader name;
        ColumnHeader sizeC;
        ColumnHeader free;
        ColumnHeader date;

        string pastName;
        bool forSort = false;

        int logicalDrivesGroup;
        int cdDrive;
        int folderGroup;
        int fileGroup;

        bool isTrue = false;

        string size;

        public Form1()
        {
            InitializeComponent();

            pastName = null;

            pathForTree = null;
            pathForList = null;

            size = null;

            InitImgs();
            CreateRoot();
            LogicalDrives();

            listView1.MultiSelect = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartPositionForListView();
        }

        private void StartPositionForListView()
        {
            string[] views = Enum.GetNames(typeof(View));
            for (int i = 0; i < views.Length; i++)
            {
                toolStripMenuItem2.DropDown.Items.Add(views[i]);
            }

            string totalSpace = null;
            string freeSpace = null;

            smallIcon = new ImageList();
            largeIcon = new ImageList();
            smallIcon.Images.Add(Image.FromFile("img/ld.png"));
            largeIcon.Images.Add(Image.FromFile("img/ld.png"));
            smallIcon.ImageSize = new Size(16, 16);
            largeIcon.ImageSize = new Size(32, 32);
            listView1.SmallImageList = smallIcon;
            listView1.LargeImageList = largeIcon;

            name = listView1.Columns.Add("Name");
            name.Width = listView1.Width / (int)Numbers.Four;
            name.TextAlign = HorizontalAlignment.Center;

            sizeC = listView1.Columns.Add("Total size");
            sizeC.Width = listView1.Width / (int)Numbers.Four;
            sizeC.TextAlign = HorizontalAlignment.Center;

            free = listView1.Columns.Add("Free size");
            free.Width = listView1.Width / (int)Numbers.Four;
            free.TextAlign = HorizontalAlignment.Center;

            logicalDrivesGroup = listView1.Groups.Add(new ListViewGroup("Logical drives: "));
            cdDrive = listView1.Groups.Add(new ListViewGroup("CD-Drives or something else: "));

            folderGroup = listView1.Groups.Add(new ListViewGroup("Folders:"));
            fileGroup = listView1.Groups.Add(new ListViewGroup("Files:"));

            for (int i = 0; i < strDrives.Length; i++)
            {
                ListViewItem item = listView1.Items.Add(strDrives[i], 0);

                try
                {
                    DriveInfo driveInfo = new DriveInfo(strDrives[i]);
                    totalSpace = driveInfo.TotalSize.ToString();
                    freeSpace = driveInfo.TotalFreeSpace.ToString();
                    if (totalSpace.Length == (int)Numbers.Twelve)
                    {
                        item.SubItems.Add((driveInfo.TotalSize.ToFileSize()).ToString());
                        item.SubItems.Add((driveInfo.TotalFreeSpace.ToFileSize()).ToString());
                    }
                    else if (totalSpace.Length == (int)Numbers.Thirty)
                    {
                        item.SubItems.Add((driveInfo.TotalSize.ToFileSize()).ToString());
                        item.SubItems.Add((driveInfo.TotalFreeSpace.ToFileSize()).ToString());
                    }
                }
                catch
                {
                    listView1.Groups[cdDrive].Items.Add(item);
                    return;
                }

                listView1.Groups[logicalDrivesGroup].Items.Add(item);
            }
        }

        /// <summary>
        /// Find logical drives
        /// </summary>
        private void LogicalDrives()
        {
            foreach (string drive in strDrives)
            {
                TreeNode node = new TreeNode(drive);
                node.ImageIndex = 1;
                node.Tag = drive;
                node.SelectedImageIndex = 1;
                root.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Create root node
        /// </summary>
        private void CreateRoot()
        {
            root = new TreeNode("MyComputer");
            strDrives = Directory.GetLogicalDrives();
            treeView1.Nodes.Add(root);
        }

        /// <summary>
        /// Download images
        /// </summary>
        private void InitImgs()
        {
            images = new ImageList();
            images.Images.Add(Image.FromFile("img/comp.png"));
            images.Images.Add(Image.FromFile("img/ld.png"));
            images.Images.Add(Image.FromFile("img/file.png"));
            images.Images.Add(Image.FromFile("img/of.png"));
            images.Images.Add(Image.FromFile("img/cf.png"));

            treeView1.ImageList = images;
        }
        /// <summary>
        /// Image for close folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewAfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.SelectedImageIndex == 4)
            {
                e.Node.SelectedImageIndex = 3;
                e.Node.ImageIndex = 3;
            }
        }
        /// <summary>
        /// Image for open folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.SelectedImageIndex == 3)
            {
                e.Node.SelectedImageIndex = 4;
                e.Node.ImageIndex = 4;
            }
        }

        /// <summary>
        /// Event for Find folders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode.Text != root.Text)
            {
                if (treeView1.SelectedNode.Nodes.Count == 0)
                {
                    pathForTree = (string)treeView1.SelectedNode.Parent.Tag;
                    if (pathForTree == null || pathForTree.EndsWith("\\"))
                    {
                        SelectNode(pathForTree + treeView1.SelectedNode.Text);
                    }
                    else
                    {
                        pathForTree = pathForTree.Insert(pathForTree.Length, "\\");
                        SelectNode(pathForTree + treeView1.SelectedNode.Text);
                    }
                }

                listView1.Items.Clear();

                ImagesForList();
                if (!isTrue)
                {
                    listView1.Columns.Remove(free);
                    sizeC.Text = "Size";
                    sizeC.Width = listView1.Width / (int)Numbers.Four;
                    sizeC.TextAlign = HorizontalAlignment.Center;

                    date = listView1.Columns.Add("Creation date");
                    date.Width = listView1.Width / (int)Numbers.Four;
                    date.TextAlign = HorizontalAlignment.Center;
                    isTrue = true;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo((string)treeView1.SelectedNode.Tag);
                DirectoryInfo[] dirs;
                FileInfo[] files;

                try
                {
                    dirs = directoryInfo.GetDirectories();
                    files = directoryInfo.GetFiles();
                }
                catch
                {
                    return;
                }

                foreach (DirectoryInfo dir in dirs)
                {
                    ListViewItem newDir = listView1.Items.Add(dir.Name, 0);
                    newDir.SubItems.Add("");
                    newDir.SubItems.Add(dir.CreationTime.ToString());
                    listView1.Groups[folderGroup].Items.Add(newDir);
                }
                foreach (FileInfo file in files)
                {
                    ListViewItem newFile = listView1.Items.Add(file.Name, 1);
                    size = file.Length.ToFileSize();
                    newFile.SubItems.Add(size);
                    newFile.SubItems.Add(file.CreationTime.ToString());
                    listView1.Groups[fileGroup].Items.Add(newFile);
                }
            }
            if (treeView1.SelectedNode.Text == root.Text)
            {
                pathForList = null;
                listView1.Items.Clear();

                string totalSpace = null;
                string freeSpace = null;

                smallIcon = new ImageList();
                largeIcon = new ImageList();
                smallIcon.Images.Add(Image.FromFile("img/ld.png"));
                largeIcon.Images.Add(Image.FromFile("img/ld.png"));
                smallIcon.ImageSize = new Size(16, 16);
                largeIcon.ImageSize = new Size(32, 32);
                listView1.SmallImageList = smallIcon;
                listView1.LargeImageList = largeIcon;

                if (isTrue)
                {
                    listView1.Columns.Remove(date);

                    sizeC.Text = "Total size";
                    free = listView1.Columns.Add("Free size");
                    isTrue = false;
                }

                logicalDrivesGroup = listView1.Groups.Add(new ListViewGroup("Logical drives: "));
                cdDrive = listView1.Groups.Add(new ListViewGroup("CD-Drives or something else: "));

                for (int i = 0; i < strDrives.Length; i++)
                {
                    ListViewItem item = listView1.Items.Add(strDrives[i], 0);

                    try
                    {
                        DriveInfo driveInfo = new DriveInfo(strDrives[i]);
                        totalSpace = driveInfo.TotalSize.ToString();
                        freeSpace = driveInfo.TotalFreeSpace.ToString();
                        if (totalSpace.Length == (int)Numbers.Twelve)
                        {
                            item.SubItems.Add((driveInfo.TotalSize.ToFileSize()).ToString());
                            item.SubItems.Add((driveInfo.TotalFreeSpace.ToFileSize()).ToString());
                        }
                        else if (totalSpace.Length == (int)Numbers.Thirty)
                        {
                            item.SubItems.Add((driveInfo.TotalSize.ToFileSize()).ToString());
                            item.SubItems.Add((driveInfo.TotalFreeSpace.ToFileSize()).ToString());
                        }
                    }
                    catch
                    {
                        listView1.Groups[cdDrive].Items.Add(item);
                        return;
                    }

                    listView1.Groups[logicalDrivesGroup].Items.Add(item);
                }
            }
        }
        /// <summary>
        /// Find folders
        /// </summary>
        /// <param name="fullPath"></param>
        private void SelectNode(string fullPath)
        {
            treeView1.SelectedNode.Tag = fullPath;
            DirectoryInfo currDir = new DirectoryInfo(fullPath);
            DirectoryInfo[] dirsIn;

            try
            {
                dirsIn = currDir.GetDirectories();
            }
            catch
            {
                return;
            }

            foreach (DirectoryInfo dirs in dirsIn)
            {
                TreeNode newNode = new TreeNode(dirs.Name);
                newNode.Tag = dirs.FullName;
                newNode.ImageIndex = 4;
                newNode.SelectedImageIndex = 4;
                treeView1.SelectedNode.Nodes.Add(newNode);
            }
        }

        /// <summary>
        /// F2 `n` Delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (treeView1.SelectedNode.Parent != root)
                {
                    if (treeView1.SelectedNode.Text != root.Text)
                    {
                        pathForTree = treeView1.SelectedNode.Tag.ToString();
                        try
                        {
                            Directory.Delete(pathForTree, true);
                            treeView1.Refresh();
                            treeView1.SelectedNode.Remove();
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show($"Application which called the error: {exc.Source}" + @"
Error msg: " + $"{exc.Message}" + @"
Returns the method which has created the error: " + $"{exc.TargetSite}");
                        }
                    }
                }
            }
            if (e.KeyCode == Keys.F2)
            {
                if (treeView1.SelectedNode.Text != root.Text)
                {
                    if (treeView1.SelectedNode.ImageIndex != 1)
                    {
                        pastName = treeView1.SelectedNode.FullPath;
                        pastName = pastName.Replace(root.FullPath + "\\", "");
                        treeView1.SelectedNode.BeginEdit();
                    }
                }
                else
                {
                    MessageBox.Show("You have not got the permissions to rename this object. Please try to rename another =)");
                }
            }
        }
        private void TreeViewAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (pastName != null && !pastName.EndsWith(treeView1.SelectedNode.Text))
            {
                Directory.Move(pastName, treeView1.SelectedNode.FullPath.Replace(root.FullPath + "\\", ""));
            }
        }

        /// <summary>
        /// Choice of the type for display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDownStyleSelectClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            View view = (View)Enum.Parse(typeof(View), e.ClickedItem.Text);
            listView1.View = view;
        }

        /// <summary>
        /// Select and refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Selected)
                {
                    if (listView1.Items[i].Group == listView1.Groups[logicalDrivesGroup] || listView1.Items[i].Group == listView1.Groups[folderGroup])
                    {
                        ImagesForList();
                        if (pathForList != null && !pathForList.EndsWith(@"\"))
                        {
                            pathForList = pathForList.Insert(pathForList.Length, @"\");
                        }
                        pathForList += listView1.Items[i].Text;

                        currentPathLine.Text = pathForList;

                        listView1.Items.Clear();
                        SearchFoldersNFilesForListView(pathForList);
                        break;
                    }
                    if (listView1.Items[i].Group == listView1.Groups[fileGroup])
                    {
                        ImagesForList();
                        string forOpen = pathForList;
                        if (!forOpen.EndsWith(@"\"))
                        {
                            forOpen = forOpen.Insert(forOpen.Length, @"\");
                        }
                        try
                        {
                            Process.Start(forOpen + listView1.Items[i].Text);
                        }
                        catch
                        {
                            MessageBox.Show("You have not got the permissions to this file");
                            return;
                        }
                    }
                    if (listView1.Items[i].Group == listView1.Groups[cdDrive])
                    { }
                }
            }
        }
        /// <summary>
        /// Searches all folders and files in the selected directory
        /// </summary>
        /// <param name="curPath"></param>
        private void SearchFoldersNFilesForListView(string curPath)
        {
            if (!isTrue)
            {
                listView1.Columns.Remove(free);
                sizeC.Text = "Size";
                sizeC.Width = listView1.Width / (int)Numbers.Four;
                sizeC.TextAlign = HorizontalAlignment.Center;

                date = listView1.Columns.Add("Creation date");
                date.Width = listView1.Width / (int)Numbers.Four;
                date.TextAlign = HorizontalAlignment.Center;
                isTrue = true;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(curPath);
            DirectoryInfo[] dirs;
            FileInfo[] files;

            try
            {
                dirs = directoryInfo.GetDirectories();
                files = directoryInfo.GetFiles();
            }
            catch
            {
                return;
            }

            foreach (DirectoryInfo dir in dirs)
            {
                ListViewItem newDir = listView1.Items.Add(dir.Name, 0);
                newDir.SubItems.Add("");
                newDir.SubItems.Add(dir.CreationTime.ToString());
                listView1.Groups[folderGroup].Items.Add(newDir);
            }
            foreach (FileInfo file in files)
            {
                ListViewItem newFile = listView1.Items.Add(file.Name, 1);
                size = file.Length.ToFileSize();
                newFile.SubItems.Add(size);
                newFile.SubItems.Add(file.CreationTime.ToString());
                listView1.Groups[fileGroup].Items.Add(newFile);
            }
        }
        /// <summary>
        /// Images for a listView
        /// </summary>
        private void ImagesForList()
        {
            smallIcon = new ImageList();
            smallIcon.Images.Add(Image.FromFile("img/cf.png"));
            smallIcon.Images.Add(Image.FromFile("img/file.png"));

            smallIcon.ImageSize = new Size(16, 16);
            listView1.SmallImageList = smallIcon;

            largeIcon = new ImageList();
            largeIcon.Images.Add(Image.FromFile("img/cf.png"));
            largeIcon.Images.Add(Image.FromFile("img/file.png"));

            largeIcon.ImageSize = new Size(32, 32);
            listView1.LargeImageList = largeIcon;
        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (!forSort)
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        listView1.Sorting = SortOrder.Descending;
                    }
                    else
                    {
                        listView1.Sorting = SortOrder.Ascending;
                    }
                }
                forSort = true;
            }
            else
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        listView1.Sorting = SortOrder.Ascending;
                    }
                    else
                    {
                        listView1.Sorting = SortOrder.Descending;
                    }
                }
                forSort = false;
            }
            listView1.Sort();
        }

    }

    public static class ExtensionMethods
    {
        public static string ToFileSize(this long l)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
        }
    }

    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }

        private const string fileSizeFormat = "fs";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(fileSizeFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            Decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = "GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = "MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = "kB";
            }
            else
            {
                suffix = " B";
            }

            string precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "2";
            return String.Format("{0:N" + precision + "}{1}", size, suffix);

        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }
            return arg.ToString();
        }

    }
}
