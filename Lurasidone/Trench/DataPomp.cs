namespace Lurasidone.Trench;

public class Datapomp
{
    private static readonly byte[] BSig = [0x1, 0x0, 0x0, 0x0];
    private static readonly byte[] ISig = [0x2, 0x0, 0x0, 0x0];
    private readonly byte[] _nameBlock;
    private readonly byte[] _data; // Includes the signature

    public string Name;
    public int Offset; // offset at the start of name pattern, not data
    public object? Value;

    public Datapomp(string name, byte[] nameBlock, byte[] data, int startoffset)
    {
        Name = name;
        _nameBlock = nameBlock;
        _data = data;
        Offset = startoffset;

        Read();
    }

    public byte[] GetFullDataBlock()
    {
        byte[] sig = IsBool() ? BSig : ISig;
        return _nameBlock.Concat(_data).ToArray();
    }

    public void ApplyChanges()
    {
        if (Value is bool boolValue)
        {
            byte e = boolValue ? (byte)0x1 : (byte)0x0;
            Console.WriteLine($"{Name}: Swapping value to {e}");
            _data[4] = e;
        }
        else if (Value is int intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            // Copy intBytes to _data starting at index 4
            Array.Copy(intBytes, 0, _data, 4, Math.Min(intBytes.Length, _data.Length - 4));
        }
    }

    private void Read()
    {
        if (IsBool())
            Value = ReadBool();
        else
            Value = ReadInt();
    }

    private bool IsBool()
    {
        return _data.Take(BSig.Length).SequenceEqual(BSig);
    }

    private bool ReadBool()
    {
        return BitConverter.ToBoolean(_data, 4);
    }

    private int ReadInt()
    {
        return BitConverter.ToInt32(_data, 4);
    }
}