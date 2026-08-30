using Application.DTOs.TablesDTOs;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Resturant_Ordering_System.Application.Services;
using Resturant_Ordering_System.Domain.Abstract;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class TableServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<ITableRepo> _tables = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<TableService>> _logger = new();
        private readonly TableService _sut;

        public TableServiceTests()
        {
            _uow.Setup(u => u.Tables).Returns(_tables.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetTablesDto>>(_cache);
            _sut = new TableService(_uow.Object, _cache.Object, _mapper.Object, _logger.Object);
        }

        private static Tables CreateTable(int id = 1, int capacity = 4) =>
            new() { Id = id, TableNumber = 10, Capacity = capacity, isActive = true };

        [Fact]
        public async Task GetTablesAsync_WhenCalled_ReturnsMappedPage()
        {
            var table = CreateTable();
            _tables.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<Tables> { table }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetTablesDto>>(It.IsAny<IEnumerable<Tables>>()))
                .Returns(new List<GetTablesDto> { new() { Id = 1 } });

            var result = await _sut.GetTablesAsync(1, 10);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetTableByIdAsync_WhenTableExists_ReturnsMappedDto()
        {
            var table = CreateTable();
            var dto = new GetTablesDto { Id = 1, TableNumber = 10 };
            _tables.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);
            _mapper.Setup(m => m.Map<GetTablesDto>(table)).Returns(dto);

            var result = await _sut.GetTableByIdAsync(1);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetTableByIdAsync_WhenTableIsMissing_ThrowsNotFoundException()
        {
            _tables.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((Tables?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetTableByIdAsync(9));

            Assert.Equal("Table with ID 9 not found.", ex.Message);
        }

        [Fact]
        public async Task CreateTableAsync_WhenCapacityIsValid_CreatesAndClearsCache()
        {
            var dto = new CreateTablesDto { TableNumber = 10, Capacity = 4, isActive = true };
            var table = CreateTable();
            var resultDto = new GetTablesDto { Id = 1, Capacity = 4 };
            _mapper.Setup(m => m.Map<Tables>(dto)).Returns(table);
            _mapper.Setup(m => m.Map<GetTablesDto>(table)).Returns(resultDto);

            var result = await _sut.CreateTableAsync(dto);

            Assert.Same(resultDto, result);
            _tables.Verify(r => r.CreateAsync(table), Times.Once);
            _cache.Verify(c => c.RemoveAsync("GetTables"), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateTableAsync_WhenCapacityIsNotPositive_ThrowsBadRequestException(int capacity)
        {
            var dto = new CreateTablesDto { TableNumber = 1, Capacity = capacity };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateTableAsync(dto));

            Assert.Equal("Capacity must be greater than zero.", ex.Message);
            _tables.Verify(r => r.CreateAsync(It.IsAny<Tables>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTableAsync_WhenTableExists_UpdatesAndClearsCache()
        {
            var table = CreateTable();
            var dto = new UpdateTablesDto { TableNumber = 11, Capacity = 6, isActive = false };
            _tables.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);
            _mapper.Setup(m => m.Map(dto, table)).Returns(table);

            await _sut.UpdateTableAsync(1, dto);

            _tables.Verify(r => r.UpdateAsync(table), Times.Once);
            _cache.Verify(c => c.RemoveAsync("GetTables"), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task UpdateTableAsync_WhenCapacityIsNotPositive_ThrowsBadRequestException(int capacity)
        {
            var dto = new UpdateTablesDto { TableNumber = 1, Capacity = capacity };

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.UpdateTableAsync(1, dto));

            Assert.Equal("Capacity must be greater than zero.", ex.Message);
            _tables.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTableAsync_WhenTableIsMissing_ThrowsNotFoundException()
        {
            _tables.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tables?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateTableAsync(1, new UpdateTablesDto { Capacity = 2, TableNumber = 1 }));

            Assert.Equal("Table with ID 1 not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteTableAsync_WhenTableExists_DeletesAndClearsCache()
        {
            var table = CreateTable();
            _tables.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);

            await _sut.DeleteTableAsync(1);

            _tables.Verify(r => r.DeleteAsync(table), Times.Once);
            _cache.Verify(c => c.RemoveAsync("GetTables"), Times.Once);
        }

        [Fact]
        public async Task DeleteTableAsync_WhenTableIsMissing_ThrowsNotFoundException()
        {
            _tables.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Tables?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteTableAsync(3));

            Assert.Equal("Table with ID 3 not found.", ex.Message);
        }

        [Fact]
        public async Task FindTablesAsync_WhenCalled_UsesSearchQuery()
        {
            var query = new List<Tables>().AsQueryable();
            _tables.Setup(r => r.Search_Table_With_SearchKey("window")).Returns(query);
            _tables.Setup(r => r.GetAllPaged(1, 5, query))
                .ReturnsAsync((Enumerable.Empty<Tables>(), 0));
            _mapper.Setup(m => m.Map<List<GetTablesDto>>(It.IsAny<IEnumerable<Tables>>()))
                .Returns(new List<GetTablesDto>());

            var result = await _sut.FindTablesAsync("window", 1, 5);

            Assert.Empty(result.Data);
            _tables.Verify(r => r.Search_Table_With_SearchKey("window"), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetTablesByActiveStatusAsync_WhenCalled_FiltersByStatus(bool status)
        {
            var query = new List<Tables>().AsQueryable();
            _tables.Setup(r => r.GetTablesByActiveStatus(status)).Returns(query);
            _tables.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Tables>(), 0));
            _mapper.Setup(m => m.Map<List<GetTablesDto>>(It.IsAny<IEnumerable<Tables>>()))
                .Returns(new List<GetTablesDto>());

            var result = await _sut.GetTablesByActiveStatusAsync(status, 1, 10);

            Assert.Empty(result.Data);
            _tables.Verify(r => r.GetTablesByActiveStatus(status), Times.Once);
        }

        [Fact]
        public async Task GetTablesByCapacityAsync_WhenCalled_FiltersByCapacity()
        {
            var query = new List<Tables>().AsQueryable();
            _tables.Setup(r => r.GetTablesByCapacity(4)).Returns(query);
            _tables.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Tables>(), 0));
            _mapper.Setup(m => m.Map<List<GetTablesDto>>(It.IsAny<IEnumerable<Tables>>()))
                .Returns(new List<GetTablesDto>());

            var result = await _sut.GetTablesByCapacityAsync(4, 1, 10);

            Assert.Empty(result.Data);
            _tables.Verify(r => r.GetTablesByCapacity(4), Times.Once);
        }
    }
}
