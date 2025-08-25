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
        Assert.NotNull(item.Id);
        Assert.Equal(string.Empty, item.Value);
    }

    [Fact]
    public void Constructor_WithInitialization_SetsValues()
    {
        // Arrange
        var value = "CNF";

        // Act
        var item = new PresetItem { Value = value };

        // Assert
        Assert.Equal(value, item.Value);
        Assert.True(item.IsEnabled);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF" };
        var item2 = new PresetItem { Value = "CNF", Id = item1.Id };

        // Act & Assert
        Assert.Equal(item1, item2);
        Assert.True(item1.Equals(item2));
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF" };
        var item2 = new PresetItem { Value = "DOC" };

        // Act & Assert
        Assert.NotEqual(item1, item2);
        Assert.False(item1.Equals(item2));
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        // Arrange
        var item1 = new PresetItem { Value = "CNF" };
        var item2 = new PresetItem { Value = "CNF", Id = item1.Id };

        // Act
        var hash1 = item1.GetHashCode();
        var hash2 = item2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var item = new PresetItem { Value = "CNF" };

        // Act
        var result = item.ToString();

        // Assert
        Assert.Equal("CNF", result);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var item = new PresetItem();

        // Act
        item.Value = "TEST";
        item.IsEnabled = false;

        // Assert
        Assert.Equal("TEST", item.Value);
        Assert.False(item.IsEnabled);
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

    [Fact]
    public void Clone_CreatesNewInstance()
    {
        // Arrange
        var original = new PresetItem 
        { 
            Value = "CNF",
            IsEnabled = false
        };

        // Act
        var cloned = original.Clone();

        // Assert
        Assert.NotSame(original, cloned);
        Assert.Equal(original.Id, cloned.Id);
        Assert.Equal(original.Value, cloned.Value);
        Assert.Equal(original.IsEnabled, cloned.IsEnabled);
    }
}