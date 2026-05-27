using Microsoft.Extensions.Options;
using RecuBackend.Api.Services;

namespace RecuBackend.Tests;

public sealed class LocalFileStorageServiceTests
{
    private static LocalFileStorageService CreateSut(string root)
    {
        var settings = Options.Create(new FileStorageSettings
        {
            RootPath = root,
            MaxFileSizeBytes = 1024,
            AllowedExtensions = [".png", ".pdf"]
        });
        return new LocalFileStorageService(settings);
    }

    [Fact]
    public void ValidateUpload_rejects_empty_file()
    {
        var sut = CreateSut(Path.GetTempPath());
        Assert.Throws<ArgumentException>(() => sut.ValidateUpload("a.png", "image/png", 0));
    }

    [Fact]
    public void ValidateUpload_rejects_disallowed_extension()
    {
        var sut = CreateSut(Path.GetTempPath());
        Assert.Throws<ArgumentException>(() => sut.ValidateUpload("virus.exe", "application/octet-stream", 10));
    }

    [Fact]
    public void ValidateUpload_accepts_png()
    {
        var sut = CreateSut(Path.GetTempPath());
        var ex = Record.Exception(() => sut.ValidateUpload("sheet.png", "image/png", 100));
        Assert.Null(ex);
    }
}
