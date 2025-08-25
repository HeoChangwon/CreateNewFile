using CreateNewFile.Models;
using CreateNewFile.Services;
using CreateNewFile.Utils;

namespace CreateNewFile.Tests.AcceptanceTests;

/// <summary>
/// 사용자 수용 테스트 - 실제 사용 시나리오 기반 테스트
/// </summary>
public class UserAcceptanceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IFileGeneratorService _fileGeneratorService;
    private readonly ISettingsService _settingsService;

    public UserAcceptanceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "CNF_UAT", Guid.NewGuid().ToString());
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
    public async Task UserScenario_CreateDocumentWithTemplate_EndToEnd()
    {
        // User Story: 사용자가 템플릿을 사용하여 문서를 생성하고 싶어한다.
        
        // 1. 템플릿 파일 준비
        var templatePath = Path.Combine(_testDirectory, "document_template.txt");
        await File.WriteAllTextAsync(templatePath, "# 문서 제목\n\n내용을 여기에 작성하세요...");

        // 2. 파일 생성 요청 준비
        var request = new FileCreationRequest
        {
            Abbreviation = "DOC",
            Title = "회의록",
            Suffix = "2025년8월",
            Extension = "md",
            OutputPath = _testDirectory,
            TemplatePath = templatePath
        };

        // 3. 파일 생성 실행
        var startTime = DateTime.Now;
        var result = await _fileGeneratorService.CreateFileAsync(request);
        var endTime = DateTime.Now;

        // 4. 결과 검증
        Assert.True(result.Success, "파일 생성이 실패했습니다: " + result.ErrorMessage);
        Assert.True(File.Exists(result.FilePath), "생성된 파일이 존재하지 않습니다.");
        
        // 5. 성능 검증 (1초 이내)
        var duration = endTime - startTime;
        Assert.True(duration.TotalSeconds < 1, $"파일 생성 시간이 너무 깁니다: {duration.TotalSeconds}초");

        // 6. 파일 내용 검증
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Contains("# 문서 제목", content);

        // 7. 파일명 형식 검증
        Assert.Matches(@"\d{8}_\d{4}_DOC_회의록_2025년8월\.md", result.FileName);
    }

    [Fact]
    public async Task UserScenario_QuickFileCreation_WithoutTemplate()
    {
        // User Story: 사용자가 빠르게 빈 파일을 생성하고 싶어한다.
        
        // 1. 간단한 파일 생성 요청
        var request = new FileCreationRequest
        {
            Abbreviation = "TMP",
            Title = "임시파일",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // 2. 파일 생성 실행
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // 3. 결과 검증
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        // 4. txt 파일은 공백 한 개 포함해야 함
        var content = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal(" ", content);
    }

    [Fact]
    public async Task UserScenario_MultipleFilesCreation_Performance()
    {
        // User Story: 사용자가 여러 파일을 연속으로 생성하고 싶어한다.
        
        var tasks = new List<Task<FileCreationResult>>();
        var startTime = DateTime.Now;

        // 1. 5개의 파일을 동시에 생성
        for (int i = 1; i <= 5; i++)
        {
            var request = new FileCreationRequest
            {
                Abbreviation = "BATCH",
                Title = $"배치파일{i}",
                Extension = "txt",
                OutputPath = _testDirectory
            };
            
            tasks.Add(_fileGeneratorService.CreateFileAsync(request));
        }

        // 2. 모든 작업 완료 대기
        var results = await Task.WhenAll(tasks);
        var endTime = DateTime.Now;

        // 3. 결과 검증
        Assert.All(results, result => Assert.True(result.Success));
        Assert.All(results, result => Assert.True(File.Exists(result.FilePath)));

        // 4. 성능 검증 (5초 이내)
        var totalDuration = endTime - startTime;
        Assert.True(totalDuration.TotalSeconds < 5, $"전체 처리 시간이 너무 깁니다: {totalDuration.TotalSeconds}초");
    }

    [Fact]
    public async Task UserScenario_ErrorHandling_InvalidPath()
    {
        // User Story: 사용자가 잘못된 경로를 입력했을 때 적절한 오류 메시지를 받고 싶어한다.
        
        // 1. 잘못된 경로로 파일 생성 시도
        var request = new FileCreationRequest
        {
            Abbreviation = "ERR",
            Title = "오류테스트",
            Extension = "txt",
            OutputPath = @"Z:\InvalidPath\DoesNotExist"
        };

        // 2. 파일 생성 실행
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // 3. 적절한 실패 처리 검증
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
        Assert.Contains("경로", result.ErrorMessage);
    }

    [Fact]
    public async Task UserScenario_SpecialCharacters_FileNaming()
    {
        // User Story: 사용자가 특수문자가 포함된 제목으로 파일을 생성하려고 한다.
        
        // 1. 특수문자 포함 파일 생성
        var request = new FileCreationRequest
        {
            Abbreviation = "SPEC",
            Title = "특수문자<>|?*테스트:파일\"이름",
            Extension = "txt",
            OutputPath = _testDirectory
        };

        // 2. 파일 생성 실행
        var result = await _fileGeneratorService.CreateFileAsync(request);

        // 3. 결과 검증
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        // 4. 특수문자가 제거/치환되었는지 확인
        Assert.DoesNotContain("<", result.FileName);
        Assert.DoesNotContain(">", result.FileName);
        Assert.DoesNotContain("|", result.FileName);
        Assert.DoesNotContain("?", result.FileName);
        Assert.DoesNotContain("*", result.FileName);
        Assert.DoesNotContain(":", result.FileName);
        Assert.DoesNotContain("\"", result.FileName);
    }

    [Fact]
    public async Task UserScenario_LargeFileHandling_Template()
    {
        // User Story: 사용자가 큰 템플릿 파일을 사용하여 파일을 생성하고 싶어한다.
        
        // 1. 큰 템플릿 파일 생성 (약 1MB)
        var largeContent = string.Join("\n", Enumerable.Repeat("이것은 테스트 내용입니다. ", 10000));
        var templatePath = Path.Combine(_testDirectory, "large_template.txt");
        await File.WriteAllTextAsync(templatePath, largeContent);

        // 2. 파일 생성 요청
        var request = new FileCreationRequest
        {
            Abbreviation = "LARGE",
            Title = "대용량테스트",
            Extension = "txt",
            OutputPath = _testDirectory,
            TemplatePath = templatePath
        };

        // 3. 파일 생성 실행
        var startTime = DateTime.Now;
        var result = await _fileGeneratorService.CreateFileAsync(request);
        var endTime = DateTime.Now;

        // 4. 결과 검증
        Assert.True(result.Success);
        Assert.True(File.Exists(result.FilePath));
        
        // 5. 내용 크기 검증
        var resultContent = await File.ReadAllTextAsync(result.FilePath);
        Assert.Equal(largeContent, resultContent);
        
        // 6. 성능 검증 (3초 이내)
        var duration = endTime - startTime;
        Assert.True(duration.TotalSeconds < 3, $"대용량 파일 처리 시간이 너무 깁니다: {duration.TotalSeconds}초");
    }

    [Fact]
    public async Task UserScenario_MemoryUsage_MultipleOperations()
    {
        // User Story: 메모리 사용량이 적절한 범위 내에서 유지되어야 한다.
        
        var initialMemory = GC.GetTotalMemory(true);
        
        // 1. 여러 번의 파일 생성 작업 수행
        for (int i = 0; i < 20; i++)
        {
            var request = new FileCreationRequest
            {
                Abbreviation = "MEM",
                Title = $"메모리테스트{i}",
                Extension = "txt",
                OutputPath = _testDirectory
            };
            
            var result = await _fileGeneratorService.CreateFileAsync(request);
            Assert.True(result.Success);
        }

        // 2. 가비지 컬렉션 수행
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;

        // 3. 메모리 사용량 검증 (10MB 이하)
        Assert.True(memoryIncrease < 10 * 1024 * 1024, 
            $"메모리 사용량이 너무 많습니다: {memoryIncrease / 1024 / 1024}MB");
    }

    [Fact]
    public void UserScenario_ApplicationResponsiveness_StartupTime()
    {
        // User Story: 애플리케이션이 빠르게 시작되어야 한다.
        
        // 1. 서비스 초기화 시간 측정
        var startTime = DateTime.Now;
        
        var fileService = new FileGeneratorService();
        var settingsService = new SettingsService();
        var validationResult = ValidationHelper.ValidateFileName("test.txt");
        
        var endTime = DateTime.Now;

        // 2. 응답 시간 검증 (1초 이내)
        var initTime = endTime - startTime;
        Assert.True(initTime.TotalSeconds < 1, 
            $"서비스 초기화 시간이 너무 깁니다: {initTime.TotalSeconds}초");
    }

    [Fact]
    public async Task UserScenario_DataPersistence_SettingsReload()
    {
        // User Story: 사용자 설정이 영구적으로 저장되고 로드되어야 한다.
        
        // 1. 설정 생성 및 저장
        var settings = new AppSettings();
        settings.Abbreviations.Add(new PresetItem { Value = "UAT" });
        
        var saveResult = await _settingsService.SaveSettingsAsync(settings);
        Assert.True(saveResult);

        // 2. 새로운 서비스 인스턴스로 설정 로드
        var newSettingsService = new SettingsService();
        var loadedSettings = await newSettingsService.LoadSettingsAsync();

        // 3. 데이터 일관성 검증
        Assert.NotNull(loadedSettings);
        Assert.NotEmpty(loadedSettings.Abbreviations);
    }
}