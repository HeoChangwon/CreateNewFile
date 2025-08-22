using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Tests.Utils;

public class FileNameBuilderTests : IDisposable
{
    private readonly string _testDirectory;

    public FileNameBuilderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "CNF_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void GenerateFileName_ValidRequest_ReturnsCorrectFormat()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Suffix = "v1",
            Extension = "txt"
        };

        // Act
        var result = FileNameBuilder.GenerateFileName(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("CNF", result);
        Assert.Contains("Test_File", result);
        Assert.Contains("v1", result);
        Assert.EndsWith(".txt", result);
        Assert.Matches(@"^\d{8}_\d{4}_", result); // 날짜/시간 형식 검증
    }

    [Fact]
    public void GenerateFileName_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FileNameBuilder.GenerateFileName(null!));
    }

    [Fact]
    public void CleanStringForFileName_InvalidCharacters_RemovesCharacters()
    {
        // Arrange
        var input = "Test<>File|Name";

        // Act
        var result = FileNameBuilder.CleanStringForFileName(input);

        // Assert
        Assert.Equal("TestFileName", result);
    }

    [Fact]
    public void CleanStringForFileName_WithSpaces_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Test File Name";

        // Act
        var result = FileNameBuilder.CleanStringForFileName(input);

        // Assert
        Assert.Equal("Test_File_Name", result);
    }

    [Fact]
    public void NormalizeExtension_WithoutDot_AddsDot()
    {
        // Arrange
        var extension = "txt";

        // Act
        var result = FileNameBuilder.NormalizeExtension(extension);

        // Assert
        Assert.Equal(".txt", result);
    }

    [Fact]
    public void NormalizeExtension_WithDot_KeepsDot()
    {
        // Arrange
        var extension = ".txt";

        // Act
        var result = FileNameBuilder.NormalizeExtension(extension);

        // Assert
        Assert.Equal(".txt", result);
    }

    [Fact]
    public void GenerateFullPath_ValidInputs_ReturnsCorrectPath()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = FileNameBuilder.GenerateFullPath(request);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(_testDirectory, result);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void ValidateRequest_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = FileNameBuilder.ValidateRequest(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateRequest_EmptyOutputPath_ReturnsInvalid()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test",
            Extension = "txt",
            OutputPath = ""
        };

        // Act
        var result = FileNameBuilder.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("출력 경로"));
    }

    [Fact]
    public void ValidateRequest_NonExistentOutputPath_ReturnsInvalid()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test",
            Extension = "txt",
            OutputPath = @"C:\NonExistentFolder"
        };

        // Act
        var result = FileNameBuilder.ValidateRequest(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("존재하지 않습니다"));
    }

    [Fact]
    public void IsValidFileName_ValidName_ReturnsTrue()
    {
        // Arrange
        var fileName = "test_file.txt";

        // Act
        var result = FileNameBuilder.IsValidFileName(fileName);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("con.txt")]
    [InlineData("aux.doc")]
    [InlineData("nul.pdf")]
    public void IsValidFileName_ReservedNames_ReturnsFalse(string fileName)
    {
        // Act
        var result = FileNameBuilder.IsValidFileName(fileName);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("file<name>.txt")]
    [InlineData("file|name.txt")]
    [InlineData("file:name.txt")]
    public void IsValidFileName_InvalidCharacters_ReturnsFalse(string fileName)
    {
        // Act
        var result = FileNameBuilder.IsValidFileName(fileName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPath_ValidPath_ReturnsTrue()
    {
        // Act
        var result = FileNameBuilder.IsValidPath(_testDirectory);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(@"invalid<>path")]
    public void IsValidPath_InvalidPath_ReturnsFalse(string path)
    {
        // Act
        var result = FileNameBuilder.IsValidPath(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateFileName_OnlyAbbreviation_GeneratesCorrectly()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Extension = "txt"
        };

        // Act
        var result = FileNameBuilder.GenerateFileName(request);

        // Assert
        Assert.Contains("CNF", result);
        Assert.EndsWith(".txt", result);
        Assert.Matches(@"^\d{8}_\d{4}_CNF\.txt$", result);
    }

    [Fact]
    public void GenerateFileName_OnlyTitle_GeneratesCorrectly()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Title = "Test File",
            Extension = "txt"
        };

        // Act
        var result = FileNameBuilder.GenerateFileName(request);

        // Assert
        Assert.Contains("Test_File", result);
        Assert.EndsWith(".txt", result);
        Assert.Matches(@"^\d{8}_\d{4}_Test_File\.txt$", result);
    }
}