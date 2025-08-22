using CreateNewFile.Models;
using CreateNewFile.Services;
using Moq;

namespace CreateNewFile.Tests.Services;

public class FileGeneratorServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileGeneratorService _service;

    public FileGeneratorServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "CNF_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _service = new FileGeneratorService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task CreateFileAsync_ValidEmptyFile_CreatesFile()
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
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        Assert.NotNull(result.FileName);
        Assert.Contains("CNF", result.FileName);
        Assert.EndsWith(".txt", result.FileName);
    }

    [Fact]
    public async Task CreateFileAsync_TxtFileWithoutTemplate_CreatesWithSpaceContent()
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
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal(" ", content); // .txt 파일은 공백 하나 포함
    }

    [Fact]
    public async Task CreateFileAsync_WithTemplate_CopiesTemplateContent()
    {
        // Arrange
        var templateFile = Path.Combine(_testDirectory, "template.txt");
        var templateContent = "This is a template file content.";
        await File.WriteAllTextAsync(templateFile, templateContent);

        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = templateFile
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal(templateContent, content);
    }

    [Fact]
    public async Task CreateFileAsync_InvalidPath_ReturnsFailure()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = @"Z:\invalid\path"
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public async Task CreateFileAsync_NonExistentTemplate_ReturnsFailure()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = @"C:\nonexistent\template.txt"
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("템플릿", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateFileAsync_EmptyRequest_ReturnsFailure()
    {
        // Arrange
        var request = new FileCreationRequest();

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public async Task CreateFileAsync_FileAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var existingFile = Path.Combine(_testDirectory, "existing.txt");
        await File.WriteAllTextAsync(existingFile, "existing content");

        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // 첫 번째 파일 생성
        var firstResult = await _service.CreateFileAsync(request);
        
        // Act - 같은 이름으로 다시 생성 시도
        var secondResult = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);
        Assert.Contains("이미 존재", secondResult.ErrorMessage);
    }

    [Theory]
    [InlineData("docx")]
    [InlineData("pdf")]
    [InlineData("xlsx")]
    public async Task CreateFileAsync_NonTxtExtension_CreatesEmptyFile(string extension)
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test File",
            Extension = extension,
            OutputPath = _testDirectory
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        Assert.EndsWith($".{extension}", result.FileName);
        
        var fileInfo = new FileInfo(result.FilePath);
        Assert.Equal(0, fileInfo.Length); // 빈 파일
    }

    [Fact]
    public async Task CreateFileAsync_LongFileName_TruncatesCorrectly()
    {
        // Arrange
        var longTitle = new string('A', 200); // 매우 긴 제목
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = longTitle,
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var fileName = Path.GetFileName(result.FilePath);
        Assert.True(fileName.Length <= 255); // Windows 파일명 길이 제한
    }

    [Fact]
    public async Task CreateFileAsync_SpecialCharactersInTitle_CleansFileName()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test<>File|Name",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = await _service.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        Assert.DoesNotContain("<", result.FileName);
        Assert.DoesNotContain(">", result.FileName);
        Assert.DoesNotContain("|", result.FileName);
    }
}