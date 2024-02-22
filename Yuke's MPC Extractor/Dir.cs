namespace Yukes_MPC_Extractor;

public class Dir
{
    public List<Dir> Directories = new List<Dir>();
    public List<FileEntry> Files = new List<FileEntry>();
    public string Name = "";
    public string Path = "";
    public int SubEntryCount;
    public uint FirstSubEntryIndex;
}