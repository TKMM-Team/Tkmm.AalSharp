using Tkmm.AalSharp.Amta;
using Tkmm.AalSharp.Amta.IO;
using AwesomeAssertions;
using Entish;

namespace Tkmm.AalSharp.Bars.Tests;

public class SerializationTests
{
    private static readonly byte[] _asset = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control.bfwav"));

    private static readonly byte[] _fileLe = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_LE.bars"));
    private static readonly byte[] _fileBe = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_BE.bars"));

    private readonly AudioResource _control = new() {
        ["Test"] = new AudioResourceAsset {
            Metadata = new AalAudioMetadata {
                Data = new AalOptionalMetadata {
                    Name = "Test"
                },
                Markers = [
                    new AalMarker {
                        Id = 0x0,
                        Name = "Test",
                        StartPos = 0,
                        Length = 16
                    },
                    new AalMarker {
                        Id = 0x0,
                        Name = "Test 2",
                        StartPos = 0,
                        Length = 16
                    }
                ],
                Attributes = [
                    new AalAttribute {
                        Key = "1",
                        Value = 1
                    },
                    new AalAttribute {
                        Key = "2",
                        Value = 2
                    }
                ]
            },
            Asset = _asset
        },
        ["Test 2"] = new AudioResourceAsset {
            Metadata = new AalAudioMetadata {
                Data = new AalOptionalMetadata {
                    Name = "Test 2"
                },
                Markers = [
                    new AalMarker {
                        Id = 0x0,
                        Name = "Test",
                        StartPos = 0,
                        Length = 16
                    },
                    new AalMarker {
                        Id = 0x0,
                        Name = "Test 2",
                        StartPos = 0,
                        Length = 16
                    }
                ],
                Attributes = [
                    new AalAttribute {
                        Key = "1",
                        Value = 1
                    },
                    new AalAttribute {
                        Key = "2",
                        Value = 2
                    }
                ]
            },
            Asset = _asset
        }
    };
    
    [Fact]
    public void SerializedLE_ShouldEqual_FileLE()
    {
        _control.ToBinary(endianness: Endianness.Little).Should().BeEquivalentTo(_fileLe);
    }
    
    [Fact]
    public void SerializedBE_ShouldEqual_FileBE()
    {
        _control.ToBinary(endianness: Endianness.Big).Should().BeEquivalentTo(_fileBe);
    }
    
    [Fact]
    public void DeserializedLE_ShouldEqual_Control()
    {
        AudioResource.FromBinary<AmtaSerializer>(_fileLe).Should().BeEquivalentTo(_control);
    }
    
    [Fact]
    public void DeserializedBE_ShouldEqual_Control()
    {
        AudioResource.FromBinary<AmtaSerializer>(_fileBe).Should().BeEquivalentTo(_control);
    }
}