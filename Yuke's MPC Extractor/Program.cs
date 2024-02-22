using System.Runtime.InteropServices;
using System.Text;
using Binft;

namespace Yukes_MPC_Extractor
{
    public class Program
    {
        static string _path;
        static bool _run = true;
        public static bool LittleEndian = false;

        public static void Main()
        {
            while (_run)
            {
                Console.Clear();
                Console.WriteLine("MPC Extractor for The DOG Island");

                string input = "";
                while (!string.Equals(input, "wii", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(input, "ps2", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Which version are you extracting files from?   \"wii\" / \"ps2\"");
                    input = Console.ReadLine();
                }
                if (string.Equals(input, "ps2", StringComparison.CurrentCultureIgnoreCase))
                    LittleEndian = true;

                input = "";
               
                Console.WriteLine("Please enter path to mpc");
                _path = Console.ReadLine().Replace("\"", "");
                while (!File.Exists(_path) || !_path.EndsWith(".mpc"))
                {
                    Console.WriteLine("Path was invalid");
                    Console.WriteLine("Please enter path to mpc");
                    _path = Console.ReadLine().Replace("\"", " ");
                }

                Console.WriteLine("Extracting...");
                if (Path.GetFileNameWithoutExtension(_path) == "static")
                    OldExtractor.Extract(_path);
                else
                    Extract(_path);
                Console.WriteLine("Extract completed successfully");
                Console.ReadLine();
            }
        }

        public static void Extract(string path)
        {
            var binf = Binft.Binft.OpenBinf(path, LittleEndian);
            var mpc = new MPC();
            mpc.Name = Path.GetFileNameWithoutExtension(path);
            var inDirPath = Path.GetDirectoryName(path);
            var outDirPath = Path.Combine(inDirPath, mpc.Name + "mpc");
            if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

            mpc.EntryListLength = binf.ReadUInt();
            mpc.EntryCount = mpc.EntryListLength / 0xC;
            mpc.FileDataLength = binf.ReadUInt();
            mpc.EntryListOffset = binf.ReadUInt(); 
            mpc.FileDataOffset = binf.ReadUInt();
            binf.GoTo(mpc.EntryListOffset);
            
            Dir root = new();
            var directoryIndicator = binf.ReadShort();
            root.SubEntryCount = binf.ReadUShort();
            root.Name = binf.ReadString(4).ToString();
            binf.Skip(4);

            HandleEntry(binf, mpc, root);
            GenerateFiles(root, mpc, binf, outDirPath);
        }

        public static void HandleEntry(Binf binf, MPC mpc, Dir root)
        {
            for (var i = 0; i < root.SubEntryCount; i++)
            {
                var directoryIndicator = binf.ReadShort();
                if (directoryIndicator == 0)
                {
                    var subDir = new Dir
                    {
                        SubEntryCount = binf.ReadUShort(),
                        Name = binf.ReadString(4).ToString(),
                        FirstSubEntryIndex = binf.ReadUInt()
                    };
                    root.Directories.Add(subDir);
                }
                else
                {
                    var subFile = new FileEntry
                    {
                        Size = binf.ReadUShort() * (uint)32,
                        Name = binf.ReadString(4).ToString(),
                        DataOffset = binf.ReadUInt()
                    };
                    root.Files.Add(subFile);
                }
            }
            foreach (var dir in root.Directories)
                HandleEntry(binf, mpc, dir);
        }
        
        public static void GenerateFiles(Dir dir, MPC mpc, Binf binf, string outDir)
        {
            Directory.CreateDirectory(Path.Combine(outDir, dir.Name));
            foreach (Dir subDir in dir.Directories)
            {
                GenerateFiles(subDir, mpc, binf, Path.Combine(outDir, dir.Name));
            }
            foreach (FileEntry file in dir.Files)
            {
                var filePath = Path.Combine(outDir, dir.Name, file.Name + ".bin");
                if (File.Exists(filePath))
                {
                    var newFilePath = GetUniqueFileName(filePath);
                    Console.WriteLine($"File already exists. Renaming to: {newFilePath}");
                    filePath = newFilePath;
                }
                var o = File.Create(filePath);
                binf.GoTo(mpc.FileDataOffset + file.DataOffset);
                o.Write(binf.ReadBytes((int)file.Size));
                o.Close();
            }
        }
        
        static string GetUniqueFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int count = 1;
            string newFilePath = filePath;
            while (File.Exists(newFilePath))
            {
                string tempFileName = $"{fileName}_{count}";
                newFilePath = Path.Combine(directory, tempFileName + extension);
                count++;
            }
            return newFilePath;
            
            
        }
    }
}