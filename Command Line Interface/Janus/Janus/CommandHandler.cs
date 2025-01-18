﻿using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;


namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands(ILogger logger, Paths paths)
        {
            var commands = new List<ICommand>
            {
                new TestCommand(logger, paths),
                new HelpCommand(logger, paths),
                new InitCommand(logger, paths),
                new AddCommand(logger, paths),
                new CommitCommand(logger, paths),
                new LogCommand(logger, paths),
                new ListBranchesCommand(logger, paths),
                new CreateBranchCommand(logger, paths),
                new DeleteBranchCommand(logger, paths),
                new SwitchBranchCommand(logger, paths),
                new SwitchCommitCommand(logger, paths),
                //new PushCommand(),
                
                
                // Add new built in commands here
            };

            return commands;
        }

        public class TestCommand : BaseCommand
        {
            public TestCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "test";
            public override string Description => "Send a test request to backend";
            public override void Execute(string[] args)
            {
                //await CommandHelper.ExecuteAsync();
                Logger.Log("Test End");
               
            }
        }


        public class HelpCommand : BaseCommand
        {
            public HelpCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "help";
            public override string Description => "Displays a list of available commands.";
            public override void Execute(string[] args)
            {
                Logger.Log("Usage: janus <command>");
                Logger.Log("Commands:");
                foreach (var command in Program.CommandList)
                {
                    Logger.Log($"{command.Name.PadRight(20)} : {command.Description}");
                }
            }
        }


        public class InitCommand : BaseCommand
        {
            public InitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "init";
            public override string Description => "Initializes the janus repository.";
            public override void Execute(string[] args)
            {
                try
                {
                    // Initialise .janus folder
                    if (Directory.Exists(Paths.JanusDir))
                    {
                        Logger.Log("Repository already initialized");
                        return;
                    }

                    Directory.CreateDirectory(Paths.JanusDir);
                    File.SetAttributes(Paths.JanusDir, File.GetAttributes(Paths.JanusDir) | FileAttributes.Hidden); // Makes the janus folder hidden


                    Directory.CreateDirectory(Paths.ObjectDir); // .janus/object folder
                    Directory.CreateDirectory(Paths.HeadsDir); // .janus/heads
                    Directory.CreateDirectory(Paths.PluginsDir); // .janus/plugins folder
                    Directory.CreateDirectory(Paths.CommitDir); // .janus/commits folder
                    Directory.CreateDirectory(Paths.BranchesDir); // .janus/branches folder


                    // Create index file
                    File.Create(Paths.Index).Close();

                    // Create HEAD file pointing at main branch
                    File.WriteAllText(Paths.HEAD, "ref: heads/main");



                    // Create initial commit
                    string initialCommitMessage = "Initial commit";
                    var emptyFileHashes = new Dictionary<string, string>();
                    string initCommitHash = CommandHelper.ComputeCommitHash(emptyFileHashes, initialCommitMessage);

                    string commitMetadata = CommandHelper.GenerateCommitMetadata("main" ,initCommitHash, emptyFileHashes, initialCommitMessage, null, null);

                    // Save the commit object in the commit directory
                    string commitFilePath = Path.Combine(Paths.CommitDir, initCommitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);

                    // Create main branch in heads/ pointing to initial commit
                    File.WriteAllText(Path.Combine(Paths.HeadsDir, "main"), initCommitHash);

                    // Create detached Head file
                    File.WriteAllText(Path.Combine(Paths.JanusDir, "DETACHED_HEAD"), initCommitHash);




                    // Create branches file for main
                    var branch = new Branch
                    {
                        Name = "main",
                        InitialCommit = initCommitHash,
                        CreatedBy = null,
                        ParentBranch = null,
                        Created = DateTimeOffset.Now
                    };

                    string branchJson = JsonSerializer.Serialize(branch, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(Path.Combine(Paths.BranchesDir, "main"), branchJson);



                    Logger.Log("Initialized janus repository");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error initializing repository: {ex.Message}");
                }
            }
        }



        public class AddCommand : BaseCommand
        {
            public AddCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "add";
            public override string Description => "Adds files to the staging area. To add all files use 'janus add all'.";
            public override void Execute(string[] args)
            {
                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Logger.Log("No files or directory specified to add.");
                    return;
                }

                // Repository has to be initialised for command to run
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }


                var filesToAdd = new List<string>();

                foreach (var arg in args)
                {
                    if (Directory.Exists(arg))
                    {
                        // Add all files in the directory recursively
                        var directoryFiles = Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories)
                                                      .Select(filePath => Path.GetRelativePath(".", filePath))
                                                      .Where(path => !path.StartsWith(".janus"));
                        filesToAdd.AddRange(directoryFiles);
                    }
                    else if (File.Exists(arg))
                    {
                        // Add the file
                        filesToAdd.Add(arg);
                    }
                    else
                    {
                        Logger.Log($"Path '{arg}' does not exist.");
                    }
                }



                // Check .janusignore for ingored patterns
                var ignoredPatterns = File.Exists(".janusignore")
                    ? File.ReadAllLines(".janusignore").Select(pattern => pattern.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList()
                    : new List<string>();

                filesToAdd = filesToAdd.Where(file => !AddHelper.IsFileIgnored(file, ignoredPatterns)).ToList();


                // Load existing staged files
                var stagedFiles = AddHelper.LoadIndex(Paths.Index);


                // Remove deleted files from the staging area
                var filesToRemove = stagedFiles.Keys.Where(filePath => !File.Exists(filePath)).ToList();
                foreach (var filePath in filesToRemove)
                {
                    stagedFiles.Remove(filePath);
                }

                Logger.Log($"{filesToRemove.Count} files removed from staging area.");



                // Stage each file
                foreach (string relativeFilePath in filesToAdd)
                {
                    // File doesnt exist so it returns error and continues staging other files
                    if (!File.Exists(relativeFilePath))
                    {
                        Logger.Log($"File at '{relativeFilePath}' not found.");
                        continue;
                    }

                    try
                    {
                        // Compute file hash
                        string fileHash = AddHelper.ComputeHash_GivenFilepath(relativeFilePath);

                        // Normalize paths for comparison
                        if (!stagedFiles.ContainsKey(relativeFilePath) || stagedFiles[relativeFilePath] != fileHash)
                        {
                            stagedFiles[relativeFilePath] = fileHash;
                            Logger.Log($"Added '{relativeFilePath}' to the staging area.");
                        }
                        else
                        {
                            Logger.Log($"File '{relativeFilePath}' is already staged.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed adding '{relativeFilePath}': {ex.Message}");
                    }

                }


                // Update index
                AddHelper.SaveIndex(Paths.Index, stagedFiles);

                Logger.Log($"{filesToAdd.Count} files processed.");
                Logger.Log($"{stagedFiles.Count} files in staging area.");
            }
        }








        public class CommitCommand : BaseCommand
        {
            public CommitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "commit";
            public override string Description => "Saves changes to the repository.";
            public override void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                string parentCommit;
                try
                {
                    parentCommit = CommandHelper.GetCurrentHEAD(Paths);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error getting current HEAD: {ex.Message}");
                    return;
                }

                string branch = CommandHelper.GetCurrentBranchName(Paths);


                // Validate commit message
                if (!CommitHelper.ValidateCommitMessage(Logger, args, out string commitMessage)) return;


                // If no files have been staged then there is nothing to commit
                if (!File.Exists(Paths.Index) || !File.ReadLines(Paths.Index).Any())
                {
                    Logger.Log("No changes to commit.");
                    return;
                }


                // Load staged files
                Dictionary<string, string> stagedFiles;
                try
                {
                    // Load staged files
                    stagedFiles = File.ReadAllLines(Paths.Index)
                                      .Select(line => line.Split('|'))
                                      .Where(parts => parts.Length == 2)
                                      .ToDictionary(parts => parts[0], parts => parts[1]);
                }
                catch (IOException ex)
                {
                    Logger.Log($"Error reading staged files: {ex.Message}");
                    return;
                }


                // Identify missing files
                var missingFiles = stagedFiles.Keys.Where(filePath => !File.Exists(Path.Combine(Directory.GetCurrentDirectory(), filePath))).ToList();

                // Log and mark missing files as deleted
                foreach (var missingFile in missingFiles)
                {
                    Logger.Log($"Staged file '{missingFile}' no longer exists and will be marked as deleted.");
                    stagedFiles[missingFile] = "Deleted";
                }





                var fileHashes = new Dictionary<string, string>();
                foreach (var file in stagedFiles)
                {
                    string fileHash = file.Value;
                    if (fileHash == "Deleted") {
                        fileHashes[file.Key] = "Deleted";
                        continue;
                    };

                    string relativeFilePath = file.Key;

                    try
                    {
                        string content = File.ReadAllText(relativeFilePath);
                        fileHashes[relativeFilePath] = fileHash;

                        // Write file content to objects directory
                        string objectFilePath = Path.Combine(Paths.ObjectDir, fileHash);
                        if (!File.Exists(objectFilePath)) // Dont rewrite existing objects
                        {
                            File.WriteAllText(objectFilePath, content);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error committing file '{relativeFilePath}': {ex.Message}");
                        return;
                    }

                }

                try
                {

                    // Generate commit metadata
                    string commitHash = CommandHelper.ComputeCommitHash(fileHashes, commitMessage);
                    string commitMetadata = CommandHelper.GenerateCommitMetadata(branch , commitHash, fileHashes, commitMessage, parentCommit, CommandHelper.GetUsername());

                    // Save commit object
                    string commitFilePath = Path.Combine(Paths.CommitDir, commitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);

                    // Update head to point to the new commit
                    HeadHelper.SetHeadCommit(Paths, commitHash);

                    // Clear the staging area
                    File.WriteAllText(Paths.Index, string.Empty);

                    Logger.Log($"Committed as {commitHash}");

                }
                catch (Exception ex)
                {
                    Logger.Log($"Error saving commit: {ex.Message}");
                }
            }
        }







        public class LogCommand : BaseCommand
        {
            public LogCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "log";
            public override string Description => "Displays the commit history. 'janus log branch=<> author=<> since=<> until=<> limit=<> verbose=<>'";
            public override void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (!Directory.Exists(Paths.CommitDir))
                {
                    Logger.Log("Error commit directory doesnt exist.");
                    return;
                }


                // Get all commit files
                var commitFiles = Directory.GetFiles(Paths.CommitDir);

                if (commitFiles.Length == 0)
                {
                    Logger.Log("Error no commits found. Repository might not have initialized correctly.");
                    return;
                }


                // Get arguments for filters

                LogFilters filters = new LogFilters {
                    Branch = args.FirstOrDefault(arg => arg.StartsWith("branch="))?.Split('=')[1],
                    Author = args.FirstOrDefault(arg => arg.StartsWith("author="))?.Split('=')[1],
                    Since = args.FirstOrDefault(arg => arg.StartsWith("since="))?.Split('=')[1],
                    Until = args.FirstOrDefault(arg => arg.StartsWith("until="))?.Split('=')[1],
                    Limit = args.FirstOrDefault(arg => arg.StartsWith("limit="))?.Split('=')[1],
                    Verbose = args.FirstOrDefault(arg => arg.StartsWith("verbose="))?.Split('=')[1]
                };
                
                
                List<CommitMetadata?> commitPathsInFolder;
                try
                {
                    commitPathsInFolder = commitFiles
                        .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                        .Where(metadata => metadata != null) // Exclude invalid or null metadata
                        .Where(metadata =>
                            
                            // Filter by branch
                            (string.IsNullOrEmpty(filters.Branch) || metadata.Branch.Equals(filters.Branch, StringComparison.OrdinalIgnoreCase)) &&
                            // Filter by author
                            (string.IsNullOrEmpty(filters.Author) || 
                                (metadata.Author != null && metadata.Author.Equals(filters.Author, StringComparison.OrdinalIgnoreCase))) &&
                            // Filter by date range
                            (string.IsNullOrEmpty(filters.Since) || metadata.Date >= DateTimeOffset.Parse(filters.Since)) &&
                            (string.IsNullOrEmpty(filters.Until) || metadata.Date <= DateTimeOffset.Parse(filters.Until))

                        )
                        .OrderBy(metadata => metadata.Date)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error reading commit files: " + ex.Message);
                    return;
                }

                // Apply the limit if specified
                if (int.TryParse(filters.Limit, out int limit))
                {
                    commitPathsInFolder = commitPathsInFolder.Take(limit).ToList();
                }


                // Display commit history
                if (!commitPathsInFolder.Any())
                {
                    Logger.Log("No commits match the provided filters.");
                    return;
                }


                // Display commit history
                foreach (var commit in commitPathsInFolder)
                {
                    // Color code the commit output

                    // Commit Hash
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Log($"Commit:  {commit.Commit}");

                    // Author
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Log($"Author:  {commit.Author}");

                    // Date
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Log($"Date:    {commit.Date}");

                    // Branch
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Logger.Log($"Branch:  {commit.Branch}");

                    // Message
                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.Log($"Message: {commit.Message}");

                    // Files
                    if (filters.Verbose == "true")
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Logger.Log("Files:");
                        foreach (var file in commit.Files)
                        {
                            Logger.Log($"  {file.Key} : {file.Value}");
                        }
                    }


                    // Reset color after each commit log
                    Console.ResetColor();
                    Logger.Log(new string('-', 50));
                }



            }
        }










        /*
        public class PushCommand : BaseCommand
        {
            public override string Name => "push";
            public override string Description => "Pushes the local repository to the remote repository.";
            public override void Execute(string[] args)
            {
                try
                {
                    Logger.Log("Attempting");
                    string commitJson = PushHelper.GetCommitMetadataFiles(); // await
                    Logger.Log("Finished commitmetadata: " + commitJson);
                    // Get branch header
                    // TODO


                    // Send to backend
                    PushHelper.PostToBackendAsync(commitJson).GetAwaiter().GetResult(); // await
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed: " + ex);
                }




            }

        }
        */









        public class CreateBranchCommand : BaseCommand
        {
            public CreateBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "create_branch";
            public override string Description => "Creates a branch from the head commit of current branch.";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];
                if (!BranchHelper.IsValidBranchName(branchName))
                {
                    Logger.Log($"Invalid branch name: {branchName}");
                    return;
                }


                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (File.Exists(branchPath))
                {
                    Console.WriteLine($"Branch '{branchName}' already exists.");
                    return;
                }


                // Put the latest commit of the current branch into the new branch file
                string branchHeadCommit = CommandHelper.GetCurrentHEAD(Paths);

                File.WriteAllText(branchPath, branchHeadCommit);

                // Create branches file for main
                var branch = new Branch
                {
                    Name = branchName,
                    InitialCommit = branchHeadCommit,
                    CreatedBy = CommandHelper.GetUsername(),
                    ParentBranch = CommandHelper.GetCurrentBranchName(Paths),
                    Created = DateTimeOffset.Now
                };

                string branchJson = JsonSerializer.Serialize(branch, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(Paths.BranchesDir, branchName), branchJson);


                Logger.Log($"Created new branch {branchName}");
            }
        }

        public class DeleteBranchCommand : BaseCommand
        {
            public DeleteBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "delete_branch";
            public override string Description => "Deletes a branch.";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];

                // Check if user is in branch
                string currentBranch = CommandHelper.GetCurrentBranchName(Paths);
                if (currentBranch == branchName)
                {
                    Logger.Log($"Cannot delete branch '{branchName}' while on it.");
                    return;
                }


                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (!File.Exists(branchPath))
                {
                    Logger.Log($"Branch '{branchName}' doesnt exists.");
                    return;
                }


                // Prompt user to confirm deletion
                if (!CommandHelper.ConfirmAction(Logger, $"Are you sure you want to delete branch '{branchName}'?"))
                {
                    Logger.Log("Branch deletion cancelled.");
                    return;
                }



                // TODO: delete all commits in the branch (maybe the objects if they arnt used by another commit)



                // Delete the branch file
                File.Delete(branchPath);

                // Delete the branch file in branches directory
                File.Delete(Path.Combine(Paths.BranchesDir, branchName));


                Logger.Log($"Deleted branch {branchName}.");
            }
        }




        public class ListBranchesCommand : BaseCommand
        {
            public ListBranchesCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "list_branch";
            public override string Description => "list branch help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // Get all branches in heads directory
                string[] allBranchesPaths = Directory.GetFiles(Paths.HeadsDir);

                var currentBranch = CommandHelper.GetCurrentBranchName(Paths);


                foreach (string branchPath in allBranchesPaths)
                {
                    string branch = Path.GetFileName(branchPath);

                    if (branch == currentBranch)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }

                    Logger.Log(branch);

                    Console.ResetColor();
                }


            }
        }



        public class SwitchBranchCommand : BaseCommand
        {
            public SwitchBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "switch_branch";
            public override string Description => "switch branch help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];
                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (!File.Exists(branchPath))
                {
                    Console.WriteLine($"Branch '{branchName}' doesnt exists.");
                    return;
                }


                // Check if there are any uncommitted changes if so prompt user to confirm (uncommitted wont be saved)
                // if(ConfirmAction($"")) 



                try
                {
                    // Update the working directory with the files from the new branch
                    BranchHelper.UpdateWorkingDirectory(Logger, Paths, branchName);

                    // Update HEAD to point to the switched to branch
                    File.WriteAllText(Paths.HEAD, $"ref: heads/{branchName}");

                    Logger.Log($"Switched to branch {branchName}.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error switching branch '{branchName}': {ex.Message}");
                    return;
                }

            }
        }




        public class SwitchCommitCommand : BaseCommand
        {
            public SwitchCommitCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "switch_commit";
            public override string Description => "switch commit help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a commit hash.");
                    return;
                }

                string commitHash = args[0];
                string commitPath = Path.Combine(Paths.CommitDir, commitHash);

                if (!File.Exists(commitPath))
                {
                    Console.WriteLine($"Commit '{commitHash}' doesnt exists.");
                    return;
                }

                // Check if there are any uncommitted changes if so prompt user to confirm (uncommitted wont be saved)
                // if(ConfirmAction($"")) 


                try
                {
                    // Update the working directory with the files from the CommitHash
                    BranchHelper.UpdateWorkingDirectory(Logger, Paths, commitHash);

                    // Update HEAD to point to the detached HEAD
                    File.WriteAllText(Paths.HEAD, $"ref: {Paths.DETACHED_HEAD}");

                    Logger.Log($"Switched to commit {commitHash}.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error switching to commit '{commitHash}': {ex.Message}");
                    return;
                }

            }
        }



    }
}
