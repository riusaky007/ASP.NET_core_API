using FluentAssertions;
using FluentAssertions.Specialized;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;
using OpenApiNSwagEndToEndTests.ClientIdGenerationModes.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ClientIdGenerationModes;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagEndToEndTests.ClientIdGenerationModes;

public sealed class ClientIdGenerationModesTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> _testContext;
    private readonly ClientIdGenerationFakers _fakers = new();

    public ClientIdGenerationModesTests(IntegrationTestContext<OpenApiStartup<ClientIdGenerationDbContext>, ClientIdGenerationDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PlayersController>();
        testContext.UseController<GamesController>();
        testContext.UseController<PlayerGroupsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_without_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        Func<Task<PlayerPrimaryResponseDocument?>> action = () => ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, requestBody));

        // Assert
        ExceptionAssertions<JsonSerializationException> assertion = await action.Should().ThrowExactlyAsync<JsonSerializationException>();
        assertion.Which.Message.Should().Be("Cannot write a null value for property 'id'. Property requires a value. Path 'data'.");
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_required()
    {
        // Arrange
        Player newPlayer = _fakers.Player.Generate();
        newPlayer.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerPostRequestDocument
        {
            Data = new PlayerDataInPostRequest
            {
                Id = newPlayer.StringId!,
                Attributes = new PlayerAttributesInPostRequest
                {
                    UserName = newPlayer.UserName
                }
            }
        };

        // Act
        PlayerPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerAsync(null, requestBody));

        // Assert
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Player playerInDatabase = await dbContext.Players.FirstWithIdAsync(newPlayer.Id);

            playerInDatabase.UserName.Should().Be(newPlayer.UserName);
        });
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game newGame = _fakers.Game.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, requestBody));

        // Assert
        document.ShouldNotBeNull();
        document.Data.Id.ShouldNotBeNullOrEmpty();

        Guid newGameId = Guid.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(newGameId);

            gameInDatabase.Title.Should().Be(newGame.Title);
            gameInDatabase.PurchasePrice.Should().Be(newGame.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ID_when_supplying_ID_is_allowed()
    {
        // Arrange
        Game newGame = _fakers.Game.Generate();
        newGame.Id = Guid.NewGuid();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new GamePostRequestDocument
        {
            Data = new GameDataInPostRequest
            {
                Id = newGame.StringId!,
                Attributes = new GameAttributesInPostRequest
                {
                    Title = newGame.Title,
                    PurchasePrice = (double)newGame.PurchasePrice
                }
            }
        };

        // Act
        GamePrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostGameAsync(null, requestBody));

        // Assert
        document.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Game gameInDatabase = await dbContext.Games.FirstWithIdAsync(newGame.Id);

            gameInDatabase.Title.Should().Be(newGame.Title);
            gameInDatabase.PurchasePrice.Should().Be(newGame.PurchasePrice);
        });
    }

    [Fact]
    public async Task Can_create_resource_without_ID_when_supplying_ID_is_forbidden()
    {
        // Arrange
        PlayerGroup newPlayerGroup = _fakers.Group.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        ClientIdGenerationModesClient apiClient = new(httpClient);

        var requestBody = new PlayerGroupPostRequestDocument
        {
            Data = new PlayerGroupDataInPostRequest
            {
                Attributes = new PlayerGroupAttributesInPostRequest
                {
                    Name = newPlayerGroup.Name
                }
            }
        };

        // Act
        PlayerGroupPrimaryResponseDocument? document = await ApiResponse.TranslateAsync(() => apiClient.PostPlayerGroupAsync(null, requestBody));

        // Assert
        document.ShouldNotBeNull();
        document.Data.Id.ShouldNotBeNullOrEmpty();

        long newPlayerGroupId = long.Parse(document.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PlayerGroup playerGroupInDatabase = await dbContext.PlayerGroups.FirstWithIdAsync(newPlayerGroupId);

            playerGroupInDatabase.Name.Should().Be(newPlayerGroup.Name);
        });
    }
}
