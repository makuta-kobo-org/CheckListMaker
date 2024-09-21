using Microsoft.Extensions.Configuration;
using CheckListMaker.Models;
using CheckListMaker.Services;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit.Abstractions;
using System.Reflection;

namespace CheckListMakerTest.Tests.MauiApps.Services;

public class ComputerVisionServiceTest
{
    private readonly ITestOutputHelper _output;
    private readonly ComputerVisionService _service;

    /// <summary> Setup </summary>
    public ComputerVisionServiceTest(ITestOutputHelper output)
    {
        _output = output;

        using var appsettings = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("CheckListMakerTest.Tests.MauiApps.appsettings.Test.json");

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonStream(appsettings)
            .Build();

        _service = ComputerVisionService.GetInstance(config);
    }

    [Fact]
    public void ExtractCheckItems_ShouldReturnCheckItems_WhenAnalyzeResultsContainsLines()
    {
        // Arrange
        var expect1 = "·Test item 1";
        var expect2 = "Test item 2";

        object[] parameters = {
            new AnalyzeResults
            {
                ReadResults =
                    [
                        new ReadResult
                        {
                            Lines =
                            [
                                new Line { Text = expect1 },
                                new Line { Text = expect2 }
                            ]
                        }
                    ]
            }
        };

        var methodInfo = typeof(ComputerVisionService)
            .GetMethod("ExtractCheckItems", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = methodInfo.Invoke(_service, parameters) as CheckItems;

        // Assert
        result.IsNotNull();
        result.Items.Count.Is(2);
        result.Items[0].ItemText.Is("Test item 1");
        result.Items[1].ItemText.Is("Test item 2");
    }

    [Fact]
    public void ExtractCheckItems_ShouldHandleEmptyLinesGracefully()
    {
        // Arrange
        object[] parameters = {
            new AnalyzeResults
            {
                ReadResults =
                [
                    new ReadResult
                    {
                        Lines = []
                    }
                ]
            }
        };

        var methodInfo = typeof(ComputerVisionService)
            .GetMethod("ExtractCheckItems", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = methodInfo.Invoke(_service, parameters) as CheckItems;

        // Assert
        result.IsNotNull();
        result.Items.Count.Is(0);
    }
}
