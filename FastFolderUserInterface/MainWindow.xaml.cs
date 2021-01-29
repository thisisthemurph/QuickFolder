using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FastFolderUserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly string MENUITEMS_PATH = @"C:\Users\micha\Desktop\Tests\menuitems.json";

        private List<ContextMenuItem> ContextMenuItems;
        private string ContextMenuSettingsPath;
        private string FolderStructuresSettingsPath;

        public MainWindow()
        {
            InitializeComponent();

            ContextMenuSettingsPath = Properties.Settings.Default["MenuItemsPath"].ToString();
            FolderStructuresSettingsPath = Properties.Settings.Default["StructuresPath"].ToString();

            ContextMenuItemsPath.Text = ContextMenuSettingsPath;
            FolderStructuresPath.Text = FolderStructuresSettingsPath;

            UpdateContextMenuItems(ContextMenuSettingsPath);
        }

        private void UpdateContextMenuItems(string menuItemsPath)
        {
            string menuItemsJson = File.ReadAllText(menuItemsPath);
            ContextMenuItems = JsonConvert.DeserializeObject<List<ContextMenuItem>>(menuItemsJson);
        }

        private string RegPath(params string[] components)
        {
            string value = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                value += "\\" + components[i];
            }

            return value;
        }

        private string GetFastFolderExeFullPath()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return Path.Combine(new FileInfo(location.AbsolutePath).Directory.FullName, "FastFolder.exe");
        }

        private string GetFastFolderBuilderExeFullPath()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return Path.Combine(new FileInfo(location.AbsolutePath).Directory.FullName, "FastFolderBuilder.exe");
        }

        /// <summary>
        /// Creates a context menu item as described by the ContextMenuItem object
        /// </summary>
        /// <param name="contextMenuItem">The context menu item to be created</param>
        private void MakeContextMenuItem(ContextMenuItem contextMenuItem)
        {
            const string currentUser = "HKEY_CURRENT_USER";

            string userBackgroundShell = RegPath(currentUser, "Software", "Classes", "Directory", "Background", "shell");
            string root = RegPath(userBackgroundShell, contextMenuItem.Key);

            Registry.SetValue(root, "MUIVerb", contextMenuItem.Verb);
            Registry.SetValue(root, "Icon", GetFastFolderExeFullPath());

            if (contextMenuItem.IsUIKey)
            {
                Registry.SetValue(RegPath(root, "command"), "", GetFastFolderExeFullPath());
            }

            if (contextMenuItem.MenuItems.Count > 0)
            {
                Registry.SetValue(root, "subcommands", "");

                foreach (ContextMenuItem cmi in contextMenuItem.MenuItems)
                {
                    string path = RegPath(root, "shell", cmi.Key);
                    Registry.SetValue(path, "MUIVerb", cmi.Verb);

                    string command = $"\"{GetFastFolderBuilderExeFullPath()}\" {cmi.Command} \"{FolderStructuresSettingsPath}\"";
                    Registry.SetValue(path + "\\" + "command", "", command);
                }
            }
        }

        /// <summary>
        /// Sets the registry menus in the right click context menu
        /// </summary>
        private void SetAllRegistryMenus()
        {
            foreach (ContextMenuItem menuItem in ContextMenuItems)
            {
                MakeContextMenuItem(menuItem);
            }

            MessageBox.Show("Menu items have been added to to the right-click context menu.", "Very nice...");
        }

        /// <summary>
        /// Removes all the registry menus having a key start with "QuickFolder"
        /// </summary>
        /// <param name="showAletrs">If the function should display success/failure alerts</param>
        private void RemoveAllregistryMenus(bool showAletrs = false)
        {
            string userBackgroundShell = RegPath("Software", "Classes", "Directory", "Background", "shell");

            RegistryKey rootKey = Registry.CurrentUser.OpenSubKey(userBackgroundShell);
            string[] subKeys = rootKey.GetSubKeyNames();

            int numKeysRemoved = 0;
            foreach (string subKey in subKeys)
            {
                if (subKey.StartsWith("QuickFolder"))
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RegPath(userBackgroundShell, subKey));
                    numKeysRemoved++;
                }
            }

            if (showAletrs)
            {
                if (numKeysRemoved == 0)
                {
                    MessageBox.Show("There were no context menu items to remove!", "Can't do that...");
                }
                else
                {
                    MessageBox.Show("Menu items have been removed from the right-click context menu.", "All gone now...");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllregistryMenus();
            SetAllRegistryMenus();            
        }

        private void UnsetRegistryMenus_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllregistryMenus(showAletrs: true);
        }

        private void BrowseContextMenuItemsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select the context menu settings file...",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                ContextMenuItemsPath.Text = dlg.FileName;
                ContextMenuSettingsPath = dlg.FileName;

                Properties.Settings.Default["MenuItemsPath"] = dlg.FileName;
                Properties.Settings.Default.Save();

                UpdateContextMenuItems(ContextMenuSettingsPath);
                RemoveAllregistryMenus();
                SetAllRegistryMenus();
            }
        }

        private void BrowseFolderStructuresFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select the folder structures settings file...",
                Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                FolderStructuresPath.Text = dlg.FileName;
                FolderStructuresSettingsPath = dlg.FileName;

                Properties.Settings.Default["StructuresPath"] = dlg.FileName;
                Properties.Settings.Default.Save();
            }
        }
    }
}
