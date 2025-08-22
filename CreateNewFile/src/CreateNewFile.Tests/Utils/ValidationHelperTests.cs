using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Tests.Utils;

public class ValidationHelperTests : IDisposable
{
    private readonly string _testDirectory;

    public ValidationHelperTests()
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

    [Theory]
    [InlineData("test.txt", true)]
    [InlineData("my_file.doc", true)]
    [InlineData("file-name.pdf", true)]
    [InlineData("", false)]
    [InlineData("con.txt", false)] // Windows 예약어
    [InlineData("file<name>.txt", false)] // 잘못된 문자
    public void ValidateFileName_VariousInputs_ReturnsExpected(string fileName, bool expectedValid)
    {
        // Act
        var result = ValidationHelper.ValidateFileName(fileName);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void CleanFileName_InvalidCharacters_RemovesCharacters()
    {
        // Arrange
        var input = "Test<>File|Name";

        // Act
        var result = ValidationHelper.CleanFileName(input);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.DoesNotContain("|", result);
    }

    [Theory]
    [InlineData("txt", true)]
    [InlineData(".txt", true)]
    [InlineData("docx", true)]
    [InlineData("", false)]
    public void ValidateFileExtension_VariousInputs_ReturnsExpected(string extension, bool expectedValid)
    {
        // Act
        var result = ValidationHelper.ValidateFileExtension(extension);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void ValidateFileCreationRequest_ValidRequest_ReturnsSuccess()
    {
        // Act
        var result = ValidationHelper.ValidateFileCreationRequest(
            "CNF", 
            "Test File", 
            "v1", 
            "txt", 
            _testDirectory, 
            "");

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessages);
    }

    [Fact]
    public void ValidateFileCreationRequest_InvalidRequest_ReturnsErrors()
    {
        // Act
        var result = ValidationHelper.ValidateFileCreationRequest(
            "", // 빈 약어
            "", // 빈 제목
            "", 
            "", // 빈 확장자
            @"Z:\invalid", // 잘못된 경로
            "");

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.ErrorMessages);
    }

    [Fact]
    public void ValidateWritePermission_ValidPath_ReturnsSuccess()
    {
        // Act
        var result = ValidationHelper.ValidateWritePermission(_testDirectory);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateWritePermission_InvalidPath_ReturnsError()
    {
        // Arrange
        var invalidPath = @"Z:\invalid\path";

        // Act
        var result = ValidationHelper.ValidateWritePermission(invalidPath);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateFolderPath_ValidDirectory_ReturnsSuccess()
    {
        // Act
        var result = ValidationHelper.ValidateFolderPath(_testDirectory);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(@"Z:\nonexistent")]
    public void ValidateFolderPath_InvalidPath_ReturnsError(string path)
    {
        // Act
        var result = ValidationHelper.ValidateFolderPath(path);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateFilePath_ExistingFile_ReturnsSuccess()
    {
        // Arrange
        var tempFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(tempFile, "test content");

        try
        {
            // Act
            var result = ValidationHelper.ValidateFilePath(tempFile);

            // Assert
            Assert.True(result.IsValid);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateFilePath_NonExistentFile_ReturnsError()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = ValidationHelper.ValidateFilePath(nonExistentFile);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateTextInput_ValidInput_ReturnsSuccess()
    {
        // Act
        var result = ValidationHelper.ValidateTextInput("Valid Text", "TestField");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateTextInput_EmptyRequiredInput_ReturnsError()
    {
        // Act
        var result = ValidationHelper.ValidateTextInput("", "TestField", isRequired: true);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("TestField", result.ErrorMessages.FirstOrDefault() ?? "");
    }

    [Fact]
    public void ValidateFolderExists_ExistingFolder_ReturnsSuccess()
    {
        // Act
        var result = ValidationHelper.ValidateFolderExists(_testDirectory);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFileExists_ExistingFile_ReturnsSuccess()
    {
        // Arrange
        var tempFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(tempFile, "test content");

        try
        {
            // Act
            var result = ValidationHelper.ValidateFileExists(tempFile);

            // Assert
            Assert.True(result.IsValid);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}