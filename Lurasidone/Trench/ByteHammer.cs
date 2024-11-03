using System.Text;

namespace Lurasidone.Trench;

// There surely must be someone who wrote a Godot binary deserializer in C# before
// Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right? Right?
public class ByteHammer
{
    public static int Datasize = 8;
    private readonly List<Datapomp> _structs;

    private readonly List<string> Keywords =
    [
        "Keys",
        "MapLayer",
        "EmeraldKeys",
        "Sewer",
        "School",
        "Library",
        "Hospital",
        "DNA",
        "Building",
        "Symbol",
        "TPSUnlocked",
        "WonGame"
    ];

    private readonly byte[] ucBlock;

    public ByteHammer(byte[] data)
    {
        ucBlock = data;
        _structs = ReadData();
    }

    public static byte[] OverwriteBytes(byte[] block, List<Datapomp> structs)
    {
        foreach (Datapomp strct in structs) ApplyData(block, strct);

        return block;
    }

    private static void ApplyData(byte[] block, Datapomp strct)
    {
        strct.ApplyChanges();
        byte[] data = strct.GetFullDataBlock();

        data.CopyTo(block, strct.Offset);
    }

    public List<Datapomp> GetStructs()
    {
        return _structs;
    }

    private List<Datapomp> ReadData()
    {
        List<Datapomp> variants = [];
        List<byte[]> blocks = GetStringByteBlocks();

        for (int i = 0; i < Keywords.Count && i < blocks.Count; i++)
        {
            string key = Keywords[i];
            byte[] block = blocks[i];

            byte[]? data = FindPatternAndReadNextBytes(block, out int offset);

            if (data is null)
                throw new IndexOutOfRangeException(
                    $"Couldn't find data associated with pattern {block}, key {key}, code {offset}");

            Datapomp vari = new Datapomp(key, block, data, offset);
            variants.Add(vari);
        }

        return variants;
    }

    private List<byte[]> GetStringByteBlocks()
    {
        List<byte[]> bytesList = [];

        foreach (string keyword in Keywords)
        {
            byte[] name = Encoding.UTF8.GetBytes(keyword);
            byte[] paddedByteBlock = PadBlock(name);
            bytesList.Add(paddedByteBlock);
        }

        return bytesList;
    }

    // Godot always pads packets to 4
    private byte[] PadBlock(byte[] block)
    {
        int rem = block.Length % 4;
        if (rem == 0)
            return block;

        byte[] padded = new byte[block.Length + (4 - rem)];
        Array.Copy(block, padded, block.Length);

        return padded;
    }

    /// <param name="offset">
    ///     Offset at which the pattern starts, -1 if not enough bytes after pattern, -2 if pattern is not
    ///     found
    /// </param>
    private byte[]? FindPatternAndReadNextBytes(byte[] pattern, out int offset)
    {
        for (int i = 0; i <= ucBlock.Length - pattern.Length; i++)
        {
            bool isMatch = true;
            for (int j = 0; j < pattern.Length; j++)
                if (ucBlock[i + j] != pattern[j])
                {
                    isMatch = false;
                    break;
                }

            if (isMatch)
            {
                int startIndex = i + pattern.Length;
                if (startIndex + Datasize <= ucBlock.Length)
                {
                    byte[] nextBytes = new byte[Datasize];
                    Array.Copy(ucBlock, startIndex, nextBytes, 0, Datasize);
                    offset = i;
                    return nextBytes;
                }

                offset = -1; // Not enough bytes after pattern
                return null;
            }
        }

        offset = -2; // Pattern not found
        return null;
    }
}