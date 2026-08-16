using System.Text;
using CM26.Application.Services;

namespace CM26.Tests;

public sealed class NameTextDecoderTests
{
    [Fact]
    public void RawDatabasePlaceholderIsRejected() =>
        Assert.Null(NameTextDecoder.Decode(new byte[] { 0xC4, 0xC4, 0x44, 0xC4 }));

    [Theory]
    [InlineData("Rhys")]
    [InlineData("Ng")]
    [InlineData("Čech")]
    [InlineData("Лев")]
    [InlineData("李")]
    public void HuffmanOutputAcceptsValidInternationalNames(string value) =>
        Assert.Equal(value, NameTextDecoder.DecodeHuffman(Encoding.UTF8.GetBytes(value)));

    [Theory]
    [InlineData("Player123")]
    [InlineData("Bad_Name")]
    [InlineData("@")]
    public void HuffmanOutputStillRejectsNonNamePayloads(string value) =>
        Assert.Null(NameTextDecoder.DecodeHuffman(Encoding.UTF8.GetBytes(value)));
}
