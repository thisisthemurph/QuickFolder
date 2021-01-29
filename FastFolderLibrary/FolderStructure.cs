using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace FastFolderUserInterface
{
    public class FolderStructure
    {
        public string Name { get; private set; }
        public List<FolderStructure> Folders { get; private set; }

        public bool HasSubFolders
        {
            get { return Folders.Count > 0; }
        }

        private List<string> FinalFolders;

        /// <summary>
        /// Creates a FolderStructure object from XML XElement
        /// </summary>
        /// <param name="root">The XElement object specifying the structure</param>
        public FolderStructure(XElement root)
        {
            Name = root.Attribute("name").Value;
            Folders = new List<FolderStructure>();
            FinalFolders = new List<string>();
            BuildSubFolders(root, this);
        }

        private FolderStructure(string name)
        {
            Name = name;
            Folders = new List<FolderStructure>();
            FinalFolders = new List<string>();
        }

        /// <summary>
        /// Creates the directory structure at the given location
        /// </summary>
        /// <param name="path">The location to create the directpry structure</param>
        public void CreateStructure(string path)
        {   
            BuildPathsToFinalFolders(this);

            foreach (string folderPath in FinalFolders)
            {
                Directory.CreateDirectory(path + "\\" + Name + "\\" + folderPath);
            }
        }

        private void BuildPathsToFinalFolders(FolderStructure rootFolder, string prevPath = null)
        {
            foreach (FolderStructure folder in rootFolder.Folders)
            {
                string path;
                if (prevPath != null)
                {
                    path = prevPath + "\\" + folder.Name;
                }
                else
                {
                    path = folder.Name;
                }

                if (folder.HasSubFolders)
                {
                    BuildPathsToFinalFolders(folder, prevPath: path);
                }
                else
                {
                    FinalFolders.Add(path);
                }
            }
        }

        private void BuildSubFolders(XElement root, FolderStructure folderStructure)
        {
            if (root.HasElements)
            {
                foreach(XElement folder in root.Elements("Folder"))
                {
                    FolderStructure fs = new FolderStructure(folder.Attribute("name").Value);
                    folderStructure.Folders.Add(fs);

                    BuildSubFolders(folder, fs);
                }
            }
        }
    }
}
