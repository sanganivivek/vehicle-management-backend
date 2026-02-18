using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vehicle_management_backend.Application.Services.Implementations;
using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace TestProject1
{
    public class VehicleServiceTests
    {
        // 1. Define Mocks and the Service
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly VehicleService _vehicleService;

        public VehicleServiceTests()
        {
            // 2. Initialize the Mocks
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            // 3. Inject BOTH Mock objects into the Service Constructor
            _vehicleService = new VehicleService(_vehicleRepositoryMock.Object, _activityLogServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfVehicles()
        {
            // --- ARRANGE (Setup data) ---
            var dummyVehicles = new List<VehicleMaster>
            {
                new VehicleMaster { VehicleId = Guid.NewGuid(), RegNo = "GJ01AB0123" },
                new VehicleMaster { VehicleId = Guid.NewGuid(), RegNo = "GJ01AB1234" }
            };

            // Tell the mock: "When GetAllAsync is called, return this list"
            _vehicleRepositoryMock.Setup(repo => repo.GetAllAsync())
                              .ReturnsAsync(dummyVehicles);

            // --- ACT (Call the method) ---
            var result = await _vehicleService.GetAllAsync();

            // --- ASSERT (Verify results) ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("GJ01AB0123", result[0].RegNo);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnVehicle_WhenExists()
        {
            // --- ARRANGE ---
            var vehicleId = Guid.NewGuid();
            var dummyVehicle = new VehicleMaster { VehicleId = vehicleId, RegNo = "GJ-TEST-01" };

            _vehicleRepositoryMock.Setup(repo => repo.GetByIdAsync(vehicleId))
                              .ReturnsAsync(dummyVehicle);

            // --- ACT ---
            var result = await _vehicleService.GetByIdAsync(vehicleId);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(vehicleId, result.VehicleId);
            Assert.Equal("GJ-TEST-01", result.RegNo);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallAddAsyncInRepository()
        {
            // --- ARRANGE ---
            var newVehicle = new VehicleMaster { VehicleId = Guid.NewGuid(), RegNo = "GJ-NEW-01" };

            // --- ACT ---
            await _vehicleService.CreateAsync(newVehicle);

            // --- ASSERT ---
            // Verify that the repository's AddAsync method was called exactly once
            _vehicleRepositoryMock.Verify(repo => repo.AddAsync(newVehicle), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAsyncInRepository()
        {
            // --- ARRANGE ---
            var vehicleId = Guid.NewGuid();

            // --- ACT ---
            await _vehicleService.DeleteAsync(vehicleId);

            // --- ASSERT ---
            _vehicleRepositoryMock.Verify(repo => repo.DeleteAsync(vehicleId), Times.Once);
        }
    }
}