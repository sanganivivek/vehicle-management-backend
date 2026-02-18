using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vehicle_management_backend;
using vehicle_management_backend.Application.Services.Implementations;
using vehicle_management_backend.Application.Services.Interfaces;
using vehicle_management_backend.Core.DTOs;
using vehicle_management_backend.Core.Models;
using vehicle_management_backend.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace TestProject1
{
    public class BrandServiceTests
    {
        // 1. Define Mocks and the Service
        // Note: Using 'IBrandRespository' (matching the typo in your interface filename)
        private readonly Mock<IBrandRespository> _brandRepositoryMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly BrandService _brandService;

        public BrandServiceTests()
        {
            // 2. Initialize the Mocks
            _brandRepositoryMock = new Mock<IBrandRespository>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            // 3. Inject BOTH Mock objects into the Service Constructor
            // This fixes the error: "There is no argument given that corresponds to required parameter 'activityLogService'"
            _brandService = new BrandService(_brandRepositoryMock.Object, _activityLogServiceMock.Object);
        }

        [Fact]
        public async Task GetBrandsAsync_ShouldReturnListOfBrandDTOs()
        {
            // --- ARRANGE ---
            var dummyBrands = new List<Brand>
            {
                new Brand { BrandId = Guid.NewGuid(), BrandName = "Toyota", BrandCode = "TYT", IsActive = true, Models = new List<Model>() },
                new Brand { BrandId = Guid.NewGuid(), BrandName = "Honda", BrandCode = "HND", IsActive = true, Models = new List<Model>() }
            };

            // Setup the mock to return our dummy list
            _brandRepositoryMock.Setup(repo => repo.GetAllAsync())
                              .ReturnsAsync(dummyBrands);

            // --- ACT ---
            var result = await _brandService.GetBrandsAsync();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Toyota", result[0].BrandName);
        }

        [Fact]
        public async Task GetBrandByIdAsync_ShouldReturnBrand_WhenExists()
        {
            // --- ARRANGE ---
            var brandId = Guid.NewGuid();
            var dummyBrand = new Brand
            {
                BrandId = brandId,
                BrandName = "Ford",
                BrandCode = "FRD",
                IsActive = true,
                Models = new List<Model>()
            };

            _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(brandId))
                              .ReturnsAsync(dummyBrand);

            // --- ACT ---
            var result = await _brandService.GetBrandByIdAsync(brandId);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(brandId, result.BrandId);
            Assert.Equal("Ford", result.BrandName);
        }

        [Fact]
        public async Task AddBrandAsync_ShouldCallRepositoryAdd()
        {
            // --- ARRANGE ---
            var newBrandDto = new CreateBrandDTO
            {
                BrandName = "Tesla",
                BrandCode = "TSLA",
                IsActive = true
            };

            // --- ACT ---
            // Note: Assuming AddBrandAsync accepts CreateBrandDTO or similar. 
            // Adjust the type if your method signature uses a different DTO.
            await _brandService.AddBrandAsync(newBrandDto);

            // --- ASSERT ---
            // Verify that the repository's AddAsync was called exactly once with the mapped entity
            _brandRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Brand>(b =>
                b.BrandName == "Tesla" &&
                b.BrandCode == "TSLA"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateBrandAsync_ShouldCallRepositoryUpdate()
        {
            // --- ARRANGE ---
            var brandId = Guid.NewGuid();
            var existingBrand = new Brand { BrandId = brandId, BrandName = "Old", BrandCode = "OLD", Models = new List<Model>() };

            // Assuming the Update DTO might be different, commonly UpdateBrandDTO or similar
            var updateDto = new BrandDTO { BrandName = "New", BrandCode = "NEW", IsActive = true };

            // Mock finding the existing brand
            _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(brandId))
                              .ReturnsAsync(existingBrand);

            // --- ACT ---
            await _brandService.UpdateBrandAsync(brandId, updateDto);

            // --- ASSERT ---
            // Verify UpdateAsync was called
            _brandRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Brand>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldCallRepositoryDelete()
        {
            // --- ARRANGE ---
            var brandId = Guid.NewGuid();

            // --- ACT ---
            await _brandService.DeleteBrandAsync(brandId);

            // --- ASSERT ---
            _brandRepositoryMock.Verify(repo => repo.DeleteAsync(brandId), Times.Once);
        }
    }
}