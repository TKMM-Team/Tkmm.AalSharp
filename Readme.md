<div align="center">
  <img src="https://raw.githubusercontent.com/EPD-Libraries/AalSharp/refs/heads/master/resources/Icon.png" width="100vh">
  <h1>- &nbsp; AAL# &nbsp; -</h1>
</div>

C# IO library for BARS and other AAL file formats.

Supports BARS **v101** and AMTA **v4**.

- [Usage](#usage)
    - [Reading a BARS File](#reading-a-bars-file)
    - [Writing a BARS File](#writing-a-bars-file)
- [Benchmarks](#benchmarks)
    - [Install](#install)
        - [NuGet](#nuget)
        - [Build From Source](#build-from-source)

## Usage

### Reading a BARS File

```cs
using AalSharp.Bars;

byte[] data = File.ReadAllBytes("path/to/file.bars");
var bars = BarsFile.FromBinary(data, out var endianness);
```

### Writing a BARS File

```cs
using AalSharp.Bars;

// Write to a stream
using FileStream fs = File.Create("path/to/output.bars");
bars.Write(fs, Endianness.Little);

// Write to a byte[]
byte[] data = bars.ToBinary(Endianness.Little, endianness: Entish.Endianness.Little);
```

> [!NOTE]
> Writing to a pointer or `Span<byte>` can be done via the [AalSharp.Bars.IO.BarsSerializer](https://github.com/EPD-Libraries/AalSharp/blob/master/src/AalSharp.Bars/IO/BarsSerializer.cs) class

## Benchmarks

> TODO

### Install

#### AalSharp

[![NuGet](https://img.shields.io/nuget/v/AalSharp.svg?style=for-the-badge&labelColor=2a2c33)](https://www.nuget.org/packages/AalSharp) [![NuGet](https://img.shields.io/nuget/dt/AalSharp.svg?style=for-the-badge&labelColor=2a2c33&color=32a852)](https://www.nuget.org/packages/AalSharp)

#### AalSharp.Bars

[![NuGet](https://img.shields.io/nuget/v/AalSharp.Bars.svg?style=for-the-badge&labelColor=2a2c33)](https://www.nuget.org/packages/AalSharp.Bars) [![NuGet](https://img.shields.io/nuget/dt/AalSharp.Bars.svg?style=for-the-badge&labelColor=2a2c33&color=32a852)](https://www.nuget.org/packages/AalSharp.Bars)

#### NuGet
```powershell
Install-Package AalSharp
Install-Package AalSharp.Bars
```

#### Build From Source
```batch
git clone https://github.com/EPD-Libraries/AalSharp.git
dotnet build AalSharp
dotnet build AalSharp.Bars
```

Special thanks to **[Yannik Marchand (kinnay)](https://reversing.live/)** for the research on the AAL file formats. 