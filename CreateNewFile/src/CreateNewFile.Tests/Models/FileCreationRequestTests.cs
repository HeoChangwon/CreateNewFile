using CreateNewFile.Models;

namespace CreateNewFile.Tests.Models;

public class FileCreationRequestTests : IDisposable
{
    private readonly string _testDirectory;

    public FileCreationRequestTests()
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
    public void IsValid_AllFieldsSet_ReturnsTrue()
    {
        // Arrange
        var templateFile = Path.Combine(_testDirectory, "template.txt");
        File.WriteAllText(templateFile, "template content");

        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Suffix = "v1",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = templateFile
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_MissingRequiredFields_ReturnsFalse()
    {
        // Arrange
        var request = new FileCreationRequest();

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
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
        var result = request.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyAbbreviationAndTitle_ReturnsError()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "",
            Title = "",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = request.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("약어 또는 제목", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyExtension_ReturnsError()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "",
            OutputPath = _testDirectory
        };

        // Act
        var result = request.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("확장자", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EmptyOutputPath_ReturnsError()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = ""
        };

        // Act
        var result = request.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("출력 폴더", result.ErrorMessage);
    }

    [Fact]
    public void Validate_NonExistentOutputPath_ReturnsError()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = @"C:\NonExistentFolder"
        };

        // Act
        var result = request.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("존재하지 않습니다", result.ErrorMessage);
    }

    [Fact]
    public void Validate_NonExistentTemplatePath_ReturnsError()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = @"C:\NonExistentTemplate.txt"
        };

        // Act
        var result = request.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("템플릿 파일", result.ErrorMessage);
    }

    [Fact]
    public void Clone_CreatesCopy_ReturnsSeparateInstance()
    {
        // Arrange
        var original = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Suffix = "v1",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = ""
        };

        // Act
        var clone = original.Clone();

        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.Abbreviation, clone.Abbreviation);
        Assert.Equal(original.Title, clone.Title);
        Assert.Equal(original.Suffix, clone.Suffix);
        Assert.Equal(original.Extension, clone.Extension);
        Assert.Equal(original.OutputPath, clone.OutputPath);
        Assert.Equal(original.TemplatePath, clone.TemplatePath);
    }

    [Fact]
    public void ToString_ValidRequest_ReturnsFormattedString()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = ".txt"
        };

        // Act
        var result = request.ToString();

        // Assert
        Assert.Contains("CNF", result);
        Assert.Contains("Test File", result);
        Assert.Contains(".txt", result);
    }

    [Fact]
    public void GetFullPath_ValidRequest_ReturnsPath()
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
        var result = request.GetFullPath();

        // Assert
        Assert.NotEmpty(result);
        Assert.StartsWith(_testDirectory, result);
        Assert.EndsWith(".txt", result);
    }

    [Fact]
    public void IsValid_WithOnlyAbbreviation_ReturnsTrue()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithOnlyTitle_ReturnsTrue()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }
}