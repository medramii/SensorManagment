using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using ServerApp.Contracts.Logging;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Data;
using ServerApp.Domain.Models;
using ServerApp.API.Controllers.v1;

using Xunit;

namespace ServerApp.Tests.Controllers
{
    public class CapteursControllerTests
    {
        private readonly ICapteurRepository<SqlServerDbContext> _fakeCapteurRepo;
        private readonly ILoggerManager _fakeLogger;
        private readonly CapteursController _controller;

        public CapteursControllerTests()
        {
            _fakeCapteurRepo = A.Fake<ICapteurRepository<SqlServerDbContext>>();
            _fakeLogger = A.Fake<ILoggerManager>();
            _controller = new CapteursController(_fakeCapteurRepo, _fakeLogger);
        }

        [Fact]
        public async Task GetCapteurs_WhenCapteursExist_ReturnsOkResult()
        {
            // Arrange
            var capteurs = new List<Capteur>
            {
                new Capteur { Id = 1, Label = "Capteur 1", Type = "Type1", Active = true },
                new Capteur { Id = 2, Label = "Capteur 2", Type = "Type2", Active = true }
            };

            A.CallTo(() => _fakeCapteurRepo.GetAllAsync())
             .Returns(capteurs);

            // Act
            var result = await _controller.GetCapteurs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCapteurs = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, returnedCapteurs.Count());

            A.CallTo(() => _fakeLogger.LogInfo(A<string>.That.Contains("Successfully retrieved 2 capteurs")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetCapteurs_WhenNoCapteurs_ReturnsNoContent()
        {
            // Arrange
            A.CallTo(() => _fakeCapteurRepo.GetAllAsync())
             .Returns(new List<Capteur>());

            // Act
            var result = await _controller.GetCapteurs();

            // Assert
            Assert.IsType<NoContentResult>(result.Result);

            A.CallTo(() => _fakeLogger.LogWarn(A<string>.That.Contains("No capteurs found")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetCapteur_WhenCapteurExists_ReturnsOkResult()
        {
            // Arrange
            var capteur = new Capteur { Id = 1, Label = "Test Capteur", Type = "Type1", Active = true };

            A.CallTo(() => _fakeCapteurRepo.FindAsync(1))
             .Returns(capteur);

            // Act
            var result = await _controller.GetCapteur(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCapteur = Assert.IsType<Capteur>(okResult.Value);
            Assert.Equal(1, returnedCapteur.Id);

            A.CallTo(() => _fakeLogger.LogInfo(A<string>.That.Contains("Successfully retrieved capteur with ID 1")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetCapteur_WhenCapteurNotFound_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeCapteurRepo.FindAsync(1))
             .Returns((Capteur)null);

            // Act
            var result = await _controller.GetCapteur(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Capteur with ID 1 not found.", notFoundResult.Value);

            A.CallTo(() => _fakeLogger.LogWarn(A<string>.That.Contains("Capteur with ID 1 not found")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PutCapteur_WhenValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var capteur = new Capteur { Id = 1, Label = "Updated Capteur" };

            A.CallTo(() => _fakeCapteurRepo.UpdateAsync(capteur))
             .Returns(Task.CompletedTask);
            A.CallTo(() => _fakeCapteurRepo.SaveAsync())
             .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutCapteur(1, capteur);

            // Assert
            Assert.IsType<NoContentResult>(result);

            A.CallTo(() => _fakeLogger.LogInfo(A<string>.That.Contains("Successfully updated capteur with ID 1")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PostCapteur_WhenValidCapteur_ReturnsCreatedResult()
        {
            // Arrange
            var capteur = new Capteur { Label = "New Capteur", Type = "Type1" };

            A.CallTo(() => _fakeCapteurRepo.CreateAsync(A<Capteur>._))
             .Returns(Task.CompletedTask);
            A.CallTo(() => _fakeCapteurRepo.SaveAsync())
             .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostCapteur(capteur);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCapteur = Assert.IsType<Capteur>(createdResult.Value);
            Assert.True(returnedCapteur.Active);
            Assert.NotEqual(default, returnedCapteur.CreatedAt);

            A.CallTo(() => _fakeLogger.LogInfo(A<string>.That.Contains("Successfully created capteur")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DeleteCapteur_WhenCapteurExists_ReturnsNoContent()
        {
            // Arrange
            var capteur = new Capteur { Id = 1, Label = "Test Capteur" };

            A.CallTo(() => _fakeCapteurRepo.FindAsync(1))
             .Returns(capteur);
            A.CallTo(() => _fakeCapteurRepo.DeleteAsync(capteur))
             .Returns(Task.CompletedTask);
            A.CallTo(() => _fakeCapteurRepo.SaveAsync())
             .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCapteur(1);

            // Assert
            Assert.IsType<NoContentResult>(result);

            A.CallTo(() => _fakeLogger.LogInfo(A<string>.That.Contains("Successfully deleted capteur with ID 1")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DeleteCapteur_WhenCapteurNotFound_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeCapteurRepo.FindAsync(1))
             .Returns((Capteur)null);

            // Act
            var result = await _controller.DeleteCapteur(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Capteur with ID 1 not found.", notFoundResult.Value);

            A.CallTo(() => _fakeLogger.LogWarn(A<string>.That.Contains("Capteur with ID 1 not found")))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PutCapteur_WhenIdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var capteur = new Capteur { Id = 2, Label = "Mismatched Capteur" };

            // Act
            var result = await _controller.PutCapteur(1, capteur);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Capteur ID mismatch.", badRequestResult.Value);

            A.CallTo(() => _fakeLogger.LogWarn(A<string>.That.Contains("Mismatched ID")))
             .MustHaveHappenedOnceExactly();
        }
    }
}
