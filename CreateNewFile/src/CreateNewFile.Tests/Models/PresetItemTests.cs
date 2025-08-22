using CreateNewFile.Models;

namespace CreateNewFile.Tests.Models;

public class PresetItemTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsCorrectDefaults()
    {
        // Act
        var item = new PresetItem();

        // Assert
        Assert.True(item.IsEnabled);
        Assert.False(item.IsFavorite);
        Assert.Equal(0, item.UsageCount);
        Assert.True(item.LastUsed > DateTime.MinValue);
    }

    [Fact]
    public void Constructor_WithInitialization_SetsValues()
    {
        // Arrange
        var value = "CNF";
        var description = "CreateNewFile";

        // Act
        var item = new PresetItem { Value = value, Description = description };

        // Assert
        Assert.Equal(value, item.Value);
        Assert.Equal(description, item.Description);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsFavorite);
    }

    [Fact]
    public void MarkAsUsed_IncrementsCountAndUpdatesTime()
    {
        // Arrange
        var item = new PresetItem { Value = "CNF", Description = "Test" };
        var initialCount = item.UsageCount;
        var beforeTime = DateTime.Now.AddSeconds(-1);

        // Act
        item.MarkAsUsed();
        var afterTime = DateTime.Now.AddSeconds(1);

        // Assert
        Assert.Equal(initialCount + 1, item.UsageCount);
        Assert.True(item.LastUsed >= beforeTime);
        Assert.True(item.LastUsed <= afterTime);
    }

    [Fact]
    public void MarkAsUsed_MultipleTimesCalls_IncrementsCorrectly()
    {
        // Arrange
        var item = new PresetItem { Value = "CNF", Description = "Test" };

        // Act
        item.MarkAsUsed();
        item.MarkAsUsed();
        item.MarkAsUsed();

        // Assert
        Assert.Equal(3, item.UsageCount);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF", Description = "Test" };
        var item2 = new PresetItem { Value = "CNF", Description = "Test", Id = item1.Id };

        // Act & Assert
        Assert.Equal(item1, item2);
        Assert.True(item1.Equals(item2));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF", Description = "Test" };
        var item2 = new PresetItem { Value = "DOC", Description = "Document" };

        // Act & Assert
        Assert.NotEqual(item1, item2);
        Assert.False(item1.Equals(item2));
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF", Description = "Test" };
        var item2 = new PresetItem { Value = "CNF", Description = "Test", Id = item1.Id };

        // Act
        var hash1 = item1.GetHashCode();
        var hash2 = item2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsValueAndDescription()
    {
        // Arrange
        var item = new PresetItem { Value = "CNF", Description = "CreateNewFile" };

        // Act
        var result = item.ToString();

        // Assert
        Assert.Contains("CNF", result);
        Assert.Contains("CreateNewFile", result);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var item = new PresetItem();

        // Act
        item.Value = "TEST";
        item.Description = "Test Description";
        item.IsEnabled = false;
        item.IsFavorite = true;

        // Assert
        Assert.Equal("TEST", item.Value);
        Assert.Equal("Test Description", item.Description);
        Assert.False(item.IsEnabled);
        Assert.True(item.IsFavorite);
    }

    [Fact]
    public void IsValid_WithValue_ReturnsTrue()
    {
        // Arrange
        var item = new PresetItem { Value = "CNF" };

        // Act
        var result = item.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithoutValue_ReturnsFalse()
    {
        // Arrange
        var item = new PresetItem { Value = "" };

        // Act
        var result = item.IsValid();

        // Assert
        Assert.False(result);
    }
}