using System;
using System.IO;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.SortFile.IntegrationTests
{
    [Collection("SmallFiles")]
    public class ExternalMergeSortIntegrationTests : IClassFixture<SmallFilesFixture>
    {
        private readonly SmallFilesFixture _fixture;
        private readonly ExternalMergeSortFileCommand _sut;

        public ExternalMergeSortIntegrationTests(SmallFilesFixture fixture)
        {
            _fixture = fixture;
            _sut = new ExternalMergeSortFileCommand(new ExternalMergeSortOptions{FileLocation = _fixture.FilesDirectory});
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        public async Task FileSortedWithExternalMergeSortCommandShouldBeIdenticalToFileSortedWithArraySort(int rows)
        {
            var filename = _fixture.Files[rows];
            var fileFullPath = Path.Combine(_fixture.FilesDirectory, filename);
            var source = File.OpenRead(fileFullPath);

            await _sut.Execute(source, new MemoryStream()); // TODO FIX
            var unsortedFileRows = await File.ReadAllLinesAsync(fileFullPath);
            Array.Sort(unsortedFileRows);
            var arraySortedFilePath = Path.Combine(_fixture.FilesDirectory, "inmemory-sorted");
            var mergeSortedFilePath = Path.Combine(_fixture.FilesDirectory, "1.sorted");
            await File.WriteAllLinesAsync(arraySortedFilePath, unsortedFileRows);
            await using var mergeSortedFile = File.OpenRead(mergeSortedFilePath);
            await using var arraySortedFile = File.OpenRead(arraySortedFilePath);
            var filesAreEqual = FileComparer.FilesAreEqual(mergeSortedFile, arraySortedFile);

            filesAreEqual.ShouldBeTrue();
            File.Delete(arraySortedFilePath);
            File.Delete(mergeSortedFilePath);
        }
    }

    [Collection("LargeFiles")]
    public class ExternalMergeSort_LargeFiles_IntegrationTests : IClassFixture<LargeFilesFixture>
    {
        private readonly LargeFilesFixture _fixture;
        private readonly ExternalMergeSortFileCommand _sut;

        public ExternalMergeSort_LargeFiles_IntegrationTests(LargeFilesFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _sut = new ExternalMergeSortFileCommand(new ExternalMergeSortOptions { FileLocation = _fixture.FilesDirectory });
        }

        [Theory]
        [InlineData(10000000)]
        [InlineData(100000000)]
        public async Task FileSortedWithExternalMergeSortCommandShouldBeIdenticalToFileSortedWithArraySort(int rows)
        {
            var filename = _fixture.Files[rows];
            var fileFullPath = Path.Combine(_fixture.FilesDirectory, filename);
            var source = File.OpenRead(fileFullPath);

            await _sut.Execute(source, new MemoryStream()); // TODO FIX
            var unsortedFileRows = await File.ReadAllLinesAsync(fileFullPath);
            Array.Sort(unsortedFileRows);
            var arraySortedFilePath = Path.Combine(_fixture.FilesDirectory, "inmemory-sorted");
            await File.WriteAllLinesAsync(arraySortedFilePath, unsortedFileRows);
            var mergeSortedFile = File.OpenRead(Path.Combine(_fixture.FilesDirectory, "1.sorted"));
            var arraySortedFile = File.OpenRead(arraySortedFilePath);
            var filesAreEqual = FileComparer.FilesAreEqual(mergeSortedFile, arraySortedFile);

            filesAreEqual.ShouldBeTrue();
        }
    }
}