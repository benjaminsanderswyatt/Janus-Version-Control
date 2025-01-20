using Janus.Helpers;
using Janus.Plugins;
using Moq;
using static Janus.CommandHandler;

namespace CLITests
{
    [TestFixture]
    public class AddCommandTest
    {
        private Mock<ILogger> _loggerMock;
        private Paths _paths;
        private AddCommand _addCommand;

        private string _testDir;


        [SetUp]
        public void Setup()
        {
            // Mock the ILogger
            _loggerMock = new Mock<ILogger>();

            // Set the base directory path for testing
            _testDir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "JanusTest")); // Using temp directory for testing
            Directory.CreateDirectory(_testDir);
            _paths = new Paths(_testDir);

            Directory.SetCurrentDirectory(_testDir);

            // Set up Janus repo
            Directory.CreateDirectory(_paths.JanusDir);
            Directory.CreateDirectory(_paths.ObjectDir);
            Directory.CreateDirectory(_paths.HeadsDir);
            File.Create(_paths.Index).Close();
            File.Create(_paths.HEAD).Close();


            // Create AddCommand instance
            _addCommand = new AddCommand(_loggerMock.Object, _paths);
        }


        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            Directory.SetCurrentDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))); // Cant delete dir if in it

            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }


        [Test]
        public void ShouldLogError_WhenNoFilesOrDirsAreProvided()
        {
            // Act: Execute 'janus add' without args
            var args = new string[] { };
            _addCommand.Execute(args);


            // Assert: Verify that the correct error was logged
            _loggerMock.Verify(logger => logger.Log("No files or directory specified to add."), Times.Once);
        }



        [Test]
        public void ShouldLogError_WhenRepositoryIsNotInitialized()
        {
            // Arrange: The repo doesnt exist
            Directory.Delete(_paths.JanusDir, true);


            // Act: Execute 'janus add file.txt' when repo is not initialized
            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);


            // Assert: Verify that the error about repo initialization is logged
            _loggerMock.Verify(logger => logger.Log("Not a janus repository. Use 'init' command to initialise repository."), Times.Once);
        }



        [Test]
        public void ShouldStageSingleFile_WhenFileIsSpecified()
        {
            // Arrange: Create a test file to add
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");


            // Act: Execute 'janus add file.txt'
            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);


            // Assert: Verify that the file is staged correctly
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.That(stagedFiles.ContainsKey("file.txt"), Is.True, "File should be staged.");
            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "file.txt"), stagedFiles["file.txt"]);

        }


        [Test]
        public void ShouldAddMultipleFiles_WhenMultipleFilesAreSpecified()
        {
            // Arrange: Create multiple test files
            var file1Path = Path.Combine(_testDir, "file1.txt");
            var file2Path = Path.Combine(_testDir, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");


            // Act: Execute 'janus add file1.txt file2.txt'
            var args = new string[] { "file1.txt", "file2.txt" };
            _addCommand.Execute(args);


            // Assert: Verify that both files are staged correctly
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.That(stagedFiles.ContainsKey("file1.txt"), Is.True);
            Assert.That(stagedFiles.ContainsKey("file2.txt"), Is.True);

            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "file1.txt"), stagedFiles["file1.txt"]);
            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "file2.txt"), stagedFiles["file2.txt"]);

        }

        [Test]
        public void AddCommand_ShouldMarkDeletedFile()
        {
            // Arrange: Create, add to index and then delete the file
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test Content");

            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            File.Delete(testFilePath);

            // Act
            _addCommand.Execute(args);

            // Assert: Check if the file is marked as deleted in the staging area
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.IsTrue(stagedFiles.ContainsKey("file.txt"));
            Assert.AreEqual(stagedFiles["file.txt"], "Deleted");
        }




        [Test]
        public void ShouldLogFileNotFound_WhenFileDoesNotExist()
        {
            // Act: Attempt to add a file that doesnt exist
            var args = new string[] { "doesntexist.txt" };
            _addCommand.Execute(args);


            // Assert: Verify that the file not found error is logged
            _loggerMock.Verify(logger => logger.Log("Error: Path 'doesntexist.txt' does not exist."), Times.Once);
        }



        [Test]
        public void ShouldRespectJanusIgnoreFile_WhenFileIsIgnored()
        {
            // Arrange: Create test files and .janusignore file
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            var ignoredFilePath = Path.Combine(_testDir, "ignoredfile.txt");
            File.WriteAllText(ignoredFilePath, "ignored content");

            // Create .janusignore to ignore "ignoredfile.txt"
            File.WriteAllLines(Path.Combine(_testDir, ".janusignore"), [ "ignoredfile.txt" ]);


            // Act: Add both files, one should be ignored
            var args = new string[] { "file.txt", "ignoredfile.txt" };
            _addCommand.Execute(args);


            // Assert: Verify that the ignored file is not added to the staging area
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.That(stagedFiles.ContainsKey("file.txt"), Is.True);
            Assert.That(stagedFiles.ContainsKey("ignoredfile.txt"), Is.False);
            _loggerMock.Verify(logger => logger.Log("Added 'file.txt' to the staging area."), Times.Once);
            _loggerMock.Verify(logger => logger.Log("Added 'ignoredfile.txt' to the staging area."), Times.Never);
        }



        [Test]
        public void ShouldNotIgnoreFiles_WhenJanusIgnoreIsEmpty()
        {
            // Arrange: Create test files and an empty .janusignore file
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            File.WriteAllText(Path.Combine(_testDir, ".janusignore"), ""); // Empty ignore file

            // Act: Execute 'janus add file.txt'
            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            // Assert: Verify that the file is staged and not ignored
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.That(stagedFiles.ContainsKey("file.txt"), Is.True);
        }



        [Test]
        public void ShouldUpdateStagedFile_WhenFileHashChanges()
        {
            // Arrange: Create a test file and stage it
            var testFilePath = Path.Combine(_testDir, "file.txt");
            File.WriteAllText(testFilePath, "test content");

            var args = new string[] { "file.txt" };
            _addCommand.Execute(args);

            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            var initialHash = stagedFiles["file.txt"];

            // Modify the file content
            File.WriteAllText(testFilePath, "updated content");


            // Act: Re execute 'janus add file.txt' to stage again
            _addCommand.Execute(args);

            stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            var updatedHash = stagedFiles["file.txt"];


            // Assert: Ensure the file's hash is updated after modification
            Assert.That(updatedHash, Is.Not.EqualTo(initialHash));
        }


        [Test]
        public void ShouldAddAllFilesInDirectory_WhenDirectoryIsProvided()
        {
            Console.WriteLine("ShouldAddAllFilesInDirectory_WhenDirectoryIsProvided");

            // Arrange: Create a test directory with multiple files
            var dirPath = Path.Combine(_testDir, "testDir");
            Directory.CreateDirectory(dirPath);
            var file1Path = Path.Combine(dirPath, "file1.txt");
            var file2Path = Path.Combine(dirPath, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            Console.WriteLine($"File1Path: {file1Path}");
            Console.WriteLine($"File2Path: {file2Path}");

            // Act: Execute 'janus add testDir'
            var args = new string[] { "testDir" };
            _addCommand.Execute(args);

            // Assert: Verify that both files in the directory are staged
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);

            Console.WriteLine("Index");
            foreach (var item in stagedFiles)
            {
                Console.WriteLine(item.Key + " | " + item.Value);
            }



            Assert.That(stagedFiles.ContainsKey("testDir/file1.txt".Replace('/', Path.DirectorySeparatorChar)), Is.True);
            Assert.That(stagedFiles.ContainsKey("testDir/file2.txt".Replace('/', Path.DirectorySeparatorChar)), Is.True);

            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "testDir/file1.txt".Replace('/', Path.DirectorySeparatorChar)), stagedFiles["testDir/file1.txt".Replace('/', Path.DirectorySeparatorChar)]);
            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "testDir/file2.txt".Replace('/', Path.DirectorySeparatorChar)), stagedFiles["testDir/file2.txt".Replace('/', Path.DirectorySeparatorChar)]);
        }

        [Test]
        public void ShouldAddAllFiles_WhenAllArgumentIsProvided()
        {
            // Arrange: Create multiple test files
            var file1Path = Path.Combine(_testDir, "file1.txt");
            var file2Path = Path.Combine(_testDir, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            // Act: Execute 'janus add all'
            var args = new string[] { "all" };
            _addCommand.Execute(args);

            // Assert: Verify that all files in the directory are staged
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.That(stagedFiles.ContainsKey("file1.txt"), Is.True);
            Assert.That(stagedFiles.ContainsKey("file2.txt"), Is.True);

            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "file1.txt"), stagedFiles["file1.txt"]);
            Assert.AreEqual(HashHelper.ComputeHashGivenFilepath(_paths.WorkingDir, "file2.txt"), stagedFiles["file2.txt"]);
        }

        [Test]
        public void ShouldWarnUser_WhenMultipleArgsGivenWithAll()
        {
            // Arrange

            // Act: Execute 'janus add test all thing'
            var args = new string[] { "test", "all", "thing" };
            _addCommand.Execute(args);

            // Assert
            _loggerMock.Verify(logger => logger.Log("Warning using 'all' will override other arguments."), Times.Once);
        }



        [Test]
        public void ShouldMarkMultipleFilesAsDeleted_WhenFilesAreDeleted()
        {
            // Arrange: Create and add multiple files
            var file1Path = Path.Combine(_testDir, "file1.txt");
            var file2Path = Path.Combine(_testDir, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            var args = new string[] { "file1.txt", "file2.txt" };
            _addCommand.Execute(args);

            // Act: Delete both files and re execute 'janus add file1.txt file2.txt'
            File.Delete(file1Path);
            File.Delete(file2Path);
            _addCommand.Execute(args);

            // Assert: Verify that both files are marked as deleted
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.AreEqual(stagedFiles["file1.txt"], "Deleted");
            Assert.AreEqual(stagedFiles["file2.txt"], "Deleted");
        }


        [Test]
        public void ShouldMarkDeletedFilesInDirectory_WhenDirectoryFilesAreDeleted()
        {
            // Arrange: Create a directory and add multiple files inside it
            var dirPath = Path.Combine(_testDir, "testDir");
            Directory.CreateDirectory(dirPath);
            var file1Path = Path.Combine(dirPath, "file1.txt");
            var file2Path = Path.Combine(dirPath, "file2.txt");

            File.WriteAllText(file1Path, "content of file1");
            File.WriteAllText(file2Path, "content of file2");

            var args = new string[] { "testDir" };
            _addCommand.Execute(args);
            
            // Act: Delete files in the directory and re execute 'janus add testDir'
            File.Delete(file1Path);
            File.Delete(file2Path);
            _addCommand.Execute(args);


            // Assert: Verify that both files in the directory are marked as deleted
            var stagedFiles = IndexHelper.LoadIndex(_paths.Index);
            Assert.AreEqual("Deleted", stagedFiles["testDir/file1.txt".Replace('/', Path.DirectorySeparatorChar)]);
            Assert.AreEqual("Deleted", stagedFiles["testDir/file2.txt".Replace('/', Path.DirectorySeparatorChar)]);
        }

    }
}