using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class DirectoryInfoExtentions
    {
        public static void CopyTo(this DirectoryInfo sourceDir, string destDirName, CancellationToken token)
        {
            var sourceDirName = sourceDir.FullName;

            if(!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));

            if (!destDirName.EndsWith(Path.DirectorySeparatorChar.ToString()))
                destDirName = destDirName + Path.DirectorySeparatorChar;

            var files = Directory.GetFiles(sourceDirName);
            foreach(var file in files)
            {
                token.ThrowIfCancellationRequested();
                File.Copy(file, destDirName + Path.GetFileName(file), true);
            }

            // コピー元のディレクトリのサブディレクトリを取得する。
            var dirs = Directory.GetDirectories(sourceDirName);
            foreach(var dir in dirs.Select(a => new DirectoryInfo(a)))
            {
                token.ThrowIfCancellationRequested();
                dir.CopyTo(destDirName + dir.Name, token);
            }
        }
    }
}
