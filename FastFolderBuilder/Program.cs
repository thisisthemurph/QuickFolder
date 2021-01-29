using FastFolderUserInterface;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FastFolderBuilder
{
    class Program
    {
        static void Main(string[] args) 
        {
            Console.WriteLine(args.Length);
            foreach (string arg in args)
            {
                Console.WriteLine($"\t{arg}");
            }

            //string structures = @"C:\Users\micha\Desktop\Tests\structures.xml";

            string target = args[0];
            string structureKey = args[1];
            string structures = args[2];

            Console.WriteLine(target);
            Console.WriteLine(structureKey);
            Console.WriteLine(structures);

            FolderStructure fs = ReadStructure(structures, structureKey);

            if (fs != null)
            {
                fs.CreateStructure(target);
            }
        }

        /// <summary>
        /// Converst the XML to a FolderStructure object
        /// </summary>
        /// <param name="folderStructuresPath">The path to the XML file containing the folder structures</param>
        /// <param name="key">The key indicating which folder structure to create</param>
        private static FolderStructure ReadStructure(string folderStructuresPath, string key)
        {
            if (!File.Exists(folderStructuresPath))
            {
                Console.WriteLine("Cannot locate the path to the structures!");
                return null;
            }

            XDocument doc = XDocument.Load(folderStructuresPath);
            var structures = doc.Descendants("Structure");

            foreach (XElement structure in structures)
            {
                if (key == structure.Attribute("key").Value)
                {
                    XElement rootFolder = structure.Element("Folder");
                    return new FolderStructure(rootFolder);
                }
            }

            return null;
        }
    }
}
