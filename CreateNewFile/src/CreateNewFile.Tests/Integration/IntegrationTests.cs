using CreateNewFile.Models;
using CreateNewFile.Services;
using CreateNewFile.Utils;

namespace CreateNewFile.Tests.Integration;

public class IntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IFileGeneratorService _fileGeneratorService;
    private readonly ISettingsService _settingsService;

    public IntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "CNF_Integration_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _fileGeneratorService = new FileGeneratorService();
        _settingsService = new SettingsService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task FullWorkflow_CreateFileWithTemplate_Success()
    {
        // Arrange
        var templateFile = Path.Combine(_testDirectory, "template.txt");
        await File.WriteAllTextAsync(templateFile, "Template Content for Testing");

        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Integration Test",
            Suffix = "v1",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = templateFile
        };

        // Act
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal("Template Content for Testing", content);
        
        // Verify file name format
        Assert.Contains("CNF", result.FileName);
        Assert.Contains("Integration_Test", result.FileName);
        Assert.Contains("v1", result.FileName);
        Assert.EndsWith(".txt", result.FileName);
    }

    [Fact]
    public async Task FullWorkflow_CreateEmptyFile_Success()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Empty File Test",
            Extension = "md",
            OutputPath = _testDirectory
        };

        // Act
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var fileInfo = new FileInfo(result.FilePath);
        Assert.Equal(0, fileInfo.Length); // Empty file
        
        Assert.Contains("CNF", result.FileName);
        Assert.Contains("Empty_File_Test", result.FileName);
        Assert.EndsWith(".md", result.FileName);
    }

    [Fact]
    public async Task FullWorkflow_CreateTxtFileWithoutTemplate_ContainsSpace()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "TXT Test",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal(" ", content); // .txt files should contain a space
    }

    [Fact]
    public async Task SettingsIntegration_SaveAndLoadPresets_Success()
    {
        // Arrange
        var settings = new AppSettings();
        settings.Abbreviations.Add(new PresetItem { Value = "CNF", Description = "CreateNewFile" });
        settings.Titles.Add(new PresetItem { Value = "Test Document", Description = "Test description" });
        settings.Extensions.Add(new PresetItem { Value = "txt", Description = "Text file" });

        // Act - Save (uses default file path)
        var saveResult = await _settingsService.SaveSettingsAsync(settings);
        
        // Act - Load (uses default file path)
        var loadedSettings = await _settingsService.LoadSettingsAsync();

        // Assert
        Assert.True(saveResult);
        Assert.NotNull(loadedSettings);
        Assert.NotEmpty(loadedSettings.Abbreviations);
        Assert.NotEmpty(loadedSettings.Titles);
        Assert.NotEmpty(loadedSettings.Extensions);
    }

    [Fact]
    public async Task ValidationIntegration_EndToEndValidation_Success()
    {
        // Arrange
        var validRequest = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Validation Test",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act - Validate using FileNameBuilder
        var validationResult = FileNameBuilder.ValidateRequest(validRequest);
        
        // Act - Create file if validation passes
        FileCreationResult? fileResult = null;
        if (validationResult.IsValid)
        {
            fileResult = await _fileGeneratorService.CreateFileAsync(validRequest);
        }

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(fileResult);
        Assert.True(fileResult.Success);
        Assert.True(File.Exists(fileResult.FilePath));
    }

    [Fact]
    public async Task ValidationIntegration_InvalidRequest_FailsGracefully()
    {
        // Arrange
        var invalidRequest = new FileCreationRequest
        {
            Abbreviation = "",
            Title = "",
            Extension = "",
            OutputPath = @"Z:\InvalidPath"
        };

        // Act - Validate using FileNameBuilder
        var validationResult = FileNameBuilder.ValidateRequest(invalidRequest);
        
        // Act - Attempt to create file
        var fileResult = await _fileGeneratorService.CreateFileAsync(invalidRequest);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.False(fileResult.Success);
        Assert.NotEmpty(validationResult.Errors);
        Assert.NotEmpty(fileResult.ErrorMessage);
    }

    [Fact]
    public void PresetUsageTracking_Integration_Success()
    {
        // Arrange
        var preset = new PresetItem { Value = "CNF", Description = "CreateNewFile" };
        Assert.Equal(0, preset.UsageCount);

        // Act - Simulate usage
        preset.MarkAsUsed();
        preset.MarkAsUsed();

        // Assert
        Assert.Equal(2, preset.UsageCount);
        Assert.True(preset.LastUsed > DateTime.Now.AddMinutes(-1));
    }

    [Fact]
    public async Task FileNameGeneration_SpecialCharacters_CleanedCorrectly()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Test<>File|With*Special?Characters",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        // Verify special characters are removed
        Assert.DoesNotContain("<", result.FileName);
        Assert.DoesNotContain(">", result.FileName);
        Assert.DoesNotContain("|", result.FileName);
        Assert.DoesNotContain("*", result.FileName);
        Assert.DoesNotContain("?", result.FileName);
    }

    [Fact]
    public async Task MultipleFileCreation_SameRequest_NoDuplicates()
    {
        // Arrange
        var request = new FileCreationRequest
        {
            Abbreviation = "CNF",
            Title = "Duplicate Test",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // Act - Create first file
        var result1 = await _fileGeneratorService.CreateFileAsync(request);
        
        // Act - Try to create same file again
        var result2 = await _fileGeneratorService.CreateFileAsync(request);

        // Assert
        Assert.True(result1.Success);
        Assert.False(result2.Success); // Should fail due to existing file
        Assert.Contains("이미 존재", result2.ErrorMessage);
    }

    [Fact]
    public void PathValidation_Integration_WorksCorrectly()
    {
        // Arrange & Act
        var validPath = FileNameBuilder.IsValidPath(_testDirectory);
        var invalidPath = FileNameBuilder.IsValidPath(@"Z:\NonExistent\Path");
        var invalidCharsPath = FileNameBuilder.IsValidPath(@"C:\Invalid<>Path");

        // Assert
        Assert.True(validPath);
        Assert.False(invalidPath);
        Assert.False(invalidCharsPath);
    }
}