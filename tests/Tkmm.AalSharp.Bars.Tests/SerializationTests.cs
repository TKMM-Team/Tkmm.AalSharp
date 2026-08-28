using AwesomeAssertions;
using Entish;

namespace Tkmm.AalSharp.Bars.Tests;

public class SerializationTests
{
    private static readonly byte[] _asset1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.bwav"));
    private static readonly byte[] _asset2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.bwav"));
    private static readonly byte[] _metadata1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.amta"));
    private static readonly byte[] _metadata2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.amta"));

    private static readonly byte[] _file = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control.bars"));

    private readonly AudioResource _control = new() {
        ["Test"] = new AudioResourceAsset {
            Metadata = _metadata1,
            Asset = _asset1
        },
        ["Test 2"] = new AudioResourceAsset {
            Metadata = _metadata2,
            Asset = _asset2
        }
    };
    
    [Fact]
    public void Serialized_ShouldEqual_File()
    {
        _control.ToBinary().Should().BeEquivalentTo(_file);
    }
    
    [Fact]
    public void Deserialized_ShouldEqual_Control()
    {
        AudioResource.FromBinary(_file).Should().BeEquivalentTo(_control);
    }
}