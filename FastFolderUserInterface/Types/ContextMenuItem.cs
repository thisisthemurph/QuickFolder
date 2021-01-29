using System;
using System.Collections.Generic;
using System.Text;

namespace FastFolderUserInterface
{
    public class ContextMenuItem
    {
        public string Key { get; set; }
        public string Verb { get; set; }
        public string Command { get; set; }
        public bool IsUIKey { get; set; }
        public List<ContextMenuItem> MenuItems { get; set; } = new List<ContextMenuItem>();
    }
}
