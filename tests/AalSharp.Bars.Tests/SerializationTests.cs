using AwesomeAssertions;
using Entish;

namespace AalSharp.Bars.Tests;

public class SerializationTests
{
    private static readonly byte[] _asset = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control.bfwav"));

    private static readonly byte[] _fileLe = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_LE.bars"));
    private static readonly byte[] _fileBe = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", "Control_BE.bars"));

    private readonly BarsFile _control = new() {
        ["Test"] = new BarsEntry {
            Metadata = new BarsMetadata {
                Data = new BarsMetadataData {
                    Name = "Test"
                },
                Marker = [
                    new BarsMetadataMarkerEntry {
                        Id = 0x0,
                        Name = "Test",
                        StartPos = 0,
                        Length = 16
                    },
                    new BarsMetadataMarkerEntry {
                        Id = 0x0,
                        Name = "Test 2",
                        StartPos = 0,
                        Length = 16
                    }
                ],
                Ext = [
                    new BarsMetadataExtEntry {
                        Unknown1 = 1,
                        Unknown2 = 2
                    },
                    new BarsMetadataExtEntry {
                        Unknown1 = 3,
                        Unknown2 = 4
                    }
                ]
            },
            Asset = _asset
        },
        ["Test 2"] = new BarsEntry {
            Metadata = new BarsMetadata {
                Data = new BarsMetadataData {
                    Name = "Test 2"
                },
                Marker = [
                    new BarsMetadataMarkerEntry {
                        Id = 0x0,
                        Name = "Test",
                        StartPos = 0,
                        Length = 16
                    },
                    new BarsMetadataMarkerEntry {
                        Id = 0x0,
                        Name = "Test 2",
                        StartPos = 0,
                        Length = 16
                    }
                ],
                Ext = [
                    new BarsMetadataExtEntry {
                        Unknown1 = 1,
                        Unknown2 = 2
                    },
                    new BarsMetadataExtEntry {
                        Unknown1 = 3,
                        Unknown2 = 4
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
        BarsFile.FromBinary(_fileLe).Should().BeEquivalentTo(_control);
    }
    
    [Fact]
    public void DeserializedBE_ShouldEqual_Control()
    {
        BarsFile.FromBinary(_fileBe).Should().BeEquivalentTo(_control);
    }
}