using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.HTML
{
    internal class FileTools
    {
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            Verse.Log.Message(targetPath);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            //Now Create all of the directories
            foreach (
                string dirPath in Directory.GetDirectories(
                    sourcePath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (
                string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
            )
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
