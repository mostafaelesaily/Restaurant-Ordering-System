using Application.DTOs.MenuItemDTOs;
using Application.Services;
using AutoMapper;
using Business_Layer.DTOs.PaginatedDtos;
using Business_Layer.Exceptions;
using Business_Layer.Interfaces;
using Domain_Layer.Abstract;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Resturant_Ordering_System.Test.Helpers;

namespace Resturant_Ordering_System.Test.Services
{
    public class MenuItemServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<IMenuItemRepo> _menuItems = new();
        private readonly Mock<ICatgoreyRepo> _categories = new();
        private readonly Mock<IGenaricRepo<Files, int>> _files = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IFileService> _fileService = new();
        private readonly Mock<ILogger<MenuItemService>> _logger = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly MenuItemService _sut;

        public MenuItemServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.MenuItems).Returns(_menuItems.Object);
            _uow.Setup(u => u.Categories).Returns(_categories.Object);
            _uow.Setup(u => u.Files).Returns(_files.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetMenuItemDto>>(_cache);
            _sut = new MenuItemService(_uow.Object, _fileService.Object, _cache.Object, _logger.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalled_ReturnsMappedPage()
        {
            _menuItems.Setup(r => r.Query()).Returns(new List<MenuItems>().AsQueryable());
            _menuItems.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<MenuItems> { new() { id = 1, name = "Burger" } }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetMenuItemDto>>(It.IsAny<IEnumerable<MenuItems>>()))
                .Returns(new List<GetMenuItemDto> { new() { id = 1, name = "Burger" } });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemExists_ReturnsMappedDto()
        {
            var item = new MenuItems { id = 3, name = "Burger" };
            var dto = new GetMenuItemDto { id = 3, name = "Burger" };
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(item);
            _mapper.Setup(m => m.Map<GetMenuItemDto>(item)).Returns(dto);

            var result = await _sut.GetByIdAsync(3);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemIsMissing_ThrowsNotFoundException()
        {
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(3));

            Assert.Equal("MenuItem Not Found", ex.Message);
        }

        [Fact]
        public async Task SearchMenuItem_WhenCalled_UsesSearchQuery()
        {
            var query = new List<MenuItems>().AsQueryable();
            _menuItems.Setup(r => r.Search_MenuItem_With_Name_Desc("bur")).Returns(query);
            _menuItems.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<MenuItems>(), 0));
            _mapper.Setup(m => m.Map<List<GetMenuItemDto>>(It.IsAny<IEnumerable<MenuItems>>()))
                .Returns(new List<GetMenuItemDto>());

            var result = await _sut.SearchMenuItem("bur", 1, 10);

            Assert.Empty(result!.Data);
        }

        [Fact]
        public async Task CreateAsync_WhenValid_CreatesItemAndClearsCache()
        {
            var dto = new CreateMenuItemDto { name = "Burger", price = 50, categoryId = 1 };
            var item = new MenuItems { id = 3, name = "Burger" };
            _mapper.Setup(m => m.Map<MenuItems>(dto)).Returns(item);
            _mapper.Setup(m => m.Map<GetMenuItemDto>(item)).Returns(new GetMenuItemDto { id = 3 });

            var result = await _sut.CreateAsync(dto, null);

            Assert.Equal(3, result.id);
            _menuItems.Verify(r => r.CreateAsync(item), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_MenuItems"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenUploadFails_DeletesUploadedFilesAndRollsBack()
        {
            var dto = new CreateMenuItemDto { name = "Burger", price = 50, categoryId = 1 };
            var file = TestMockHelpers.CreateFormFile();
            _mapper.Setup(m => m.Map<MenuItems>(dto)).Returns(new MenuItems { id = 3 });
            _fileService.Setup(f => f.UploadFileAsync(file.Object, "MenuItems")).ReturnsAsync("/m.png");
            _files.Setup(r => r.CreateAsync(It.IsAny<Files>())).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(dto, new[] { file.Object }));

            _fileService.Verify(f => f.DeleteFileAsync("/m.png"), Times.Once);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenItemIsMissing_ThrowsNotFoundException()
        {
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateAsync(3, new UpdateMenuItemDto { name = "X", price = 1, categoryId = 1 }, null));

            Assert.Equal("MenuItem Not Found", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenItemExists_SavesAndClearsCache()
        {
            var item = new MenuItems { id = 3, name = "Old" };
            var dto = new UpdateMenuItemDto { name = "New", price = 20, categoryId = 1 };
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(item);
            _mapper.Setup(m => m.Map(dto, item)).Returns(item);

            await _sut.UpdateAsync(3, dto, null);

            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_MenuItems"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenFilesAreProvided_ReplacesOldFiles()
        {
            var item = new MenuItems { id = 3, name = "Burger" };
            var oldFile = new Files { id = 1, menuItemId = 3, FilePath = "/old.png" };
            var newFile = TestMockHelpers.CreateFormFile();
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(item);
            _mapper.Setup(m => m.Map(It.IsAny<UpdateMenuItemDto>(), item)).Returns(item);
            _files.Setup(r => r.Query()).Returns(new List<Files> { oldFile }.BuildMock());
            _fileService.Setup(f => f.UploadFileAsync(newFile.Object, "MenuItems")).ReturnsAsync("/new.png");

            await _sut.UpdateAsync(3, new UpdateMenuItemDto { name = "Burger", price = 10, categoryId = 1 }, new[] { newFile.Object });

            _fileService.Verify(f => f.DeleteFileAsync("/old.png"), Times.Once);
            _files.Verify(r => r.DeleteAsync(oldFile), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenItemExists_DeletesFilesThenItem()
        {
            var item = new MenuItems { id = 3 };
            var file = new Files { id = 1, menuItemId = 3, FilePath = "/a.png" };
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(item);
            _files.Setup(r => r.Query()).Returns(new List<Files> { file }.BuildMock());

            await _sut.DeleteAsync(3);

            _files.Verify(r => r.DeleteAsync(file), Times.Once);
            _menuItems.Verify(r => r.DeleteAsync(item), Times.Once);
            _fileService.Verify(f => f.DeleteFileAsync("/a.png"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenItemIsMissing_ThrowsNotFoundException()
        {
            _menuItems.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((MenuItems?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(3));

            Assert.Equal("MenuItem Not Found", ex.Message);
        }

        [Fact]
        public async Task GetCategoryMenuItemsAsync_WhenCategoryIsMissing_ThrowsNotFoundException()
        {
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Categories?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCategoryMenuItemsAsync(2, 1, 10));

            Assert.Equal("Category Not Found", ex.Message);
        }

        [Fact]
        public async Task GetCategoryMenuItemsAsync_WhenNoItemsExist_ThrowsNotFoundException()
        {
            var query = new List<MenuItems>().AsQueryable();
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Categories { id = 2 });
            _menuItems.Setup(r => r.GetCategoreyMenuItems(2)).Returns(query);
            _menuItems.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<MenuItems>(), 0));

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCategoryMenuItemsAsync(2, 1, 10));

            Assert.Equal("No MenuItems For This Category", ex.Message);
        }

        [Fact]
        public async Task GetCategoryMenuItemsAsync_WhenItemsExist_ReturnsMappedPage()
        {
            var query = new List<MenuItems>().AsQueryable();
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Categories { id = 2 });
            _menuItems.Setup(r => r.GetCategoreyMenuItems(2)).Returns(query);
            _menuItems.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((new List<MenuItems> { new() { id = 3 } }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetMenuItemDto>>(It.IsAny<IEnumerable<MenuItems>>()))
                .Returns(new List<GetMenuItemDto> { new() { id = 3 } });

            var result = await _sut.GetCategoryMenuItemsAsync(2, 1, 10);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }
    }
}
