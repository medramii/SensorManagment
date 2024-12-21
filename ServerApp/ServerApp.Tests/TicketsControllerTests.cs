using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using ServerApp.Contracts.Logging;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Models;
using ServerApp.API.Extensions;
using ServerApp.API.Controllers.v2;

namespace ServerApp.Tests;

public class TicketsControllerTests
{
    private readonly ITicketRepository _fakeTicketRepo;
    private readonly ILoggerManager _fakeLogger;
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        // FakeItEasy fakes for the repository and logger
        _fakeTicketRepo = A.Fake<ITicketRepository>();
        _fakeLogger = A.Fake<ILoggerManager>();

        // Instantiate the controller
        _controller = new TicketsController(_fakeTicketRepo, _fakeLogger);
    }

    [Fact]
    public async Task GetTickets_ReturnsOk_WithTickets()
    {
        // Arrange
        var fakeTickets = new List<Ticket>
        {
            new Ticket { Id = 1, Description = "Test Ticket 1", Status = (byte?)NomenclatureExtensions.TicketStatus.Open, CreatedAt = System.DateTime.Now },
            new Ticket { Id = 2, Description = "Test Ticket 2", Status = (byte?)NomenclatureExtensions.TicketStatus.Closed, CreatedAt = System.DateTime.Now }
        };

        A.CallTo(() => _fakeTicketRepo.GetAllAsync()).Returns(Task.FromResult((IEnumerable<Ticket>)fakeTickets));

        // Act
        var result = await _controller.GetTickets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tickets = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, tickets.Count());
        A.CallTo(() => _fakeLogger.LogInfo(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task GetTickets_ReturnsNoContent_WhenNoTicketsExist()
    {
        // Arrange
        A.CallTo(() => _fakeTicketRepo.GetAllAsync()).Returns(Task.FromResult(Enumerable.Empty<Ticket>()));

        // Act
        var result = await _controller.GetTickets();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
        A.CallTo(() => _fakeLogger.LogWarn(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task GetTicket_ReturnsOk_WithTicket()
    {
        // Arrange
        var fakeTicket = new Ticket { Id = 1, Description = "Test Ticket", Status = (byte?)NomenclatureExtensions.TicketStatus.Open, CreatedAt = System.DateTime.Now };
        A.CallTo(() => _fakeTicketRepo.FindAsync(1)).Returns(Task.FromResult(fakeTicket));

        // Act
        var result = await _controller.GetTicket(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var ticket = Assert.IsAssignableFrom<Ticket>(okResult.Value);
        Assert.Equal(1, ticket.Id);
        A.CallTo(() => _fakeLogger.LogInfo(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task GetTicket_ReturnsNotFound_WhenTicketDoesNotExist()
    {
        // Arrange
        A.CallTo(() => _fakeTicketRepo.FindAsync(1)).Returns(Task.FromResult<Ticket>(null));

        // Act
        var result = await _controller.GetTicket(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        A.CallTo(() => _fakeLogger.LogWarn(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task PutTicket_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        var fakeTicket = new Ticket { Id = 1, Description = "Updated Ticket", Status = (byte?)NomenclatureExtensions.TicketStatus.Open, CreatedAt = System.DateTime.Now };
        A.CallTo(() => _fakeTicketRepo.UpdateAsync(fakeTicket)).Returns(Task.CompletedTask);
        A.CallTo(() => _fakeTicketRepo.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PutTicket(1, fakeTicket);

        // Assert
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _fakeLogger.LogInfo(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task PostTicket_ReturnsCreatedAtAction_OnSuccess()
    {
        // Arrange
        var fakeTicket = new Ticket { Id = 1, Description = "New Ticket", Status = (byte?)NomenclatureExtensions.TicketStatus.Open, CreatedAt = System.DateTime.Now };
        A.CallTo(() => _fakeTicketRepo.CreateAsync(fakeTicket)).Returns(Task.CompletedTask);
        A.CallTo(() => _fakeTicketRepo.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PostTicket(fakeTicket);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var ticket = Assert.IsAssignableFrom<Ticket>(createdAtActionResult.Value);
        Assert.Equal(1, ticket.Id);
        A.CallTo(() => _fakeLogger.LogInfo(A<string>.Ignored)).MustHaveHappened();
    }

    [Fact]
    public async Task DeleteTicket_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        var fakeTicket = new Ticket { Id = 1, Description = "Delete Ticket", Status = (byte?)NomenclatureExtensions.TicketStatus.Closed, CreatedAt = System.DateTime.Now };
        A.CallTo(() => _fakeTicketRepo.FindAsync(1)).Returns(Task.FromResult(fakeTicket));
        A.CallTo(() => _fakeTicketRepo.DeleteAsync(fakeTicket)).Returns(Task.CompletedTask);
        A.CallTo(() => _fakeTicketRepo.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteTicket(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _fakeLogger.LogInfo(A<string>.Ignored)).MustHaveHappened();
    }
}
