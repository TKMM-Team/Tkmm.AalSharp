using AwesomeAssertions;
using Entish;

namespace Tkmm.AalSharp.Bars.Tests;

public class SerializationTests
{
    private static readonly byte[] _asset1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.bwav"));
    private static readonly byte[] _asset2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.bwav"));
    private static readonly byte[] _metadata1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.amta"));
    private static readonly byte[] _metadata2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.amta"));
    private static readonly byte[] _metadata3 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_3.amta"));

    private static readonly byte[] _file1 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_1.bars"));
    private static readonly byte[] _file2 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_2.bars"));

    private readonly AudioResource _control1 = new() {
        ["Test 1"] = new AudioResourceAsset {
            Metadata = _metadata1,
            Asset = _asset1
        },
        ["Test 2"] = new AudioResourceAsset {
            Metadata = _metadata2,
            Asset = _asset2
        },
        ["Test 3"] = new AudioResourceAsset {
            Metadata = _metadata3,
            Asset = _asset1
        }
    };

    private readonly AudioResource _control2 = new() {
        ["Test 1"] = new AudioResourceAsset {
            Metadata = _metadata1,
            Asset = _asset1
        },
        ["Test 2"] = new AudioResourceAsset {
            Metadata = _metadata2,
            Asset = _asset1
        },
        ["Test 3"] = new AudioResourceAsset {
            Metadata = _metadata3,
            Asset = _asset2
        }
    };
    
    [Fact]
    public void Serialized_ShouldEqual_File()
    {
        _control1.ToBinary().Should().BeEquivalentTo(_file1);
        _control2.ToBinary().Should().BeEquivalentTo(_file2);
    }
    
    [Fact]
    public void Deserialized_ShouldEqual_Control()
    {
        AudioResource.FromBinary(_file1).Should().BeEquivalentTo(_control1);
        AudioResource.FromBinary(_file2).Should().BeEquivalentTo(_control2);
    }
}