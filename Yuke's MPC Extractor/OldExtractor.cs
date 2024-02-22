using System.Text;

namespace Yukes_MPC_Extractor;

public class OldExtractor
{
    public static void Extract(string path)
    {
        string mpcName = Path.GetFileNameWithoutExtension(path);
        string inDirPath = Path.GetDirectoryName(path);
        string outDirPath = Path.Combine(inDirPath, mpcName + "mpc");
        if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

        var f = File.OpenRead(path);
        int mpcEntriesLength = DataRead.ToInt32(ReadBytes(f, new byte[4]), 0);
        int mpcEntryCount = mpcEntriesLength / 0x28;
        f.Seek(0x4, SeekOrigin.Current);
        int entriesOffset = DataRead.ToInt32(ReadBytes(f, new byte[4]), 0);
        int dataOffset = DataRead.ToInt32(ReadBytes(f, new byte[4]), 0);

        for (int i = 0; i < mpcEntryCount; i++)
        {
            f.Seek(0x20 + 0x28 * i, SeekOrigin.Begin);
            bool isDir = f.ReadByte() == 0x0;
            int fileCount;
            int fileLength = 0;
            f.Seek(1, SeekOrigin.Current);
            Console.WriteLine(f.Position);
            if (isDir) fileCount = DataRead.ToUInt16(ReadBytes(f, new byte[2]), 0);
            else fileLength = DataRead.ToUInt16(ReadBytes(f, new byte[2]), 0) * 32;
            Console.WriteLine(f.Position);
            string fileName = ReadString(ReadBytes(f, new byte[0x8]), 0);
            string fileExt = fileName;
            f.Seek(24, SeekOrigin.Current);
            uint dirNum = 0;
            uint fileOffset = 0;
            if (isDir)
            {
                dirNum = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);
                outDirPath += "\\" + fileName;
                if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);
            }
            else
            {
                fileOffset = DataRead.ToUInt32(ReadBytes(f, new byte[4]), 0);
                f.Seek(fileOffset + dataOffset, SeekOrigin.Begin);
                var outFile = File.Create(Path.Combine(outDirPath, fileName + "." + fileExt));
                outFile.Write(ReadBytes(f, new byte[fileLength]), 0, (int)fileLength);
                outFile.Close();
            }
        }
    }

    public static void ExtractAll(string dirPath)
    {
        foreach (var f in Directory.GetFiles(dirPath, "*.mpc", SearchOption.AllDirectories))
        {
            Extract(f);
        }
    }

    public static byte[] ReadBytes(FileStream f, byte[] buffer)
    {
        f.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public static string ReadString(byte[] bytes, int position)
    {
        int endOfString = Array.IndexOf<byte>(bytes, 0x0, position);
        if (endOfString == position) return string.Empty;
        string s = Encoding.ASCII.GetString(bytes, position, endOfString - position);
        return s;
    }
}