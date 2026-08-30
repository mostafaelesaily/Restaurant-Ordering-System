using Application.DTOs.CatgoreyDTOs;
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
    public class CategoryServiceTests
    {
        private readonly Mock<IUow> _uow = new();
        private readonly Mock<ICatgoreyRepo> _categories = new();
        private readonly Mock<IGenaricRepo<Files, int>> _files = new();
        private readonly Mock<ICacheService> _cache = new();
        private readonly Mock<IFileService> _fileService = new();
        private readonly Mock<ILogger<CategoryService>> _logger = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IDbContextTransaction> _transaction;
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _transaction = TestMockHelpers.CreateTransaction();
            _uow.Setup(u => u.Categories).Returns(_categories.Object);
            _uow.Setup(u => u.Files).Returns(_files.Object);
            _uow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_transaction.Object);
            _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            TestMockHelpers.SetupCacheToExecuteFactory<PaginatedResultDto<GetCatgoreyDto>>(_cache);
            _sut = new CategoryService(_uow.Object, _fileService.Object, _cache.Object, _logger.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalled_ReturnsMappedPage()
        {
            _categories.Setup(r => r.Query()).Returns(new List<Categories>().AsQueryable());
            _categories.Setup(r => r.GetAllPaged(1, 10))
                .ReturnsAsync((new List<Categories> { new() { id = 1, name = "Drinks" } }.AsEnumerable(), 1));
            _mapper.Setup(m => m.Map<List<GetCatgoreyDto>>(It.IsAny<IEnumerable<Categories>>()))
                .Returns(new List<GetCatgoreyDto> { new() { id = 1, name = "Drinks" } });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Data);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryExists_ReturnsMappedDto()
        {
            var category = new Categories { id = 2, name = "Pizza" };
            var dto = new GetCatgoreyDto { id = 2, name = "Pizza" };
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);
            _mapper.Setup(m => m.Map<GetCatgoreyDto>(category)).Returns(dto);

            var result = await _sut.GetByIdAsync(2);

            Assert.Same(dto, result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryIsMissing_ThrowsNotFoundException()
        {
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Categories?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(2));

            Assert.Equal("Catgorey Not Found", ex.Message);
        }

        [Fact]
        public async Task SearchCatgorey_WhenCalled_UsesSearchQuery()
        {
            var query = new List<Categories>().AsQueryable();
            _categories.Setup(r => r.Search_Catgorey_With_Name_Desc("pi")).Returns(query);
            _categories.Setup(r => r.GetAllPaged(1, 10, query))
                .ReturnsAsync((Enumerable.Empty<Categories>(), 0));
            _mapper.Setup(m => m.Map<List<GetCatgoreyDto>>(It.IsAny<IEnumerable<Categories>>()))
                .Returns(new List<GetCatgoreyDto>());

            var result = await _sut.SearchCatgorey("pi", 1, 10);

            Assert.Empty(result!.Data);
            _categories.Verify(r => r.Search_Catgorey_With_Name_Desc("pi"), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateAsync_WhenNoFiles_CreatesCategoryAndClearsCache(bool useEmptyFiles)
        {
            var dto = new CreateCatgoreyDto { name = "Drinks" };
            var category = new Categories { id = 1, name = "Drinks" };
            var resultDto = new GetCatgoreyDto { id = 1, name = "Drinks" };
            _mapper.Setup(m => m.Map<Categories>(dto)).Returns(category);
            _mapper.Setup(m => m.Map<GetCatgoreyDto>(category)).Returns(resultDto);
            IEnumerable<Microsoft.AspNetCore.Http.IFormFile>? files = useEmptyFiles
                ? Enumerable.Empty<Microsoft.AspNetCore.Http.IFormFile>()
                : null;

            var result = await _sut.CreateAsync(dto, files);

            Assert.Same(resultDto, result);
            _fileService.Verify(f => f.UploadFileAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<string>()), Times.Never);
            _cache.Verify(c => c.RemoveAsync("Get_Categorey"), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Search_Category"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenFilesAreProvided_UploadsFilesAndSavesThem()
        {
            var dto = new CreateCatgoreyDto { name = "Drinks" };
            var category = new Categories { id = 5, name = "Drinks" };
            var file = TestMockHelpers.CreateFormFile();
            _mapper.Setup(m => m.Map<Categories>(dto)).Returns(category);
            _mapper.Setup(m => m.Map<GetCatgoreyDto>(category)).Returns(new GetCatgoreyDto { id = 5 });
            _fileService.Setup(f => f.UploadFileAsync(file.Object, "Categories")).ReturnsAsync("/files/a.png");

            await _sut.CreateAsync(dto, new[] { file.Object });

            _files.Verify(r => r.CreateAsync(It.Is<Files>(f => f.categoryId == 5 && f.FilePath == "/files/a.png")), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCreateFailsAfterUpload_DeletesUploadedFilesAndRollsBack()
        {
            var dto = new CreateCatgoreyDto { name = "Drinks" };
            var category = new Categories { id = 5, name = "Drinks" };
            var file = TestMockHelpers.CreateFormFile();
            _mapper.Setup(m => m.Map<Categories>(dto)).Returns(category);
            _fileService.Setup(f => f.UploadFileAsync(file.Object, "Categories")).ReturnsAsync("/files/a.png");
            _files.Setup(r => r.CreateAsync(It.IsAny<Files>())).ThrowsAsync(new InvalidOperationException("db"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(dto, new[] { file.Object }));

            _fileService.Verify(f => f.DeleteFileAsync("/files/a.png"), Times.Once);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryExistsWithoutFiles_SavesAndClearsCache()
        {
            var category = new Categories { id = 2, name = "Old" };
            var dto = new UpdateCategoryDto { name = "New" };
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);
            _mapper.Setup(m => m.Map(dto, category)).Returns(category);

            await _sut.UpdateAsync(2, dto, null);

            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Categorey"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryIsMissing_ThrowsNotFoundExceptionAndRollsBack()
        {
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Categories?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.UpdateAsync(2, new UpdateCategoryDto { name = "New" }, null));

            Assert.Equal("Category Not Found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenFilesAreProvided_ReplacesOldFiles()
        {
            var category = new Categories { id = 2, name = "Drinks" };
            var oldFile = new Files { id = 9, categoryId = 2, FilePath = "/old.png" };
            var newFile = TestMockHelpers.CreateFormFile();
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);
            _mapper.Setup(m => m.Map(It.IsAny<UpdateCategoryDto>(), category)).Returns(category);
            _files.Setup(r => r.Query()).Returns(new List<Files> { oldFile }.BuildMock());
            _fileService.Setup(f => f.UploadFileAsync(newFile.Object, "Categories")).ReturnsAsync("/new.png");

            await _sut.UpdateAsync(2, new UpdateCategoryDto { name = "Drinks" }, new[] { newFile.Object });

            _fileService.Verify(f => f.DeleteFileAsync("/old.png"), Times.Once);
            _files.Verify(r => r.DeleteAsync(oldFile), Times.Once);
            _files.Verify(r => r.CreateAsync(It.Is<Files>(f => f.FilePath == "/new.png")), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryExists_DeletesFilesThenCategory()
        {
            var category = new Categories { id = 2, name = "Drinks" };
            var file = new Files { id = 9, categoryId = 2, FilePath = "/a.png" };
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);
            _files.Setup(r => r.Query()).Returns(new List<Files> { file }.BuildMock());

            await _sut.DeleteAsync(2);

            _files.Verify(r => r.DeleteAsync(file), Times.Once);
            _categories.Verify(r => r.DeleteAsync(category), Times.Once);
            _fileService.Verify(f => f.DeleteFileAsync("/a.png"), Times.Once);
            _cache.Verify(c => c.RemoveAsync("Get_Categorey"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryIsMissing_ThrowsNotFoundException()
        {
            _categories.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Categories?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(2));

            Assert.Equal("Category Not Found", ex.Message);
            _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
