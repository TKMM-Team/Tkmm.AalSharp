using AwesomeAssertions;
using Entish;

namespace Tkmm.AalSharp.Bars.Tests;

public class SerializationTests
{
    private static readonly byte[] _asset = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control.bfwav"));
    private static readonly byte[] _metadata1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.amta"));
    private static readonly byte[] _metadata2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.amta"));

    private static readonly byte[] _fileLe = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_LE.bars"));

    private readonly AudioResource _control = new() {
        ["Test"] = new AudioResourceAsset {
            Metadata = _metadata1,
            Asset = _asset
        },
        ["Test 2"] = new AudioResourceAsset {
            Metadata = _metadata2,
            Asset = _asset
        }
    };
    
    [Fact]
    public void SerializedLE_ShouldEqual_FileLE()
    {
        _control.ToBinary(endianness: Endianness.Little).Should().BeEquivalentTo(_fileLe);
    }
    
    [Fact]
    public void DeserializedLE_ShouldEqual_Control()
    {
        AudioResource.FromBinary(_fileLe).Should().BeEquivalentTo(_control);
    }
}