using System.IO;
using Verse;

namespace DiaryMod.HTML
{
    internal class FileTools
    {
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            Log.Message(targetPath);
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            //Now Create all of the directories
            foreach (
                var dirPath in Directory.GetDirectories(
                    sourcePath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            //Copy all the files & Replaces any files with the same name
            foreach (
                var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
            )
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}